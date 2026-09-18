using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using The_Timestopper.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace The_Timestopper
{
    [HarmonyPatch(typeof(AudioSource))]
    internal static class AudioPropertyPatch
    {
        [ThreadStatic] internal static bool InternalWrite;

        [HarmonyPatch("set_volume")]
        [HarmonyPrefix]
        static bool SetVolume(AudioSource __instance, ref float value)
        {
            if (InternalWrite) return true; // set actual value when setting from audioPitcher
            if (!AudioPitcher.Registry.TryGetValue(__instance, out var pitcher)) return true; // or when no AudioPitcher

            pitcher.sourceVolume = value;
            return false; // suppress original
        }

        [HarmonyPatch("get_volume")]
        [HarmonyPostfix]
        static void GetVolume(AudioSource __instance, ref float __result)
        {
            if (InternalWrite) return;
            if (AudioPitcher.Registry.TryGetValue(__instance, out var pitcher))
                __result = pitcher.sourceVolume; // return unaltered volume just in case
        }

        [HarmonyPatch("set_pitch")]
        [HarmonyPrefix]
        static bool SetPitch(AudioSource __instance, ref float value)
        {
            if (InternalWrite) return true; // ensure audioPitcher can write directly
            if (!AudioPitcher.Registry.TryGetValue(__instance, out var pitcher)) return true; // pitcherless audio

            pitcher.sourcePitch = value;
            return false; // suppress
        }

        [HarmonyPatch("get_pitch")]
        [HarmonyPostfix]
        static void GetPitch(AudioSource __instance, ref float __result)
        {
            if (InternalWrite) return;
            if (AudioPitcher.Registry.TryGetValue(__instance, out var pitcher))
                __result = pitcher.sourcePitch; // return unaltered pitch just in case
        }
    }
    public class AudioPitcher : MonoBehaviour
    {
        internal static readonly ConditionalWeakTable<AudioSource, AudioPitcher> Registry = new ConditionalWeakTable<AudioSource, AudioPitcher>();
        
        AudioSource source;
        public float sourceVolume;
        public float sourcePitch;
        float localTimeScale = 1;


        private bool isMusic; // or ambience 

        private void Awake()
        {
            if (!source){
                source = GetComponent<AudioSource>();
                if (!source)
                {
                    Destroy(this);
                    return;
                }
                sourceVolume = source.volume;
                sourcePitch = source.pitch;
            }

            if (!source.clip)
            {
                enabled = false;
                return;
            }
            
            isMusic = (source.spatialBlend == 0 && source.clip?.length > 10) 
                      || gameObject.name == "Battle Theme"
                      || gameObject.name == "Clean Theme"
                      || gameObject.name == "Boss Theme";
            LateUpdate();
        }

        private void OnEnable()
        {
            if (isMusic)
            {
                Timestopper.Log(gameObject.name + " Music detected with mixer: " + source.outputAudioMixerGroup.name,
                    true, ErrorLevel.Warning);
                return;
            }
            
            if (!source){
                source = GetComponent<AudioSource>();
                if (!source)
                {
                    Destroy(this);
                    return;
                }
                sourceVolume = source.volume;
                sourcePitch = source.pitch;
            }
            if (!source.clip)
            {
                enabled = false;
                return;
            }
            
            Registry.Add(source, this);
            LateUpdate();
        }

        private void OnDisable()
        {
            if (source)
                Registry.Remove(source);
        }

        private void LateUpdate()
        {
            AudioPropertyPatch.InternalWrite = true;
            if (isMusic)
            {
                source.volume = Mathf.Lerp(Timestopper.stoppedMusicVolume.value*sourceVolume, sourceVolume, Timestopper.realTimeScale);
                source.pitch = Mathf.Lerp(Timestopper.stoppedMusicPitch.value*sourcePitch, sourcePitch, Timestopper.realTimeScale);
                return;
            }
            
            if (!source) {
                enabled = false;
                return;
            }
            
            
            if (localTimeScale > Timestopper.realTimeScale)
                localTimeScale = Mathf.Max(localTimeScale - Time.unscaledDeltaTime / Timestopper.affectSpeed.value, Timestopper.realTimeScale);
            if (localTimeScale < Timestopper.realTimeScale)
                localTimeScale = Mathf.Min(localTimeScale + Time.unscaledDeltaTime / Timestopper.affectSpeed.value, Timestopper.realTimeScale);
            localTimeScale = Mathf.Clamp(localTimeScale, 0, 1);

            source.pitch = sourcePitch * localTimeScale;
            AudioPropertyPatch.InternalWrite = false;
        }
        
    }
}