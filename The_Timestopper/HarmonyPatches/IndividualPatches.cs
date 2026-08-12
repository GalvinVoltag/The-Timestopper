using HarmonyLib;
using The_Timestopper.Internal;
using ULTRAKILL.Cheats;
using UnityEngine;

namespace The_Timestopper.HarmonyPatches
{
    [HarmonyPatch(typeof(PlayerTracker), nameof(PlayerTracker.GetPlayerVelocity))]
    public class PLayerTrackerPatch
    {
        // ReSharper disable once ArrangeTypeMemberModifiers
        // ReSharper disable once RedundantAssignment
        // ReSharper disable once UnusedMember.Local
        static bool Prefix(ref Vector3 __result, bool trueVelocity)
        {
            __result = Timestopper.Instance.GetPlayerVelocity(trueVelocity);
            return false; // skip original code
        }
    }

    [HarmonyPatch(typeof(MonoBehaviour), nameof(MonoBehaviour.Invoke))]
    public class InvokePatch
    {
        // ReSharper disable once ArrangeTypeMemberModifiers
        // ReSharper disable once UnusedMember.Local
        static bool Prefix(MonoBehaviour __instance, string methodName, float time) {
            return !InvokeCaller.Add(__instance, methodName, time);
            // true: run original code, false: skip original code
        }
    }
    
    [HarmonyPatch(typeof(Noclip), nameof(Noclip.Enable))]
    public class NoclipPatch
    {
        // ReSharper disable once ArrangeTypeMemberModifiers
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        static void Postfix(Noclip __instance)
        {
            AccessTools.Field(typeof(Noclip), "rb").SetValue(__instance, MonoSingleton<NewMovement>.Instance?.rb);
        }
    }


    [HarmonyPatch(typeof(TimeSince), "op_Implicit", new [] { typeof(TimeSince) })]
    public class TimeSinceReplacer1
    {
        [HarmonyPrefix]
        static bool Prefix(ref float __result, TimeSince ts)
        {
            if (Timestopper.UnscaleTimeSince)
            {
                __result = Time.unscaledTime - (float)AccessTools.Field(typeof(TimeSince), "time").GetValue(ts);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(TimeSince), "op_Implicit", new [] { typeof(float) })]
    public class TimeSinceReplacer2
    {
        [HarmonyPrefix]
        static bool Prefix(ref TimeSince __result, float ts)
        {
            if (Timestopper.UnscaleTimeSince)
            {
                object result = new TimeSince();
                AccessTools.Field(typeof(TimeSince), "time").SetValue(result, Time.unscaledTime - ts);
                __result = (TimeSince)result;
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(SpriteController), "Awake")]
    class GetSpritesOutOfHUDLayer
    {
        static void Postfix(SpriteController __instance)
        {
            if (__instance.gameObject.layer != 0)
                __instance.gameObject.layer = 0;
            foreach (Transform child in __instance.GetComponentsInChildren<Transform>())
                child.gameObject.layer = 0;
        }
    }
}