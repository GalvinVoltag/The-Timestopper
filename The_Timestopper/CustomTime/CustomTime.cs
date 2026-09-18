using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using Sandbox;
using The_Timestopper.Internal;
using UnityEngine;

namespace The_Timestopper.CustomTimeLibrary
{
    public static class UltimateTimeReplacer
    {
        private static readonly MethodInfo TimeScaleG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.timeScale));
        private static readonly MethodInfo DeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
        private static readonly MethodInfo FixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime));
        private static readonly MethodInfo CustomTimeScaleG = AccessTools.Method(typeof(CustomTime), nameof(CustomTime.GetTimeScale));
        private static readonly MethodInfo CustomDeltaTimeG = AccessTools.Method(typeof(CustomTime), nameof(CustomTime.GetDeltaTime));
        private static readonly MethodInfo CustomFixedDeltaTimeG = AccessTools.Method(typeof(CustomTime), nameof(CustomTime.GetFixedDeltaTime));
        private static readonly MethodInfo GameObjectGetter = AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            // bool isStatic = original.IsStatic;

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(DeltaTimeG))
                {
                    var newInst = new CodeInstruction(OpCodes.Ldarg_0);
                    newInst.labels.AddRange(instruction.labels);
                    newInst.blocks.AddRange(instruction.blocks);
                    yield return newInst;
                    
                    yield return new CodeInstruction(OpCodes.Callvirt, GameObjectGetter);
                    yield return new CodeInstruction(OpCodes.Call, CustomDeltaTimeG);
                    continue;
                }
                if (instruction.Calls(FixedDeltaTimeG))
                {
                    var newInst = new CodeInstruction(OpCodes.Ldarg_0);
                    newInst.labels.AddRange(instruction.labels);
                    newInst.blocks.AddRange(instruction.blocks);
                    yield return newInst;
                    
                    yield return new CodeInstruction(OpCodes.Callvirt, GameObjectGetter);
                    yield return new CodeInstruction(OpCodes.Call, CustomFixedDeltaTimeG);
                    continue;
                }
                if (instruction.Calls(TimeScaleG))
                {
                    var newInst = new CodeInstruction(OpCodes.Ldarg_0);
                    newInst.labels.AddRange(instruction.labels);
                    newInst.blocks.AddRange(instruction.blocks);
                    yield return newInst;
                    
                    yield return new CodeInstruction(OpCodes.Callvirt, GameObjectGetter);
                    yield return new CodeInstruction(OpCodes.Call, CustomTimeScaleG);
                    continue;
                }
                yield return instruction;
            }
        }
    }
    
    
    // IAlter for sandbox options of ULTRAKILL.
    // Todo: If you're going to make this class generic then get rid of it and use it on a derived class
    public class CustomTime : MonoBehaviour, IAlter, IAlterOptions<float> 
    {
        internal static readonly ConditionalWeakTable<GameObject, CustomTime> Registry =
            new ConditionalWeakTable<GameObject, CustomTime>();
        private static bool initialized = false;

        private static CustomTimeModule[] _modulePrototypes;
        private static CustomTimeModule[] modulePrototypes
        {
            get
            {
                if (_modulePrototypes != null) return _modulePrototypes;
                _modulePrototypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
                    })
                    .Where(t => t != null && t.IsSubclassOf(typeof(CustomTimeModule)) && !t.IsAbstract).Select(t => Activator.CreateInstance(t) as CustomTimeModule ).ToArray();
                return _modulePrototypes;
            }
        }

        public static void ReplaceConsoleLine(object o)
        {
            Stream stdout = Console.OpenStandardOutput();
            int lines = o.ToString().Split('\n').Length;
            int oldTop = Console.CursorTop;
            for (int i = 0; i <= lines; i++)
            {
                Console.CursorLeft = 0;
                byte[] clear = Encoding.UTF8.GetBytes( new string(' ', Console.WindowWidth-1));
                stdout.Write(clear, 0, clear.Length);
                Console.CursorTop--;
            }
            Console.CursorTop++;
            Console.CursorLeft = 0;
            byte[] bytes = Encoding.UTF8.GetBytes(o.ToString());
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
            Console.CursorTop = oldTop;
            Console.CursorLeft = 0;
        }
        public static bool ReferencesDeltaTime(MethodInfo method)
        {
            var body = method.GetMethodBody();
            if (body == null) return false;
            var raw = body.GetILAsByteArray();
            var instructions = HarmonyLib.PatchProcessor.GetOriginalInstructions(method);
            foreach (var i in instructions)
            {
                if (
                    i.Calls(AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime))) ||
                    i.Calls(AccessTools.PropertyGetter(typeof(Time), nameof(Time.timeScale))) ||
                    i.Calls(AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime))) 
                )
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// Start patching all required code to replace Time.deltaTime with CustomTime.GetDeltaTime(GameObject go) etc.
        /// </summary>
        public static void Initiate(ManualLogSource mls, Harmony harmony)
        {
            if (initialized) return;
            mls.LogWarning("Initiating experimental CustomTime transpiling process...");
            HarmonyMethod transpiler = new HarmonyMethod(typeof(UltimateTimeReplacer).GetMethod(nameof(UltimateTimeReplacer.Transpiler)));

            string barspace = "\n\n";

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm == null) continue;
                string assemblyName = asm.GetName().Name;
                if (assemblyName.StartsWith("System") || assemblyName.StartsWith("mscorlib") ||
                    assemblyName.StartsWith("0Harmony") || assemblyName.StartsWith("BepInEx") || assemblyName.StartsWith("The Timestopper") ||
                    assemblyName.StartsWith("Mono") || assemblyName.StartsWith("Harmony") || assemblyName.StartsWith("NewBlood")) continue;
                // if (!assemblyName.StartsWith("Assembly")) continue;
                
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e)
                { mls.LogError("cannot get types from " + assemblyName); types = e.Types.Where(t => t != null).ToArray(); }

                mls.LogWarning("total of " + types.Length + " types found in " + assemblyName + barspace);
                
                int totalTypes = types.Length;
                int patchTypes = 0;
                int totalChanges = 0;
                
                foreach (Type type in types)
                {
                    if (!typeof(Component).IsAssignableFrom(type)) continue;
                    
                    MethodInfo[] methods;
                    try { methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                    BindingFlags.Public | BindingFlags.NonPublic |
                                                    BindingFlags.DeclaredOnly); }catch {
                        mls.LogError("error while getting methods of " + type.Name + ". " + barspace);
                        continue;
                    }

                    foreach (MethodInfo method  in methods)
                    {
                        if (method.IsAbstract || method.ContainsGenericParameters || method.IsGenericMethodDefinition)
                            continue;
                        
                        try { if (method == null || !method.HasMethodBody() || method.GetMethodBody() == null) continue; }
                        catch {
                            mls.LogError("error while getting body of " + method.Name + " of " + type.Name + ". " + barspace);
                            continue; }
                        
                        try {
                            if (!ReferencesDeltaTime(method)) continue;
                            harmony.Patch(method, transpiler: transpiler);
                            totalChanges++;
                        }catch (Exception e) {
                            mls.LogError("error while transpiling " + method.Name + " of " + type.Name + ".\n" + e + barspace);
                            continue;
                        }
                    }
                    
                    string loadingString = "transpiling: " + (((float)patchTypes / totalTypes) * 100.0f).ToString("000.00") + " %\n";
                    loadingString += "[";
                    for (int i = 0; i < 100; i++)
                    {
                        if (i < ((float)patchTypes / totalTypes)*100) loadingString += "#";
                        else loadingString += " ";
                    }
                    loadingString += "]";
                    patchTypes++;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    ReplaceConsoleLine(loadingString);
                    Console.ResetColor();
                }
                
                if (totalChanges > 0)
                    mls.LogWarning("Transpiled with total of " + totalChanges + " changes.");
                else
                    mls.LogError("Transpiled with no changes (" + totalChanges + ").");
                
            }
            
            mls.LogWarning("patching ended ");
        
        } // Todo: eventually make CustomTime into its own plugin and this method into Awake()
        
        
        public static CustomTime GetCustomTimeOf(Transform t)
        {
            Transform target = t;
            CustomTime result = null;
            while (target != null && !Registry.TryGetValue(target.gameObject, out result))
            {
                target = target.parent;
            }
            return result;
        }

        public static float GetDeltaTime(GameObject go)
        {
            CustomTime CT = GetCustomTimeOf(go.transform);
            float timeScale = CT? CT.timeScale : Time.timeScale;
            return Time.unscaledDeltaTime*timeScale;
        }
        public static float GetFixedDeltaTime(GameObject go)
        {
            CustomTime CT = GetCustomTimeOf(go.transform);
            float timeScale = CT? CT.timeScale : Time.timeScale;
            return Time.fixedUnscaledDeltaTime*timeScale;
        }
        public static float GetTimeScale(GameObject go)
        {
            CustomTime CT = GetCustomTimeOf(go.transform);
            return CT? CT.timeScale : Time.timeScale;
        }
        public static void SetTimeScale(GameObject go, float f)
        {
            CustomTime CT = GetCustomTimeOf(go.transform);
            if (CT) CT.timeScale = f;
            else Time.timeScale = f;
        }



        private CustomTimeModule[] modules = {};
        private float _timeScale = 1;
        public float timeScale
        {
            get => _timeScale;
            set
            {
                if (Mathf.Approximately(_timeScale, value)) return;
                _timeScale = value;
                foreach (CustomTimeModule module in modules)
                    module?.OnTimeScaleChange(_timeScale);
                // foreach (Animator animator in animators)
                // {
                //     if (!animator) continue;
                //     animator.speed = _timeScale;
                // }
            }
        }
        
        private void Awake()
        {
            Registry.Add(gameObject, this);
            foreach (CustomTimeModule moduleType in modulePrototypes)
            {
                List<Component> components = new List<Component>();
                foreach (Type t in moduleType.GetValidTypes())
                {
                    components.AddRange(GetComponentsInChildren(t, true));
                }
                if (components.Count == 0) continue;
                
                CustomTimeModule newModule = Activator.CreateInstance(moduleType.GetType()) as CustomTimeModule;
                if ( newModule == null ) continue;

                newModule.customTime = this;
                newModule.Awake(components.ToArray());
                
                modules = modules.Concat( new CustomTimeModule[] { newModule } ).ToArray();
            }
            // animators = GetComponentsInChildren<Animator>();
        }

        private void OnDestroy()
        {
            Registry.Remove(gameObject);
            foreach (CustomTimeModule module in modules) {
                module.OnDestroy();
                module.customTime = null;
            }
        }

        public string alterKey => "CustomTime";
        public string alterCategoryName => "Custom Time Scale";

        public AlterOption<float>[] options
        {
            get
            {
                return new AlterOption<float>[1]
                {
                    new AlterOption<float>()
                    {
                        key = "timescale",
                        name = "Time Scale %",
                        value = timeScale*100.0f,
                        callback = (value => timeScale = value/100.0f)
                    }
                };
            }
        }
    }
}