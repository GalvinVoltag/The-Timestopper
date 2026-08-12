using System;
using The_Timestopper.Internal;
using UnityEngine;

namespace The_Timestopper
{
    public class AudioPitcher : MonoBehaviour
    {
        AudioSource source;
        AudioSource audio;
        float localTimeScale = 1;
        private bool cyberGrindCompatability = false;
        private float cybergrindVolume;
        private float cybergrindPitch;


        private bool isMusic = false; // or ambience 

        private void Awake()
        {
            if (!source){
                source = GetComponent<AudioSource>();
                if (!source)
                {
                    Destroy(this);
                    return;
                }
                if (!cyberGrindCompatability) source.mute = true;
            }
            isMusic = (source.spatialBlend == 0 && source.clip.length > 10) 
                      || gameObject.name == "Battle Theme"
                      || gameObject.name == "Clean Theme"
                      || gameObject.name == "Boss Theme";
            cyberGrindCompatability = Timestopper.cybergrind && Timestopper.Compatability_JukeBox && isMusic;

            if (cyberGrindCompatability)
            {
                cybergrindVolume = source.volume;
                cybergrindPitch = source.pitch;
            }
            if (!audio && !cyberGrindCompatability)
            {
                audio = gameObject.CopyComponent(source);
                audio.mute = false;
                if (source.isPlaying) audio.Play();
                audio.time = source.time;
            }
        }

        private void OnEnable()
        {
            if (!source){
                if (audio)
                {
                    Destroy(this);
                    return;
                }
                source = GetComponent<AudioSource>();
                if (!source)
                {
                    Destroy(this);
                    return;
                }
                source.mute = true;
            }

            if (cyberGrindCompatability) return;
            if (audio)
            {
                if (audio.mute) audio.mute = false;
                if (source.isPlaying) audio.Play();
                audio.time = source.time;
                return;
            }
            
            audio = gameObject.CopyComponent(source);
            audio.mute = false;
            if (source.isPlaying) audio.Play();
            audio.time = source.time;
            
            if (isMusic) Timestopper.Log(gameObject.name + " Music detected with mixer: " + source.outputAudioMixerGroup.name, true, ErrorLevel.Warning);
            
        }

        private void OnDisable()
        {
            if (source) source.mute = false;
            if (audio) audio.mute = true;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        private void LateUpdate()
        {
            if (!source) {
                enabled = false;
                return;
            }

            if (cyberGrindCompatability)
            {
                source.volume = Mathf.Lerp(Timestopper.stoppedMusicVolume.value, cybergrindVolume, Timestopper.realTimeScale);
                source.pitch = Mathf.Lerp(Timestopper.stoppedMusicPitch.value, cybergrindPitch, Timestopper.realTimeScale);
                return;
            }
            
            ////////////////////////////////////////////////////////////// SYNC ISSUES AAAAUUUGHGHHGH!
            if (source.isPlaying != audio.isPlaying)
            {
                if (source.isPlaying) audio.Play();
                else audio.Stop();
                audio.time = source.time;
            }
            if (source.clip !=  audio.clip) audio.clip = source.clip;
            if (source.enabled != audio.enabled) audio.enabled = source.enabled;
            //\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\ SYNC END >:C
            
            if (isMusic)
            {
                audio.volume = Mathf.Lerp(Timestopper.stoppedMusicVolume.value*source.volume, source.volume, Timestopper.realTimeScale);
                audio.pitch = Mathf.Lerp(Timestopper.stoppedMusicPitch.value*source.pitch, source.pitch, Timestopper.realTimeScale);
                return;
            }
            
            
            if (localTimeScale > Timestopper.realTimeScale)
                localTimeScale = Mathf.Max(localTimeScale - Time.unscaledDeltaTime / Timestopper.affectSpeed.value, Timestopper.realTimeScale);
            if (localTimeScale < Timestopper.realTimeScale)
                localTimeScale = Mathf.Min(localTimeScale + Time.unscaledDeltaTime / Timestopper.affectSpeed.value, Timestopper.realTimeScale);
            localTimeScale = Mathf.Clamp(localTimeScale, 0, 1);

            audio.pitch = source.pitch * localTimeScale;
        }
        
    }
}