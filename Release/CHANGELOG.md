# CHANGELOG:

<details>
  <summary>1.5.3 | folder structure fix </summary>

#### Fix
- **fixed** folder structure being messed up after downloading from r2modman

</details>

-----

<details>
  <summary>1.5.0 | REMASTER + tons of bugfixes + QoL </summary>

  #### Summary
  - Various performance improvements
  - New shaders!
  - Better HUD integration
  - Full audio customization
  - **DualWield powerup now works as intended!**
  - **JackHammer now works in timestop as intended!**
  - TimeArmPickup prefab in the assetbundle
  - Enemies now take time to relocate the Player after timestop
  - **Fixed** weird enemy behavior due to use of BlindCheat in stopped time
  - **Fixed** weird backface issue in Timestopper's fingers
  - **Fixed** disappearing text completely
  - **Fixed** V2 softlock when V2 was defeated during timestop
  - **Fixed** Speedometer and other player speed getter related problems
  - **Fixed** Inverted crosshair not working with grayscale shaders


  #### Technical Wrapup
  - Added Timestopper Pickup prefab into the assetbundle
  - Fixed text disappearance by not using Graphics.Blit()
    - Instead, a very thick cube right in front of the Main Camera is spawned with a transparent grayscale shader
    - A depth camera renders a depth texture for the shader to use, inside a global variable called _MyCustomDepth
    - This shader should be more bug free, since there is no more rendering the HUD twice to maintain color
  - BlindCheat is no longer used to prevent visual bugs during timestop, instead a Player dummy is replaced in the PlayerTracker instance
    - This also fixes V2-1st softlock if she was defeated during timestop
  - Added the WaitForPlayerSeconds() yieldInstruction
    - Patched JackHammer to change all WaitForSeconds() to WaitForPlayerSeconds()
  - Performance inprovements
    - Gibs are no longer simulated when they are completely halt
    - Hud no longer renders twice for the grayscale effect
    - Epensive function calls per frame have been reduced
    - HUD elements operate on custom scripts directly attached to them
    - Active game object tracking instead of per frame search operations
  - The arm is now called Time Arm instead of Gold Arm to avoid confusion upon the release of official Golden Arm
  - More efficient asset bundle and TimeArm prefab
  - Functionality is now inside the arm instance itself rather than looking up variables tied to the Player
  - Certain functions were carried to their own classes that make more sense
  - got rid of FindRootGameobject()
  - added TimeHUD class for Time Juice Bar HUD element

</details>

---

<details>
  <summary>1.0.5 | config change </summary>

  #### Very Small
  - Grayscale amount can now be set to any number

</details>

---

<details>
  <summary>1.0.4 | text disappear fix + new message + music </summary>

  #### Small
  - The arm now displays a messsage of which button to use it when you pick it up
  - Menu texts no longer disappear because of the mod in some custom maps
  - Music detection is now more accurate and should work in all levels
  - Fixed a bug where the pickup animation wouldn't play

</details>

---

<details>
  <summary>1.0.3 | bugfix + sandbox arm</summary>

  #### Small
  - Bugfixes related to configgy menus
  - The Sandbox Arm now works in stopped time

</details>

---

<details>
  <summary>1.0.2 | another bugfix and hud improvement</summary>

  #### Small
  - HUD for time juice should now act as intended when any changes occur in the HUD
  - More null preventers were added to the save system for timestopper.state

</details>

---

<details>
  <summary>1.0.1 | very small bugfix</summary>

  #### Very small
  - Fixed a bug where The Timestopper would disappear when died
  - Removed the Under Construction text from the Gold Door in 7 - 1
  - Updated the arm description, so the mechanics are more clear
  - Updated the graphics in the README.md file

</details>

---

<details>
  <summary> 1.0.0 FULL RELEASE </summary>

  #### >Grayscale Shaders are not supported in Linux Machines yet!

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
