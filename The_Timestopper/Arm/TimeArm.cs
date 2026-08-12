using System.Collections.Generic;
using BepInEx;
using The_Timestopper.Internal;
using The_Timestopper.Player;
using UnityEngine;

namespace The_Timestopper.Arm
{
    // public class TimeArm : MonoBehaviour   // Component on the arm of player
    // {
    //     public static TimeArm Instance;
    //     public float timeLeft = TimestopperProgress.MaxTime;
    //     public AudioSource armAudio;
    //     public NewMovement movement;
    //     public Animator animator;
    //     public bool localTimeStopTracker;
    //
    //     public void Equip()
    //     {
    //         gameObject.SetActive(true);
    //         TimestopperProgress.EquipArm(true);
    //     }
    //
    //     public void Reset()
    //     {
    //         animator.Play("Idle");
    //         timeLeft = TimestopperProgress.MaxTime;
    //         localTimeStopTracker = Timestopper.TimeStop;
    //     }
    //     public void Awake()
    //     {
    //         Instance = this;
    //         timeLeft = TimestopperProgress.MaxTime;
    //         gameObject.SetActive(TimestopperProgress.EquippedArm);
    //         movement = Playerstopper.Instance.movement;
    //         animator = GetComponentInChildren<Animator>();
    //         armAudio = GetComponentInChildren<AudioSource>();
    //         animator.Play("Idle");
    //     }
    //
    //     public void UpdateTimeJuice()
    //     {
    //         if (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown)
    //         {
    //             timeLeft = TimestopperProgress.MaxTime;
    //             return;
    //         }
    //         if (localTimeStopTracker)
    //         {
    //             timeLeft -= Timestopper.playerDeltaTime * (1.0f - Timestopper.realTimeScale);
    //             Timestopper.StoppedTimeAmount += Timestopper.playerDeltaTime * (1.0f - Timestopper.realTimeScale);
    //             if (timeLeft < 0)
    //                 timeLeft = 0;
    //         }
    //         else
    //         {
    //             if (Timestopper.realTimeScale <= 0.3f)
    //                 if (MonoSingleton<NewMovement>.Instance)
    //                     MonoSingleton<NewMovement>.Instance.walking = false;
    //             timeLeft += Time.deltaTime * Timestopper.refillMultiplier.value;
    //             if (timeLeft > TimestopperProgress.MaxTime)
    //                 timeLeft = TimestopperProgress.MaxTime;
    //         }
    //     }
    //     
    //     
    //     public void Update()
    //     {
    //         if (!MonoSingleton<FistControl>.Instance) return;
    //         //decoration
    //         Vector3 newRot = new Vector3(0f, (0.3f * MonoSingleton<FistControl>.Instance.fistCooldown), (0.1f * MonoSingleton<FistControl>.Instance.fistCooldown));
    //         transform.localEulerAngles = (newRot * (20 * Timestopper.playerDeltaTime) + transform.localEulerAngles) / (1 + Timestopper.playerDeltaTime*20);
    //         //decoration end
    //         if (movement.dead && Timestopper.TimeStop && localTimeStopTracker)
    //         {
    //             Timestopper.StartTime(0);
    //             animator.Play("Idle");
    //             return;
    //         }
    //         UpdateTimeJuice();
    //         if ((UnityInput.Current.GetKeyDown(Timestopper.stopKey.value))
    //             || (Timestopper.TimeStop && timeLeft <= 0.0f))
    //         {
    //             if (MonoSingleton<OptionsManager>.Instance.paused) //if game paused
    //                 return;
    //             if (!localTimeStopTracker && !MonoSingleton<FistControl>.Instance.shopping && timeLeft > Timestopper.lowerTreshold.value)
    //             {
    //                 PlayRespectiveSound(true);
    //                 AnimatorsFix();
    //                 Timestopper.StopTime(Timestopper.stopSpeed.value);
    //                 Grayscaler.Instance.intensityControl = 1;
    //                 Grayscaler.Instance.grayscaleBubbleExpansion = 0;
    //                 animator.speed = Timestopper.animationSpeed.value;
    //                 animator.Play("Stop");
    //                 Timestopper.FixedUpdateFix(movement.transform);
    //                 Timestopper.Log("Time stops!", true);
    //             }
    //             else if (localTimeStopTracker)
    //             {
    //                 PlayRespectiveSound(false);
    //                 Timestopper.StartTime(Timestopper.startSpeed.value);
    //                 animator.speed = Timestopper.animationSpeed.value;
    //                 animator.Play("Start");
    //                 Timestopper.Log("Time flows normally.", true);
    //             }
    //             PlayRespectiveSound(Timestopper.TimeStop);
    //             localTimeStopTracker = Timestopper.TimeStop;
    //             ParticlesFix();
    //         }
    //
    //         if (localTimeStopTracker != Timestopper.TimeStop)
    //         {
    //             ParticlesFix();
    //         }
    //         
    //
    //         if (Timestopper.TimeStop)
    //         {
    //             if (Timestopper.timestopHardDamage.value)
    //             {
    //                 movement.ForceAddAntiHP(Timestopper.antiHpMultiplier.value * Time.unscaledDeltaTime * (1 - Timestopper.realTimeScale), true, true, true, false);
    //             }
    //         }
    //     }
    //
    //     private void AnimatorsFix()
    //     {
    //         foreach (Animator A in movement.GetComponentsInChildren<Animator>(true))
    //         {
    //             if (A.GetComponent<AnimatorUpdater>() == null)
    //                 A.gameObject.AddComponent<AnimatorUpdater>();
    //         }
    //     }
    //
    //     private HashSet<Transform> latestObjects = new HashSet<Transform>();
    //     private void ParticlesFix()
    //     {
    //         foreach (Transform t in movement.transform)
    //         {
    //             RecrusiveTracking(t);
    //         }
    //     }
    //
    //     private void RecrusiveTracking(Transform t)
    //     {
    //         if (!latestObjects.Add(t)) return;
    //         Timestopper.Log("particle system updated: " + t.name, true, ErrorLevel.Info);
    //         ParticleSystem P = t.GetComponent<ParticleSystem>();
    //         Timestopper.FixedUpdateFix(t);
    //         if (P != null) P.gameObject.AddComponent<ParticleSystemUpdater>();
    //     }
    //
    //     private bool oldPause;
    //     public void LateUpdate() // pause animations in pause menu
    //     {
    //         if (Timestopper.TimeStop && MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused != oldPause)
    //         {
    //             oldPause = MonoSingleton<OptionsManager>.Instance.paused;
    //             ParticlesFix();
    //         }
    //     }
    //     public void PlayRespectiveSound(bool istimestopped)
    //     {
    //         if (istimestopped != Timestopper.TimeStop)
    //             if (istimestopped)
    //             {
    //                 if (Timestopper.StoppedTimeAmbiences[Timestopper.stoppedSound.valueIndex] != null)
    //                 {
    //                     armAudio.volume = Timestopper.soundEffectVolume.value;
    //                     armAudio.clip = Timestopper.StoppedTimeAmbiences[Timestopper.stoppedSound.valueIndex];
    //                     armAudio.Play();
    //                     armAudio.loop = true;
    //                     armAudio.PlayOneShot(Timestopper.TimestopSounds[Timestopper.stopSound.valueIndex]);
    //                 }
    //             }
    //             else
    //             {
    //                 armAudio.Stop();
    //                 armAudio.volume = Timestopper.soundEffectVolume.value;
    //                 if (Timestopper.TimestartSounds[Timestopper.startSound.valueIndex] != null)
    //                     armAudio.PlayOneShot(Timestopper.TimestartSounds[Timestopper.startSound.valueIndex]);
    //             }
    //     }
    // }
}