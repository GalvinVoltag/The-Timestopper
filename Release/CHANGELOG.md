# CHANGELOG:


<details>
  <summary>0.9.6</summary>
  
  ### Little Update
  - Fixed a bug where the time juice would still drain in the pause menu
  - Fixed a typo in configgy settings, "Interaction Slowdown Pultiplier"
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
  - Movement in timestop got reorked
  - A bug fixed where timestart would capatult Player
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
  - Added compatability with Cybergrind Music Explorer mod, the HUD doesn't overlap
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
