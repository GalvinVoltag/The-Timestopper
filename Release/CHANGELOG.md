# CHANGELOG:


<details>
  <summary> 1.0.0 </summary>

  ### FULL RELEASE!
  - Performance improvements
  - Now partially Linux compatible!
  - New Timestopper model!
  - Parrying now fills time juiced
  - Timestopper now moves out of the way when punching.
  - Added the Timestopper arm textmode image to the main menu!
  - Rockets are now rideable in stopped time!
  - Landmines do not explode in stopped time (except when you slam onto them)
  - Timestopper now bobs while walking
  - Removed Configgy dependency
  - Restructured configs
  - Fixed the Time Stop style effect being spammable
  - Fixed the Whiplash in Stopped Time
  - Fixed Audio effects not applying to CyberGrind music

  ### technical changes
  - mod GUID has been changed to "dev.galvin.timestopper"
  - Timestopper now uses ULTRAKILL/Master shader
  - Better code structure
  - Started using Unity Addressables for the asset bundle
  - StopTime() and StartTime() functions no longer require Player, NewMovement or Playerstopper components
  - Playerstopper component now has a static Instance
  - Finally figured out how MonoSingletons work

  ## I have a donation link, and would appreaciate some help  \^v\^

</details>

---

<details>
  <summary>0.9.9</summary>

  ### The Integration Update!
  - Added new Style "TIME STOP" which is worth 200 points, subject to change
  - JackHammer now works although a bit janky
  - Added Alt and Alt White HUD elements for the Time Juice
  - Time Juice now resets properly when died or reset to checkpoint
  - Time Juice bar now doesn't overlap with the Speedometer
  - The Speedometer now updates as intended in stopped time
  - Fixed a little bug with Compatability with CyberGrindMusicExplorer

  ### technical changes
  - Patched TimeSince to use unscaledDeltaTime when Timestopper.unscaleTimeSince is true. Used in FixeUpdateCaller when calling FixedUpdate manually.
  - Time Juice resets now use StatsManager.checkpointRestart
  - Speedometer and other HUD elements which overlapped with Time Juice bar are now moved instead of set to position
  - More integration with already there Ultrakill classes and structs

</details>

---

<details>
  <summary>0.9.8</summary>

  ### The Freezeframe Comeback!
  - The freezeframe effect now allows rockets to move through stopped time.
  - The Timestopper can no longer be upgraded indefinitely, 10 is max by default (customizable in the configgy menu)
  - Added the option to downgrade the arm to the maximum upgrade count
  - Added a message to indicate the appearance of a new door in 7-1
  - Fixed inconsistent movement in timestop
  - Fixed parrying catapulting Player
  - Fixed physics speeding up during the timestop sequence

  ### technical changes
  - Reworked FixedUpdateCaller system
  - Timestop Jump Fix has been reworked, but still doesn't act identically to non-timestop
  - Reworked Timestopper.playerTimeScale, Timestopper.playerDeltaTime and Timestopper.playerFixedDeltaTime
  - Timestopper.playerDeltaTime and Timestopper.playerFixedDeltaTime are now read only properties instead of fiels

</details>

---

<details>
  <summary>0.9.7</summary>
  
  ### Global Fix
  - Fixed a bug where movement was FPS dependent
  
</details>

---

<details>
  <summary>0.9.6</summary>
  
  ### Little Update
  - Fixed a bug where the time juice would still drain in the pause menu
  - Fixed a typo in configgy settings, "Interaction Slowdown Multiplier"
  - Recalibrated default configgy settings
  - Hopefully fixed some Null Reference Exceptions
  - Added temporary fixes to configgy menu for a bug where Player slowed down
  
</details>

---

<details>
  <summary>0.9.5</summary>
  
  ### Emergency QuickFix
  - Fixed a bug where the mod didn't work at all
  
</details>

---

<details>
  <summary>0.9.4</summary>
  
  ### Ultra Bugfix
  - Movement in timestop got reworked
  - A bug fixed where timestart would catapult Player
  - Every gun except the jackhammer works (hopefully) properly now
  - Added animation speed multiplier to settings
  - Complete (90%) code rework
  - Improved performance (probably)
  - #### Jackhammer still doesn't work!

  ### technical changes
  - Codebase cleaned, now it is easier to use, for possible use as timestop library
  - Timestop is now Action based instead of hard setting timeScale every frame
  - Many hardcoded main game modifications are automated, so they won't break with further updates (hopefully)
  - Main game patches now change Time.deltaTime with Timestopper.playerDeltaTime instead of Time.unscaledDeltaTime

  ## notes:
  Even though you can use this mod as a library to stop and start time, I recommend you to communicate with me before doing so, for convenience sake. I may release a separate library for timestop related functions.
  Right now, if you include and reference Timestopper in your mod base, you should be able to use Timestopper.StopTime() and Timestopper.StartTime() easily. I don't think I will ever change the function names, but new ones may be added or current ones might be removed.
</details>

---

<details>
  <summary>0.9.2</summary>
  
  ### Cybergrind Fix
  - A bug fixed where Timestopper didn't work in cybergrind
  - Added compatibility with Cybergrind Music Explorer mod, the HUD doesn't overlap
  - Cleaned the code a little bit
  - Properly added github repository
  - Learned how to properly use SceneManager.SceneLoaded
</details>

---

<details>
  <summary>0.9.1</summary>
  
  ### Quick bugfix
  - readme updated
  - manifest updated
  - fixed dependency strings
</details>

---

<details>
  <summary>0.9.0</summary>
  
  ### Initial public release
</details>
