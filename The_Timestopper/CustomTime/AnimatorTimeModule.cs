using System;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace The_Timestopper.CustomTimeLibrary
{
    [HarmonyPatch(typeof(Animator))]
    internal static class AnimatorSpeedPatch
    {
        [ThreadStatic] internal static int InternalWrite = -1;

        [HarmonyPatch("set_speed")]
        [HarmonyPrefix]
        static bool SetVolume(Animator __instance, ref float value)
        {
            if (InternalWrite >= 0) return true; // set actual value when setting from audioPitcher
            if (!AnimatorTimeModule.Registry.TryGetValue(__instance, out var animator)) return true; // or when no AudioPitcher

            animator.Re = value;
            return false; // suppress original
        }

        [HarmonyPatch("get_speed")]
        [HarmonyPostfix]
        static void GetVolume(Animator __instance, ref float __result)
        {
            if (InternalWrite >= 0) return;
            if (AnimatorTimeModule.Registry.TryGetValue(__instance, out var animator))
                __result = animator.sourceVolume; // return unaltered volume just in case
        }
    }
    
    /// <summary>
    /// Wrapper for Animator component to obey CustomTime time system. Can be used as a basic starting point for custom modules.
    /// </summary>
    public class AnimatorTimeModule : CustomTimeModule
    {
        internal static readonly ConditionalWeakTable<Animator, AnimatorTimeModule> Registry = new ConditionalWeakTable<Animator, AnimatorTimeModule>();
        
        
        private Animator[] animators;
        private float[] realSpeeds;
        public override Type[] GetValidTypes()
        {
            return new Type[] { typeof(Animator) };
        }

        public override void OnTimeScaleChange(float newTimeScale)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                AnimatorSpeedPatch.InternalWrite = i;
                animators[i].speed = newTimeScale*realSpeeds[i];
            }
            foreach (Animator animator in animators)
            {
                if (!animator) continue;
                animator.speed = newTimeScale;
            }
        }

        public override void Awake(Component[] components)
        {
            animators = components.Select(c => c as Animator).ToArray();
            realSpeeds = new float[animators.Length];
            for (int i=0; i<realSpeeds.Length; i++) realSpeeds[i] = animators[i].speed;
            
        }

        public override void OnDestroy()
        {
            animators = Array.Empty<Animator>();
            realSpeeds = Array.Empty<float>();
        }
    }
}