using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using PluginConfig.API.Functionals;
using PluginConfiguratorComponents;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ULTRAKILL.Portal;
// using UnityEngine.Rendering;
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

    [Serializable]
    public class TimestopperProgress
    {
        public bool hasArm;
        public bool equippedArm;
        public bool firstWarning;
        public int upgradeCount;
        public float maxTime = 3.0f;
        public float version = 0.9f;
        public const float latestVersion = 1.0f;

        public static bool HasArm {
            get { TimestopperProgress progress = Read(); return progress.hasArm; }
        }
        public static bool EquippedArm {
            get { TimestopperProgress progress = Read(); return progress.equippedArm; }
        }
        public static bool FirstWarning {
            get { TimestopperProgress progress = Read(); return progress.firstWarning; }
        }
        public static int UpgradeCount {
            get { TimestopperProgress progress = Read(); return progress.upgradeCount; }
        }
        public static string UpgradeText {
            get { TimestopperProgress progress = Read(); return "<align=\"center\"><color=#FFFF42>" + GenerateTextBar('▮', progress.upgradeCount) + "</color>"; }
        }
        public static float MaxTime {
            get { TimestopperProgress progress = Read(); return progress.maxTime; }
        }
        public static float UpgradeCost {
            get { TimestopperProgress progress = Read(); return 150000 + progress.upgradeCount * 66000; }
        }

        public static new string ToString()
        {
            TimestopperProgress progress = Read();
            return $@"Timestopper saved progress:
            - has arm: {progress.hasArm}
            - equipped: {progress.equippedArm}
            - firstwarning: {progress.firstWarning}
            - upgrades: {progress.upgradeCount}
            - max time: {progress.maxTime}
            - version: {progress.version}";
        }
        public int upgradeCost
        {
            get { return 150000 + upgradeCount * 66000; }
        }
        private const string PROGRESS_FILE = "timestopper.state";
        private static TimestopperProgress inst;
        private static string GenerateTextBar(char c, int b)
        {
            string s = "";
            for (int i = 0; i < b; i++)
                s += c;
            return s;
        }
        public static void UpgradeArm()
        {
            TimestopperProgress progress = Read();
            GameProgressSaver.AddMoney(-progress.upgradeCost);
            progress.maxTime += 1 + 1 / (progress.upgradeCount + 0.5f);
            progress.upgradeCount++;
            Write(progress);
        }
        public static void ForceDowngradeArm()
        {
            TimestopperProgress progress = Read();
            if (Timestopper.maxUpgrades.value < 0)
                Timestopper.maxUpgrades.value = 1;
            while (progress.upgradeCount > Timestopper.maxUpgrades.value)
            {
                progress.upgradeCount--;
                progress.maxTime -= 1 + 1 / (progress.upgradeCount + 0.5f);
            }
            Write(progress);
        }
        public static void AcceptWarning()
        {
            TimestopperProgress progress = Read();
            progress.firstWarning = true;
            Write(progress);

        }
        public static void GiveArm()
        {
            TimestopperProgress progress = Read();
            progress.hasArm = true;
            progress.equippedArm = true;
            Write(progress);
            Timestopper.mls.LogInfo("Received Golden Arm");
            Playerstopper.Instance.EquipTimeArm();
        }
        public static void ChangeEquipmentStatus()
        {
            if (Timestopper.LatestTerminal != null)
                EquipArm(Timestopper.LatestTerminal.transform.Find("Canvas/Background/Main Panel/Weapons/" +
              "Arm Window/Variation Screen/Variations/Arm Panel (Gold)/Equipment/Equipment Status/Text (TMP)").GetComponent<TextMeshProUGUI>().text[0] == 'E');
            else
                Timestopper.mls.LogWarning("LatestTerminal is Null!");
            TimeHUD.ReconsiderAll();
            Timestopper.Log("Changed equipment status", true, 1);
        }
        public static void EquipArm(bool equipped)
        {
            TimestopperProgress progress = Read();
            if (Playerstopper.Instance.timeArm == null)
                return;
            if (progress.hasArm)
            {
                progress.equippedArm = equipped;
                Playerstopper.Instance.timeArm.SetActive(equipped);
                Timestopper.Log("Gold Arm Equipment Status changed: " + progress.equippedArm.ToString(), true, 1);
            }
            else
            {
                Timestopper.Log("Invalid request of arm equipment, user doesn't have the arm yet!", true, 2);
                GiveArm();
                return;
            }
            Write(progress);
        }

        public static void Reset()
        {
            Write(new TimestopperProgress());
        }

        public static TimestopperProgress Read()
        {
            try
            {
                string filePath = Path.Combine(GameProgressSaver.SavePath, PROGRESS_FILE);
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    inst = JsonUtility.FromJson<TimestopperProgress>(jsonData);
                    if (inst == null)
                        inst = new TimestopperProgress();
                    if (inst.version < latestVersion)
                    {
                        if (inst.version == 0.9f)
                        {
                            inst.version = 1.0f;
                        }
                    }
                }
                else
                {
                    inst = new TimestopperProgress();
                }
            }
            catch (Exception e)
            {
                Timestopper.mls.LogError($"Failed to read progress: {e.Message}, resetting save file {GameProgressSaver.currentSlot}");
                inst = new TimestopperProgress();
                Write(inst);
            }
            return inst;
        }

        public static void Write(TimestopperProgress progress)
        {
            try
            {
                string filePath = Path.Combine(GameProgressSaver.SavePath, PROGRESS_FILE);
                string jsonData = JsonUtility.ToJson(progress, true);
                File.WriteAllText(filePath, jsonData);
            }
            catch (Exception e)
            {
                Timestopper.mls.LogError($"Failed to write progress: {e.Message}");
            }
        }
    }

    


    [BepInPlugin(GUID, Name, Version)]
    public class Timestopper : BaseUnityPlugin
    {
        public const string GUID = "dev.galvin.timestopper";
        public const string Name = "The Timestopper";
        public const string Version = "1.6.0";
        public const string SubVersion = "rc.1";

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
        public static Texture2D armGoldColor;
        public static GameObject armGoldObj;
        public static RuntimeAnimatorController armGoldAC;
        public AssetBundle bundle;

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
        public static GameObject TheCube;
        public static GameObject MenuCanvas;

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
                else if (TimeStop) return Time.unscaledDeltaTime * playerTimeScale;
                else return Time.deltaTime;
            }
        }
        public static float playerFixedDeltaTime
        {
            get
            {
                if (fixedCall) return Time.fixedDeltaTime;
                else if (TimeStop) return Time.fixedDeltaTime * playerTimeScale;
                else return Time.fixedDeltaTime;
            }
        }
        //________________________ COROUTINES __________________________\\
        private IEnumerator timeStopper;
        private IEnumerator timeStarter;
        // _______________ COMPATBILITY WITH OTHER MODS _________________\\
        public static bool Compatability_JukeBox;

        //$$$$$$$$$$$$$$$$$$$$$$$$$ CONFIG FILES $$$$$$$$$$$$$$$$$$$$$$$$$$$$$\\
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
        //-------------------------colors--------------------------\\
        public static ColorField timeJuiceColorNormal;
        public static ColorField timeJuiceColorInsufficient;
        public static ColorField timeJuiceColorUsing;
        public static ColorField timeJuiceColorNoCooldown;

        private PluginConfigurator config;

        /// <summary>
        /// Logs information or error, hides extensive logs if extensive logging is false.
        /// </summary>
        /// <param name="log">Message to display</param>
        /// <param name="extensive">Extensive messages only display if extensive logging is set to true</param>
        /// <param name="err_lvl">Error level: 0-Debug  1-Info  2-Warning  3-Error  4-Fatal</param>
        public static void Log(string log, bool extensive = false, int err_lvl = 0 /*dbg-inf-war-err-ftl*/)
        {
            if (extensive)
                if (!extensiveLogging.value)
                    return;
            if (err_lvl == 0) mls.LogDebug(log);
            if (err_lvl == 1) mls.LogInfo(log);
            if (err_lvl == 2) mls.LogWarning(log);
            if (err_lvl == 3) mls.LogError(log);
            if (err_lvl == 4) mls.LogFatal(log);
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
            InitializeConfig();

            playerTimeScale = 1.0f;

            harmony.PatchAll();
            
            //***********DEBUG**************\\
            TheCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            TheCube.name = "The Cube";

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
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
                BindingFlags.NonPublic |      // Because it's protected
                BindingFlags.Instance |       // Instance method (not static)
                BindingFlags.FlattenHierarchy // Include base class methods
            );
            method?.Invoke(slf.parentPanel, null);
        }
        public void ReloadSoundProfilesList()
        {
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
                string modPath = "";
                Log("searching modpath ", false, 3);
                Log("found mod files: " + Directory.GetDirectories(Paths.PluginPath).Length, false, 3);
                foreach (string s in Directory.GetDirectories(Paths.PluginPath))
                {
                    // if (!s.ToLower().Contains("timestopper")) continue;
                    if (!File.Exists(Path.Combine(s, "The Timestopper.dll"))) continue;
                    Log("FOUND TIMESTOPPER: " + s, false, 2);
                    modPath = s;
                    break;
                }
                Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds"));
                foreach (string sounddirectory in  sounddirectories)
                {
                    Directory.CreateDirectory(Path.Combine(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", sounddirectory)));
                    
                    foreach (string file in Directory.GetFiles(
                                 modPath,
                                 "*.*").Select(Path.GetFileName).Where(filename => filename.EndsWith(".ogg") && filename.StartsWith(sounddirectory)))
                    {
                        Log("copying over sound file: " + sounddirectory + "/" + file, false, 1);
                        if (!File.Exists(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", sounddirectory, file)))
                            File.Copy(Path.Combine(modPath, sounddirectory + "-" + file.TrimStart((sounddirectory + "-").ToCharArray())),
                                        Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", sounddirectory, file.TrimStart((sounddirectory + "-").ToCharArray()) ));
                    }
                }
                if (!File.Exists(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "README.txt")))
                    File.Copy(Path.Combine(modPath, "Sounds-readme.txt"),
                                Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "README.txt"));
            }
            string[] timestopSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Stopping"), "*.*").
                Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).Select(Path.GetFileNameWithoutExtension).ToArray();
            string[] stopambienceSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Stopped"), "*.*").
                Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).Select(Path.GetFileNameWithoutExtension).ToArray();
            string[] timestartSoundsList = Directory.GetFiles(Path.Combine(Paths.ConfigPath, "Timestopper", "Sounds", "Starting"), "*.*").
                Where(file => file.EndsWith(".wav") || file.EndsWith(".ogg")).Select(Path.GetFileNameWithoutExtension).ToArray();
            if (timestopSoundsList.Length < 1) {
                Log("No time stop sounds found!", false, 3);
                timestopSoundsList = new [] { "FILE ERROR" };
            }
            if (stopambienceSoundsList.Length < 1) {
                Log("No time stop sounds found!", false, 3);
                stopambienceSoundsList = new [] { "FILE ERROR" };
            }
            if (timestartSoundsList.Length < 1) {
                Log("No time stop sounds found!", false, 3);
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

                stopKey = new KeyCodeField(config.rootPanel, "Timestopper Key", "stopkey", KeyCode.V);
                timestopHardDamage = new BoolField(config.rootPanel, "Timestop Hard Damage", "harddamage", true); // reverse input
                stopSpeed = new FloatField(config.rootPanel, "Timestop Speed", "stopspeed", 0.6f);
                startSpeed = new FloatField(config.rootPanel, "Timestart Speed", "startspeed", 0.8f);
                affectSpeed = new FloatField(config.rootPanel, "Interaction Speed", "interactionspeed", 1.0f);
                animationSpeed = new FloatField(config.rootPanel, "Animation Speed", "animationspeed", 1.3f);

                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                // ReSharper disable once ObjectCreationAsStatement
                new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- GRAPHICS --");


                grayscale = new BoolField(config.rootPanel, "Shader Effects", "doGrayscale", false);
                ConfigDivision grayscaleOptions = new ConfigDivision(config.rootPanel, "grayscaleOptions") {
                    interactable = grayscale.value
                };
                grayscale.onValueChange += (e) => { 
                    grayscaleOptions.interactable = e.value;
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
                grayscaleColorSpace = new ColorField(shaderOptions, "Grayscale Color Space", "grayscalecolorspace", new Color(0.299f, 0.587f, 0.114f));
                grayscaleColorSpaceIntensity = new FloatField(shaderOptions, "Grayscale Color Space Multiplier", "grayscalecolorspaceintensity", 1.0f);
                
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
                    interactable = false };

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
                resetSaveButton.onClick += () => { TimestopperProgress.Reset(); };
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
                Log("Total assets loaded: " + newBundle.GetAllAssetNames().Length, true, 1);
                foreach (var asset in newBundle.GetAllAssetNames())
                {
                    Log(asset, true, 1);
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
                Log("Shop Component is null, cannot update terminal!", false, 3);
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
                    firstWarning.transform.localPosition = new Vector3(-9.4417f, 6.0f, 0.0002f);
                    firstWarning.transform.Find("Button 1").gameObject.SetActive(false);
                    firstWarning.transform.Find("Button 2").gameObject.SetActive(false);
                    firstWarning.transform.Find("Button 3").gameObject.SetActive(false);
                    firstWarning.transform.Find("Icon").gameObject.SetActive(false);
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
        
        public void PreventNull()
        {
            if (Player.GetComponent<TerminalUpdater>() == null)
            {
                Player.AddComponent<TerminalUpdater>();
            }
            if (MenuCanvas == null)
            {
                MenuCanvas = FindRootGameObject("Canvas");
            }
        }
        public static Shader FindShader(string name)
        {
            foreach (Shader shader in Resources.FindObjectsOfTypeAll<Shader>())
            {
                if (shader.name == name)
                {
                    return shader;
                }
            }
            Log("Couldn't find " + name + " shader!", true, 3);
            return null;
        }
        public IEnumerator LoadHUD()
        {
            float elapsedTime = 0;
            Log("Loading HUD Elements...", true);
            do
            {
                if (elapsedTime > 5)
                {
                    Log("Time Juice Bar creation failed after 5 seconds!", false, 3);
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
                    Log("Time Juice Alt HUD creation failed after 5 seconds!", false, 3);
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
            // yield break; // TempRemove
            // foreach (var text in FindObjectsOfType<TextMeshProUGUI>(true)) //Fix Disappeared Text #FixTexts
            // {
            //     if (fixedTexts.Add(text))
            //     {
            //         text.fontMaterial = text.fontSharedMaterial;
            //         text.SetMaterialDirty();
            //     }
            // }
        }
        // HashSet<TextMeshProUGUI> fixedTexts = new HashSet<TextMeshProUGUI>();
        public static void ResetGoldArm()
        {
            StartTime(0);
            if (TimeArm.Instance != null)
            {
                TimeArm.Instance.Reset();
            }
        }
        public GameObject FindRootGameObject(string _name)
        {
            foreach (GameObject G in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (G.name == _name)
                    return G;
            }
            return null;
        }

        public IEnumerator InstantiateMenuItems()
        {
            yield return new WaitUntil(() => LoadDone);
            Log("custom menu items are loaded", false, 2);
            GameObject timeArmText = Instantiate(armTimeText, FindRootGameObject("Canvas").transform.Find("Main Menu (1)/V1"));
            timeArmText.SetActive(TimestopperProgress.HasArm);
            GameObject timeArmText2 = Instantiate(armTimeText, FindRootGameObject("Canvas").transform.Find("Difficulty Select (1)/Info Background/V1"));
            timeArmText2.GetComponent<Image>().color = new Color(0.125f, 0.125f, 0.125f, 1);
            timeArmText2.SetActive(TimestopperProgress.HasArm);
        }

        private IEnumerator StopAllRBDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            foreach (Rigidbody R in FindObjectsOfType<Rigidbody>())
            {
                RigidbodyStopper.AddRBS(R.gameObject);
            }
        }
        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RigidbodyStopper.HashedGameObjects.Clear();
            AudioPitcher.HashedGameObjects.Clear();
            if (scene.name == "b3e7f2f8052488a45b35549efb98d902" /*main menu*/)
            {
                mls.LogWarning("main menu loaded");
                if (armTimeText == null)
                {
                    StartCoroutine(LoadBundle());   //Load all assets
                }
                StartCoroutine(InstantiateMenuItems());
                
            }
            if (!forbiddenScene)
            {
                InvokeCaller.ClearMonos();
                StartCoroutine(StopAllRBDelayed(2));
                if (Playerstopper.Instance == null)
                    Player.AddComponent<Playerstopper>();
                Playerstopper.Instance.AddInvokeCallers(Playerstopper.Instance.transform);
                if (forceDowngrade.value)
                    TimestopperProgress.ForceDowngradeArm();
                StartCoroutine(LoadHUD());
                // return; // Temp Remove
                StatsManager.checkpointRestart += ResetGoldArm;
                if (firstLoad && !TimestopperProgress.HasArm) //display the message for newcomers
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(ARM_NEW_MESSAGE, "", "", 2);
                    messageTimer.done += () =>
                    {
                        MonoSingleton<HudMessageReceiver>.Instance.Invoke("Done", 0);
                        firstLoad = false;
                    };
                    messageTimer.SetTimer(6, true);
                }
                foreach(FishObjectReference F in FindObjectsOfType<FishObjectReference>(true))
                {
                    if (F.gameObject.name == "GoldArmPickup")
                    {
                        F.gameObject.AddComponent<TimeArmPickup>();
                    }
                }
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem("timestopper.timestop", TIMESTOP_STYLE); // register timestop style
            }
            else
            {
                timeStopper = CStopTime(0);
                timeStarter = CStartTime(0);
            }
            // return; // Temp Remove
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
                Log("Added The New Arm Altar", true);
            }
            // Cybergrind Music Explorer Compatability
            if (scene.name == "9240e656c89994d44b21940f65ab57da")
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
                        else Log("Component C is null!", true, 3);
                    }
                    else Log("Could not get Jukebox.Components.NowPlayingHud, Cybergrind Music Explorer may have errors", true, 3);
                }
            }
            else
            {
                cybergrind = false;
                Compatability_JukeBox = false;
            }
        }
        public void OnSceneUnloaded(Scene scene)
        {
            // if (scene.name != "b3e7f2f8052488a45b35549efb98d902" /*main menu*/ &&
            //     scene.name != "Bootstrap" && LoadDone &&
            //     scene.name != "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/ &&
            //     realTimeScale < 1)
            // {
            //     StartTime(0, true);
            // }
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
            if (forbiddenScene) yield break;
            StopCoroutine(timeStarter);
            StoppedTimeAmount = 0;
            //Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.GetComponent<Animator>().speed = animationSpeed.value;
            if (filterMusic.value)
                MonoSingleton<MusicManager>.Instance.FilterMusic();
            Physics.simulationMode = SimulationMode.Script;
            RigidbodyStopper.FreezeAll();
            foreach (Animator A in FindObjectsOfType<Animator>())   // Make animations work in stopped time when time is stopped
                if (A.gameObject.transform.IsChildOf(Player.transform) && A.updateMode == AnimatorUpdateMode.Normal)
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
                Time.timeScale -= Time.unscaledDeltaTime / speed * (MonoSingleton<OptionsManager>.Instance.paused ? 0 : 1);
                realTimeScale -= Time.unscaledDeltaTime / speed * (MonoSingleton<OptionsManager>.Instance.paused ? 0 : 1);
                yield return null;
            } while (Time.timeScale > Time.unscaledDeltaTime / speed);
            Time.timeScale = 0;
            realTimeScale = 0;
        }
        public IEnumerator CStartTime(float speed, bool preventStyle = false)
        {
            if (forbiddenScene) yield break;
            StopCoroutine(timeStopper);
            if (filterMusic.value)
                MonoSingleton<MusicManager>.Instance.UnfilterMusic();
            Physics.simulationMode = SimulationMode.FixedUpdate;
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
                Time.timeScale += Time.unscaledDeltaTime / speed * (MonoSingleton<OptionsManager>.Instance.paused? 0 : 1);
                realTimeScale += Time.unscaledDeltaTime / speed * (MonoSingleton<OptionsManager>.Instance.paused ? 0 : 1)   ;
                yield return null;
            } while (Time.timeScale < 1);
            if (!preventStyle && StoppedTimeAmount > 2)
                MonoSingleton<StyleHUD>.Instance.AddPoints((int)StoppedTimeAmount * 100, "timestopper.timestop", Playerstopper.Instance.gameObject);
            StoppedTimeAmount = 0;
            Time.timeScale = 1;
            realTimeScale = 1;
        }
        public static void StopTime(float time)
        {
            if (forbiddenScene) return;
            Instance.timeStopper = Instance.CStopTime(time);
            Instance.StartCoroutine(Instance.timeStopper);
            TimeStop = true;
        }
        public static void StartTime(float time, bool preventStyle = false)
        {
            if (forbiddenScene) return;
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
                    MonoSingleton<TimeController>.Instance.timeScaleModifier = 1;
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
            if (MonoSingleton<OptionsManager>.Instance.paused)
                playerTimeScale = 0;
            else if (menuOpenLastFrame != MonoSingleton<OptionsManager>.Instance.paused)
                playerTimeScale = 1;
            menuOpenLastFrame = MonoSingleton<OptionsManager>.Instance.paused;
        }
        public void FakeFixedUpdate()
        {
            if (TimeStop)
            {
                Time.timeScale = realTimeScale;
                if (playerDeltaTime > 0)
                    Physics.Simulate(Time.fixedDeltaTime * (1 - realTimeScale));   // Manually simulate Rigidbody physics
            }
        }
        float time;
        
        public Vector3 GetPlayerVelocity(bool trueVelocity = false)
        {
            Vector3 velocity = MonoSingleton<NewMovement>.Instance.rb.velocity;
            if (!trueVelocity && MonoSingleton<NewMovement>.Instance.boost && !MonoSingleton<NewMovement>.Instance.sliding)
                velocity /= 3f;
            if ((bool) (UnityEngine.Object) MonoSingleton<NewMovement>.Instance.ridingRocket)
                velocity += MonoSingleton<NewMovement>.Instance.ridingRocket.rb.velocity;
            if ((UnityEngine.Object) MonoSingleton<PlayerMovementParenting>.Instance != (UnityEngine.Object) null)
            {
                Vector3 vector3 = MonoSingleton<PlayerMovementParenting>.Instance.currentDelta * 60f;
                velocity += vector3;
            }
            return velocity;
        }

        private FieldInfo travellersField = AccessTools.Field(typeof(PortalManagerV2), "travellers"); // portal traveller list getter
        private MethodInfo cacheTravelerValues = AccessTools.Method(typeof(SimplePortalTraveler), "CacheTravelerValues");
        private void Update()
        {
            InvokeCaller.Update();
            if (forbiddenScene) return;
            if (TimeStop)
            {
                foreach (var traveller in (List<IPortalTraveller>)travellersField.GetValue(MonoSingleton<PortalManagerV2>.Instance))
                {
                    cacheTravelerValues.Invoke(traveller, null);
                }
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

        public static bool forbiddenScene
        {
            get
            {
                return (SceneManager.GetActiveScene().name == "b3e7f2f8052488a45b35549efb98d902" /*main menu*/ ||
                        SceneManager.GetActiveScene().name == "Bootstrap" ||
                        SceneManager.GetActiveScene().name == "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/ ||
                        SceneManager.GetActiveScene().name == "4c18368dae54f154da2ae65baf0e630e" /*intermission 1*/ ||
                        SceneManager.GetActiveScene().name == "d8e7c3bbb0c2f3940aa7c51dc5849781" /*intermission 2*/);
            }
        }
        private void LateUpdate()
        {
            // return; // Temp Remove
            if (Player == null) return;
            if (forbiddenScene)
            {
                return;
            }
            if (cybergrind) //reset time juice when wave is done
            {
                if (cybergrindWave != MonoSingleton<EndlessGrid>.Instance.currentWave)
                {
                    Playerstopper.Instance.timeArm.GetComponent<TimeArm>().timeLeft = TimestopperProgress.MaxTime;
                    cybergrindWave = MonoSingleton<EndlessGrid>.Instance.currentWave;
                }
            }
            PreventNull();
            if (MonoSingleton<NewMovement>.Instance.dead && TimeStop)
            {
                StartTime(0);
            }
            if (TimeStop)
            {
                HandleHitstop();
                HandleMenuPause();
                time += Time.unscaledDeltaTime;
                if (time > Time.maximumDeltaTime)
                    time = Time.maximumDeltaTime;
                while (time >= Time.fixedDeltaTime)
                {
                    time -= Time.fixedDeltaTime;
                    FakeFixedUpdate();
                }
            }
        }
    }

    /// <summary>
    /// /////////////////////////////////////// UNDONE - WIP FOR DUAL WIELD /////////////////////////////////
    /// </summary>
    public class InvokeCaller 
    {
        public static HashSet<MonoBehaviour> targets = new HashSet<MonoBehaviour>();
        public static HashSet<InvokeCaller> invokers = new HashSet<InvokeCaller>();
        private MonoBehaviour mono;
        private MethodInfo method;
        private float time;

        public static void Update()
        {
            foreach (InvokeCaller IC in invokers.ToArray())
            {
                IC.Iterate();
            }
        }

        public static void ClearDestroyed()
        {
            targets.RemoveWhere(item => item == null);
            invokers.RemoveWhere(ic => ic.mono == null);
        }
        public static void ClearMonos()
        {
            targets.Clear();
        }
        public static void RegisterMonos(IEnumerable<MonoBehaviour> values)
        {
            foreach (MonoBehaviour M in values)
            {
                targets.Add(M);
            }
        }
        public static void UnRegisterMonos(IEnumerable<MonoBehaviour> values)
        {
            foreach (MonoBehaviour M in values)
            {
                targets.Remove(M);
            }
        }

        private InvokeCaller(MonoBehaviour instance, string methodName, float delay)
        {
            time = delay;
            mono = instance;
            try
            {
                method = mono.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod);
            }
            catch
            {
                method = null;
            }
        }
        private void Iterate()
        {
            time -= Timestopper.playerDeltaTime;
            if (time < 0)
            {
                if (mono!=null)
                    method?.Invoke(mono, null);
                invokers.Remove(this);
            }
        }
        public static bool Add(MonoBehaviour instance, string methodName, float delay)
        {
            if (targets.Contains(instance))
            {
                invokers.Add(new InvokeCaller(instance, methodName, delay));
                return false;
            }
            return true;
        }
    }
    public class AudioPitcher : MonoBehaviour
    {
        private static HashSet<GameObject> gos = new HashSet<GameObject>();
        public static HashSet<GameObject> HashedGameObjects { get { return gos; } }
        AudioSource audio;
        private bool useOriginal;
        float originalPitch = 1;
        private float localScale = 1;

        bool music
        {
            get
            {
                if (!audio || !audio.clip)
                    return false;
                return (audio.spatialBlend == 0 && audio.clip.length > 10);
            }
        }

        public static void AddAP(GameObject go)
        {
            if (gos.Add(go))
                go.AddComponent<AudioPitcher>();
        }
        private void Awake()
        {
            audio = GetComponent<AudioSource>();
            originalPitch = audio.pitch;
        }
        public void LateUpdate()
        {
            if (!audio)
                return;
            if (Timestopper.realTimeScale == 1.0f && !useOriginal)
            {
                audio.pitch = originalPitch;
                useOriginal = true;
                return;
            }
            if (Timestopper.realTimeScale < 1)
            {
                if (useOriginal)
                {
                    originalPitch = audio.pitch;
                    localScale = 1;
                    useOriginal = false;
                }
                if (!music)
                {
                    if (Timestopper.TimeStop)
                    {
                        audio.pitch = originalPitch * localScale;
                        if (localScale > 0) 
                            localScale = Mathf.Max(localScale - Timestopper.playerDeltaTime * Timestopper.affectSpeed.value, 0);
                    }
                    else
                    {
                        audio.pitch = originalPitch * Mathf.Max(Timestopper.realTimeScale, localScale);
                    }
                } else
                {
                    audio.pitch = Timestopper.realTimeScale * (1 - Timestopper.stoppedMusicPitch.value) + Timestopper.stoppedMusicPitch.value;
                    MonoSingleton<MusicManager>.Instance.volume = Timestopper.realTimeScale * (1 - Timestopper.stoppedMusicVolume.value) + Timestopper.stoppedMusicVolume.value;
                }
            }
        }
    }

    public class TerminalExcluder : MonoBehaviour  // Make sure they cannot unequip the arm when time is stopped
    {
        public bool done = true;
        public void OverrideInfoMenu()
        {
            if (transform.Find("Canvas").GetComponent<CanvasExcluder>() == null)
                transform.Find("Canvas").gameObject.AddComponent<CanvasExcluder>();
            GameObject armWindow = transform.Find("Canvas/Background/Main Panel/Weapons/Arm Window").gameObject;
            GameObject armInfoGold = armWindow.transform.Find("Arm Info (Gold)").gameObject;
            armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<Image>().sprite =
                                armInfoGold.transform.Find("Panel/Back Button").GetComponent<Image>().sprite;
            armInfoGold.transform.Find("Panel/Description").GetComponent<TextMeshProUGUI>().text = Timestopper.ARM_DESCRIPTION + TimestopperProgress.UpgradeText;
            if (TimestopperProgress.UpgradeCount < Timestopper.maxUpgrades.value)
            {
                if (GameProgressSaver.GetMoney() > TimestopperProgress.UpgradeCost)
                {
                    armInfoGold.transform.Find("Panel/Purchase Button/Text").GetComponent<TextMeshProUGUI>().text = (int)TimestopperProgress.UpgradeCost + " <color=#FF4343>P</color>";
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().failure = false;
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<Button>().interactable = true;
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<Image>().color = Color.white;
                }
                else
                {
                    armInfoGold.transform.Find("Panel/Purchase Button/Text").GetComponent<TextMeshProUGUI>().text = "<color=#FF4343>" + (int)TimestopperProgress.UpgradeCost + " P</color>";
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().failure = true;
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<Button>().interactable = false;
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<Image>().color = Color.red;
                }
            }
            else
            {
                armInfoGold.transform.Find("Panel/Purchase Button/Text").GetComponent<TextMeshProUGUI>().text = "<color=#FFEE43>MAX</color>";
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().failure = true;
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<Button>().interactable = false;
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<Image>().color = Color.gray;
            }
        }
        public void OnTriggerStay(Collider col)
        {
            if (col.gameObject.name == "Player" && Timestopper.TimeStop)
            {
                transform.Find("Canvas").gameObject.SetActive(false);
            }
        }
    }
    public class CanvasExcluder : MonoBehaviour
    {
        void OnEnable()
        {
            transform.parent.GetComponent<TerminalExcluder>()?.OverrideInfoMenu();
        }
    }

    public class Grayscaler : MonoBehaviour
    {
        private static readonly int MyCustomDepth = Shader.PropertyToID("_MyCustomDepth");
        private GameObject GrayscaleCube;
        public static Grayscaler Instance;
        public RenderTexture depth;
        public Camera depthCamera;
        public Camera mainCamera;
        public Material grayscaleMaterial;
        public float grayscaleBubbleExpansion = 0;
        public float intensityControl = 0;
        public void Awake()
        {
            Instance = this;
            mainCamera = GetComponent<Camera>();
            depth = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 24, RenderTextureFormat.RFloat);
            depth.Create();
            
            GameObject depthCamObj =  new GameObject("Depth Camera");
            depthCamObj.transform.parent = transform;
            depthCamObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            depthCamera = depthCamObj.AddComponent<Camera>();

            depthCamera.CopyFrom(mainCamera);
            depthCamera.enabled = false;
            depthCamera.targetTexture = depth;
            depthCamera.depthTextureMode = DepthTextureMode.Depth;
            depthCamera.aspect = mainCamera.aspect;
            depthCamera.SetReplacementShader(Timestopper.depthShader, "RenderType");

            grayscaleMaterial = new Material(Timestopper.grayscaleShader);
            
            GrayscaleCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(GrayscaleCube.GetComponent<BoxCollider>());
            GrayscaleCube.name = "Grayscale Cube";
            GrayscaleCube.transform.SetParent(transform);
            GrayscaleCube.transform.localRotation = Quaternion.Euler(0, 0, 0);
            GrayscaleCube.transform.localPosition = Vector3.forward * 0.61f;
            GrayscaleCube.transform.localScale = new Vector3(50, 50, 1);
            GrayscaleCube.GetComponent<MeshRenderer>().SetMaterials(new List<Material>(){grayscaleMaterial});
        }

        public void LateUpdate()
        {
            
            depthCamera.fieldOfView = mainCamera.fieldOfView;
            depthCamera.aspect = mainCamera.aspect;
            if (depth.width != mainCamera.pixelWidth || depth.height != mainCamera.pixelHeight)
            {
                depth.Release();
                depth = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 24, RenderTextureFormat.RFloat);
                depth.Create();
                depthCamera.targetTexture = depth;
            }
            depthCamera.Render();
            Shader.SetGlobalTexture("_MyCustomDepth", depth);
            // bubbleEffect;
            // overallEffectIntensity;
            // grayscaleIntensity;
            // bubbleSmoothness;
            // colorInversionArea;
            // skyTransitionTreshold;
            // bubbleDistance;
            // bubbleProgression;
            // grayscaleColorSpace;
            // grayscaleColorSpaceIntensity;
            grayscaleMaterial.SetFloat("_DoExpansion", Timestopper.bubbleEffect.value? 1 : 0);
            if (Timestopper.TimeStop)
            {
                if (intensityControl < 1) intensityControl += Timestopper.playerDeltaTime * Timestopper.stopSpeed.value;
                if (intensityControl > 1) intensityControl = 1;
            }
            else
            {
                if (intensityControl > 0) intensityControl -= Timestopper.playerDeltaTime * Timestopper.stopSpeed.value;
                if (intensityControl < 0) intensityControl = 0;
            }
            grayscaleMaterial.SetFloat("_AllIntensity", Timestopper.overallEffectIntensity.value * intensityControl);
            grayscaleMaterial.SetFloat("_Intensity", Timestopper.grayscaleIntensity.value);
            grayscaleMaterial.SetFloat("_Smoothness", Timestopper.bubbleSmoothness.value);
            grayscaleMaterial.SetFloat("_SmoothnessInvert", Timestopper.colorInversionArea.value);
            grayscaleMaterial.SetFloat("_NoDepthDistance", Timestopper.skyTransitionTreshold.value);
            grayscaleMaterial.SetFloat("_Distance", grayscaleBubbleExpansion);
            if (grayscaleBubbleExpansion < 20) grayscaleBubbleExpansion += Timestopper.playerDeltaTime * Timestopper.bubbleDistance.value;
            grayscaleMaterial.SetFloat("_Progression", Timestopper.bubbleProgression.value);
            grayscaleMaterial.SetVector("_ColorSpace", new Vector4(
                Timestopper.grayscaleColorSpace.value.r,
                Timestopper.grayscaleColorSpace.value.g,
                Timestopper.grayscaleColorSpace.value.b,
                Timestopper.grayscaleColorSpaceIntensity.value));
        }
    }



    // ############################################# PLAYER SCRIPTS ########################################## \\
    public class TerminalUpdater : MonoBehaviour
    {
        public int wid;
        private IEnumerator DelayedActivator(GameObject O, float delay)
        {
            yield return new WaitForSeconds(delay);
            O.SetActive(true);
        }
        public void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.GetComponent<ShopZone>() != null)
            {
                if (col.gameObject.GetComponent<TerminalExcluder>() == null)
                {
                    GameObject G = Timestopper.UpdateTerminal(col.gameObject.GetComponent<ShopZone>());
                    if (G != null)
                    {
                        StartCoroutine(DelayedActivator(G, 1.0f));
                    }
                }
                if (!col.gameObject.GetComponent<TerminalExcluder>().enabled)
                    return;
                if (col.transform.Find("Canvas/Background/Main Panel/Weapons/Arm Window") == null)
                    return;
                GameObject armWindow = col.transform.Find("Canvas/Background/Main Panel/Weapons/Arm Window").gameObject;
                GameObject armPanelGold = armWindow.transform.Find("Variation Screen/Variations/Arm Panel (Gold)").gameObject;
                armPanelGold.GetComponent<ShopButton>().failure = false;
                if (Timestopper.TimeStop)
                {
                    col.gameObject.GetComponent<ShopZone>().enabled = false;
                }
                else
                {
                    col.gameObject.GetComponent<ShopZone>().enabled = true;
                    Timestopper.LatestTerminal = col.gameObject;
                }
            }
        }
    }

    public class TimeArmPickup : MonoBehaviour
    {
        public UltrakillEvent onPickup;
        public void OnCollisionEnter(Collision col)
        {
            Timestopper.Log("Armitem Has Collided With " + col.gameObject.name, true);
            if (col.gameObject.GetComponent<Playerstopper>() != null)
            {
                TimestopperProgress.GiveArm();
                TimeArm.Instance.animator.Play("Pickup");
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(string.Format(Timestopper.ARM_PICKUP_MESSAGE, Timestopper.stopKey.value), "", "", 2);
                // gameObject.SetActive(false);
                onPickup?.Invoke();
                enabled = false;
            }
        }
    }

    public class PrivateInsideTimer
    {
        private float time;
        private bool scaled;
        public Action done;
        public bool SetTimer(float _time, bool _scaled, bool force = false)
        {
            if (time <= 0 || force)
            {
                time = _time;
                scaled = _scaled;
                return true;
            }
            return false;
        }
        public void Update()
        {
            if (time == 0)
                return;
            if (scaled)
                time -= Time.deltaTime;
            else
                time -= Time.unscaledDeltaTime;
            if (time < 0)
            {
                time = 0;
                done.Invoke();
            }
        }
    }

    public class PrivateTimer : MonoBehaviour
    {
        private float time;
        private bool scaled;
        public Action done;
        public bool SetTimer(float _time, bool _scaled, bool force = false)
        {
            if (time <= 0 || force)
            {
                time = _time;
                scaled = _scaled;
                return true;
            }
            return false;
        }
        public bool SetScaled(bool _scaled)
        {
            if (scaled == _scaled)
            {
                scaled = _scaled;
                return true;
            }
            return false;
        }
        private void Update()
        {
            if (time == 0)
                return;
            if (scaled)
                time -= Time.deltaTime;
            else
                time -= Time.unscaledDeltaTime;
            if (time < 0)
            {
                time = 0;
                done.Invoke();
            }
        }
    }
    public class RigidbodyStopper : MonoBehaviour    // Added to all Rigidbodies when time stops
    {
        private static HashSet<RigidbodyStopper> instances = new HashSet<RigidbodyStopper>();
        private static HashSet<GameObject> gos = new HashSet<GameObject>();
        public static HashSet<GameObject> HashedGameObjects { get{ return gos; } }

        public float localScale = 1.0f; // local timescale, so that coins and stuff freeze slowly
        bool gravity;
        bool frozen;
        bool byDio;
        bool isCoin;
        bool isRocket;
        bool isNail;
        bool isLandmine;
        private bool isChainsaw;
        private Coin coin;
        private SphereCollider sc;
        private BoxCollider bc;
        public EnemyIdentifier eid;
        private Grenade grenade;
        private GameObject freezeEffect;
        public Vector3 unscaledVelocity = new Vector3(0, 0, 0);
        public Vector3 unscaledAngularVelocity = new Vector3(0, 0, 0);
        public Rigidbody R;
        private float time;

        public static void FreezeAll()
        {
            foreach (var RS in instances) RS.Freeze();
        }

        public static void AddRBS(GameObject go)
        {
            if (gos.Add(go))
                go.AddComponent<RigidbodyStopper>();
        }

        public void Freeze()
        {
            isCoin = coin;
            if (isLandmine)
                transform.Find("Trigger").gameObject.SetActive(false);
            if (!R) R = gameObject.GetComponent<Rigidbody>();
            if (R)
            {
                if (!R.isKinematic)
                {
                    unscaledVelocity = R.velocity;
                    unscaledAngularVelocity = R.angularVelocity;
                    if (R.useGravity)
                    {
                        R.useGravity = false;
                        gravity = true;
                    }
                    frozen = true;
                }
            }
            else
            {
                enabled = false;
                return;
            }
            if (Timestopper.realTimeScale == 0.0f)
            {
                byDio = true;
            }
        }

        private void UnFreeze()
        {
            if (isLandmine)
                transform.Find("Trigger").gameObject.SetActive(true);
            if (isChainsaw)
                R.isKinematic = false;
            if (!R) R = gameObject.GetComponent<Rigidbody>();
            if (R)
            {
                if (!R.isKinematic)
                {
                    localScale = 1.0f;
                    R.velocity = unscaledVelocity;
                    R.angularVelocity = unscaledAngularVelocity;
                    if (gravity)
                        R.useGravity = true;
                    gravity = false;
                    frozen = false;
                }
            }
            else
            {
                enabled = false;
            }
        }

        private void Awake()
        {
            freezeEffect = transform.Find("FreezeEffect")?.gameObject;
            grenade = GetComponent<Grenade>();
            isChainsaw = GetComponent<Chainsaw>();
            coin = GetComponent<Coin>();
            sc = gameObject.GetComponent<SphereCollider>();
            bc = gameObject.GetComponent<BoxCollider>();
            eid = GetComponent<EnemyIdentifier>();
            if (transform.IsChildOf(Timestopper.Player.transform) || transform == Timestopper.Player.transform)
            {
                enabled = false;
                return;
            }
            if (GetComponent<Nail>() != null)
            {
                if (GetComponent<Nail>().sawblade || GetComponent<Nail>().chainsaw)
                {
                    if (GetComponent<FixedUpdateCaller>() == null)
                        gameObject.AddComponent<FixedUpdateCaller>();
                }
                isNail = true;
            }
            if (GetComponent<Landmine>() != null) isLandmine = true;
            if (gameObject.GetComponent<Coin>() != null) isCoin = true;
            if (GetComponent<Chainsaw>() != null)
                if (GetComponent<FixedUpdateCaller>() == null)
                    gameObject.AddComponent<FixedUpdateCaller>();
            if (gameObject.GetComponent<Grenade>() != null) 
                isRocket = gameObject.GetComponent<Grenade>().rocket;
            R = gameObject.GetComponent<Rigidbody>();
            instances.Add(this);
            if (Timestopper.TimeStop)
            {
                Freeze();
            }
        }

        public void OnDestroy()
        {
            instances.Remove(this);
            gos.Remove(gameObject);
        }

        public void Update()
        {
            if (Timestopper.TimeStop)
            {
                if (R != null && R.isKinematic)
                    localScale = 0.5f;
                if (!frozen)
                    Freeze();
                time += Time.unscaledDeltaTime;
                if (time > Time.maximumDeltaTime)
                    time = Time.maximumDeltaTime;
                while (time >= Time.fixedDeltaTime)
                {
                    time -= Time.fixedDeltaTime;
                    FakeFixedUpdate();
                }
            }
            else if (frozen)
                UnFreeze();
        }

        public void FakeFixedUpdate()
        {
            if (eid)
            {
                if (eid.hooked && ((List<EnemyType>)AccessTools.Field(typeof(HookArm), "lightEnemies").GetValue(MonoSingleton<HookArm>.Instance)).Contains(eid.enemyType))
                {
                    R.isKinematic = false;
                    localScale = 1;
                    unscaledVelocity = R.velocity;
                    return;
                }
            }
            if (isCoin)
            {
                sc.enabled = true;
                bc.enabled = true;
                coin.Invoke("StartCheckingSpeed", 0);
                isCoin = false;
            }
            if (Timestopper.TimeStop && R)
            {
                if (transform.parent && transform.parent.name == "GibZone" && localScale == 0 && R)
                {
                    return;
                }
                if (isChainsaw && localScale == 0)
                    R.isKinematic = true;
                if (R.isKinematic || !gameObject.activeInHierarchy)
                {
                    unscaledVelocity = Vector3.zero;
                    unscaledAngularVelocity = Vector3.zero;
                    return;
                }
                if (isRocket)
                {
                    R.isKinematic = !grenade.frozen;
                    grenade.rideable = true;
                    MethodInfo MFixedUpdate = grenade.GetType().GetMethod("FixedUpdate",
                                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    MFixedUpdate?.Invoke(grenade, null);
                    R.isKinematic = false;
                }
                if (!R.isKinematic)
                {
                    R.useGravity = false;
                    unscaledVelocity += R.GetAccumulatedForce(Time.fixedDeltaTime) * Time.fixedDeltaTime;
                    R.AddForce(-R.GetAccumulatedForce(Time.fixedDeltaTime));
                    if (gravity)
                        unscaledVelocity += R.velocity - (unscaledVelocity - Physics.gravity * Time.fixedDeltaTime) * localScale;
                    else
                        unscaledVelocity += R.velocity - (unscaledVelocity) * localScale;
                    unscaledAngularVelocity += R.angularVelocity - unscaledAngularVelocity * localScale;
                    R.velocity = unscaledVelocity * localScale;
                    R.angularVelocity = unscaledAngularVelocity * localScale;
                    if (!isRocket || !(MonoSingleton<WeaponCharges>.Instance && MonoSingleton<WeaponCharges>.Instance.rocketFrozen))
                    {
                        if (isNail)
                            localScale -= Timestopper.playerDeltaTime / Timestopper.stopSpeed.value * 32;
                        else if (byDio)
                            localScale -= Timestopper.playerDeltaTime / Timestopper.affectSpeed.value;
                        else
                            localScale -= Timestopper.playerDeltaTime / Timestopper.stopSpeed.value;
                        if (localScale < 0)
                            localScale = 0.0f;
                    }
                    else
                    {
                        MonoSingleton<WeaponCharges>.Instance.rocketFrozen = false;
                        grenade.rideable = true;
                        MethodInfo MFixedUpdate = grenade.GetType().GetMethod("FixedUpdate",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        if (MFixedUpdate != null) MFixedUpdate.Invoke(grenade, null);
                        freezeEffect.SetActive(true);
                        MonoSingleton<WeaponCharges>.Instance.rocketFrozen = true;
                        if (localScale < 1)
                            localScale += Timestopper.playerDeltaTime / Timestopper.stopSpeed.value;
                        if (localScale > 1)
                            localScale = 1.0f;
                    }
                }
            }
            else if (frozen && R)
            {
                UnFreeze();
            }
        }
    }
    public class FixedUpdateCaller : MonoBehaviour
    {
        private float time;
        public void Update()
        {
            if (Timestopper.TimeStop)
            {
                time += Timestopper.playerDeltaTime;
                Timestopper.UnscaleTimeSince = true;
                Timestopper.fixedCall = true;
                if (time > Time.maximumDeltaTime)
                    time = Time.maximumDeltaTime;
                while (time >= Time.fixedDeltaTime)
                {
                    time -= Time.fixedDeltaTime;
                    if (gameObject.GetComponent<Nailgun>() != null)
                    {
                        foreach (ScaleNFade C in gameObject.GetComponentsInChildren<ScaleNFade>())
                        {
                            Destroy(C.gameObject);
                        }
                    }
                    foreach (Component C in gameObject.GetComponents(typeof(MonoBehaviour)))
                    {
                        MethodInfo MFixedUpdate = C.GetType().GetMethod("FixedUpdate",
                                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        MFixedUpdate?.Invoke(C, null);
                    }

                }
                Timestopper.UnscaleTimeSince = true;
                Timestopper.fixedCall = false;
            }
        }
    }

    public class TimeHUD : MonoBehaviour
    {
        public static HashSet<TimeHUD> instances = new HashSet<TimeHUD>();
        public Color color = Timestopper.timeJuiceColorNormal.value;
        public int type;
        private Image _image;
        private Image _image1;
        private TextMeshProUGUI _textMeshProUGUI;

        private void Awake()
        {
            instances.Add(this);
            _textMeshProUGUI = transform.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            _image1 = transform.Find("Image")?.gameObject.GetComponent<Image>();
            _image = transform.Find("Image/Image (1)")?.gameObject.GetComponent<Image>();
        }

        private void OnDestroy()
        {
            instances.Remove(this);
        }

        public static void ReconsiderAll()
        {
            foreach (TimeHUD T in instances)
            {
                T.Reconsider();
            }
        }
        public void Reconsider()
        {
            if (TimestopperProgress.EquippedArm)
                gameObject.SetActive(true);
            else
                gameObject.SetActive(false);
        }
        public void Update()
        {
            if (TimeArm.Instance == null)
                return;
            if (type < 2)
            {
                if (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown)
                    color = Timestopper.timeJuiceColorNoCooldown.value;
                else if (TimeArm.Instance.localTimeStopTracker)
                    color = Timestopper.timeJuiceColorUsing.value;
                else if (TimeArm.Instance.timeLeft < Timestopper.lowerTreshold.value)
                    color = Timestopper.timeJuiceColorInsufficient.value;
                else
                    color = Timestopper.timeJuiceColorNormal.value;
            }
            if (TimestopperProgress.EquippedArm)
            {
                if (type == 0)
                {
                    if (HudController.Instance.altHud || HudController.Instance.colorless)
                        foreach (TimeHUD element in instances)
                            element.Reconsider();
                    // gameObject.SetActive(Time.timeSinceLevelLoad > 2.3f);
                    Color G = _image.color;
                    float F = _image1.fillAmount;
                    _image1.enabled = true;
                    _image.enabled = true;
                    _image.color
                        = (G * 5 + color) * (Time.unscaledDeltaTime) / (6 * Time.unscaledDeltaTime);
                    _image1.fillAmount
                        = (F * 8 + (TimeArm.Instance.timeLeft / TimestopperProgress.MaxTime)) * (Time.unscaledDeltaTime) / (9 * Time.unscaledDeltaTime);
                    if (HudController.Instance.weaponIcon.activeSelf) {
                        transform.localPosition = new Vector3(0f, 124.5f, 0f);
                        HudController.Instance.speedometer.gameObject.transform.localPosition = new Vector3(-520, 64 + 342, 45f);
                    } else {
                        transform.localPosition = new Vector3(0f, 24f, 0f);
                        HudController.Instance.speedometer.gameObject.transform.localPosition = new Vector3(-520, 64 - 58, 45f);
                    }
                }
                if (type == 1)
                {
                    if (!HudController.Instance.altHud || HudController.Instance.colorless) 
                        foreach (TimeHUD element in instances)
                            element.Reconsider();
                    
                    Color G = _textMeshProUGUI.color;
                    _textMeshProUGUI.text = (TimeArm.Instance.timeLeft).ToString(CultureInfo.CurrentCulture).Substring(0, Math.Min(4, (TimeArm.Instance.timeLeft).ToString(CultureInfo.CurrentCulture).Length));
                    _textMeshProUGUI.color = (G * 5 + color) * (Time.unscaledDeltaTime) / (6 * Time.unscaledDeltaTime);
                }
                else if (type == 2)
                {
                    if (!HudController.Instance.colorless) 
                        foreach (TimeHUD element in instances)
                            element.Reconsider();
                    _textMeshProUGUI.text = (TimeArm.Instance.timeLeft).ToString(CultureInfo.CurrentCulture).Substring(0, Math.Min(4, (TimeArm.Instance.timeLeft).ToString(CultureInfo.CurrentCulture).Length));
                }
            } else {
                if (HudController.Instance.weaponIcon.activeSelf)
                    HudController.Instance.speedometer.gameObject.transform.localPosition = new Vector3(-520, 342, 45f);
                else
                    HudController.Instance.speedometer.gameObject.transform.localPosition = new Vector3(-520, -58, 45f);
                gameObject.SetActive(false);
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
                    MonoSingleton<NewMovement>.Instance.walking = false;
                timeLeft += Time.deltaTime * Timestopper.refillMultiplier.value;
                if (timeLeft > TimestopperProgress.MaxTime)
                    timeLeft = TimestopperProgress.MaxTime;
            }
        }
        
        
        public void Update()
        {
            //decoration
            Vector3 newRot = new Vector3(0f, (0.3f * MonoSingleton<FistControl>.Instance.fistCooldown), (0.1f * MonoSingleton<FistControl>.Instance.fistCooldown));
            transform.localEulerAngles = (newRot * (20 * Timestopper.playerDeltaTime) + transform.localEulerAngles) / (1 + Timestopper.playerDeltaTime*20);
            //decoration end
            if (movement.dead && Timestopper.TimeStop && localTimeStopTracker)
            {
                Timestopper.StartTime(0);
                // gameObject.SetActive(false);
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
            Timestopper.Log("particle system updated: " + t.name, true, 2);
            ParticleSystem P = t.GetComponent<ParticleSystem>();
            Timestopper.FixedUpdateFix(t);
            if (P != null) P.gameObject.AddComponent<ParticleSystemUpdater>();
        }

        private bool oldPause;
        public void LateUpdate() // pause animations in pause menu
        {
            if (Timestopper.TimeStop && MonoSingleton<OptionsManager>.Instance.paused != oldPause)
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

    public class ParticleSystemUpdater : MonoBehaviour
    {

        public ParticleSystem PS;
        private void Awake()
        {
            PS = GetComponent<ParticleSystem>();
            if (PS == null) Destroy(this);
        }

        private void OnEnable()
        {
            UpdateState();
        }

        public void UpdateState()
        {
            if (PS == null) Destroy(this);
            var main = PS.main;
            main.useUnscaledTime = (Timestopper.playerDeltaTime > 0 && !MonoSingleton<OptionsManager>.Instance.paused);
        }
    }
    public class AnimatorUpdater : MonoBehaviour
    {
        public Animator animator;
        private bool shouldUseNormalMode = true;
        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null) Destroy(this);
            // animatorUpdaters.Add(this);
        }
        private void Update()
        {
            bool useNormal = Timestopper.playerTimeScale == 0.0f || 
                             MonoSingleton<OptionsManager>.Instance.paused || 
                             !Timestopper.TimeStop;

            if (useNormal == shouldUseNormalMode) return;
            shouldUseNormalMode = useNormal;
            animator.updateMode = useNormal ? AnimatorUpdateMode.Normal : AnimatorUpdateMode.UnscaledTime;
        }
    }
    public class Playerstopper : MonoBehaviour   // The other side of the magic
    {
        public static Playerstopper Instance;
        [FormerlySerializedAs("GoldArm")] public GameObject timeArm;
        public NewMovement movement;

        public IEnumerator LoadTimeArm()
        {
            Timestopper.Log("Golden Arm created successfully: " + timeArm, true);
            yield return Timestopper.newTimeArm;
            Timestopper.Log("Golden Arm created successfully: " + timeArm, true);
            yield return transform.Find("Main Camera/Punch");
            timeArm = Instantiate(Timestopper.newTimeArm, transform.Find("Main Camera/Punch"));
            Timestopper.Log("Golden Arm created successfully: " + timeArm, true);
        }

        public void EquipTimeArm()
        {
            if (timeArm == null) LoadTimeArm();
            timeArm.GetComponent<TimeArm>().Equip();
            TimeHUD.ReconsiderAll();
        }

        public void AddInvokeCallers(Transform t)
        {
            InvokeCaller.RegisterMonos(t.GetComponents<MonoBehaviour>());
            foreach (Transform T in t)
            {
                AddInvokeCallers(T);
            }
        }
        
        public void Awake()
        {
            AddInvokeCallers(transform);
            Instance = this;
            Timestopper.FixedUpdateFix(transform);
            StartCoroutine(LoadTimeArm());
            Timestopper.FixedUpdateFix(GameObject.Find("GameController").transform);
            transform.Find("Main Camera").GetOrAddComponent<Grayscaler>();
            movement = gameObject.GetComponent<NewMovement>();
        }

        private void AddTimestopComponents(GameObject go = null)
        {
            if (go is null)
            {
                foreach (GameObject target in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (!target.activeSelf) continue;
                    AddTimestopComponents(target);
                }
                return;
            }
            if (!RigidbodyStopper.HashedGameObjects.Contains(go))
            {
                if (go.GetComponent<Rigidbody>())
                    RigidbodyStopper.AddRBS(go.gameObject);
            }

            if (go == Timestopper.Player) return;
            if (!AudioPitcher.HashedGameObjects.Contains(go))
            {
                if (go.GetComponent<AudioSource>())
                    AudioPitcher.AddAP(go);
            }
            
            foreach (Transform t in go.transform)
            {
                if (!t.gameObject.activeSelf) continue;
                AddTimestopComponents(t.gameObject);
            }
        }

        private int oldGunsCount;
        private int oldPunchCount;
        private bool[] oldGunsState;
        private bool[] oldPunchState;

        private bool[] GetChildrenState(Transform t)
        {
            bool[] states = new bool[t.childCount];
            for (int i = 0; i < t.childCount; i++)
            {
                states[i] = t.GetChild(i).gameObject.activeSelf;
            }
            return states;
        }
        private void Update()
        {
            AddTimestopComponents();
            bool[] currentGunsState = GetChildrenState(GunControl.Instance.transform);
            bool[] currentPunchState = GetChildrenState(FistControl.Instance.transform);
            if (GunControl.Instance.transform.childCount != oldGunsCount || oldGunsState != currentGunsState)
            {
                InvokeCaller.ClearDestroyed();
                oldGunsCount = GunControl.Instance.transform.childCount;
                oldGunsState = currentGunsState;
                AddInvokeCallers(GunControl.Instance.transform);
            }
            if (FistControl.Instance.transform.childCount != oldPunchCount || oldPunchState != currentPunchState)
            {
                InvokeCaller.ClearDestroyed();
                oldPunchCount = FistControl.Instance.transform.childCount;
                oldPunchState = currentPunchState;
                AddInvokeCallers(FistControl.Instance.transform);
            }
            if (Time.timeSinceLevelLoad < 0.2f)
                return;
            // JumpReadyTimer.Update();
            // NotJumpingTimer.Update();
            if (Timestopper.specialMode.value)
            {
                if (UnityInput.Current.GetKeyDown(KeyCode.J))
                {
                    GameProgressSaver.AddMoney((int)TimestopperProgress.UpgradeCost);
                    Timestopper.Log("MONEY MONEY MONEY", false);
                }
            }
            // if (GoldArm == null) GoldArm = transform.Find("Main Camera/Punch/Arm Gold").gameObject;
            


            // if (Timestopper.TimeStop)
            // {
            //     if (movement.jumping && movement.falling && !movement.boost)
            //     {
            //         JumpReadyTimer.SetTimer(0.2f, false);
            //         NotJumpingTimer.SetTimer(0.25f, false);
            //     }
            // }
        }
        
    }

    public class WaitForPlayerSeconds : CustomYieldInstruction
    {
        private float second = 0;
        public override bool keepWaiting
        {
            get
            {
                second -= Timestopper.playerDeltaTime;
                return second > 0;
            }
        }

        public WaitForPlayerSeconds(float seconds)
        {
            // Timestopper.Log("waiting for " + seconds + "player seconds...", false, 4);
            second = seconds;
        }
    }


    // ################################################  PATCHWORK  ######################################################## \\
    /*                                DeltaTimeReplacer replaces the following code:                                         *\
     *                                Time.deltaTime -> Timestopper.playerDeltaTime                                          *
    \*                                Time.fixedDeltaTime -> Time.unscaledFixedDeltaTime                                     */
    public class DeltaTimeReplacer
    {
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
            
            
            var timeScaleG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.timeScale));
            var deltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
            var fixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime));
            var playerTimeScaleG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerTimeScale));
            var playerDeltaTimeG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerDeltaTime));
            var playerFixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerFixedDeltaTime));

            var waitForSecondsG = AccessTools.Constructor(typeof(WaitForSeconds), new Type[] { typeof(float) });
            var waitForPlayerSecondsG = AccessTools.Constructor(typeof(WaitForPlayerSeconds), new Type[] { typeof(float) });
                
            ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(Timestopper.Name);

            mls.LogWarning($"Transpiling " + name + "...");
            foreach (var i in instructions)
            {
                if (i.Calls(deltaTimeG))
                {
                    var newInst = new CodeInstruction(OpCodes.Call, playerDeltaTimeG);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                    //mls.LogInfo($"modified deltaTime");
                }
                else if (i.Calls(fixedDeltaTimeG))
                {
                    var newInst = new CodeInstruction(OpCodes.Call, playerFixedDeltaTimeG);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                    //mls.LogInfo($"modified fixedDeltaTime");
                }
                else if (i.Calls(timeScaleG))
                {
                    var newInst = new CodeInstruction(OpCodes.Call, playerTimeScaleG);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                    mls.LogInfo($"modified timeScale");
                }
                else if (i.opcode == OpCodes.Newobj && i.operand is ConstructorInfo ctor && ctor.DeclaringType == typeof(WaitForSeconds))
                {
                    var newInst = new CodeInstruction(OpCodes.Newobj, waitForPlayerSecondsG);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                    mls.LogInfo($"modified WaitForSeconds");
                }
                else if (i.Calls(smoothDampAngle4))
                {
                    // stack has: current, target, ref velocity, smoothTime
                    // push maxSpeed, deltaTime
                    yield return new CodeInstruction(OpCodes.Ldc_R4, float.PositiveInfinity);
                    yield return new CodeInstruction(OpCodes.Call, playerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, smoothDampAngle6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(smoothDampAngle5))
                {
                    // stack has: current, target, ref velocity, smoothTime, maxSpeed
                    // push deltaTime
                    yield return new CodeInstruction(OpCodes.Call, playerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, smoothDampAngle6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(smoothDamp4))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, float.PositiveInfinity);
                    yield return new CodeInstruction(OpCodes.Call, playerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, smoothDamp6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(smoothDamp5))
                {
                    yield return new CodeInstruction(OpCodes.Call, playerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, smoothDamp6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(vec3SmoothDamp4))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, float.PositiveInfinity);
                    yield return new CodeInstruction(OpCodes.Call, playerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, vec3SmoothDamp6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else if (i.Calls(vec3SmoothDamp5))
                {
                    yield return new CodeInstruction(OpCodes.Call, playerDeltaTimeG);
                    var newInst = new CodeInstruction(OpCodes.Call, vec3SmoothDamp6);
                    newInst.labels.AddRange(i.labels);
                    newInst.blocks.AddRange(i.blocks);
                    yield return newInst;
                }
                else
                {
                    yield return i;
                }
            }
        }
    }

    [HarmonyPatch]
    public class TranspileShotgunHammer4
    {
        static MethodBase TargetMethod()
        {
            // Get the ImpactRoutine method
            var impactRoutine = AccessTools.Method(typeof(ShotgunHammer), "ImpactRoutine");
        
            // Get the compiler-generated nested type
            var nestedType = impactRoutine.DeclaringType?.GetNestedTypes(BindingFlags.NonPublic)
                .FirstOrDefault(t => t.Name.Contains("ImpactRoutine"));
        
            // Get MoveNext from the nested type
            return AccessTools.Method(nestedType, "MoveNext");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return DeltaTimeReplacer.Transpiler(instructions, "ShotgunHammer.ImpactRoutine");
        }
    }
    
    [HarmonyPatch(typeof(PlayerTracker), nameof(PlayerTracker.GetPlayerVelocity))]
    public class PLayerTrackerPatch
    {
        static bool Prefix(ref Vector3 __result, bool trueVelocity)
        {
            __result = Timestopper.Instance.GetPlayerVelocity(trueVelocity);
            return false; // make sure you only skip if really necessary
        }
    }

    [HarmonyPatch(typeof(MonoBehaviour), nameof(MonoBehaviour.Invoke))]
    public class InvokePatch
    {
        static bool Prefix(MonoBehaviour __instance, string methodName, float time) {
            return InvokeCaller.Add(__instance, methodName, time);
            // Optionally prevent the original call
        }
    }
    
    [HarmonyPatch(typeof(NewMovement), "Parry")]
    public class ParryTimeFiller
    {
        [HarmonyPrefix]
        // ReSharper disable UnusedMember.Local
        static bool Prefix()
        {
            if (!Timestopper.TimeStop)
                TimeArm.Instance.timeLeft += Timestopper.bonusTimeForParry.value;
            return true;
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
    
    

    [HarmonyPatch(typeof(WeaponCharges), "Update")] public class TranspileWeaponCharges0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[WeaponCharges]=> Update"); }
    [HarmonyPatch(typeof(WeaponCharges), "Charge")] public class TranspileWeaponCharges1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[WeaponCharges]=> Charge"); }
    [HarmonyPatch(typeof(GunControl), "Update")] public class TranspileGunControl { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[GunControl]=> Update"); }
    [HarmonyPatch(typeof(Revolver), "Update")] public class TranspileRevolver0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Revolver]=> Update"); }
    [HarmonyPatch(typeof(Revolver), "LateUpdate")] public class TranspileRevolver1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Revolver]=> LateUpdate"); }
    [HarmonyPatch(typeof(ShotgunHammer), "UpdateMeter")] public class TranspileShotgunHammer0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> UpdateMeter"); }
    [HarmonyPatch(typeof(ShotgunHammer), "Update")] public class TranspileShotgunHammer1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> Update"); }
    [HarmonyPatch(typeof(ShotgunHammer), "LateUpdate")] public class TranspileShotgunHammer2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> LateUpdate"); }
    // [HarmonyPatch(typeof(ShotgunHammer), " ImpactRoutine")] public class TranspileShotgunHammer3 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> ImpactRoutine"); }
    [HarmonyPatch(typeof(Shotgun), "Update")] public class TranspileShotgun0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Shotgun]=> Update"); }
    [HarmonyPatch(typeof(Shotgun), "UpdateMeter")] public class TranspileShotgun1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Shotgun]=> UpdateMeter"); }
    [HarmonyPatch(typeof(Nailgun), "Update")] public class TranspileNailgun0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Nailgun]=> Update"); }
    [HarmonyPatch(typeof(Nailgun), "UpdateZapHud")] public class TranspileNailgun1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Nailgun]=> UpdateZpHud"); }
    [HarmonyPatch(typeof(Nailgun), "FixedUpdate")] public class TranspileNailgun2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Nailgun]=> FixedUpdate"); }
    [HarmonyPatch(typeof(Nailgun), "RefreshHeatSinkFill")] public class TranspileNailgun3 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Nailgun]=> RefreshHeatSinkFill"); }
    [HarmonyPatch(typeof(RocketLauncher), "Update")] public class TranspileRocketLauncher0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[RocketLauncher]=> Update"); }
    [HarmonyPatch(typeof(RocketLauncher), "FixedUpdate")] public class TranspileRocketLauncher1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[RocketLauncher]=> FixedUpdate"); }
    [HarmonyPatch(typeof(NewMovement), "Update")] public class TranspileNewMovement0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> Update"); }
    [HarmonyPatch(typeof(NewMovement), "FixedUpdate")] public class TranspileNewMovement1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> FixedUpdate"); }
    [HarmonyPatch(typeof(NewMovement), "Move")] public class TranspileNewMovement2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> Move"); }
    [HarmonyPatch(typeof(NewMovement), "Jump")] public class TranspileNewMovement3 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> Jump"); }
    [HarmonyPatch(typeof(NewMovement), "Dodge")] public class TranspileNewMovement4 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> Dodge"); }
    [HarmonyPatch(typeof(NewMovement), "TrySSJ")] public class TranspileNewMovement5 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> TrySSJ"); }
    [HarmonyPatch(typeof(NewMovement), "WallJump")] public class TranspileNewMovement6 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> WallJump"); }
    [HarmonyPatch(typeof(NewMovement), "CheckForGasoline")] public class TranspileNewMovement7 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> CheckForGasoline"); }
    [HarmonyPatch(typeof(NewMovement), "FrictionlessSlideParticle")] public class TranspileNewMovement8 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> FrictionlessSlideParticle"); }
    [HarmonyPatch(typeof(NewMovement), "DetachSlideScrape")] public class TranspileNewMovement9 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> DetachSlideScrape"); }
    [HarmonyPatch(typeof(FistControl), "Update")] public class TranspileFistControl { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[FistControl]=> Update"); }
    [HarmonyPatch(typeof(GroundCheck), "Update")] public class TranspileGroundCheck0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[GroundCheck]=> Update"); }
    [HarmonyPatch(typeof(GroundCheck), "FixedUpdate")] public class TranspileGroundCheck1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[GroundCheck]=> FixedUpdate"); }
    [HarmonyPatch(typeof(GroundCheck), MethodType.Constructor)] public class TranspileGroundCheck2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "Constructor<GroundCheck>"); }
    [HarmonyPatch(typeof(ClimbStep), "FixedUpdate")] public class TranspileClimbStep { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ClimbStep]=> FixedUpdate"); }
    [HarmonyPatch(typeof(CameraController), "LateUpdate")] public class TranspileCameraController { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[CameraController]=> Update"); }
    [HarmonyPatch(typeof(Punch), "Update")] public class TranspilePunch { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Punch]=> Update"); }
    [HarmonyPatch(typeof(WalkingBob), "Update")] public class TranspileWalkingBob { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[WalkingBob]=> Update"); }
    [HarmonyPatch(typeof(StaminaMeter), "Update")] public class TranspileStaminaMeter { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[StaminaMeter]=> Update"); }
    [HarmonyPatch(typeof(HealthBar), "Update")] public class TranspileHealthBar { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[HealthBar]=> Update"); }
    [HarmonyPatch(typeof(HurtZone), "FixedUpdate")] public class TranspileHurtZone { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[HurtZone]=> FixedUpdate"); }
    [HarmonyPatch(typeof(Grenade), "LateUpdate")] public class TranspileGrenade { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Grenade]=> LateUpdate"); }
    [HarmonyPatch(typeof(Chainsaw), "Update")] public class TranspileChainsaw { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Chainsaw]=> Update"); }
    [HarmonyPatch(typeof(HookArm), "Update")] public class TranspileHookArm { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[HookArm]=> Update"); }
    [HarmonyPatch(typeof(HookArm), "FixedUpdate")] public class TranspileHookArm1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[HookArm]=> FixedUpdate()"); }
    [HarmonyPatch(typeof(HookArm), "SemiBlockCheck")] public class TranspileHookArm2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[HookArm]=> SemiBlockCheck()"); }
    [HarmonyPatch(typeof(Sandbox.Arm.SandboxArm), "Update")] public class TranspileSandboxArm { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[SandboxArm]=> Update()"); }
    [HarmonyPatch(typeof(Spin), "FixedUpdate")] public class TranspileSpin { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Spin]=> FixedUpdate()"); }
    [HarmonyPatch(typeof(Spin), "LateUpdate")] public class TranspileSpin2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Spin]=> LateUpdate()"); }
    [HarmonyPatch(typeof(ScreenBlood), "Update")] public class TranspileScreenBlood { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ScreenBlood]=> Update()"); }
    [HarmonyPatch(typeof(SpriteController), "Awake")]
    class Patch
    {
        static void Postfix(SpriteController __instance)
        {
            //UnityEngine.Debug.Log("Unity, all awake is modified bro!");
            if (__instance.gameObject.layer != 0)
                __instance.gameObject.layer = 0;
            foreach (Transform child in __instance.GetComponentsInChildren<Transform>())
                child.gameObject.layer = 0;
        }
    }

    //====================[ PATCHWORK END ]======================\\


    



}