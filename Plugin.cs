using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System.ComponentModel;
using System.Linq;
using Gravity;
using PluginConfig.API.Functionals;
using PluginConfiguratorComponents;
using The_Timestopper.Player;
using The_Timestopper.Arm;
using The_Timestopper.HarmonyPatches;
using The_Timestopper.Internal;
using The_Timestopper.Physics;
using The_Timestopper.Player;
using ULTRAKILL.Portal;
using UnityEngine.Networking;
using UnityEngine.UI;
using Component = UnityEngine.Component;

// ReSharper disable ArrangeModifiersOrder
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable TooWideLocalVariableScope
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace The_Timestopper
{
    [BepInPlugin(GUID, Name, Version)]
    public class Timestopper : BaseUnityPlugin
    {
        public const string GUID = "dev.galvin.timestopper";
        public const string Name = "The Timestopper";
        public const string Version = "1.6.11";
        public const string SubVersion = "0";

        private readonly Harmony harmony = new Harmony(GUID);
        public static Timestopper Instance;

        //private ConfigBuilder config;
        public static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(Name);
        public const string ARM_PICKUP_MESSAGE = "<color=#FFFF23>TIMESTOPPER</color>: Use \"<color=#FF4223>{0}</color>\" to stop and start time at will.";
        public const string ARM_DESCRIPTION = @"A Godfist that <color=#FFFF43>stops</color> time.

Recharges very slow, but <color=#FF4343>parrying</color> helps it recharge faster.

Can be <color=#FFFF24>upgraded</color> through terminals.
";
        public const string ARM_NEW_MESSAGE = "Somewhere in the depths of <color=#FF0000>Violence /// First</color>, a new <color=#FFFF23>golden</color> door appears";
        public const string TIMESTOP_STYLE = "<color=#FFCF21>TIME STOP</color>";

        // %%%%%%%%%%%%%%%%%% ASSETS %%%%%%%%%%%%%%%%%%%%%%%% \\
        public static Shader grayscaleShader;
        public static Shader depthShader;
        public static AudioClip[] TimestopSounds;
        public static AudioClip[] StoppedTimeAmbiences;
        public static AudioClip[] TimestartSounds;
        public static Texture2D armGoldLogo;
        public static Texture2D modLogo;
        public static GameObject armTimeText;

        // vvvvvvvvvvvvv REFERENCES vvvvvvvvvvvvvvvvvvvvvv\\
        public static GameObject Player { 
            get
            {
                if (MonoSingleton<NewMovement>.Instance == null) return null; 
                return MonoSingleton<NewMovement>.Instance.gameObject; 
            } 
        }
        public static GameObject Dummy;
        public static GameObject LatestTerminal;

        private static GameObject _menuCanvas;

        // ###############  CLOCKWORK VARIABLES  ############### \\
        public static bool TimeStop;
        public static float StoppedTimeAmount;
        public static bool LoadDone;
        public static float realTimeScale = 1.0f;
        [DefaultValue(1.0f)]
        public static float playerTimeScale { get; private set; }
        public static bool fixedCall;
        public static bool firstLoad = true;
        public static bool cybergrind;
        public static int cybergrindWave;
        public static bool UnscaleTimeSince;
        public static PrivateInsideTimer messageTimer = new PrivateInsideTimer();
        private GameObject currentLevelInfo;
        private TimeSince timeSinceLastTimestop = 0;

        public static float playerDeltaTime
        {
            get
            {
                if (fixedCall) return Time.fixedDeltaTime;
                if (TimeStop) return Time.unscaledDeltaTime * playerTimeScale;
                return Time.deltaTime;
            }
        }
        public static float playerFixedDeltaTime
        {
            get
            {
                if (fixedCall) return Time.fixedDeltaTime;
                if (TimeStop) return Time.fixedDeltaTime * playerTimeScale;
                return Time.fixedDeltaTime;
            }
        }
        //________________________ COROUTINES __________________________\\
        private IEnumerator timeStopper;
        private IEnumerator timeStarter;
        // _______________________ COMPATBILITY ________________________\\
        public static bool Compatability_JukeBox;

        //$$$$$$$$$$$$$$$$$$$$$$$$$ CONFIG FILES $$$$$$$$$$$$$$$$$$$$$$$$$$$$$\\
        public static BoolField alterMainMenu;
        public static BoolField aprilFools;
        public static KeyCodeField stopKey;
        public static StringListField stopSound;
        public static StringListField stoppedSound;
        public static StringListField startSound;
        public static ButtonField soundFileButton;
        public static ButtonField soundReloadButton;
        public static FloatField stopSpeed;
        public static FloatField startSpeed;
        public static FloatField affectSpeed;
        public static FloatField animationSpeed;
        public static FloatSliderField soundEffectVolume;
        public static BoolField filterMusic;
        public static FloatSliderField stoppedMusicPitch;
        public static FloatSliderField stoppedMusicVolume;
        //---------------------shaders---------------------------\\
        public static BoolField grayscale;
        public static BoolField bubbleEffect;
        public static FloatField overallEffectIntensity;
        public static FloatField grayscaleIntensity;
        public static FloatField bubbleSmoothness;
        public static FloatField colorInversionArea;
        public static FloatField skyTransitionTreshold;
        public static FloatField bubbleDistance;
        public static FloatField bubbleProgression;
        public static ColorField grayscaleColorSpace;
        public static FloatField grayscaleColorSpaceIntensity;
        //-------------------------------------------------------\\
        public static BoolField timestopHardDamage;
        public static IntField maxUpgrades;
        public static BoolField forceDowngrade;
        public static BoolField specialMode;
        public static BoolField extensiveLogging;
        //---------------------technical stuff--------------------\\
        public static FloatField lowerTreshold; //2.0f
        public static FloatField refillMultiplier; //0.12f
        public static FloatField bonusTimeForParry;
        public static FloatField antiHpMultiplier;
        public static ButtonField resetSaveButton;
        public static ButtonField giveArmButton;
        //-------------------------colors--------------------------\\
        public static ColorField timeJuiceColorNormal;
        public static ColorField timeJuiceColorInsufficient;
        public static ColorField timeJuiceColorUsing;
        public static ColorField timeJuiceColorNoCooldown;
        private PluginConfigurator config;
        
        //$$$$$$$$$$$$$$$$$$$$$$$$$ SPECIAL $$$$$$$$$$$$$$$$$$$$$$$$$$$$$\\
        public static Sprite[] aprilFoolsPFPList = new Sprite[] {};
        public static GameObject rickrollObject;
        public static string rickrollPath;
        public static bool isAprilFools => aprilFools.value || (DateTime.Today.Month == 4 && DateTime.Today.Day == 1);


        /// <summary>
        /// Logs information or error, hides extensive logs if extensive logging is false.
        /// </summary>
        /// <param name="log">Message to display</param>
        /// <param name="extensive">Extensive messages only display if extensive logging is set to true</param>
        /// <param name="err_lvl">Error level: 0-Info  1-Warning  2-Error  3-Fatal</param>
        public static void Log(object log, bool extensive = false, ErrorLevel err_lvl = ErrorLevel.Info)
        {
            if (extensiveLogging != null && !extensiveLogging.value && extensive)
                    return;
            switch (err_lvl) {
                case ErrorLevel.Info: mls.LogInfo(log); break;
                case ErrorLevel.Warning: mls.LogWarning(log); break;
                case ErrorLevel.Error: mls.LogError(log); break;
                case ErrorLevel.Fatal: mls.LogFatal(log); break;
                default: mls.LogInfo(log); break;
            }
        }
        
        public static void FixedUpdateFix(Transform target)
        {
            if (target.GetComponent(typeof(MonoBehaviour)) != null)
            {
                if (target.GetComponent<FixedUpdateCaller>() == null)
                    target.gameObject.AddComponent<FixedUpdateCaller>();
            }
            foreach (Transform child in target)
            {
                if (child.GetComponent(typeof(MonoBehaviour)) != null)
                {
                    if (child.GetComponent<FixedUpdateCaller>() == null)
                        child.gameObject.AddComponent<FixedUpdateCaller>();
                }
                FixedUpdateFix(child);
            }
        }
        void Awake()
        {
            if (Instance == null) { Instance = this; }

            Log("The Timestopper has awakened!");

            try
            {
                // ReSharper disable once UnusedVariable
                bool m = PortalManagerV2.Instance == null;
            }
            catch
            {
                mls.LogFatal("ULTRAKILL source code does not define PortalManagerV2, make sure you are running the Timestopper mod with the appropriate version of the game!");
                mls.LogFatal("Otherwise expect lots of bugs");
            }

            InitializeConfig();

            playerTimeScale = 1.0f;

            try
            {
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Log("An error occured while patching the game for TimeStopper mod.", false, ErrorLevel.Error);
                Log(e.ToString(), false, ErrorLevel.Error);
                Log("The mod will attempt to load, but expect issues regarding gameplay.", false, ErrorLevel.Warning);
                Log(@"The most prominent cause of this issue is version mismatch between the game and the mod, 
                        please make sure your game is up to date, or consider downgrading the Timestopper mod.", 
                    false, ErrorLevel.Warning);
            }
            
            
            Type cameraController = typeof(CameraController);   // ULTRAKILL's new update carried Update method to LateUpdate, add back support
            MethodInfo target = cameraController.GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (target == null)
            {
                target = cameraController.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (target == null)
                {
                    mls.LogFatal("ULTRAKILL source code does not define an Update nor LateUpdate method for CameraController, " +
                                 "are you sure this is the right version for the game!?");
                }
                else
                {
                    mls.LogFatal("CameraController defines an Update method instead of LateUpdate, " +
                                 "are you sure you are running the latest version of ULTRAKILL?");
                    // mls.LogWarning("If you have to use the mod with an older version of ULTRAKILL, please check the mod's GitHub for a legacy support version!");
                }
            }
            else
            {
                var transpilerMethod = typeof(TranspileCameraController).GetMethod("Transpiler", BindingFlags.Static | BindingFlags.NonPublic);
                harmony.Patch(target, transpiler: new HarmonyMethod(transpilerMethod));
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            ExecuteOnTreeChange.onNewGameObject += OnNewGameObject;
            
            StatsManager.checkpointRestart += ResetGoldArm;
            
        }

        static void OnNewGameObject(GameObject go)
        {
            if (go == Player || go.transform.IsChildOf(Player.transform))
            {
                if (go.name == "Main Camera" && TimestopperProgress.HasArm)
                    Grayscaler.UpdateShaderSettings();
                return;
            }

            if (go.GetComponent<Rigidbody>() && !go.GetComponent<RigidbodyStopper>())  // not this line, even tho it also includes RigidbodyStopper in code
                go.AddComponent<RigidbodyStopper>();  // that line is this line
            
            if (go.GetComponent<AudioSource>() && !go.GetComponent<AudioPitcher>() && !go.transform.IsChildOf(Player.transform) && !go.GetComponent<Chainsaw>())
                go.AddComponent<AudioPitcher>();

            SimplePortalTraveler spt = go.GetComponent<SimplePortalTraveler>();
            if (spt)
            {
                FixedUpdateCaller fuc = go.AddComponent<FixedUpdateCaller>();
                fuc.targets = new Component[] { spt };
            }
            
            FakeFallZone ffz = go.GetComponent<FakeFallZone>();
            if (ffz)
            {
                FixedUpdateCaller fuc = go.AddComponent<FixedUpdateCaller>();
                fuc.targets = new[] { (Component)ffz };
            }
        }

        public static void LoadHUDIfAppropriate()
        {
            if (TimestopperProgress.HasArm && TimestopperProgress.EquippedArm) Instance.StartCoroutine(Instance.LoadHUD());
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isInForbiddenScene = forbiddenSceneList.Contains(SceneManager.GetActiveScene().name);
            
            if (scene.name == "b3e7f2f8052488a45b35549efb98d902" /*main menu*/)
            {
                mls.LogWarning("main menu loaded");
                if (armTimeText == null)
                {
                    StartCoroutine(LoadBundle());   //Load all assets
                }
                if (alterMainMenu.value)
                    StartCoroutine(InstantiateMenuItems());
                
            }
            if (!isInForbiddenScene)
            {
                SceneTreeChangeWatcher.StartWatchingSceneForTreeChanges();
                InvokeCaller.ClearMonos();
                InvokeCaller.RegisterMethods(typeof(Coin), new [] { "StartCheckingSpeed", "TripleTime" });
                InvokeCaller.RegisterType(typeof(ScaleNFade));
                PortalManagerV2.Instance?.gameObject.AddComponent<FixedUpdateCaller>();
                Playerstopper.Instance.AddInvokeCallers(Playerstopper.Instance.transform);
                if (Player.GetComponent<TerminalUpdater>() == null)
                    Player.AddComponent<TerminalUpdater>();
                if (forceDowngrade.value)
                    TimestopperProgress.ForceDowngradeArm();
                if (TimestopperProgress.HasArm && TimestopperProgress.EquippedArm) StartCoroutine(LoadHUD());
                if (firstLoad && !TimestopperProgress.HasArm) //display the message for newcomers
                {
                    MonoSingleton<HudMessageReceiver>.Instance?.SendHudMessage(ARM_NEW_MESSAGE, "", "", 2);
                    messageTimer.done += () =>
                    {
                        MonoSingleton<HudMessageReceiver>.Instance?.Invoke("Done", 0);
                        firstLoad = false;
                    };
                    messageTimer.SetTimer(6, true);
                }
                if (isAprilFools)
                {
                    MonoSingleton<HudMessageReceiver>.Instance?.SendHudMessage("Meet me at the terminal.", "", "", 2);
                    messageTimer.done += () =>
                    {
                        MonoSingleton<HudMessageReceiver>.Instance?.Invoke("Done", 0);
                        firstLoad = false;
                    };
                    messageTimer.SetTimer(6, true);
                }
                MonoSingleton<StyleHUD>.Instance?.RegisterStyleItem("timestopper.timestop", TIMESTOP_STYLE); // register timestop style
            }
            else
            {
                if (timeStopper != null) StopCoroutine(timeStopper);
                timeStopper = null;
                timeStarter = CStartTime(0);
            }
            // Update the Level
            if (ConfirmLevel("VIOLENCE /// FIRST")) // Add the door to the level
            {
                Log("7-1 level detected", true);
                GameObject newdoor = Instantiate(GameObject.Find("Crossroads -> Forward Hall"), GameObject.Find("Stairway Down").transform);
                newdoor.name = "Stairway Down -> Gold Arm Hall";
                newdoor.transform.position = new Vector3(-14.6292f, -25.0312f, 590.2311f);
                newdoor.transform.eulerAngles = new Vector3(0, 270, 0);
                newdoor.transform.GetChild(0).GetComponent<MeshRenderer>().materials[1].color = Color.yellow;
                newdoor.transform.GetChild(0).GetComponent<MeshRenderer>().materials[2].color = Color.yellow;
                newdoor.transform.GetChild(1).GetComponent<MeshRenderer>().materials[1].color = Color.yellow;
                newdoor.transform.GetChild(1).GetComponent<MeshRenderer>().materials[2].color = Color.yellow;
                newdoor.GetComponent<Door>().Close();
                newdoor.GetComponent<Door>().Lock();
                newdoor.GetComponent<Door>().activatedRooms = new GameObject[] { };
                GameObject newaltar = Instantiate(newArmAltar, GameObject.Find("Stairway Down").transform);
                newaltar.transform.position = new Vector3(-10.0146f, -24.9875f, 590.0158f);
                newaltar.transform.localEulerAngles = new Vector3(0, 0, 0);
                newaltar.transform.Find("TimeArmPickup").gameObject.AddComponent<TimeArmPickup>();
                Log("Added The New Arm Altar", true);
            }
            // Cybergrind Music Explorer Compatability
            if (scene.name == "9240e656c89994d44b21940f65ab57da" /*cybergrind*/)
            {
                cybergrind = true;
                if (Chainloader.PluginInfos.ContainsKey("dev.flazhik.jukebox"))
                {
                    Compatability_JukeBox = true;
                    Type Comp = Type.GetType("Jukebox.Components.NowPlayingHud, Jukebox");
                    if (Comp != null)
                    {
                        Component C = FindObjectOfType(Comp) as Component;
                        if (C != null) C.gameObject.transform.localPosition += new Vector3(0, 60, 0);
                        else Log("Component C is null!", true, ErrorLevel.Error);
                    }
                    else Log("Could not get Jukebox.Components.NowPlayingHud, Cybergrind Music Explorer may have errors", true, ErrorLevel.Error);
                }
            }
            else
            {
                cybergrind = false;
                Compatability_JukeBox = false;
            }
        }
        public void ReloadStringListField(StringListField slf ,IEnumerable<string> values)
        {
            FieldInfo field = typeof(StringListField).GetField("values", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field2 = typeof(StringListField).GetField("currentUi", BindingFlags.NonPublic | BindingFlags.Instance);
            var enumerable = values as string[] ?? values.ToArray();
            field?.SetValue(slf, enumerable.ToList());
            if (!enumerable.ToArray().Contains(slf.defaultValue)) slf.defaultValue = enumerable.ToList()[0];
            if (field2 != null && field2.GetValue(slf) != null)
            {
                ((ConfigDropdownField)field2.GetValue(slf)).dropdown.options.Clear();
                foreach (string s in enumerable.ToArray() )
                {
                    ((ConfigDropdownField)field2.GetValue(slf)).dropdown.options.Add(new TMP_Dropdown.OptionData(s));
                }
            }
            var method = typeof(ConfigPanel).GetMethod(
                "ProtectedInternalMethod",
                BindingFlags.NonPublic |      // protected
                BindingFlags.Instance |       // not static
                BindingFlags.FlattenHierarchy // idk, works
            );
            method?.Invoke(slf.parentPanel, null);
        }

        public static string SearchForFile(string path, string filename)
        {
            if (!File.Exists(Path.Combine(path, filename))) return path;
            foreach (string s in Directory.GetDirectories(path))
            {
                string ss = SearchForFile(s, filename);
                if (ss != null) return ss;
            }
            return null;
        }
        public void ReloadSoundProfilesList()
        {
            Log("Info.Location: " + Info.Location, false, ErrorLevel.Warning);
            bool reCopyFiles = false;
            string[] sounddirectories = new[] { "Stopping", "Stopped", "Starting"};
            
            if (Directory.Exists(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds")))
                foreach (string sounddirectory in sounddirectories)
                {
                    if (!Directory.Exists(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", sounddirectory)) ||
                        Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", sounddirectory),
                            "*.*").Length == 0)
                    {
                        reCopyFiles = true;
                        break;
                    }
                }
            else reCopyFiles = true;
            
            if (reCopyFiles)
            {
                try
                {
                    string modPath = Path.GetDirectoryName(Info.Location);
                    if (modPath == null)
                    {
                        Log("The Timestoper.dll could not be found, please make sure the mod is installed correctly!",
                            false, ErrorLevel.Error);
                        return;
                    }

                    Log("The Timestopper.dll found, searching for audio files...", false, ErrorLevel.Warning);

                    Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "Timestopper",
                        "Sounds")); // create if it doesn't exist
                    List<string> defaultAudioFiles =
                        Directory.EnumerateFiles(modPath, "*.ogg", SearchOption.AllDirectories).ToList();
                    string readmeFile = Directory.EnumerateFiles(modPath, "*.txt", SearchOption.AllDirectories).First();

                    foreach (string sounddirectory in sounddirectories) // create sound folders
                        Directory.CreateDirectory(Path.Combine(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds",
                            sounddirectory)));

                    foreach (string audioPath in defaultAudioFiles) // copy over from mod folder
                    {
                        string audioFile = Path.GetFileName(audioPath);
                        string audioDirectory =
                            Path.GetFileName(Path.GetDirectoryName(audioPath)?.TrimEnd(Path.DirectorySeparatorChar));
                        Log("copying over audio file to " + audioDirectory);
                        File.Copy(audioPath,
                            Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", audioDirectory ?? "unknown",
                                audioFile), true);
                    }

                    if (!File.Exists(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds",
                            "README.txt"))) // copy over readme
                        File.Copy(readmeFile, Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "README.txt"),
                            true);
                }
                catch (Exception e)
                {
                    Log("An error occurred while copying audio files to config, trace:", false, ErrorLevel.Error);
                    Log(e.Message, false, ErrorLevel.Error);
                    Log(e.Source, false, ErrorLevel.Error);
                    Log(e.StackTrace, false, ErrorLevel.Error);
                    Log(e.TargetSite, false, ErrorLevel.Error);
                }
            }
            
            string[] timestopSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Stopping"), "*.*").
                Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).Select(Path.GetFileNameWithoutExtension).ToArray();
            string[] stopambienceSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Stopped"), "*.*").
                Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).Select(Path.GetFileNameWithoutExtension).ToArray();
            string[] timestartSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Starting"), "*.*").
                Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).Select(Path.GetFileNameWithoutExtension).ToArray();
            
            if (timestopSoundsList.Length < 1) {
                Log("No time stop sounds found!", false, ErrorLevel.Error);
                timestopSoundsList = new [] { "FILE ERROR" };
            }
            if (stopambienceSoundsList.Length < 1) {
                Log("No ambience sounds found!", false, ErrorLevel.Error);
                stopambienceSoundsList = new [] { "FILE ERROR" };
            }
            if (timestartSoundsList.Length < 1) {
                Log("No time start sounds found!", false, ErrorLevel.Error);
                timestartSoundsList = new [] { "FILE ERROR" };
            }

            string defaultStopProfile = "Classic";
            string defaultStoppedProfile = "Classic";
            string defaultStartProfile = "Classic";
            if (!timestopSoundsList.Contains(defaultStopProfile)) defaultStopProfile = timestopSoundsList[0];
            if (!stopambienceSoundsList.Contains(defaultStoppedProfile)) defaultStoppedProfile = stopambienceSoundsList[0];
            if (!timestartSoundsList.Contains(defaultStartProfile)) defaultStartProfile = timestartSoundsList[0];
            
            if (stopSound == null) stopSound = new StringListField(config.rootPanel, "Timestop Sound", "timestopprofile", timestopSoundsList, defaultStopProfile);
            else ReloadStringListField(stopSound, timestopSoundsList);
            if (!timestopSoundsList.Contains(stopSound.value)) stopSound.value = timestopSoundsList[0];
            
            if (stoppedSound == null) stoppedSound = new StringListField(config.rootPanel, "Stopped Time Ambience", "ambienceprofile", stopambienceSoundsList, defaultStoppedProfile);
            else ReloadStringListField(stoppedSound, stopambienceSoundsList);
            if (!stopambienceSoundsList.Contains(stoppedSound.value)) stoppedSound.value = stopambienceSoundsList[0];
            
            if (startSound == null) startSound = new StringListField(config.rootPanel, "Timestart Sound", "timestartprofile", timestartSoundsList, defaultStartProfile);
            else ReloadStringListField(startSound, timestartSoundsList);
            if (!timestartSoundsList.Contains(startSound.value)) startSound.value = timestartSoundsList[0];
        }
        void InitializeConfig()
        {
            if (config == null)
            {
                config = PluginConfigurator.Create(Name, GUID);

                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 6);
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- GENERAL --");

                alterMainMenu = new BoolField(config.rootPanel, "Alter Main Menu", "altermainmenu", true);
                stopKey = new KeyCodeField(config.rootPanel, "Timestopper Key", "stopkey", KeyCode.V);
                timestopHardDamage = new BoolField(config.rootPanel, "Timestop Hard Damage", "harddamage", true); // reverse input
                stopSpeed = new FloatField(config.rootPanel, "Timestop Speed", "stopspeed", 0.6f);
                startSpeed = new FloatField(config.rootPanel, "Timestart Speed", "startspeed", 0.8f);
                affectSpeed = new FloatField(config.rootPanel, "Interaction Speed", "interactionspeed", 1.0f);
                animationSpeed = new FloatField(config.rootPanel, "Animation Speed", "animationspeed", 1.3f);
                aprilFools = new BoolField(config.rootPanel, "Enable April Fools Mode", "aprilfools", false);

                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- GRAPHICS --");


                grayscale = new BoolField(config.rootPanel, "Do Shader Effects", "doGrayscale", false);
                ConfigDivision grayscaleOptions = new ConfigDivision(config.rootPanel, "grayscaleOptions") {
                    interactable = grayscale.value
                };
                grayscale.onValueChange += (e) => { 
                    grayscaleOptions.interactable = e.value;
                    Grayscaler.UpdateShaderSettings();
                };
                ConfigPanel shaderOptions = new ConfigPanel(grayscaleOptions, "SHADER OPTIONS", "shaderoptions");
                bubbleEffect = new BoolField(shaderOptions, "Expanding Bubble Effect", "bubbleeffect", true);
                overallEffectIntensity = new FloatField(shaderOptions, "Overall Intensity", "overalleffectintensity", 1.0f);
                grayscaleIntensity = new FloatField(shaderOptions, "Grayscale Intensity", "grayscaleintensity", 1.0f);
                bubbleSmoothness = new FloatField(shaderOptions, "Bubble Border Smoothness", "bubblesmoothness", 0.1f);
                colorInversionArea = new FloatField(shaderOptions, "Inverted Border Thickness", "colorinversionarea", 0.01f);
                skyTransitionTreshold = new FloatField(shaderOptions, "Sky Transition Treshold", "skytransitiontreshold", 10.0f);
                bubbleDistance = new FloatField(shaderOptions, "Bubble Expansion rate", "bubbledistance", 20.0f);
                bubbleProgression = new FloatField(shaderOptions, "Inverse Color Intensity", "bubbleprogression", 1.0f);
                grayscaleColorSpace = new ColorField(shaderOptions, "Grayscale Color Space", "grayscalecolorspace", new Color(0.212f, 0.71f, 0.072f));
                grayscaleColorSpaceIntensity = new FloatField(shaderOptions, "Grayscale Color Space Multiplier", "grayscalecolorspaceintensity", 1.0f);

                bubbleEffect.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                overallEffectIntensity.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                grayscaleIntensity.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                bubbleSmoothness.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                colorInversionArea.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                skyTransitionTreshold.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                bubbleDistance.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                bubbleProgression.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                grayscaleColorSpace.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                grayscaleColorSpaceIntensity.onValueChange += (e) => Grayscaler.UpdateShaderSettings();
                
                
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- AUDIO --");

                soundEffectVolume = new FloatSliderField(config.rootPanel, "Sound Effects Volume", "effectvolume", new Tuple<float, float>(0, 2), 1);
                stoppedMusicPitch = new FloatSliderField(config.rootPanel, "Music Pitch in Stopped Time", "musicpitch", new Tuple<float, float>(0, 1), 0.6f);
                stoppedMusicVolume = new FloatSliderField(config.rootPanel, "Music volume in Stopped Time", "musicvolume", new Tuple<float, float>(0, 1), 0.8f);
                filterMusic = new BoolField(config.rootPanel, "Filter Music in Stopped Time", "filtermusic", false);
                
                ReloadSoundProfilesList();
                
                soundReloadButton = new ButtonField(config.rootPanel, "Reload Sound Profiles", "soundprofilebutton");
                soundReloadButton.onClick += () => { StartCoroutine(LoadSoundProfiles()); };

                soundFileButton = new ButtonField(config.rootPanel, "Open Sound Profile Folder", "soundprofilebutton");
                soundFileButton.onClick += () => { Application.OpenURL(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds")); };
                
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- GAMEPLAY --");

                maxUpgrades = new IntField(config.rootPanel, "Maximum Number of Upgrades", "maxupgrades", 10) {
                    minimumValue = 1
                };
                maxUpgrades.onValueChange += ( e) => {
                    if (e.value < 1)
                    {
                        e.value = 1;
                        maxUpgrades.value = 1;
                    }
                };
                refillMultiplier = new FloatField(config.rootPanel, "Passive Income Multiplier", "refillmultiplier", 0.1f);
                bonusTimeForParry = new FloatField(config.rootPanel, "Time Juice Refill Per Parry", "bonustimeperparry", 1.0f);
                specialMode = new BoolField(config.rootPanel, "Special Mode", "specialmode", false) {
                    interactable = false, value = false };

                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- COLORS --");

                timeJuiceColorNormal = new ColorField(config.rootPanel, "Time Juice Bar Normal Color", "timejuicecolornormal", new Color(1, 1, 0, 1));
                timeJuiceColorInsufficient = new ColorField(config.rootPanel, "Time Juice Bar Insufficient Color", "timejuicecolorinsufficient", new Color(1, 0, 0, 1));
                timeJuiceColorUsing = new ColorField(config.rootPanel, "Time Juice Bar Draining Color", "timejuicecolorusing", new Color(1, 0.6f, 0, 1));
                timeJuiceColorNoCooldown = new ColorField(config.rootPanel, "Time Juice Bar No Cooldown Color", "timejuicecolornocooldown", new Color(0, 1, 1, 1));

                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- ADVANCED OPTIONS --");

                ConfigPanel advancedOptions = new ConfigPanel(config.rootPanel, "ADVANCED", "advancedoptions");
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 8);

                extensiveLogging = new BoolField(advancedOptions, "Extensive Logging", "extensivelogging", false);
                forceDowngrade = new BoolField(advancedOptions, "Force Downgrade Arm", "forcedowngrade", true);
                lowerTreshold = new FloatField(advancedOptions, "Min Time Juice to Stop Time", "lowertreshold", 2.0f);
                antiHpMultiplier = new FloatField(advancedOptions, "Hard Damage Buildup Multiplier", "antihpmultiplier", 30);

                resetSaveButton = new ButtonField(config.rootPanel, "RESET TIMESTOPPER PROGRESS", "resetsavebutton");
                resetSaveButton.onClick += TimestopperProgress.Reset;
                
                giveArmButton = new ButtonField(config.rootPanel, "GIVE TIMESTOPPER ARM", "givearmbutton");
                giveArmButton.onClick += TimestopperProgress.GiveArm;
            }
        }

        public static GameObject newTimeArm;
        public static GameObject newArmAltar;

        public IEnumerator LoadSoundProfiles()
        {
            stopSound.interactable = false;
            startSound.interactable = false;
            stoppedSound.interactable = false;
            soundReloadButton.interactable = false;
            ReloadSoundProfilesList();
            string[] timestopSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Stopping"), "*.*").
                    Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).ToArray();
            TimestopSounds = new AudioClip[timestopSoundsList.Length];
            for (int i = 0; i < timestopSoundsList.Length; i++)
            {
                AudioType audioType = AudioType.WAV;
                if (timestopSoundsList[i].EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;
                if (timestopSoundsList[i].EndsWith(".wav")) audioType = AudioType.WAV;
                if (timestopSoundsList[i].EndsWith(".mp3")) audioType = AudioType.MPEG;
                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" +  timestopSoundsList[i], audioType))
                {
                    yield return request.SendWebRequest();
                    TimestopSounds[i] = DownloadHandlerAudioClip.GetContent(request);
                    Log("downloaded timestop audio cussessfully!");
                }
            }
            
            string[] stopambienceSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Stopped"), "*.*").
                Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).ToArray();
            StoppedTimeAmbiences = new AudioClip[stopambienceSoundsList.Length];
            for (int i = 0; i < stopambienceSoundsList.Length; i++)
            {
                AudioType audioType = AudioType.WAV;
                if (stopambienceSoundsList[i].EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;
                if (stopambienceSoundsList[i].EndsWith(".wav")) audioType = AudioType.WAV;
                if (stopambienceSoundsList[i].EndsWith(".mp3")) audioType = AudioType.MPEG;
                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + stopambienceSoundsList[i], audioType))
                {
                    yield return request.SendWebRequest();
                    StoppedTimeAmbiences[i] = DownloadHandlerAudioClip.GetContent(request);
                    Log("downloaded stopambience audio successfully!");
                }
            }
            
            string[] timestartSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Starting"), "*.*").
                Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).ToArray();
            TimestartSounds = new AudioClip[timestartSoundsList.Length];
            for (int i = 0; i < timestartSoundsList.Length; i++)
            {
                AudioType audioType = AudioType.WAV;
                if (timestartSoundsList[i].EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;
                if (timestartSoundsList[i].EndsWith(".wav")) audioType = AudioType.WAV;
                if (timestartSoundsList[i].EndsWith(".mp3")) audioType = AudioType.MPEG;
                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + timestartSoundsList[i], audioType))
                {
                    yield return request.SendWebRequest();
                    TimestartSounds[i] = DownloadHandlerAudioClip.GetContent(request);
                    Log("downloaded timestart audio successfully!");
                }
            }
            stopSound.interactable = true;
            startSound.interactable = true;
            stoppedSound.interactable = true;
            soundReloadButton.interactable = true;
            ReloadSoundProfilesList();
        }
        public IEnumerator LoadBundle()
        {
            LoadDone = false;
            var imageType = typeof(Image);
            GC.KeepAlive(imageType);
            var assembler = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembler.GetManifestResourceNames();
            Log("Scanning newly embedded resources: " + string.Join(", ", resourceNames), true);
            AssetBundle aprilFoolsBundle;
            using (var stream =
                   assembler.GetManifestResourceStream("The_Timestopper.aprilfools.bundle"))
            {
                mls.LogWarning("started loading something special girrrl!");
                aprilFoolsBundle = AssetBundle.LoadFromStream(stream);
                aprilFoolsPFPList = aprilFoolsBundle.LoadAllAssets<Sprite>();
                rickrollObject = aprilFoolsBundle.LoadAllAssets<GameObject>()[0];
                foreach (UnityEngine.Object s in aprilFoolsBundle.LoadAllAssets()) {
                    mls.LogWarning(s.ToString());
                }
                mls.LogWarning("END ---//");
            }
            // if (isAprilFools)
            // {
            // } 
            AssetBundle newBundle;
            using (var stream = assembler.GetManifestResourceStream("The_Timestopper.timestopper_assets_assets_all.bundle"))
            {
                newBundle = AssetBundle.LoadFromStream(stream);
                newTimeArm = newBundle.LoadAsset<GameObject>("Assets/TimestopperMod/TimeArm.prefab");
                newArmAltar = newBundle.LoadAsset<GameObject>("Assets/TimestopperMod/TimeArmAltar.prefab");
                armTimeText = newBundle.LoadAsset<GameObject>("Assets/TimestopperMod/TimestopperText.prefab");
                armGoldLogo = newBundle.LoadAsset<Texture2D>("Assets/TimestopperMod/ArmTimestopper.png");
                modLogo = newBundle.LoadAsset<Texture2D>("Assets/TimestopperMod/icon_big.png");
                grayscaleShader = newBundle.LoadAsset<Shader>("Assets/TimestopperMod/GrayscaleObject.shader");
                depthShader = newBundle.LoadAsset<Shader>("Assets/TimestopperMod/DepthRenderer.shader");
                config.icon = Sprite.Create(modLogo, new Rect(0, 0, 750, 750), new Vector2(750/2f, 750/2f));
                Log("Total assets loaded: " + newBundle.GetAllAssetNames().Length, true, ErrorLevel.Info);
                foreach (var asset in newBundle.GetAllAssetNames())
                {
                    Log(asset, true, ErrorLevel.Info);
                }
                yield return LoadSoundProfiles();
            }
            Log("Scanning embedded resources: " + string.Join(", ", resourceNames), true);
            Log("      >:Bundle extraction done!", true);
            
            LoadDone = true;
        }
        public static GameObject UpdateTerminal(ShopZone ShopComp)
        {
            // return null; // TempRemove
            if (ShopComp == null)
            {
                Log("Shop Component is null, cannot update terminal!", false, ErrorLevel.Error);
                return null;
            }
            GameObject Shop = ShopComp.gameObject;
            if (Shop.transform.Find("Canvas/Background/Main Panel/Weapons/Arm Window") == null)
            {
                ShopComp.gameObject.AddComponent<TerminalExcluder>();
                return null;
            }
            GameObject armWindow = Shop.transform.Find("Canvas/Background/Main Panel/Weapons/Arm Window").gameObject;
            GameObject armPanelGold = armWindow.transform.Find("Variation Screen/Variations/Arm Panel (Gold)").gameObject;
            GameObject armInfoGold = armWindow.transform.Find("Arm Info (Gold)").gameObject;
            if (isAprilFools)
            {
                Debug.LogWarning("You are now dawn!");
                int randomPfpIndex = UnityEngine.Random.Range(0, aprilFoolsPFPList.Length);
                ShopComp.gameObject.AddComponent<TerminalExcluder>();

                GameObject rickroll = Instantiate(rickrollObject, Shop.transform.Find("Canvas/Background/Main Panel"));
                // RenderTexture videoTexture = new CustomRenderTexture(1920, 1080);
                // videoTexture.Create();
                // rickroll.GetComponentInChildren<VideoPlayer>().source = VideoSource.Url;
                // Debug.LogWarning(rickrollPath);
                // rickroll.GetComponentInChildren<VideoPlayer>().url = rickrollPath;
                // rickroll.GetComponentInChildren<VideoPlayer>().targetTexture = videoTexture;
                // rickroll.GetComponentInChildren<RawImage>().texture = videoTexture;
                rickroll.transform.Find("close").GetComponent<ShopButton>().toActivate = new[] {
                    Shop.transform.Find("Canvas/Background/Main Panel/Main Menu").gameObject,
                    Shop.transform.Find("Canvas/Background/Main Panel/Tip of the Day").gameObject
                };
                rickroll.SetActive(false);
                
                GameObject aprilMessage = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/The Cyber Grind/Cyber Grind Panel").gameObject,
                                                                Shop.transform.Find("Canvas/Background/Main Panel"));
                aprilMessage.name = "New Mail!";
                aprilMessage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
                aprilMessage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 230);
                aprilMessage.transform.localPosition = new Vector3(-9.4417f, 6.0f, 0.0002f);
                aprilMessage.transform.Find("Button 1").gameObject.SetActive(false);
                aprilMessage.transform.Find("Button 2").gameObject.SetActive(false);
                aprilMessage.transform.Find("Button 3").gameObject.SetActive(false);
                if (aprilMessage.GetComponentInChildren<HudMessage>())
                    Destroy(aprilMessage.GetComponentInChildren<HudMessage>().gameObject);

                string aprilText = "something is wrong...";
                if (aprilFoolsPFPList[randomPfpIndex].name == "brakxypfp")
                    aprilText = @"<color=#FF0000>@hellbrakxy123</color><color=#EEEEEE> has invited you to commit Fraud in Minecraft, accept? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "dialyultrakillnewspfp")
                    aprilText = @"<color=#FF0000>@dailyultrakillnewsofficialnofake</color><color=#EEEEEE> has sent you an ULTRAKILL leak, accept it? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "galvinpfp")
                    aprilText = @"<color=#FF0000>@xXxgalvinvoltagxXx</color><color=#EEEEEE> has invited you to a private conversation, accept it? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "hakitapfp")
                    aprilText = @"<color=#FF0000>@arsihakita</color><color=#EEEEEE> has invited you to a public video call, accept it? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "librarianpfp")
                    aprilText = @"<color=#FF0000>@thelibrarian</color><color=#EEEEEE> has sent you a very comfy and creepy pocket dimension, accept? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "markpfp")
                    aprilText = @"<color=#FF0000>@realmarkiplier</color><color=#EEEEEE> announced that you won a special prize, accept the suspicious link? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "martapfp")
                    aprilText = @"<color=#FF0000>@martaspidetty</color><color=#EEEEEE> offered you a drawing class in the Treachery layer, accept offer? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "newbloodpfp")
                    aprilText = @"<color=#FF0000>@newbloodofficial</color><color=#EEEEEE> has offered you a sale on merch and games, accept offer? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "newtonpfp")
                    aprilText = @"<color=#FF0000>@isaacnewtonrblx</color><color=#EEEEEE> has offered you a class on Einstein's relativity principle, accept? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "radiationpfp")
                    aprilText = @"<color=#FF0000>@tobynotradiationfox</color><color=#EEEEEE> has some of your delta rune, would you like to rob him? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "mindflayerpfp")
                    aprilText = @"<color=#FF0000>@sexflayer3169</color><color=#EEEEEE> has sent you and invitation to the Lust layer, alone, accept offer? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "sisyphuspfp")
                    aprilText = @"<color=#FF0000>@hotprimesoul</color><color=#EEEEEE> has sent you an invitation to the Greed layer, alone, accept offer? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "rickpfp")
                    aprilText = @"<color=#FF0000>@rickastley</color><color=#EEEEEE> announced you as his new legal daughter, accept your new self? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "earthpfp")
                    aprilText = @"<color=#FF0000>@earthchannotflat</color><color=#EEEEEE> has sent you a new blood-y mail, open and view it? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "linguinipfp")
                    aprilText = @"<color=#FF0000>@linguiniwithoutlasagna</color><color=#EEEEEE> has defeated you in 8-S speedrun already, take revenge? </color>";
                if (aprilFoolsPFPList[randomPfpIndex].name == "gronf")
                    aprilText = @"<color=#FF0000>@gronf</color><color=#EEEEEE> has forgotten to install The Timestopper, remind him to do so? </color>";
                

                aprilMessage.transform.Find("Panel/Text Inset/Text").GetComponent<TextMeshProUGUI>().text = aprilText;
                aprilMessage.transform.Find("Panel/Text Inset/Text").GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 10, 350);
                
                GameObject iconR = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/Enemies/Enemies Panel/Icon").gameObject, aprilMessage.transform.Find("Title"));
                GameObject iconL = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/Enemies/Enemies Panel/Icon").gameObject, aprilMessage.transform.Find("Title"));
                iconL.transform.localPosition = new Vector3(-37, 0, 0);
                iconR.transform.localPosition = new Vector3(190, 0, 0);
                aprilMessage.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "NEWBLOOD-Y MAIL";
                aprilMessage.transform.Find("Title").GetComponent<TextMeshProUGUI>().transform.localPosition = new Vector3(-100, 116, 0);
                
                GameObject icon = aprilMessage.transform.Find("Icon").gameObject;
                icon.transform.SetParent(aprilMessage.transform.Find("Panel/Text Inset"), true);
                icon.transform.localPosition = new Vector3(-225, 40, 0);
                icon.transform.localScale = new Vector3(2, 2, 2);
                icon.GetComponent<Image>().sprite = aprilFoolsPFPList[randomPfpIndex];
                
                GameObject acceptButton = aprilMessage.transform.Find("Panel/Enter Button").gameObject;
                acceptButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "YES";
                acceptButton.GetComponent<Image>().color = new Color(0, 1, 0, 1);
                acceptButton.name = "Accept Button";
                Destroy(acceptButton.GetComponent<AbruptLevelChanger>());
                aprilMessage.SetActive(true);
                Shop.transform.Find("Canvas/Background/Main Panel/Main Menu").gameObject.SetActive(false);
                Shop.transform.Find("Canvas/Background/Main Panel/Tip of the Day").gameObject.SetActive(false);
                
                acceptButton.GetComponent<ShopButton>().PointerClickSuccess += () => { Log("this is good", false, ErrorLevel.Warning); };
                acceptButton.GetComponent<ShopButton>().toDeactivate = new [] { aprilMessage };
                acceptButton.GetComponent<ShopButton>().toActivate = new [] {
                                        rickroll };
                acceptButton.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 10, 220);
                
                GameObject declineButton = Instantiate(acceptButton, acceptButton.transform.parent, true);
                declineButton.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 10, 220);
                declineButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "NOOo";
                declineButton.GetComponent<Image>().color = new Color(1, 0, 0, 1);
                Destroy(declineButton.GetComponent<AbruptLevelChanger>());
                declineButton.GetComponent<ShopButton>().PointerClickSuccess += () => { Log("this is good", false, ErrorLevel.Warning); };
                declineButton.GetComponent<ShopButton>().toDeactivate = new [] { aprilMessage };
                declineButton.GetComponent<ShopButton>().toActivate = new [] {
                    Shop.transform.Find("Canvas/Background/Main Panel/Tip of the Day").gameObject,
                    Shop.transform.Find("Canvas/Background/Main Panel/Main Menu").gameObject
                };
                declineButton.name = "Decline Button";
            }
            if (TimestopperProgress.HasArm)
            {
                ShopComp.gameObject.AddComponent<TerminalExcluder>();
                armPanelGold.GetComponent<ShopButton>().toActivate = new [] { armInfoGold };
                armPanelGold.transform.Find("Variation Name").GetComponent<TextMeshProUGUI>().text = "TIMESTOPPER";
                armPanelGold.GetComponent<VariationInfo>().enabled = true;
                armPanelGold.GetComponent<VariationInfo>().alreadyOwned = true;
                armPanelGold.GetComponent<VariationInfo>().varPage = armInfoGold;
                armPanelGold.GetComponent<VariationInfo>().weaponName = "arm4";
                armPanelGold.GetComponent<ShopButton>().PointerClickSuccess += Shop.GetComponent<TerminalExcluder>().OverrideInfoMenu;
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.UpgradeArm;
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().PointerClickSuccess += Shop.GetComponent<TerminalExcluder>().OverrideInfoMenu;
                armPanelGold.GetComponent<VariationInfo>().cost = (int)TimestopperProgress.UpgradeCost;
                armPanelGold.transform.Find("Equipment/Equipment Status").GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.ChangeEquipmentStatus;
                armPanelGold.transform.Find("Equipment/Buttons/Previous Button").GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.ChangeEquipmentStatus;
                armPanelGold.transform.Find("Equipment/Buttons/Next Button").GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.ChangeEquipmentStatus;
                Sprite mm = Sprite.Create(armGoldLogo, new Rect(0, 0, 750, 750), new Vector2(750, 750)/2);
                armPanelGold.transform.Find("Weapon Icon").GetComponent<Image>().sprite = mm;
                armPanelGold.transform.Find("Weapon Icon").GetComponent<Image>().color = Color.yellow;
                armInfoGold.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "Timestopper";
                armInfoGold.transform.Find("Panel/Name").GetComponent<TextMeshProUGUI>().text = "TIMESTOPPER";
                armInfoGold.transform.Find("Panel/Description").GetComponent<TextMeshProUGUI>().text = ARM_DESCRIPTION + TimestopperProgress.UpgradeText;
                Sprite nn = Sprite.Create(armGoldLogo, new Rect(0, 0, 750, 750), new Vector2(750, 750)/2);
                armInfoGold.transform.Find("Panel/Icon Inset/Icon").GetComponent<Image>().sprite = nn;
                if (!TimestopperProgress.FirstWarning)
                {
                    GameObject firstWarning = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/The Cyber Grind/Cyber Grind Panel").gameObject,
                                                                Shop.transform.Find("Canvas/Background/Main Panel"));
                    firstWarning.name = "Warning Panel";
                    if (firstWarning.GetComponentInChildren<HudMessage>())
                        Destroy(firstWarning.GetComponentInChildren<HudMessage>().gameObject);
                    firstWarning.transform.localPosition = new Vector3(-9.4417f, 6.0f, 0.0002f);
                    firstWarning.transform.Find("Button 1").gameObject.SetActive(false);
                    firstWarning.transform.Find("Button 2").gameObject.SetActive(false);
                    firstWarning.transform.Find("Button 3").gameObject.SetActive(false);
                    firstWarning.transform.Find("Icon").gameObject.SetActive(false);
                    Destroy(firstWarning.transform.Find("GameObject"));
                    firstWarning.transform.Find("Panel/Text Inset/Text").GetComponent<TextMeshProUGUI>().text = @"<color=#FF4343>!!! Extreme Hazard Detected !!!</color> 

You have <color=#FF4343>The Timestopper</color> in your possession. Using this item may cause disturbance in space-time continuum.

<color=#FF4343>Please acknowledge the consequences before proceeding further.</color>";
                    GameObject iconR = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/Enemies/Enemies Panel/Icon").gameObject, firstWarning.transform.Find("Title"));
                    GameObject iconL = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/Enemies/Enemies Panel/Icon").gameObject, firstWarning.transform.Find("Title"));
                    iconL.transform.localPosition = new Vector3(-37.1206f, -0.0031f, 0);
                    iconR.transform.localPosition = new Vector3(97.8522f, -0.0031f, 0);
                    firstWarning.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "WARNING";
                    firstWarning.transform.Find("Title").GetComponent<TextMeshProUGUI>().transform.localPosition = new Vector3(-51.5847f, 189.9997f, 0);
                    GameObject button = firstWarning.transform.Find("Panel/Enter Button").gameObject;
                    button.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "ACCEPT";
                    button.GetComponent<Image>().color = new Color(0, 1, 0, 1);
                    button.name = "Accept Button";
                    Destroy(button.GetComponent<AbruptLevelChanger>());
                    firstWarning.SetActive(false);
                    Shop.transform.Find("Canvas/Background/Main Panel/Main Menu").gameObject.SetActive(false);
                    Shop.transform.Find("Canvas/Background/Main Panel/Tip of the Day").gameObject.SetActive(false);
                    button.GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.AcceptWarning;
                    button.GetComponent<ShopButton>().toDeactivate = new [] { firstWarning };
                    button.GetComponent<ShopButton>().toActivate = new [] {
                                            Shop.transform.Find("Canvas/Background/Main Panel/Tip of the Day").gameObject,
                                            Shop.transform.Find("Canvas/Background/Main Panel/Main Menu").gameObject
                                            };
                    return firstWarning;
                }
            }
            else
            {
                armPanelGold.GetComponent<VariationInfo>().enabled = true;
                armPanelGold.GetComponent<VariationInfo>().alreadyOwned = false;
                armPanelGold.GetComponent<VariationInfo>().varPage = armInfoGold;
            }
            return null;
        }
        
        public IEnumerator LoadHUD()
        {
            if (!TimestopperProgress.HasArm) yield break;
            float elapsedTime = 0;
            Log("Loading HUD Elements...", true);
            do
            {
                if (elapsedTime > 5)
                {
                    Log("Time Juice Bar creation failed after 5 seconds!", false, ErrorLevel.Error);
                    yield break;
                }
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            } while (Player.transform.Find("Main Camera/HUD Camera/HUD/GunCanvas/StatsPanel/Filler/AltRailcannonPanel") == null);

            GameObject[] TimeHUD = new GameObject[] {null, null, null};
            TimeHUD[0] = Instantiate(Player.transform.Find("Main Camera/HUD Camera/HUD/GunCanvas/StatsPanel/Filler/AltRailcannonPanel").gameObject,
                        Player.transform.Find("Main Camera/HUD Camera/HUD/GunCanvas/StatsPanel/Filler"));
            TimeHUD[0].SetActive(true);
            TimeHUD[0].name = "Golden Time";
            TimeHUD[0].transform.localPosition = new Vector3(0f, 124.5f, 0f);
            TimeHUD[0].transform.Find("Image").gameObject.GetComponent<Image>().fillAmount = 0;
            // TimeHUD[0].transform.Find("Image/Image (1)").gameObject.GetComponent<UnityEngine.UI.Image>().color = TimeColor;
            Sprite mm = Sprite.Create(armGoldLogo, new Rect(0, 0, 750, 750), new Vector2(750, 750)/2);
            TimeHUD[0].transform.Find("Icon").gameObject.GetComponent<Image>().sprite = mm;
            TimeHUD[0].AddComponent<TimeHUD>();
            TimeHUD[0].GetComponent<TimeHUD>().type = 0;
            HudController.Instance.speedometer.gameObject.transform.localPosition += new Vector3(0, 64, 0);
            Log("Time Juice Bar created successfully.", true);
            do
            {
                if (elapsedTime > 5)
                {
                    Log("Time Juice Alt HUD creation failed after 5 seconds!", false, ErrorLevel.Error);
                    yield break;
                }
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            } while (FindRootGameObject("Canvas")?.transform.Find("Crosshair Filler/AltHud/Filler/Speedometer") == null);
            TimeHUD[1] = Instantiate(FindRootGameObject("Canvas").transform.Find("Crosshair Filler/AltHud/Filler/Speedometer").gameObject,
                                            FindRootGameObject("Canvas").transform.Find("Crosshair Filler/AltHud/Filler"));
            TimeHUD[2] = Instantiate(FindRootGameObject("Canvas").transform.Find("Crosshair Filler/AltHud (2)/Filler/Speedometer").gameObject,
                                        FindRootGameObject("Canvas").transform.Find("Crosshair Filler/AltHud (2)/Filler"));

            TimeHUD[1].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().fontMaterial = TimeHUD[1].transform
                .Find("Text (TMP)").GetComponent<TextMeshProUGUI>().fontSharedMaterial;
            TimeHUD[1].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().SetMaterialDirty();
            TimeHUD[1].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = new Color(1, 0.9f, 0.2f);
            TimeHUD[1].transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "TIME";
            TimeHUD[1].transform.localPosition = new Vector3(360, -360, 0);
            Destroy(TimeHUD[1].GetComponent<Speedometer>());
            TimeHUD[1].name = "Time Juice";
            TimeHUD[2].transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "TIME";
            TimeHUD[2].transform.localPosition = new Vector3(360, -360, 0);
            Destroy(TimeHUD[2].GetComponent<Speedometer>());
            TimeHUD[2].name = "Time Juice";
            TimeHUD[1].AddComponent<TimeHUD>();
            TimeHUD[1].GetComponent<TimeHUD>().type = 1;
            TimeHUD[2].AddComponent<TimeHUD>();
            TimeHUD[2].GetComponent<TimeHUD>().type = 2;
            Log("Golden Time Alt HUD created successfully.", true);
        }
        public static void ResetGoldArm()
        {
            StartTime(0);
            TimeArm.Instance?.Reset();
        }
        public static GameObject FindRootGameObject(string _name)
        {
            return SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(G => G.name == _name);
        }

        public IEnumerator InstantiateMenuItems()
        {
            yield return new WaitUntil(() => LoadDone);
            Log("custom menu items are loaded", false, ErrorLevel.Info);
            GameObject timeArmText = Instantiate(armTimeText, FindRootGameObject("Canvas").transform.Find("Main Menu (1)/V1"));
            timeArmText.SetActive(TimestopperProgress.HasArm);
            GameObject timeArmText2 = Instantiate(armTimeText, FindRootGameObject("Canvas").transform.Find("Difficulty Select (1)/Info Background/V1"));
            timeArmText2.GetComponent<Image>().color = new Color(0.125f, 0.125f, 0.125f, 1);
            timeArmText2.SetActive(TimestopperProgress.HasArm);
        }

        public static readonly HashSet<string> forbiddenSceneList = new HashSet<string>()
        {
            "b3e7f2f8052488a45b35549efb98d902" /*main menu*/,
            "Bootstrap",
            "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/,
            "4c18368dae54f154da2ae65baf0e630e" /*intermission 1*/,
            "d8e7c3bbb0c2f3940aa7c51dc5849781" /*intermission 2*/,
        };
        
        public void OnSceneUnloaded(Scene scene)
        {
            
        }


        private bool ConfirmLevel(string LayerName)
        {
            if (currentLevelInfo == null)
                foreach (GameObject G in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (G.name == "Level Info")
                    {
                        currentLevelInfo = G;
                        if (G.GetComponent<StockMapInfo>().layerName == LayerName)
                            return true;
                    }
                }
            else
                if (currentLevelInfo.GetComponent<StockMapInfo>().layerName == LayerName)
                return true;
            return false;
        }
        public IEnumerator CStopTime(float speed)
        {
            if (isInForbiddenScene) yield break;
            StopCoroutine(timeStarter);
            StoppedTimeAmount = 0;
            //Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.GetComponent<Animator>().speed = animationSpeed.value;
            if (filterMusic.value)
                MonoSingleton<MusicManager>.Instance?.FilterMusic();
            UnityEngine.Physics.simulationMode = SimulationMode.Script;
            RigidbodyStopper.FreezeAll();
            foreach (Animator A in Player.GetComponentsInChildren<Animator>())   // Make player animations unscaled
                A.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (speed == 0)
            {
                Time.timeScale = 0;
                realTimeScale = 0;
                playerTimeScale = 1;
                yield break;
            }
            do
            {
                if (!MonoSingleton<OptionsManager>.Instance) break;
                Time.timeScale -= Time.unscaledDeltaTime / speed * (MonoSingleton<OptionsManager>.Instance.paused ? 0 : 1);
                realTimeScale -= Time.unscaledDeltaTime / speed * (MonoSingleton<OptionsManager>.Instance.paused ? 0 : 1);
                yield return null;
            } while (Time.timeScale > Time.unscaledDeltaTime / speed);
            
            Time.timeScale = 0;
            realTimeScale = 0;
        }
        public IEnumerator CStartTime(float speed, bool preventStyle = false)
        {
            if (isInForbiddenScene) yield break;
            StopCoroutine(timeStopper);
            if (filterMusic.value)
                MonoSingleton<MusicManager>.Instance?.UnfilterMusic();
            UnityEngine.Physics.simulationMode = SimulationMode.FixedUpdate;
            RigidbodyStopper.UnfreezeAll();
            foreach (Animator A in FindObjectsOfType<Animator>())  // Make animations not work in stopped time when time isn't stopped
                if (A.gameObject.transform.IsChildOf(Player.transform) && A.updateMode == AnimatorUpdateMode.UnscaledTime)
                    A.updateMode = AnimatorUpdateMode.Normal;
            if (speed == 0)
            {
                Time.timeScale = 1;
                realTimeScale = 1;
                StoppedTimeAmount = 0;
                yield break;
            }
            if (Time.timeScale < 0)
                Time.timeScale = 0;
            do
            {
                if (!MonoSingleton<OptionsManager>.Instance) break;
                Time.timeScale += Time.unscaledDeltaTime / speed * (MonoSingleton<OptionsManager>.Instance.paused? 0 : 1);
                realTimeScale += Time.unscaledDeltaTime / speed * (MonoSingleton<OptionsManager>.Instance.paused ? 0 : 1)   ;
                yield return null;
            } while (Time.timeScale < 1);
            if (!preventStyle && StoppedTimeAmount > 2)
                MonoSingleton<StyleHUD>.Instance?.AddPoints((int)StoppedTimeAmount * 100, "timestopper.timestop", Playerstopper.Instance.gameObject);
            StoppedTimeAmount = 0;
            Time.timeScale = 1;
            realTimeScale = 1;
        }
        public static void StopTime(float time)
        {
            if (isInForbiddenScene) return;
            Instance.timeStopper = Instance.CStopTime(time);
            Instance.StartCoroutine(Instance.timeStopper);
            TimeStop = true;
        }
        public static void StartTime(float time, bool preventStyle = false)
        {
            if (isInForbiddenScene) return;
            Instance.timeStarter = Instance.CStartTime(time, preventStyle);
            Instance.StartCoroutine(Instance.timeStarter);
            Instance.timeSinceLastTimestop = TimeSince.Now;
            TimeStop = false;
        }
        
        public static bool frameLaterer;
        private void HandleHitstop()
        {
            if ((float)AccessTools.Field(typeof(TimeController), "currentStop").GetValue(MonoSingleton<TimeController>.Instance) <= 0)
            {
                if (playerTimeScale <= 0)
                {
                    playerTimeScale = 1;
                    Time.timeScale = 0;
                    var instance = MonoSingleton<TimeController>.Instance;
                    if (instance != null) instance.timeScaleModifier = 1;
                    (AccessTools.Field(typeof(TimeController), "parryFlash").GetValue(MonoSingleton<TimeController>.Instance) as GameObject)?.SetActive(false);
                    foreach (Transform child in Player.transform.Find("Main Camera/New Game Object").transform)
                        Destroy(child.gameObject);
                }
            }
            else
            {
                frameLaterer = true;
                playerTimeScale = 0;
                Time.timeScale = 0;
            }
        }
        private bool menuOpenLastFrame;
        private void HandleMenuPause()
        {
            if (MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused)
                playerTimeScale = 0;
            else if (menuOpenLastFrame != MonoSingleton<OptionsManager>.Instance?.paused)
                playerTimeScale = 1;
            if (MonoSingleton<OptionsManager>.Instance)
                menuOpenLastFrame = MonoSingleton<OptionsManager>.Instance.paused;
        }

        /// <summary>
        /// This event will shoot in place of FixedUpdate when time is stopped.
        /// </summary>
        public void FakeFixedUpdate()
        {
            if (TimeStop && MonoSingleton<OptionsManager>.Instance && !MonoSingleton<OptionsManager>.Instance.paused)
            {
                Time.timeScale = realTimeScale;
                // if (MonoSingleton<NewMovement>.Instance.rb.useGravity)
                //     MonoSingleton<NewMovement>.Instance.rb.AddForce(Physics.gravity, ForceMode.Acceleration);
                FixedUpdateCaller.CallAllFixedUpdates();
                // Vector3 oldGravity = Physics.gravity;
                // Physics.gravity = Vector3.zero;
                if (playerDeltaTime > 0)
                    UnityEngine.Physics.Simulate(Mathf.Max(Time.fixedDeltaTime * (1 - realTimeScale), 0));   // Manually simulate Rigidbody physics
                // Physics.gravity = oldGravity;
            }
        }
        float time;
        
        public Vector3 GetPlayerVelocity(bool trueVelocity = false)
        {
            if (!MonoSingleton<NewMovement>.Instance) return Vector3.zero;
            Vector3 velocity = MonoSingleton<NewMovement>.Instance.rb.velocity;
            if (!trueVelocity && MonoSingleton<NewMovement>.Instance.boost && !MonoSingleton<NewMovement>.Instance.sliding)
                velocity /= 3f;
            if ((bool) (UnityEngine.Object) MonoSingleton<NewMovement>.Instance.ridingRocket)
                velocity += MonoSingleton<NewMovement>.Instance.ridingRocket.rb.velocity;
            if ( MonoSingleton<PlayerMovementParenting>.Instance)
            {
                Vector3 vector3 = MonoSingleton<PlayerMovementParenting>.Instance.currentDelta * 60f;
                velocity += vector3;
            }
            return velocity;
        }

        private static Type portalManagerV2Type = AccessTools.TypeByName("PortalManagerV2");            // these two are for legacy support
        private static Type simplePortalTravelerType = AccessTools.TypeByName("SimplePortalTraveler"); // ///////
        
        private FieldInfo travellersField = AccessTools.Field(portalManagerV2Type, "travellers"); // portal traveller list getter
        private MethodInfo cacheTravelerValues = AccessTools.Method(simplePortalTravelerType, "CacheTravelerValues");
        private void Update()
        {
            if (TimeStop)
            {
                HandleHitstop();
                HandleMenuPause();
                time += Timestopper.playerDeltaTime;
                if (time > Time.maximumDeltaTime)
                    time = Time.maximumDeltaTime;
                UnscaleTimeSince = true;
                fixedCall = true;
                while (time >= Time.fixedDeltaTime)
                {
                    time -= Time.fixedDeltaTime;
                    FakeFixedUpdate();
                }
                fixedCall = false;
            }
            InvokeCaller.Update();
            if (isInForbiddenScene) return;
            if (TimeStop)
            {
                // carried this to fixedupdatecaller
                // foreach (IPortalTraveller traveller in (List<IPortalTraveller>)travellersField.GetValue(MonoSingleton<PortalManagerV2>.Instance))
                // {  
                //     if (traveller is SimplePortalTraveler simpleTraveller)
                //         cacheTravelerValues.Invoke(simpleTraveller, null); // Todo: switch to delegate
                // } 
                if (!Dummy)
                {
                    Dummy = new GameObject("Player Dummy");
                    Rigidbody R = Dummy.AddComponent<Rigidbody>();
                    R.isKinematic = true;
                    GameObject DummyHead = new GameObject("Head");
                    DummyHead.transform.parent = Dummy.transform;
                    DummyHead.transform.localPosition = Player.transform.Find("Main Camera").Find("New Game Object").transform.localPosition;
                    DummyHead.transform.localRotation = Player.transform.Find("Main Camera").Find("New Game Object").transform.localRotation;
                    Dummy.transform.position = Player.transform.Find("New Game Object").position;
                    Dummy.transform.rotation = Player.transform.Find("New Game Object").rotation;
                }
                // MonoSingleton<PlayerTracker>.Instance
                typeof(PlayerTracker).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(MonoSingleton<PlayerTracker>.Instance, Dummy.transform);
                typeof(PlayerTracker).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(MonoSingleton<PlayerTracker>.Instance, Dummy.transform.GetChild(0));
                typeof(PlayerTracker).GetField("playerRb", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(MonoSingleton<PlayerTracker>.Instance, Dummy.GetComponent<Rigidbody>());
            }
            else
            {
                if (Dummy && timeSinceLastTimestop > 1)
                {
                    if (Vector3.Distance(Dummy.transform.position, Player.transform.position) > 1)
                    {
                        Dummy.transform.position -=
                            Vector3.Normalize(Dummy.transform.position - Player.transform.position) * (200 * playerDeltaTime);
                    }
                    else
                    {
                        typeof(PlayerTracker).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance)
                            ?.SetValue(MonoSingleton<PlayerTracker>.Instance, Player.transform.Find("Main Camera").Find("New Game Object"));
                        typeof(PlayerTracker).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance)
                            ?.SetValue(MonoSingleton<PlayerTracker>.Instance, Player.transform.Find("New Game Object").transform);
                        typeof(PlayerTracker).GetField("playerRb", BindingFlags.NonPublic | BindingFlags.Instance)
                            ?.SetValue(MonoSingleton<PlayerTracker>.Instance, Player.GetComponent<Rigidbody>());
                        DestroyImmediate(Dummy.transform.GetChild(0).gameObject);
                        DestroyImmediate(Dummy);
                        Dummy = null;
                    }
                }
            }
        }
        
        public static bool isInForbiddenScene;
        private void LateUpdate()
        {
            if (!Player) return;
            if (isInForbiddenScene)
            {
                return;
            }
            if (cybergrind) //reset time juice when wave is done
            {
                if (!MonoSingleton<EndlessGrid>.Instance) return;
                if (cybergrindWave != MonoSingleton<EndlessGrid>.Instance.currentWave)
                {
                    Playerstopper.Instance.timeArm.GetComponent<TimeArm>().timeLeft = TimestopperProgress.MaxTime;
                    cybergrindWave = MonoSingleton<EndlessGrid>.Instance.currentWave;
                }
            }
        }
    }
    
    public class TimeArm : MonoBehaviour   // Component on the arm of player
    {
        public static TimeArm Instance;
        public float timeLeft = TimestopperProgress.MaxTime;
        public AudioSource armAudio;
        public NewMovement movement;
        public Animator animator;
        public bool localTimeStopTracker;

        public void Equip()
        {
            gameObject.SetActive(true);
            TimestopperProgress.EquipArm(true);
        }

        public void Reset()
        {
            animator.Play("Idle");
            timeLeft = TimestopperProgress.MaxTime;
            localTimeStopTracker = Timestopper.TimeStop;
        }
        public void Awake()
        {
            Instance = this;
            timeLeft = TimestopperProgress.MaxTime;
            gameObject.SetActive(TimestopperProgress.EquippedArm);
            movement = Playerstopper.Instance.movement;
            animator = GetComponentInChildren<Animator>();
            armAudio = GetComponentInChildren<AudioSource>();
            animator.Play("Idle");
        }

        public void UpdateTimeJuice()
        {
            if (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown)
            {
                timeLeft = TimestopperProgress.MaxTime;
                return;
            }
            if (localTimeStopTracker)
            {
                timeLeft -= Timestopper.playerDeltaTime * (1.0f - Timestopper.realTimeScale);
                Timestopper.StoppedTimeAmount += Timestopper.playerDeltaTime * (1.0f - Timestopper.realTimeScale);
                if (timeLeft < 0)
                    timeLeft = 0;
            }
            else
            {
                if (Timestopper.realTimeScale <= 0.3f)
                    if (MonoSingleton<NewMovement>.Instance)
                        MonoSingleton<NewMovement>.Instance.walking = false;
                timeLeft += Time.deltaTime * Timestopper.refillMultiplier.value;
                if (timeLeft > TimestopperProgress.MaxTime)
                    timeLeft = TimestopperProgress.MaxTime;
            }
        }
        
        
        public void Update()
        {
            if (!MonoSingleton<FistControl>.Instance) return;
            //decoration
            Vector3 newRot = new Vector3(0f, (0.3f * MonoSingleton<FistControl>.Instance.fistCooldown), (0.1f * MonoSingleton<FistControl>.Instance.fistCooldown));
            transform.localEulerAngles = (newRot * (20 * Timestopper.playerDeltaTime) + transform.localEulerAngles) / (1 + Timestopper.playerDeltaTime*20);
            //decoration end
            if (movement.dead && Timestopper.TimeStop && localTimeStopTracker)
            {
                Timestopper.StartTime(0);
                animator.Play("Idle");
                return;
            }
            UpdateTimeJuice();
            if ((UnityInput.Current.GetKeyDown(Timestopper.stopKey.value))
                || (Timestopper.TimeStop && timeLeft <= 0.0f))
            {
                if (MonoSingleton<OptionsManager>.Instance.paused) //if game paused
                    return;
                if (!localTimeStopTracker && !MonoSingleton<FistControl>.Instance.shopping && timeLeft > Timestopper.lowerTreshold.value)
                {
                    PlayRespectiveSound(true);
                    AnimatorsFix();
                    Timestopper.StopTime(Timestopper.stopSpeed.value);
                    Grayscaler.Instance.intensityControl = 1;
                    Grayscaler.Instance.grayscaleBubbleExpansion = 0;
                    animator.speed = Timestopper.animationSpeed.value;
                    animator.Play("Stop");
                    Timestopper.FixedUpdateFix(movement.transform);
                    Timestopper.Log("Time stops!", true);
                }
                else if (localTimeStopTracker)
                {
                    PlayRespectiveSound(false);
                    Timestopper.StartTime(Timestopper.startSpeed.value);
                    animator.speed = Timestopper.animationSpeed.value;
                    animator.Play("Start");
                    Timestopper.Log("Time flows normally.", true);
                }
                PlayRespectiveSound(Timestopper.TimeStop);
                localTimeStopTracker = Timestopper.TimeStop;
                ParticlesFix();
            }

            if (localTimeStopTracker != Timestopper.TimeStop)
            {
                ParticlesFix();
            }
            

            if (Timestopper.TimeStop)
            {
                if (Timestopper.timestopHardDamage.value)
                {
                    movement.ForceAddAntiHP(Timestopper.antiHpMultiplier.value * Time.unscaledDeltaTime * (1 - Timestopper.realTimeScale), true, true, true, false);
                }
            }
        }

        private void AnimatorsFix()
        {
            foreach (Animator A in movement.GetComponentsInChildren<Animator>(true))
            {
                if (A.GetComponent<AnimatorUpdater>() == null)
                    A.gameObject.AddComponent<AnimatorUpdater>();
            }
        }

        private HashSet<Transform> latestObjects = new HashSet<Transform>();
        private void ParticlesFix()
        {
            foreach (Transform t in movement.transform)
            {
                RecrusiveTracking(t);
            }
        }

        private void RecrusiveTracking(Transform t)
        {
            if (!latestObjects.Add(t)) return;
            Timestopper.Log("particle system updated: " + t.name, true, ErrorLevel.Info);
            ParticleSystem P = t.GetComponent<ParticleSystem>();
            Timestopper.FixedUpdateFix(t);
            if (P != null) P.gameObject.AddComponent<ParticleSystemUpdater>();
        }

        private bool oldPause;
        public void LateUpdate() // pause animations in pause menu
        {
            if (Timestopper.TimeStop && MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused != oldPause)
            {
                oldPause = MonoSingleton<OptionsManager>.Instance.paused;
                ParticlesFix();
            }
        }
        public void PlayRespectiveSound(bool istimestopped)
        {
            if (istimestopped != Timestopper.TimeStop)
                if (istimestopped)
                {
                    if (Timestopper.StoppedTimeAmbiences[Timestopper.stoppedSound.valueIndex] != null)
                    {
                        armAudio.volume = Timestopper.soundEffectVolume.value;
                        armAudio.clip = Timestopper.StoppedTimeAmbiences[Timestopper.stoppedSound.valueIndex];
                        armAudio.Play();
                        armAudio.loop = true;
                        armAudio.PlayOneShot(Timestopper.TimestopSounds[Timestopper.stopSound.valueIndex]);
                    }
                }
                else
                {
                    armAudio.Stop();
                    armAudio.volume = Timestopper.soundEffectVolume.value;
                    if (Timestopper.TimestartSounds[Timestopper.startSound.valueIndex] != null)
                        armAudio.PlayOneShot(Timestopper.TimestartSounds[Timestopper.startSound.valueIndex]);
                }
        }
    }
}