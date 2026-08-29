using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox;
using The_Timestopper.Internal;
using UnityEngine;

namespace The_Timestopper
{
    // [HarmonyPatch(typeof(Time))]
    // internal static class DeltaTimePatch
    // {
    //     [HarmonyPatch(nameof(Time.deltaTime), MethodType.Getter)]
    //     [HarmonyPostfix]
    //     static void GetDeltaTime(ref float __result)
    //     {
    //         if (!Timestopper.TimeStop) return;
    //         __result = Timestopper.playerDeltaTime;
    //     }
    //     
    //     [HarmonyPatch(nameof(Time.fixedDeltaTime), MethodType.Getter)]
    //     [HarmonyPostfix]
    //     static void GetFixedDeltaTime(ref float __result)
    //     {
    //         if (!Timestopper.TimeStop) return;
    //         __result = Timestopper.playerFixedDeltaTime;
    //     }
    //
    //     [HarmonyPatch(nameof(Time.timeScale), MethodType.Getter)]
    //     [HarmonyPostfix]
    //     static void GetTimeScale(ref float __result)
    //     {
    //         if (!Timestopper.TimeStop) return;
    //         __result = Timestopper.playerTimeScale;
    //     }
    // }

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
            bool isStatic = original.IsStatic;

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
    
    
    public class CustomTime : MonoBehaviour, IAlter, IAlterOptions<float>
    {
        internal static readonly ConditionalWeakTable<GameObject, CustomTime> Registry =
            new ConditionalWeakTable<GameObject, CustomTime>();
        
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
            return Time.fixedDeltaTime*timeScale;
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



        private Animator[] animators;
        private float _timeScale = 1;
        public float timeScale
        {
            get => _timeScale;
            set
            {
                _timeScale = value;
                foreach (Animator animator in animators)
                {
                    if (!animator) continue;
                    animator.speed = _timeScale;
                }
            }
        }
        
        private void Awake()
        {
            Registry.Add(gameObject, this);
            animators = GetComponentsInChildren<Animator>();
        }

        private void OnDestroy()
        {
            Registry.Remove(gameObject);
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