using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;
using The_Timestopper.Internal;
using UnityEngine;

namespace The_Timestopper.HarmonyPatches
{
    // ################################################  PATCHWORK  ######################################################## \\
    /*                                DeltaTimeReplacer replaces the following code:                                         *\
     *                                Time.timeScale -> Timestopper.playerTimeScale                                          *
     *                                Time.deltaTime -> Timestopper.playerDeltaTime                                          *
     *                                Time.fixedDeltaTime -> Time.unscaledFixedDeltaTime                                     *
    \*                                WaitForSeconds -> WaitForPlayerSeconds                                                 */
    
    public class DeltaTimeReplacer
    {
        private static ManualLogSource mls;
        
        private static readonly MethodInfo TimeScaleG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.timeScale));
        private static readonly MethodInfo DeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
        private static readonly MethodInfo FixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime));
        private static readonly MethodInfo PlayerTimeScaleG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerTimeScale));
        private static readonly MethodInfo PlayerDeltaTimeG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerDeltaTime));
        private static readonly MethodInfo PlayerFixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerFixedDeltaTime));
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, string name = "CODE")
        {
            var smoothDampAngle4 = AccessTools.Method(typeof(Mathf), nameof(Mathf.SmoothDampAngle),
                new[] { typeof(float), typeof(float), typeof(float).MakeByRefType(), typeof(float) });
            var smoothDampAngle5 = AccessTools.Method(typeof(Mathf), nameof(Mathf.SmoothDampAngle),
                new[] { typeof(float), typeof(float), typeof(float).MakeByRefType(), typeof(float), typeof(float) });
            var smoothDampAngle6 = AccessTools.Method(typeof(Mathf), nameof(Mathf.SmoothDampAngle),
                new[] { typeof(float), typeof(float), typeof(float).MakeByRefType(), typeof(float), typeof(float), typeof(float) });

            var smoothDamp4 = AccessTools.Method(typeof(Mathf), nameof(Mathf.SmoothDamp),
                new[] { typeof(float), typeof(float), typeof(float).MakeByRefType(), typeof(float) });
            var smoothDamp5 = AccessTools.Method(typeof(Mathf), nameof(Mathf.SmoothDamp),
                new[] { typeof(float), typeof(float), typeof(float).MakeByRefType(), typeof(float), typeof(float) });
            var smoothDamp6 = AccessTools.Method(typeof(Mathf), nameof(Mathf.SmoothDamp),
                new[] { typeof(float), typeof(float), typeof(float).MakeByRefType(), typeof(float), typeof(float), typeof(float) });

            var vec3SmoothDamp4 = AccessTools.Method(typeof(Vector3), nameof(Vector3.SmoothDamp),
                new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3).MakeByRefType(), typeof(float) });
            var vec3SmoothDamp5 = AccessTools.Method(typeof(Vector3), nameof(Vector3.SmoothDamp),
                new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3).MakeByRefType(), typeof(float), typeof(float) });
            var vec3SmoothDamp6 = AccessTools.Method(typeof(Vector3), nameof(Vector3.SmoothDamp),
                new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3).MakeByRefType(), typeof(float), typeof(float), typeof(float) });
            
            
            // var TimeScaleG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.timeScale));
            // var DeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
            // var FixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime));
            // var PlayerTimeScaleG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerTimeScale));
            // var PlayerDeltaTimeG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerDeltaTime));
            // var PlayerFixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerFixedDeltaTime));
            
            // var waitForSecondsG = AccessTools.Constructor(typeof(WaitForSeconds), new Type[] { typeof(float) });
            var waitForPlayerSecondsG = AccessTools.Constructor(typeof(WaitForPlayerSeconds), new [] { typeof(float) });
            
            if (mls == null) mls = BepInEx.Logging.Logger.CreateLogSource(Timestopper.Name);
            
            mls.LogWarning($"Transpiling " + name + "...");
            int totalChanges = 0;
            foreach (var i in instructions)
            {
                totalChanges++;
                if (i.Calls(DeltaTimeG))
                {
                    var newInst = new CodeInstruction(OpCodes.Call, PlayerDeltaTimeG);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                    //mls.LogInfo($"modified deltaTime");
                }
                else if (i.Calls(FixedDeltaTimeG))
                {
                    var newInst = new CodeInstruction(OpCodes.Call, PlayerFixedDeltaTimeG);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                    //mls.LogInfo($"modified fixedDeltaTime");
                }
                else if (i.Calls(TimeScaleG))
                {
                    var newInst = new CodeInstruction(OpCodes.Call, PlayerTimeScaleG);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                    // mls.LogInfo($"modified timeScale");
                }
                else if (i.opcode == OpCodes.Newobj && i.operand is ConstructorInfo ctor && ctor.DeclaringType == typeof(WaitForSeconds))
                {
                    var newInst = new CodeInstruction(OpCodes.Newobj, waitForPlayerSecondsG);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                    // mls.LogInfo($"modified WaitForSeconds");
                }
                else if (i.Calls(smoothDampAngle4))
                {
                    // stack has: current, target, ref velocity, smoothTime
                    // push maxSpeed, deltaTime
                    yield return new CodeInstruction(OpCodes.Ldc_R4, float.PositiveInfinity);
                    yield return new CodeInstruction(OpCodes.Call, PlayerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, smoothDampAngle6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(smoothDampAngle5))
                {
                    // stack has: current, target, ref velocity, smoothTime, maxSpeed
                    // push deltaTime
                    yield return new CodeInstruction(OpCodes.Call, PlayerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, smoothDampAngle6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(smoothDamp4))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, float.PositiveInfinity);
                    yield return new CodeInstruction(OpCodes.Call, PlayerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, smoothDamp6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(smoothDamp5))
                {
                    yield return new CodeInstruction(OpCodes.Call, PlayerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, smoothDamp6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(vec3SmoothDamp4))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, float.PositiveInfinity);
                    yield return new CodeInstruction(OpCodes.Call, PlayerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, vec3SmoothDamp6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(vec3SmoothDamp5))
                {
                    yield return new CodeInstruction(OpCodes.Call, PlayerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, vec3SmoothDamp6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else
                {
                    totalChanges--;
                    yield return i;
                }
            }
            if (totalChanges > 0)
                mls.LogWarning($"Transpiling " + name + " done with total of " + totalChanges + " changes.");
            else
                mls.LogError($"Transpiling " + name + " done with total of " + totalChanges + " changes.");
        }
    }
}