﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows;
using UnityEngine.Internal;
using UnityEngine.Rendering;
using UnityEngine.PlayerLoop;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using System.Reflection.Emit;
using System.Globalization;
using System.Collections;
using Configgy;
using BepInEx.Configuration;
using static System.Net.Mime.MediaTypeNames;
using Configgable.Assets;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Resources;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using BepInEx.Bootstrap;
using System.CodeDom;
using UnityEngine.Animations;
using System.Diagnostics;
using SettingsMenu.Components.Pages;
using SettingsMenu.Models;
using SettingsMenu.Components;
using System.Runtime.InteropServices;

namespace TimelessConfig //work in progress custom config library: Timeless Config
{
    [Serializable]
    public class Config<T>
    {
        public static List<object> All = new List<object>();

        public const int NO_OP = -1;
        public const int FAILURE = 0;
        public const int SUCCESS = 1;

        bool modified = false;  // if the value is modified, needed for when default values are updated
        bool locked = false;    // if the setting can be changed from the settings menu
        string _path;           // path of the setting in the menu
        string _name;           // name of the setting in the menu
        string _tooltip;        // tip of the setting in the menu
        Type _type;              // type of the config
        T _value;               // current value
        T _default;             // default value
        public Config(T defaultvalue, bool islocked = false, string path = "", string name = "", string tooltip = "") {
            _value = defaultvalue;
            _default = defaultvalue;
            _path = path;
            _name = name;
            _tooltip = tooltip;
            locked = islocked;
            _type = typeof(T);
            All.Add(this);
        }
        public void Restore() {     // return to default
            Value = _default;
        }
        public void Lock() { locked = true; }
        public void Unlock() { locked = false; }
        public int Modify(T newvalue) // use Value if you don't need feedback
        {
            if (_value.Equals(newvalue))
                return NO_OP;
            _value = newvalue;
            modified = !_value.Equals(_default);
            if (!_value.Equals(newvalue))
                return FAILURE;
            return SUCCESS;
        }
        public T Value { get { return _value; } set { modified = !_value.Equals(value); _value = value; } }
        public T Default { get { return _default; } }
        public Type type { get { return _type; } }
        public bool Modified { get { return modified; } }
        public string Name { get { return _name; } }
        public string Path { get { return _path; } }
        public string Tooltip { get { return _tooltip; } }

        public static implicit operator T(Config<T> entry) { return entry.Value; }
    }

    [Serializable]
    public class ConfigManager
    {
        ConfigManager(string ModGUID, string ModName)
        {
            GUID = ModGUID;
            Name = ModName;
        }
        void BuildAll()
        {

        }
        void TestFunction() //examples for how the config can be used with the library
        {
            Config<float> game = new Config<float>(0.5f, false, "games/game", "Game", "nothing is better than a little game!");
            game.Value = 0.5f;
        }
        string GUID;
        string Name;

    }
}

namespace The_Timestopper
{
    public enum TProgress { hasArm, equippedArm, firstWarning, upgradeCount, maxTime, upgradeText, upgradeCost };

    [Serializable]
    public class TimestopperProgress
    {
        public bool hasArm = false;
        public bool equippedArm = false;
        public bool firstWarning = false;
        public int upgradeCount = 0;
        public float maxTime = 3.0f;
        public int upgradeCost
        {
            get { return 150000 + upgradeCount * 66000; }
        }
        private const string PROGRESS_FILE = "timestopper.state";
        private static TimestopperProgress inst;
        private static string GenerateTextBar(char c, int b)
        {   string s = "";
            for (int i=0; i < b; i++)
                s += c;
            return s;
        }
        public static object ArmStatus(TProgress id)
        {   TimestopperProgress progress = Read();
            switch (id)
            {   case TProgress.hasArm:
                    return progress.hasArm;
                case TProgress.equippedArm:
                    return progress.equippedArm;
                case TProgress.firstWarning:
                    return progress.firstWarning;
                case TProgress.upgradeCount:
                    return progress.upgradeCount;
                case TProgress.maxTime:
                    return progress.maxTime;
                case TProgress.upgradeText:
                    return (string)"<align=\"center\"><color=#FFFF42>" + GenerateTextBar('▮', progress.upgradeCount) + "</color>";
                case TProgress.upgradeCost:
                    return 150000 + progress.upgradeCount*66000;
                default:
                    return null;
            }
        }
        public static void UpgradeArm()
        {   TimestopperProgress progress = Read();
            GameProgressSaver.AddMoney(-progress.upgradeCost);
            progress.maxTime += 1 + 1 / (progress.upgradeCount + 0.5f);
            progress.upgradeCount++;
            Write(progress);
        }
        public static void ForceDownngradeArm()
        {   TimestopperProgress progress = Read();
            while (progress.upgradeCount > Timestopper.maxUpgrades.Value)
            {
                progress.upgradeCount--;
                progress.maxTime -= 1 + 1 / (progress.upgradeCount + 0.5f);
            }
            Write(progress);
        }
        public static void AceptWarning()
        {   TimestopperProgress progress = Read();
            progress.firstWarning = true;
            Write(progress);

        }
        public static void GiveArm()
        {   TimestopperProgress progress = Read();
            progress.hasArm = true;
            progress.equippedArm = true;
            Timestopper.Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.SetActive(true);
            Timestopper.Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.GetComponent<Animator>().Play("Pickup");
            Timestopper.mls.LogInfo("Received Golden Arm");
            Write(progress);
        }
        public static void ChangeEquipmentStatus()
        {   Timestopper.mls.LogWarning("Dum dum gumgineashi");
            if (Timestopper.LatestTerminal != null)
                EquipArm(Timestopper.LatestTerminal.transform.Find("Canvas/Background/Main Panel/Weapons/" +
              "Arm Window/Variation Screen/Variations/Arm Panel (Gold)/Equipment/Equipment Status/Text (TMP)").GetComponent<TextMeshProUGUI>().text[0] == 'E');
            else
                Timestopper.mls.LogWarning("LatestTerminal is Null!");
        }
        public static void EquipArm(bool equipped)
        {   TimestopperProgress progress = Read();
            if (Timestopper.GoldArm == null)
                return;
            if (progress.hasArm)
            {
                progress.equippedArm = equipped;
                Timestopper.GoldArm.SetActive(equipped);
                Timestopper.mls.LogInfo("Gold Arm Equipment Status changed: " + progress.equippedArm.ToString());
            }
            else
                Timestopper.mls.LogError("Invalid request of arm equipment, user doesn't have the arm!");
            Write(progress);
        }
        public static TimestopperProgress Read()
        {   try
            {   string filePath = Path.Combine(GameProgressSaver.SavePath, PROGRESS_FILE);
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    inst = JsonUtility.FromJson<TimestopperProgress>(jsonData);
                }
                else
                {
                    inst = new TimestopperProgress();
                }                                               }
            catch (Exception e)
            {   Timestopper.mls.LogError($"Failed to read progress: {e.Message}, resetting save file {GameProgressSaver.currentSlot}");
                inst = new TimestopperProgress();
                Write(inst);                            }
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
        public const string GUID = "TheTimestopper";
        public const string Name = "The Timestopper";
        public const string Version = "0.9.13";

        private readonly Harmony harmony = new Harmony(GUID);
        public static Timestopper Instance;

        private ConfigBuilder config;
        public static ManualLogSource mls;
        public const string ARM_DESCRIPTION = @"A Godfist that <color=#FFFF43>stops</color> time.

<color=#FF4343>Punch</color> to stop time, and <color=#FF4343>punch again</color> to start it.

Takes time to recharge, can be upgraded through the terminals.
";
        public const string ARM_NEW_MESSAGE = "Somewhere in the depths of <color=#FF0000>Violence /// First</color>, a new <color=#FFFF23>golden</color> door appears";
        public const string TIMESTOP_STYLE = "<color=#FFCF21>TIME STOP</color>";

        // %%%%%%%%%%%%%%%%%% ASSETS %%%%%%%%%%%%%%%%%%%%%%%% \\
        public static Shader grayscaleShader;
        public static Shader slappShader;
        public static Camera HUDCamera;
        public static RenderTexture HUDRender;
        public AudioClip[] TimestopSounds = new AudioClip[4] { null, null, null, null };
        public AudioClip[] StoppedTimeAmbiences = new AudioClip[4] { null, null, null, null };
        public AudioClip[] TimestartSounds = new AudioClip[4] { null, null, null, null };
        public static Texture2D armGoldLogo;
        public static Texture2D armGoldColor;
        public static GameObject armGoldObj;
        public static Animator armGoldAnimator = new Animator();
        public static RuntimeAnimatorController armGoldAC;
        public AssetBundle bundle = null;

        // vvvvvvvvvvvvv REFERENCES vvvvvvvvvvvvvvvvvvvvvv\\
        private FistControl Fist;
        private GameObject[] TimeHUD = new GameObject[3];
        public static GameObject Player;
        public static GameObject GoldArm;
        public static GameObject musicManager;
        public static GameObject LatestTerminal;
        public static GameObject TheCube;
        public static GameObject MenuCanvas;

        // ###############  CLOCKWORK VARIABLES  ############### \\
        public static bool TimeStop = false;
        public static bool StopTimeStop = false;
        public static float TimeLeft = 0.0f;
        public static float StoppedTimeAmount = 0.0f;
        public static Color TimeColor = new Color(1, 1, 0, 1);
        public static bool terminalUpdate = false;
        public static bool LoadDone = false;
        public static bool LoadStarted = false;
        public static float realTimeScale = 1.0f;
        public static float playerTimeScale = 1.0f;
        public static bool fixedCall = false;
        public static bool parryWindow = false;
        public static bool firstLoad = true;
        public static bool cybergrind = false;
        public static int cybergrindWave = 0;
        public static bool UnscaleTimeSince = false;
        public static PrivateInsideTimer messageTimer = new PrivateInsideTimer();
        private GameObject currentLevelInfo = null;

        public static float playerDeltaTime {
            get { 
                if (fixedCall) return Time.fixedDeltaTime; 
                else if (TimeStop) return Time.unscaledDeltaTime * playerTimeScale;
                else return Time.deltaTime; }
        }
        public static float playerFixedDeltaTime { 
            get { 
                if(fixedCall) return Time.fixedDeltaTime;
                else if (TimeStop) return Time.fixedDeltaTime * playerTimeScale;
                else return Time.fixedDeltaTime; }
        }
        public static ULTRAKILL.Cheats.BlindEnemies BlindCheat = new ULTRAKILL.Cheats.BlindEnemies();
        //________________________ COROUTINES __________________________\\
        private IEnumerator timeStopper;
        private IEnumerator timeStarter;
        // _______________ COMPATBILITY WITH OTHER MODS _________________\\
        public static bool Compatability_JukeBox = false;

        //$$$$$$$$$$$$$$$$$$$$$$$$$ CONFIG FILES $$$$$$$$$$$$$$$$$$$$$$$$$$$$$\\
        public enum SoundTypeA { None, Classic, Alternate, Za_Warudo };
        public enum SoundTypeB { None, Classic, Alternate, Ambience };
        public enum SoundTypeC { None, Classic, Alternate };
        public static ConfigEntry<KeyCode> stopKey;
        public static ConfigEntry<SoundTypeA> stopSound;
        public static ConfigEntry<SoundTypeB> stoppedSound;
        public static ConfigEntry<SoundTypeC> startSound;
        public static ConfigEntry<float> stopSpeed;
        public static ConfigEntry<float> startSpeed;
        public static ConfigEntry<float> affectSpeed;
        public static ConfigEntry<float> animationSpeed;
        public static ConfigEntry<float> soundEffectVolume;
        public static ConfigEntry<bool> filterMusic;
        public static ConfigEntry<float> stoppedMusicPitch;
        public static ConfigEntry<float> stoppedMusicVolume;
        public static ConfigEntry<bool> grayscale;
        public static ConfigEntry<float> grayscaleAmount;
        public static ConfigEntry<bool> exclusiveGrayscale;
        public static ConfigEntry<bool> healInTimestop;
        public static ConfigEntry<int> maxUpgrades;
        public static ConfigEntry<bool> forceDowngrade;
        public static ConfigEntry<bool> specialMode;
        //---------------------technical stuff--------------------\\
        public static ConfigEntry<float> lowerTreshold; //2.0f
        public static ConfigEntry<float> blindScale; //0.4f
        public static ConfigEntry<float> refillMultiplier; //0.12f
        public static ConfigEntry<float> bonusTimeForParry;
        public static ConfigEntry<float> antiHpMultiplier;



        void InitializeConfig()
        {
            if (config == null)
            {
                //******NORMAL SETTINGS******\\
                stopKey = Config.Bind<KeyCode>("", "Timestopper Key", KeyCode.V);
                stopSound = Config.Bind<SoundTypeA>("", "Timestop Sound", SoundTypeA.Za_Warudo);
                stoppedSound = Config.Bind<SoundTypeB>("", "Stooped Time Ambience", SoundTypeB.Classic);
                startSound = Config.Bind<SoundTypeC>("", "Timestart Sound", SoundTypeC.Classic);
                stopSpeed = Config.Bind<float>("", "Slowdown Multiplier", 0.6f, "How many seconds it takes for time to stop, set to 0 for instant timestop.");
                startSpeed = Config.Bind<float>("", "Speedup Multiplier", 0.8f, "How many seconds it takes for time to start, set to 0 for instant timestart.");
                affectSpeed = Config.Bind<float>("", "Interaction Slowdown Multiplier", 1f, "How many seconds it takes for player interactions to stop in time (coins tossed in stopped time, for example), set to zero for no timestop interaction.");
                animationSpeed = Config.Bind<float>("", "Animation Speed Multiplier", 1.3f, "How fast the Timestopper's animation plays");
                soundEffectVolume = Config.Bind<float>("", "Sound Effect Volume", 1f, "How loud the timestop and timestart sound effects are.");
                grayscale = Config.Bind<bool>("", "Grayscale", true, "Wheter to apply grayscale effect when time is stopped or not, setting this to false may increase performance.");
                grayscaleAmount = Config.Bind<float>("", "Grayscale Amount", 1.0f, "Amount of grayscale the screen gets when time is stopped. You are free to change it to ANY number you want. (between 0-1 is intended)");
                exclusiveGrayscale = Config.Bind<bool>("", "Exclusive Grayscale", true, "Turn screen grayscale only when timestop effect is on play. When false, screen grayscale is applied any time when time stops (main menu, parry, impact frames, etc.), otherwise only when the Timestopper is used.");
                filterMusic = Config.Bind<bool>("", "Filter Music", false, "Filter music when time is stopped just like in the menu");
                stoppedMusicPitch = Config.Bind<float>("", "Stopped time Music Pitch", 0.6f, "Pitch of the music when time is stopped, set to 0 to stop the music");
                stoppedMusicVolume = Config.Bind<float>("", "Stopped time Music Volume", 0.8f, "Volume of the music when time is stopped, set to 0 to stop the music");
                healInTimestop = Config.Bind<bool>("", "Heal in Stopped Time", false, "Wether Player can heal in stopped time or not.");
                maxUpgrades = Config.Bind<int>("", "Maximum Upgrades", 10, "Maximum number of upgrades the Timestopper can get.");
                forceDowngrade = Config.Bind<bool>("", "Force Downgrade", false, "If the arm is upgraded more than maximum upgrade count (by cheating, or due to different save file), downgrade the arm to the maximum upgrade automatically.");
                specialMode = Config.Bind<bool>("", "Special Mode", false, "Try and see          >:D) ");
                //*******TECHNICAL********\\
                lowerTreshold = Config.Bind<float>("{TECHNICAL STUFF}", "! LowerTreshold !", 2.0f, "Minimum time juice you need to have to do timestop in Seconds");
                blindScale = Config.Bind<float>("{TECHNICAL STUFF}", "! BlindScale !", 0.2f, "Timescale where enemies can't see you.");
                refillMultiplier = Config.Bind<float>("{TECHNICAL STUFF}", "! RefillMultiplier !", 0.09f, "Time juice amount you get per second.");
                antiHpMultiplier = Config.Bind<float>("{TECHNICAL STUFF}", "! AntiHPMultiplier !", 20, "How fast the hard damage per second builds in stopped time.");
                bonusTimeForParry = Config.Bind<float>("{TECHNICAL STUFF}", "! BonusTimeJuicePerParry !", 1, "How much time juice does parrying outside stopped time refill.");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    grayscale.Value = false;
                }

                config = new ConfigBuilder(GUID, Name);
                config.BuildAll();
            }
        }
        void InitializeShaders()
        {
            //.....................SHADERWORK..........................\\
            HUDRender = new RenderTexture(Screen.width, Screen.height, 0);
            HUDRender.depth = 0;
            HUDRender.Create();
            Player = GameObject.Find("Player");
            if (Player != null)
                HUDCamera = Player.transform.Find("Main Camera/HUD Camera").GetComponent<Camera>();
            if (HUDCamera != null)
                HUDCamera.targetTexture = HUDRender;
        }
        public IEnumerator LoadBundle()
        {
            LoadDone = false;
            LoadStarted = true;
            var assembler = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembler.GetManifestResourceNames();
            mls.LogInfo("Scanning embedded resources: " + string.Join(", ", resourceNames));
            using (var stream = assembler.GetManifestResourceStream("The_Timestopper.timestopper.bundle"))
            {
                if (bundle == null)
                    bundle = AssetBundle.LoadFromStream(stream);
                else
                    mls.LogInfo("Bundle is already loaded!");
                if (bundle == null){
                    mls.LogError("AssetBundle failed to load!");
                    yield break;    }
                mls.LogInfo(">Assets in bundle:");
                foreach (string assetName in bundle.GetAllAssetNames())
                {
                    mls.LogInfo("-->" + assetName);
                }
                do  // Beat the shit out of resources until they are done loading
                {
                    grayscaleShader = bundle.LoadAsset<Shader>("assets/bundledassets/grayscale.shader");
                    slappShader = bundle.LoadAsset<Shader>("assets/bundledassets/slapp.shader");
                    TimestopSounds[(int)SoundTypeA.Classic] = bundle.LoadAsset<AudioClip>("assets/bundledassets/timestop-classic.mp3");
                    TimestopSounds[(int)SoundTypeA.Alternate] = bundle.LoadAsset<AudioClip>("assets/bundledassets/timestop-alternate.mp3");
                    TimestopSounds[(int)SoundTypeA.Za_Warudo] = bundle.LoadAsset<AudioClip>("assets/bundledassets/timestop-zawarudo.mp3");
                    StoppedTimeAmbiences[(int)SoundTypeB.Classic] = bundle.LoadAsset<AudioClip>("assets/bundledassets/timejuice-classic.mp3");
                    StoppedTimeAmbiences[(int)SoundTypeB.Alternate] = bundle.LoadAsset<AudioClip>("assets/bundledassets/timejuice-alternate.mp3");
                    StoppedTimeAmbiences[(int)SoundTypeB.Ambience] = bundle.LoadAsset<AudioClip>("assets/bundledassets/timejuice-ambience.mp3");
                    TimestartSounds[(int)SoundTypeC.Classic] = bundle.LoadAsset<AudioClip>("assets/bundledassets/timestart-classic.mp3");
                    TimestartSounds[(int)SoundTypeC.Alternate] = bundle.LoadAsset<AudioClip>("assets/bundledassets/timestart-alternate.mp3");
                    armGoldLogo = bundle.LoadAsset<Texture2D>("assets/bundledassets/timestopper.png");
                    armGoldObj = bundle.LoadAsset<GameObject>("assets/bundledassets/timestopper.fbx");
                    armGoldColor = bundle.LoadAsset<Texture2D>("assets/bundledassets/goldarmcolor.fbx");
                    armGoldAC = bundle.LoadAsset<RuntimeAnimatorController>("assets/bundledassets/ac.controller");
                    yield return null;
                } while (grayscaleShader == null);
                bundle.Unload(false);       //Release the bundle so it doesn't cause leakage?
                mls.LogInfo("Asset extraction status:");
            }
            mls.LogInfo("Bundle extraction done!");
            if (Player == null) {
                mls.LogError("Player is null, shaders cannot apply!");
                LoadDone = true;
                LoadStarted = false;
                yield break;        }
            if (grayscaleShader.isSupported)
            {
                Camera c = Player.transform.Find("Main Camera/Virtual Camera").GetComponent<Camera>();
                if (c.gameObject.GetComponent<Grayscaler>() == null)
                    c.gameObject.AddComponent<Grayscaler>();
                c.gameObject.GetComponent<Grayscaler>().DoIt();
                InitializeShaders();
            }
            TimeColor = new Color(1, 1, 0, 1);
            TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
            LoadDone = true;
            LoadStarted = false;
        }
        public IEnumerator EnsureShader()
        {
            mls.LogInfo("Ensuring shaders load...");
            do
            {
                if (Player == null || grayscaleShader == null)
                    yield return null;
                if (!grayscaleShader.isSupported)
                {
                    mls.LogWarning("grayscaleShaders are not compatible, skipping grayscale effect!");
                    break;
                }
                Camera c = Player.transform.Find("Main Camera/Virtual Camera").GetComponent<Camera>();
                if (c.gameObject.GetComponent<Grayscaler>() == null)
                    c.gameObject.AddComponent<Grayscaler>();
                c.gameObject.GetComponent<Grayscaler>().DoIt();
            } while (Player == null || grayscaleShader == null);
            mls.LogInfo("Shaders have loaded into the player!");
        }
        public static GameObject UpdateTerminal(ShopZone ShopComp)
        {
            if (ShopComp == null) {
                mls.LogError("Shop Component is null, cannot update terminal!");
                return null; }
            GameObject Shop = ShopComp.gameObject;
            if (Shop.transform.Find("Canvas/Background/Main Panel/Weapons/Arm Window") == null)
            {
                ShopComp.gameObject.AddComponent<TerminalExcluder>();
                return null;
            }
            GameObject armWindow = Shop.transform.Find("Canvas/Background/Main Panel/Weapons/Arm Window").gameObject;
            GameObject armPanelGold = armWindow.transform.Find("Variation Screen/Variations/Arm Panel (Gold)").gameObject;
            GameObject armInfoGold = armWindow.transform.Find("Arm Info (Gold)").gameObject;
            if ((bool)TimestopperProgress.ArmStatus(TProgress.hasArm))
            {
                ShopComp.gameObject.AddComponent<TerminalExcluder>();
                armPanelGold.GetComponent<ShopButton>().toActivate = new GameObject[] { armInfoGold };
                armPanelGold.transform.Find("Variation Name").GetComponent<TMPro.TextMeshProUGUI>().text = "TIMESTOPPER";
                armPanelGold.GetComponent<VariationInfo>().enabled = true;
                armPanelGold.GetComponent<VariationInfo>().alreadyOwned = true;
                armPanelGold.GetComponent<VariationInfo>().varPage = armInfoGold;
                armPanelGold.GetComponent<VariationInfo>().weaponName = "arm4";
                armPanelGold.GetComponent<ShopButton>().PointerClickSuccess += Shop.GetComponent<TerminalExcluder>().OverrideInfoMenu;
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.UpgradeArm;
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().PointerClickSuccess += Shop.GetComponent<TerminalExcluder>().OverrideInfoMenu;
                armPanelGold.GetComponent<VariationInfo>().cost = (int)TimestopperProgress.ArmStatus(TProgress.upgradeCost);
                armPanelGold.transform.Find("Equipment/Equipment Status").GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.ChangeEquipmentStatus;
                armPanelGold.transform.Find("Equipment/Buttons/Previous Button").GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.ChangeEquipmentStatus;
                armPanelGold.transform.Find("Equipment/Buttons/Next Button").GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.ChangeEquipmentStatus;
                Sprite mm = Sprite.Create(armGoldLogo, new Rect(0, 0, 512, 512), new Vector2(256, 256));
                armPanelGold.transform.Find("Weapon Icon").GetComponent<UnityEngine.UI.Image>().sprite = mm;
                armInfoGold.transform.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = "Timestopper";
                armInfoGold.transform.Find("Panel/Name").GetComponent<TMPro.TextMeshProUGUI>().text = "TIMESTOPPER";
                armInfoGold.transform.Find("Panel/Description").GetComponent<TMPro.TextMeshProUGUI>().text = ARM_DESCRIPTION + (string)TimestopperProgress.ArmStatus(TProgress.upgradeText);
                Sprite nn = Sprite.Create(armGoldLogo, new Rect(0, 0, 512, 512), new Vector2(256, 256));
                armInfoGold.transform.Find("Panel/Icon Inset/Icon").GetComponent<UnityEngine.UI.Image>().sprite = nn;
                if (!(bool)TimestopperProgress.ArmStatus(TProgress.firstWarning))
                {
                    GameObject firstWarning = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/The Cyber Grind/Cyber Grind Panel").gameObject,
                                                                Shop.transform.Find("Canvas/Background/Main Panel"));
                    firstWarning.name = "Warning Panel";
                    firstWarning.transform.localPosition = new Vector3(-9.4417f, 6.0f, 0.0002f);
                    firstWarning.transform.Find("Button 1").gameObject.SetActive(false);
                    firstWarning.transform.Find("Button 2").gameObject.SetActive(false);
                    firstWarning.transform.Find("Button 3").gameObject.SetActive(false);
                    firstWarning.transform.Find("Icon"    ).gameObject.SetActive(false);
                    firstWarning.transform.Find("Panel/Text Inset/Text").GetComponent<TextMeshProUGUI>().text = @"<color=#FF4343>!!! Extreme Hazard Detected !!!</color> 

You have <color=#FF4343>The Timestopper</color> in your possession. Using this item may cause disturbance in space-time continuum.

<color=#FF4343>Please acknowledge the consequences before proceeding further.</color>";
                    GameObject iconR = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/Enemies/Enemies Panel/Icon").gameObject, firstWarning.transform.Find("Title"));
                    GameObject iconL = Instantiate(Shop.transform.Find("Canvas/Background/Main Panel/Enemies/Enemies Panel/Icon").gameObject, firstWarning.transform.Find("Title"));
                    iconL.transform.localPosition = new Vector3(-37.1206f, -0.0031f, 0);
                    iconR.transform.localPosition = new Vector3(97.8522f , -0.0031f, 0);
                    firstWarning.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "WARNING";
                    firstWarning.transform.Find("Title").GetComponent<TextMeshProUGUI>().transform.localPosition = new Vector3(-51.5847f, 189.9997f, 0);
                    GameObject button = firstWarning.transform.Find("Panel/Enter Button").gameObject;
                    button.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "ACCEPT";
                    button.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 1, 0, 1);
                    button.name = "Accept Button";
                    Destroy(button.GetComponent<AbruptLevelChanger>());
                    firstWarning.SetActive(false);
                    Shop.transform.Find("Canvas/Background/Main Panel/Main Menu").gameObject.SetActive(false);
                    Shop.transform.Find("Canvas/Background/Main Panel/Tip of the Day").gameObject.SetActive(false);
                    button.GetComponent<ShopButton>().PointerClickSuccess += TimestopperProgress.AceptWarning;
                    button.GetComponent<ShopButton>().toDeactivate = new GameObject[] { firstWarning };
                    button.GetComponent<ShopButton>().toActivate = new GameObject[] {
                                            Shop.transform.Find("Canvas/Background/Main Panel/Tip of the Day").gameObject,
                                            Shop.transform.Find("Canvas/Background/Main Panel/Main Menu").gameObject
                                            };
                    return firstWarning;
                }
            } else
            {
                armPanelGold.GetComponent<VariationInfo>().enabled = true;
                armPanelGold.GetComponent<VariationInfo>().alreadyOwned = false;
                armPanelGold.GetComponent<VariationInfo>().varPage = armInfoGold;
            }
            return null;
        }
        public void PlayRespectiveSound(bool istimestopped)
        {
            if (Player.transform.Find("Main Camera").GetComponent<AudioSource>() == null) {
                Player.transform.Find("Main Camera").gameObject.AddComponent<AudioSource>();
            }
            if (istimestopped != TimeStop)
                if (istimestopped)
                {
                    if (TimestopSounds[(int)stopSound.Value] != null)
                        Player.transform.Find("Main Camera").GetComponent<AudioSource>().PlayOneShot(TimestopSounds[(int)stopSound.Value]);
                    if (StoppedTimeAmbiences[(int)stoppedSound.Value] != null)
                    {
                        Player.transform.Find("Main Camera").GetComponent<AudioSource>().clip = StoppedTimeAmbiences[(int)stoppedSound.Value];
                        Player.transform.Find("Main Camera").GetComponent<AudioSource>().Play();
                        Player.transform.Find("Main Camera").GetComponent<AudioSource>().loop = true;
                    }
                } else
                {
                    Player.transform.Find("Main Camera").GetComponent<AudioSource>().Stop();
                    if (TimestartSounds[(int)startSound.Value] != null)
                        Player.transform.Find("Main Camera").GetComponent<AudioSource>().PlayOneShot(TimestartSounds[(int)startSound.Value]);
                }
        }
        public void PreventNull()
        {
            if (Player == null)  //Never lose track of the player, NEVER!
            {
                Player = GameObject.Find("Player");
                if (Player == null) return;
                if (Player.GetComponent<Playerstopper>() == null) Player.AddComponent<Playerstopper>();
                Fist = Player.transform.Find("Main Camera").GetComponentInChildren<FistControl>();
            }
            if (musicManager == null)
                musicManager = GameObject.Find("MusicManager");
            if (Player.GetComponent<TerminalUpdater>() == null)
            {
                Player.AddComponent<TerminalUpdater>();
            }
            if (MenuCanvas == null)
            {
                MenuCanvas = FindRootGameObject("Canvas");
            }
        }
        public IEnumerator LoadHUD()
        {
            float elapsedTime = 0;
            mls.LogInfo("Loading HUD Elements...");
            do
            {
                if (elapsedTime > 5)
                {
                    mls.LogError("Golden Time Bar creation failed after 5 seconds!");
                    yield break;
                }
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            } while (Player.transform.Find("Main Camera/HUD Camera/HUD/GunCanvas/StatsPanel/Filler/AltRailcannonPanel") == null);
            TimeHUD[0] = Instantiate(Player.transform.Find("Main Camera/HUD Camera/HUD/GunCanvas/StatsPanel/Filler/AltRailcannonPanel").gameObject,
                        Player.transform.Find("Main Camera/HUD Camera/HUD/GunCanvas/StatsPanel/Filler"));
            TimeHUD[0].SetActive(true);
            TimeHUD[0].name = "Golden Time";
            TimeHUD[0].transform.localPosition = new Vector3(0f, 124.5f, 0f);
            TimeHUD[0].transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount = 0;
            TimeHUD[0].transform.Find("Image/Image (1)").gameObject.GetComponent<UnityEngine.UI.Image>().color = TimeColor;
            Sprite mm = Sprite.Create(armGoldLogo, new Rect(0, 0, 512, 512), new Vector2(256, 256));
            TimeHUD[0].transform.Find("Icon").gameObject.GetComponent<UnityEngine.UI.Image>().sprite = mm;
            HudController.Instance.speedometer.gameObject.transform.localPosition += new Vector3(0, 64, 0);
            mls.LogInfo("Golden Time Bar created successfully.");
            do
            {
                if (elapsedTime > 5)
                {
                    mls.LogError("Golden Time Alt HUD creation failed after 5 seconds!");
                    yield break;
                }
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            } while (FindRootGameObject("Canvas")?.transform.Find("Crosshair Filler/AltHud/Filler/Speedometer") == null);
            TimeHUD[1] = Instantiate(FindRootGameObject("Canvas").transform.Find("Crosshair Filler/AltHud/Filler/Speedometer").gameObject,
                                            FindRootGameObject("Canvas").transform.Find("Crosshair Filler/AltHud/Filler"));
            TimeHUD[2] = Instantiate(FindRootGameObject("Canvas").transform.Find("Crosshair Filler/AltHud (2)/Filler/Speedometer").gameObject,
                                        FindRootGameObject("Canvas").transform.Find("Crosshair Filler/AltHud (2)/Filler"));

            TimeHUD[1].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = new Color(1, 0.9f, 0.2f);
            TimeHUD[1].transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "TIME";
            TimeHUD[1].transform.localPosition = new Vector3(360, -360, 0);
            Destroy(TimeHUD[1].GetComponent<Speedometer>());
            TimeHUD[1].name = "Time Juice";
            TimeHUD[2].transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "TIME";
            TimeHUD[2].transform.localPosition = new Vector3(360, -360, 0);
            Destroy(TimeHUD[2].GetComponent<Speedometer>());
            TimeHUD[2].name = "Time Juice";
            mls.LogInfo("Golden Time Alt HUD created successfully.");
            yield break;
        }
        public IEnumerator LoadGoldArm()
        {
            float elapsedTime = 0;
            mls.LogInfo("Loading Golden Arm into game...");
            do
            {
                elapsedTime += Time.unscaledDeltaTime;
                if (elapsedTime > 5)
                    yield break;
                yield return null;
            } while (armGoldAC == null);
            GoldArm = Instantiate(armGoldObj, Player.transform.Find("Main Camera/Punch").transform);
            GoldArm.name = "Arm Gold";
            GoldArm.layer = 13;
            foreach (Transform t in GoldArm.transform)
            {
                t.gameObject.layer = 13;
            }
            GoldArm.AddComponent<Animator>();
            GoldArm.GetComponent<Animator>().runtimeAnimatorController = armGoldAC;
            GoldArm.transform.localEulerAngles = new Vector3(72.867f, 60.269f, 160.903f);
            //GO.transform.localEulerAngles = new Vector3(74.867f, 75.269f, 161.903f);
            GoldArm.transform.localPosition = new Vector3(-0.8f, -0.8f, -2.0f);
            GoldArm.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            GoldArm.SetActive((bool)TimestopperProgress.ArmStatus(TProgress.equippedArm));
            Player.transform.Find("Main Camera/Punch").GetComponent<FistControl>().goldArm = new AssetReference();
            /////////// Player Moement Fix \\\\\\\\\\\\\\
            mls.LogInfo("Golden Arm created successfully.");
            yield break;
        }
        public IEnumerator ModifySettingsPage()
        {
            mls.LogWarning("creating instances...");
            SettingsPage sp = SettingsPage.CreateInstance<SettingsPage>();
            SettingsCategory sc = SettingsCategory.CreateInstance<SettingsCategory>();
            SettingsItem item = SettingsItem.CreateInstance<SettingsItem>();
            mls.LogWarning("Created instances");
            sc.title = "Test Title";
            item.buttonLabel = "test button";
            item.label = "test";
            item.name = "test";
            item.style = SettingsItemStyle.Normal;
            item.dropdownList = new string[] { "test1", "test2", "test3" };
            item.itemType = SettingsItemType.Slider;
            item.sideNote = "this is a literal side note, you can change it or set it to any value you want";
            item.dropdownList = new string[] { "one", "two", "three" };
            sc.items = new List<SettingsItem> { item };
            sc.name = "TestCategory";
            sc.title = "testCategory";
            sc.description = "this is a test category, it will be used for Timeless Config";
            sc.titleDecorator = "++";
            sp.categories = new SettingsCategory[] { sc };
            sp.name = "TestPage";
            mls.LogWarning("Waiting for menu");
            while (FindRootGameObject("Canvas") == null)
            {
                yield return null;
            }
            mls.LogWarning("Waiting for options");
            while (FindRootGameObject("Canvas").transform.Find("OptionsMenu/Pages/Graphics") == null)
            {
                yield return null;
            }
            SettingsPageBuilder test = FindRootGameObject("Canvas").transform.Find("OptionsMenu/Pages/Graphics").GetComponent<SettingsPageBuilder>();
            typeof(SettingsPageBuilder).GetField("page", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(test, sp);
            GameObject G = Instantiate(test.assets.categoryTitlePrefab, test.transform.Find("Scroll Rect/Contents/")).gameObject;
            G.transform.Find("Text").GetComponent<TextMeshProUGUI>().fontSize = 12;
            //typeof(SettingsPageBuilder).GetMethod("BuildPage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(test, new object[] { sp });
            mls.LogWarning("Test Should Work");
        }
        public static void ResetGoldArm()
        {
            TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
            StartTime(0);
        }
        public GameObject FindRootGameObject(string name)
        {
            foreach (GameObject G in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (G.name == name)
                    return G;
            }
            return null;
        }
        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Bootstrap" && LoadDone &&
                scene.name != "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/)
            {
                mls.LogWarning("Main Menu Detected");
                //StartCoroutine(ModifySettingsPage());
            }
            TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
            if (scene.name != "b3e7f2f8052488a45b35549efb98d902" /*main menu*/ &&
                scene.name != "Bootstrap" && LoadDone &&
                scene.name != "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/)
            {
                StartCoroutine(LoadHUD());
                StartCoroutine(LoadGoldArm());
                StartCoroutine(EnsureShader());
                StatsManager.checkpointRestart += ResetGoldArm;
                if (firstLoad && !(bool)TimestopperProgress.ArmStatus(TProgress.hasArm)) //display the message for newcomers
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage( ARM_NEW_MESSAGE, "", "", 2);
                    messageTimer.done += () =>
                    {
                        MonoSingleton<HudMessageReceiver>.Instance.Invoke("Done", 0);
                        firstLoad = false;
                    };
                    messageTimer.SetTimer(6, true);
                }
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem("timestopper.timestop", TIMESTOP_STYLE);
            } else
            {
                timeStopper = CStopTime(0);
                timeStarter = CStartTime(0);
            }
            // Update the Level
            if (ConfirmLevel("VIOLENCE /// FIRST")) // Add the door to the level
            {
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
                GameObject newaltar = Instantiate(GameObject.Find("First Section").transform.Find("Opening Halls Geometry/Opening Nonstuff/Forward Hall/Floor/AltarFloor/Altar (Red)").gameObject
                                                                                                   , GameObject.Find("Stairway Down").transform);
                newaltar.name = "Altar (Blue Book) Variant";
                newaltar.transform.Find("Altars/Altar_Sulfur").gameObject.SetActive(false);
                newaltar.transform.Find("Altars/Altar_Mercury").gameObject.SetActive(true);
                Destroy(newaltar.transform.Find("Cube").GetComponent<ItemPlaceZone>());
                ItemPlaceZone altarcomp = newaltar.transform.Find("Cube").gameObject.AddComponent<ItemPlaceZone>();
                altarcomp.acceptedItemType = ItemType.SkullBlue;
                altarcomp.Invoke("Awake", 0);
                altarcomp.Invoke("Start", 0);
                altarcomp.enabled = true;
                altarcomp.altarElements = new InstantiateObject[]
                {
                    newaltar.transform.Find("Altars/Altar_Mercury/Mercury_Raise").GetComponent<InstantiateObject>(),
                    newaltar.transform.Find("Altars/Altar_Salt/Salt_Raise").GetComponent<InstantiateObject>(),
                    newaltar.transform.Find("Altars/Altar_Sulfur/Sulfur_Raise").GetComponent<InstantiateObject>()
                };
                newaltar.transform.Find("Cube").localEulerAngles = new Vector3(0, 0, 0);
                newaltar.transform.position = new Vector3(-10.0146f, -24.9875f, 590.0158f);
                newaltar.transform.localEulerAngles = new Vector3(0, 0, 0);
                GameObject insttext = new GameObject();
                GameObject newtext = Instantiate(insttext, newdoor.transform);
                newtext.name = "Text";
                newtext.AddComponent<TextMeshPro>();
                newtext.GetComponent<TextMeshPro>().text = "<color=#FF6600>UNDER\nCONSTRUCTION</color>\n<size=42%><color=#1818FF>(use the altar instead)</color></size>";
                newtext.GetComponent<TextMeshPro>().fontSize = 12;
                newtext.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
                newtext.transform.localPosition = new Vector3(0, 7.5f, -0.3f);
                TheCube = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
                TheCube.name = "Gold Arm Item";
                TheCube.AddComponent<Rigidbody>();
                TheCube.AddComponent<GoldArmItem>();
                TheCube.GetComponent<MeshRenderer>().enabled = false;
                TheCube.transform.position = newaltar.transform.position + new Vector3(0, 2, 0);
                GameObject itemArm = Instantiate(armGoldObj, TheCube.transform);
                itemArm.transform.localScale *= 0.17f;
                itemArm.transform.localPosition = new Vector3(-1.1f, -0.1f, -1.3f);
                itemArm.transform.localEulerAngles = new Vector3(10.0f, 0f, 0f);
                TheCube.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).localEulerAngles = new Vector3(0f, 0f, 50.0f);
                TheCube.SetActive(false);
                altarcomp.activateOnSuccess = new GameObject[1] { TheCube };
                altarcomp.activateOnFailure = new GameObject[0] { };
                altarcomp.deactivateOnSuccess = new GameObject[0] { };
                altarcomp.arenaStatuses = new ArenaStatus[0] { };
                altarcomp.doors = new Door[0] { };
                altarcomp.reverseArenaStatuses = new ArenaStatus[0] { };
                altarcomp.reverseDoors = new Door[0] { };
                altarcomp.elementChangeEffect = newaltar.transform.Find("Variant/Altars/ChangeEffect").GetComponent<ParticleSystem>();
                //altarcomp.deactivateOnSuccess = new GameObject[1] {
                //                GameObject.Find("First Section/Opening Halls Geometry/Opening Nonstuff/Forward Hall -> Stairway Down/Blocker")
                //};
                mls.LogInfo("Added The Cube");
            }
            // Cybergrind Music Explorer Compatability
            if (scene.name == "9240e656c89994d44b21940f65ab57da" && Chainloader.PluginInfos.ContainsKey("dev.flazhik.jukebox"))
            {
                cybergrind = true;
                Type Comp = Type.GetType("Jukebox.Components.NowPlayingHud, Jukebox");
                if (Comp != null)
                {   Component C = FindObjectOfType(Comp) as Component;
                    if (C != null) C.gameObject.transform.localPosition += new Vector3(0, 60, 0);
                    else mls.LogError("Component C is null!");
                }
                else mls.LogError("Could not get Jukebox.Components.NowPlayingHud");
            } else if (Chainloader.PluginInfos.ContainsKey("dev.flazhik.jukebox") ||!Chainloader.PluginInfos.ContainsKey("dev.flazhik.jukebox"))
            {
                cybergrind = false;
            }
        }
        public void OnSceneUnloaded(Scene scene)
        {
            if (scene.name != "b3e7f2f8052488a45b35549efb98d902" /*main menu*/ &&
                scene.name != "Bootstrap" && LoadDone &&
                scene.name != "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/ &&
                realTimeScale < 1)
            { 
                StartTime(0, true);
            }
        }
        void Awake()
        {
            if (Instance == null) { Instance = this; }

            mls = BepInEx.Logging.Logger.CreateLogSource(GUID);
            mls.LogInfo("The Timestopper has awakened!");
            //foreach(var s in Chainloader.PluginInfos)
            //    mls.LogInfo(s.Key);
            InitializeConfig();

            harmony.PatchAll();

            base.StartCoroutine(LoadBundle());   //Load all assets

            //***********DEBUG**************\\
            TheCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            TheCube.name = "The Cube";
//          \\******************************//


            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
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
            StopCoroutine(timeStarter);
            StoppedTimeAmount = 0;
            Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.GetComponent<Animator>().Play("Stop");
            Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.GetComponent<Animator>().speed = animationSpeed.Value;
            if (filterMusic.Value)
                musicManager.GetComponent<MusicManager>().FilterMusic();
            PlayRespectiveSound(true);
            Physics.simulationMode = SimulationMode.Script;
            foreach (Rigidbody R in FindObjectsOfType<Rigidbody>())  //Save everyone's states and freeze them except Player
            {
                GameObject G = R.gameObject;
                if (G.GetComponent<Playerstopper>() == null)
                {
                    if (G.GetComponent<ConstraintSaver>() == null)
                        G.AddComponent<ConstraintSaver>();
                    G.GetComponent<ConstraintSaver>().enabled = true;
                    G.GetComponent<ConstraintSaver>().Freeze();
                }
            }
            foreach (AudioSource A in FindObjectsOfType<AudioSource>())  //Add the pitching effect to all AudioSources except the ones on Player
                if (A.gameObject.GetComponent<Audiopitcher>() == null && A.gameObject.transform.parent != Player.transform)
                    A.gameObject.AddComponent<Audiopitcher>();
            foreach (Animator A in FindObjectsOfType<Animator>())   // Make animations work in stopped time when time is stopped
                if (A.gameObject.transform.IsChildOf(Player.transform) && A.updateMode == AnimatorUpdateMode.Normal)
                    A.updateMode = AnimatorUpdateMode.UnscaledTime;

            Player.GetComponent<Playerstopper>().Awake();
            if (speed == 0)
            {
                Time.timeScale = 0;
                realTimeScale = 0;
                playerTimeScale = 1;
                yield break;
            }
            do
            {
                Time.timeScale -= Time.unscaledDeltaTime / speed;
                realTimeScale -= Time.unscaledDeltaTime / speed;
                yield return null;
            } while (Time.timeScale > Time.unscaledDeltaTime / speed);
            Time.timeScale = 0;
            realTimeScale = 0;
            yield break;
        }
        public IEnumerator CStartTime(float speed, bool preventStyle = false)
        {
            StopCoroutine(timeStopper);
            Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.GetComponent<Animator>().Play("Release");
            if (filterMusic.Value)  
                musicManager.GetComponent<MusicManager>().UnfilterMusic();
            PlayRespectiveSound(false);
            Physics.simulationMode = SimulationMode.FixedUpdate;
            foreach (Animator A in FindObjectsOfType<Animator>())  // Make animations not work in stopped time when time isn't stopped
                if (A.gameObject.transform.IsChildOf(Player.transform) && A.updateMode == AnimatorUpdateMode.UnscaledTime)
                    A.updateMode = AnimatorUpdateMode.Normal;
            if (speed == 0) {
                Time.timeScale = 1;
                realTimeScale = 1;
                StoppedTimeAmount = 0;
                yield break;            }
            if (Time.timeScale < 0)
                Time.timeScale = 0;
            do
            {
                Time.timeScale += Time.unscaledDeltaTime / speed;
                realTimeScale += Time.unscaledDeltaTime / speed;
                yield return null;
            } while (Time.timeScale < 1);
            if (!preventStyle && StoppedTimeAmount > 2)
                MonoSingleton<StyleHUD>.Instance.AddPoints((int)StoppedTimeAmount*100, "timestopper.timestop");
            StoppedTimeAmount = 0;
            Time.timeScale = 1;
            realTimeScale = 1;
            yield break;
        }
        public static void StopTime(float time)
        {
            Instance.timeStopper = Instance.CStopTime(time);
            Instance.StartCoroutine(Instance.timeStopper);
            TimeStop = true;
        }
        public static void StartTime(float time, bool preventStyle = false)
        {
            Instance.timeStarter = Instance.CStartTime(time, preventStyle);
            Instance.StartCoroutine(Instance.timeStarter);
            TimeStop = false;
        }
        public void UpdateTimeJuice()
        {
            if (TimeStop)
            {
                TimeLeft -= playerDeltaTime * (1.0f - realTimeScale);
                StoppedTimeAmount += playerDeltaTime * (1.0f - realTimeScale);
                if (TimeLeft < 0)
                    TimeLeft = 0;
                TimeColor.g = 0.6f;
            }
            else
            {
                if (realTimeScale <= 0.3f)
                    Player.GetComponent<NewMovement>().walking = false;
                TimeLeft += Time.deltaTime * refillMultiplier.Value;
                if (TimeLeft > (float)TimestopperProgress.ArmStatus(TProgress.maxTime))
                    TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
                if (TimeLeft < lowerTreshold.Value)
                    TimeColor.g = 0;
                else
                    TimeColor.g = 1;
            }
            if (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown)
            {
                TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
                TimeColor.r = 0;
                TimeColor.b = 1;
            }
            else
            {
                TimeColor.r = 1;
                TimeColor.b = 0;
            }
        }
        public void UpdateHUD()
        {
            if (TimeHUD != null)
            {
                Color G = TimeHUD[0].transform.Find("Image/Image (1)").gameObject.GetComponent<UnityEngine.UI.Image>().color;
                float F = TimeHUD[0].transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount;
                TimeHUD[0].transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>().enabled = true;
                TimeHUD[0].transform.Find("Image/Image (1)").gameObject.GetComponent<UnityEngine.UI.Image>().enabled = true;
                TimeHUD[0].transform.Find("Image/Image (1)").gameObject.GetComponent<UnityEngine.UI.Image>().color
                    = (G * 5 + TimeColor) * (Time.unscaledDeltaTime) / (6 * Time.unscaledDeltaTime);
                TimeHUD[0].transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount
                    = (F * 8 + (TimeLeft / (float)TimestopperProgress.ArmStatus(TProgress.maxTime))) * (Time.unscaledDeltaTime) / (9 * Time.unscaledDeltaTime);
                if ((bool)TimestopperProgress.ArmStatus(TProgress.equippedArm) && Player.transform.Find("Main Camera/HUD Camera/HUD/GunCanvas/GunPanel/Filler").gameObject.activeInHierarchy)
                {
                    TimeHUD[0].SetActive(true);
                }
                else
                {
                    TimeHUD[0].SetActive(false);
                    TimeHUD[0].transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount = 0;
                }
                if ((bool)TimestopperProgress.ArmStatus(TProgress.equippedArm))
                {
                    if (TimeHUD[1].transform.Find("Text (TMP)").gameObject.activeInHierarchy)
                    {
                        TimeHUD[1].SetActive(true);
                        TimeHUD[1].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = (TimeLeft).ToString().Substring(0, 4);
                        TimeHUD[1].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = (G * 5 + TimeColor) * (Time.unscaledDeltaTime) / (6 * Time.unscaledDeltaTime);
                    } else if (TimeHUD[2].transform.Find("Text (TMP)").gameObject.activeInHierarchy)
                    {
                        TimeHUD[2].SetActive(true);
                        TimeHUD[2].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = (TimeLeft).ToString().Substring(0, 4);
                    }
                }
                else
                {
                    TimeHUD[1].SetActive(false);
                    TimeHUD[2].SetActive(false);
                }
            }
        }

        private void Update()
        {
            messageTimer.Update();
            if (SceneManager.GetActiveScene().name == "b3e7f2f8052488a45b35549efb98d902" /*main menu*/ ||
            SceneManager.GetActiveScene().name == "Bootstrap" ||
            SceneManager.GetActiveScene().name == "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/)   {
                return;
            }
            if (cybergrind) //reset time juice when wave is done
            {
                if (cybergrindWave != MonoSingleton<EndlessGrid>.Instance.currentWave)
                {
                    TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
                    cybergrindWave = MonoSingleton<EndlessGrid>.Instance.currentWave;
                }
            }
            PreventNull();
            if (Player == null)
                return;
            // trigger function
            if (((UnityInput.Current.GetKeyDown(stopKey.Value)) && Player.transform.Find("Main Camera/Punch/Arm Gold") != null && (bool)TimestopperProgress.ArmStatus(TProgress.equippedArm))
                || (TimeStop && TimeLeft <= 0.0f)
                || (Player.GetComponent<NewMovement>().dead && TimeStop) )
            {
                if (MenuCanvas.transform.Find("PauseMenu").gameObject.activeSelf ||
                    MenuCanvas.transform.Find("OptionsMenu").gameObject.activeSelf) //if game paused
                    return;
                if (!Player.GetComponent<NewMovement>().dead) {
                    Player.GetComponent<Playerstopper>().enabled = true;
                }
                //====================================[ GLOBAL TIME STOP ]================================\\
                if (!TimeStop && !Fist.shopping && TimeColor.g == 1 /*smurt optimization to avoid separate variable*/)
                {
                    StopTime(stopSpeed.Value);
                    mls.LogInfo("Time stops!");
                }
                else 
                {
                    StartTime(startSpeed.Value);
                    BlindCheat.Disable();
                    mls.LogInfo("Time resumes.");
                }
            }
            UpdateTimeJuice();
            UpdateHUD();
            if (Player.GetComponent<NewMovement>().dead){
                StartTime(0);
            }
        }
    }

    public class Audiopitcher : MonoBehaviour{
        public void Update(){
            if (GetComponent<AudioSource>() != null && Timestopper.realTimeScale <= 1.0f)
            {
                if (transform.parent != null)
                {
                    if (GetComponent<AudioSource>() == MonoSingleton<MusicManager>.Instance.targetTheme)
                        if (Timestopper.realTimeScale > 0.0f)
                        {
                            GetComponent<AudioSource>().pitch = Timestopper.realTimeScale*(1-Timestopper.stoppedMusicPitch.Value) + Timestopper.stoppedMusicPitch.Value;
                            MonoSingleton<MusicManager>.Instance.volume = Timestopper.realTimeScale*(1-Timestopper.stoppedMusicVolume.Value) + Timestopper.stoppedMusicVolume.Value;
                        }
                        else
                        {
                            GetComponent<AudioSource>().pitch = Timestopper.stoppedMusicPitch.Value;
                            MonoSingleton<MusicManager>.Instance.volume = Timestopper.stoppedMusicVolume.Value;
                        }
                } else
                {
                    if (Timestopper.realTimeScale > 0.0f)
                        GetComponent<AudioSource>().pitch = Timestopper.realTimeScale;
                    else
                        GetComponent<AudioSource>().pitch = 0;
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
            armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<UnityEngine.UI.Image>().sprite =
                                armInfoGold.transform.Find("Panel/Back Button").GetComponent<UnityEngine.UI.Image>().sprite;
            armInfoGold.transform.Find("Panel/Description").GetComponent<TMPro.TextMeshProUGUI>().text = Timestopper.ARM_DESCRIPTION + (string)TimestopperProgress.ArmStatus(TProgress.upgradeText);
            if ((int)TimestopperProgress.ArmStatus(TProgress.upgradeCount) < Timestopper.maxUpgrades.Value)
            {
                if (GameProgressSaver.GetMoney() > (int)TimestopperProgress.ArmStatus(TProgress.upgradeCost))
                {
                    armInfoGold.transform.Find("Panel/Purchase Button/Text").GetComponent<TextMeshProUGUI>().text = TimestopperProgress.ArmStatus(TProgress.upgradeCost).ToString() + " <color=#FF4343>P</color>";
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().failure = false;
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<UnityEngine.UI.Button>().interactable = true;
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<UnityEngine.UI.Image>().color = Color.white;
                }
                else
                {
                    armInfoGold.transform.Find("Panel/Purchase Button/Text").GetComponent<TextMeshProUGUI>().text = "<color=#FF4343>" + TimestopperProgress.ArmStatus(TProgress.upgradeCost).ToString() + " P</color>";
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().failure = true;
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<UnityEngine.UI.Button>().interactable = false;
                    armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<UnityEngine.UI.Image>().color = Color.red;
                }
            } else
            {
                armInfoGold.transform.Find("Panel/Purchase Button/Text").GetComponent<TextMeshProUGUI>().text = "<color=#FFEE43>MAX</color>";
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<ShopButton>().failure = true;
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<UnityEngine.UI.Button>().interactable = false;
                armInfoGold.transform.Find("Panel/Purchase Button").GetComponent<UnityEngine.UI.Image>().color = Color.gray;
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
            if (transform.parent.GetComponent<TerminalExcluder>() != null)
                transform.parent.GetComponent<TerminalExcluder>().OverrideInfoMenu();
        }
    }

    public class Grayscaler : MonoBehaviour
    {
        RenderTexture R = new RenderTexture(Screen.width, Screen.height, 32);
        RenderTexture T = new RenderTexture(Screen.width, Screen.height, 32);
        Camera camera;
        private Material _material;
        private Material _slapper;
        public float Grayscale;
        public void DoIt()
        {
            if (!Timestopper.grayscale.Value || !Timestopper.grayscaleShader.isSupported)
                return;
            if (!R.IsCreated())
                R.Create();
            if (!T.IsCreated())
                T.Create();
            if (Timestopper.grayscaleShader != null)
                _material = new Material(Timestopper.grayscaleShader);
            if (Timestopper.slappShader != null)
                _slapper = new Material(Timestopper.slappShader);
            GameObject C = new GameObject();
            GameObject B = Instantiate(C, transform.parent);
            B.AddComponent<Camera>();
            camera = B.GetComponent<Camera>();
        }
        void LateUpdate()
        {
            if (Timestopper.grayscaleShader != null)
                if (!Timestopper.grayscaleShader.isSupported)
                {
                    Timestopper.mls.LogError("grayscaler attempted grayscale but the shader was null, turning geayscale effect off!");
                    Timestopper.grayscale.Value = false;
                }
            if (Timestopper.grayscale.Value)
                transform.parent.Find("HUD Camera").gameObject.GetComponent<Camera>().enabled = false;
            else
                transform.parent.Find("HUD Camera").gameObject.GetComponent<Camera>().enabled = true;
        }
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material != null && _slapper != null && Timestopper.grayscale.Value)
            {
                if (Timestopper.exclusiveGrayscale.Value)
                    Grayscale = 1 - Timestopper.realTimeScale;
                else
                    Grayscale = 1 - Time.timeScale;

                RenderTexture rt = RenderTexture.active;  //Clear R to empty
                RenderTexture.active = R;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = rt;

                _material.SetFloat("_Grayscale", Grayscale*Timestopper.grayscaleAmount.Value);
                camera.CopyFrom(transform.parent.Find("HUD Camera").gameObject.GetComponent<Camera>());
                camera.targetTexture = R;
                camera.Render();
                _slapper.SetTexture("_SlappedTex", R);
                Graphics.Blit(source, T, _material);
                Graphics.Blit(T, destination, _slapper);

            }
            else {
                if (!Timestopper.grayscale.Value || !Timestopper.grayscaleShader.isSupported)
                {
                    transform.parent.Find("HUD Camera").gameObject.GetComponent<Camera>().enabled = true;
                    Timestopper.Player.transform.Find("Main Camera/HUD Camera").GetComponent<Camera>().targetTexture = null;
                    Graphics.Blit(source, destination);
                    Timestopper.HUDCamera.enabled = true;
                    Timestopper.HUDCamera.targetTexture = null;
                    this.enabled = false;
                    return;
                }
                if (Timestopper.grayscaleShader != null)
                    _material = new Material(Timestopper.grayscaleShader);
                if (Timestopper.slappShader != null)
                    _slapper = new Material(Timestopper.slappShader);
                //Timestopper.mls.LogError("From Inside Grayscaler [Runtime] >> Material is null or cannot be used. Grayscale disabled.");
                //this.enabled = false;
            }
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
            var deltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
            var fixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime));
            var playerDeltaTimeG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerDeltaTime));
            //var unscaledGetter = AccessTools.PropertyGetter(typeof(Time), nameof(Time.unscaledDeltaTime));
            //var fixedUnscaledDeltaTimeG = AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedUnscaledDeltaTime));
            var playerFixedDeltaTimeG = AccessTools.PropertyGetter(typeof(Timestopper), nameof(Timestopper.playerFixedDeltaTime));
            ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(Timestopper.GUID);

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
                else
                {
                    yield return i;
                }
            }
        }

    }

    [HarmonyPatch(typeof(NewMovement), "Parry")]
    public class ParryTimeFiller
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if (!Timestopper.TimeStop)
                Timestopper.TimeLeft += Timestopper.bonusTimeForParry.Value;
            return true;
        }
    }

    [HarmonyPatch(typeof(TimeSince), "op_Implicit", new Type[] { typeof(TimeSince) })]
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
    [HarmonyPatch(typeof(TimeSince), "op_Implicit", new Type[] { typeof(float) })]
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

    [HarmonyPatch(typeof(WeaponCharges), "Update")] public class TranspileWeaponCharges0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[WeaponCharges]=> Update");}
    [HarmonyPatch(typeof(WeaponCharges), "Charge")] public class TranspileWeaponCharges1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[WeaponCharges]=> Charge");}
    [HarmonyPatch(typeof(GunControl), "Update")] public class TranspileGunControl { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[GunControl]=> Update");}
    [HarmonyPatch(typeof(Revolver), "Update")] public class TranspileRevolver0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[Revolver]=> Update");}
    [HarmonyPatch(typeof(Revolver), "LateUpdate")] public class TranspileRevolver1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[Revolver]=> LateUpdate");}
    [HarmonyPatch(typeof(ShotgunHammer), "UpdateMeter")] public class TranspileShotgunHammer0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> UpdateMeter");}
    [HarmonyPatch(typeof(ShotgunHammer), "Update")] public class TranspileShotgunHammer1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> Update"); }
    [HarmonyPatch(typeof(ShotgunHammer), "LateUpdate")] public class TranspileShotgunHammer2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> LateUpdate"); }
    [HarmonyPatch(typeof(Shotgun), "Update")] public class TranspileShotgun0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[Shotgun]=> Update"); }
    [HarmonyPatch(typeof(Shotgun), "UpdateMeter")] public class TranspileShotgun1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[Shotgun]=> UpdateMeter"); }
    [HarmonyPatch(typeof(Nailgun), "Update")] public class TranspileNailgun0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[Nailgun]=> Update"); }
    [HarmonyPatch(typeof(Nailgun), "UpdateZapHud")] public class TranspileNailgun1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[Nailgun]=> UpdateZpHud"); }
    [HarmonyPatch(typeof(Nailgun), "FixedUpdate")] public class TranspileNailgun2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Nailgun]=> FixedUpdate"); }
    [HarmonyPatch(typeof(Nailgun), "RefreshHeatSinkFill")] public class TranspileNailgun3 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[Nailgun]=> RefreshHeatSinkFill"); }
    [HarmonyPatch(typeof(RocketLauncher), "Update")] public class TranspileRocketLauncher0{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[RocketLauncher]=> Update");}
    [HarmonyPatch(typeof(RocketLauncher), "FixedUpdate")] public class TranspileRocketLauncher1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[RocketLauncher]=> FixedUpdate");}
    [HarmonyPatch(typeof(NewMovement), "Update")] public class TranspileNewMovement0{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> Update");}
    [HarmonyPatch(typeof(NewMovement), "FixedUpdate")] public class TranspileNewMovement1{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> FixedUpdate");}
    [HarmonyPatch(typeof(NewMovement), "Move")] public class TranspileNewMovement2{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> Move");}
    [HarmonyPatch(typeof(NewMovement), "Jump")] public class TranspileNewMovement3{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> Jump");}
    [HarmonyPatch(typeof(NewMovement), "Dodge")] public class TranspileNewMovement4{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> Dodge");}
    [HarmonyPatch(typeof(NewMovement), "TrySSJ")] public class TranspileNewMovement5{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> TrySSJ");}
    [HarmonyPatch(typeof(NewMovement), "WallJump")] public class TranspileNewMovement6{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> WallJump");}
    [HarmonyPatch(typeof(NewMovement), "CheckForGasoline")] public class TranspileNewMovement7{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> CheckForGasoline");}
    [HarmonyPatch(typeof(NewMovement), "FrictionlessSlideParticle")] public class TranspileNewMovement8{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> FrictionlessSlideParticle");}
    [HarmonyPatch(typeof(NewMovement), "DetachSlideScrape")] public class TranspileNewMovement9{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[NewMovement]=> DetachSlideScrape");}
    [HarmonyPatch(typeof(FistControl), "Update")] public class TranspileFistControl{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[FistControl]=> Update");}
    [HarmonyPatch(typeof(GroundCheck), "Update")] public class TranspileGroundCheck0{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[GroundCheck]=> Update");}
    [HarmonyPatch(typeof(GroundCheck), "FixedUpdate")] public class TranspileGroundCheck1{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[GroundCheck]=> FixedUpdate");}
    [HarmonyPatch(typeof(GroundCheck), MethodType.Constructor)] public class TranspileGroundCheck2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "Constructor<GroundCheck>"); }
    [HarmonyPatch(typeof(ClimbStep), "FixedUpdate")] public class TranspileClimbStep{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ClimbStep]=> FixedUpdate");}
    [HarmonyPatch(typeof(CameraController), "Update")] public class TranspileCameraController{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[CameraController]=> Update");}
    [HarmonyPatch(typeof(Punch), "Update")] public class TranspilePunch{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[Punch]=> Update");}
    [HarmonyPatch(typeof(WalkingBob), "Update")] public class TranspileWalkingBob{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[WalkingBob]=> Update");}
    [HarmonyPatch(typeof(StaminaMeter), "Update")] public class TranspileStaminaMeter{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[StaminaMeter]=> Update");}
    [HarmonyPatch(typeof(HealthBar), "Update")] public class TranspileHealthBar{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>DeltaTimeReplacer.Transpiler(instructions, "[HealthBar]=> Update");}
    [HarmonyPatch(typeof(HurtZone), "FixedUpdate")] public class TranspileHurtZone{ [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[HurtZone]=> FixedUpdate");}
    [HarmonyPatch(typeof(SpriteController), "Awake")]
    class Patch
    {
        static void Postfix(SpriteController __instance)
        {
            //UnityEngine.Debug.Log("Unity, all awake is modified bro!");
            if (__instance.gameObject.layer != 0)
                __instance.gameObject.layer = 0;
            foreach(Transform child in __instance.GetComponentsInChildren<Transform>())
                child.gameObject.layer = 0;
        }
    }

    //====================[ PATCHWORK END ]======================\\


    // ############################################# PLAYER SCRIPTS ########################################## \\
    public class TerminalUpdater : MonoBehaviour
    {
        public int wid = 0;
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
                if (Timestopper.TimeStop) {
                    col.gameObject.GetComponent<ShopZone>().enabled = false;
                } else {
                    col.gameObject.GetComponent<ShopZone>().enabled = true;
                    Timestopper.LatestTerminal = col.gameObject;
                }
            }
        }   
    }

    public class GoldArmItem : MonoBehaviour
    {
        public void OnCollisionEnter(Collision col)
        {
            Timestopper.mls.LogInfo("Armitem Has Collided With " + col.gameObject.name);
            if (col.gameObject.name == "Player")
            {
                TimestopperProgress.GiveArm();
                gameObject.SetActive(false);
                gameObject.transform.GetChild(0).gameObject.SetActive(false);
                enabled = false;
            }
        }
        public void Update()
        {
            GameObject handcircle = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).gameObject;
            handcircle.transform.localEulerAngles = new Vector3(Time.timeSinceLevelLoad * 360 * 4, 0f, -90f);
        }
    }

    public class PrivateInsideTimer
    {
        private float time = 0;
        private bool scaled = false;
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
        private float time = 0;
        private bool scaled = false;
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
    public class ConstraintSaver : MonoBehaviour    // Added to all Rigidbodies when time stops
    {
        public bool gravity = false;
        public bool frozen = false;
        public bool byDio = false;
        public bool isCoin = false;
        public bool isRocket = false;
        public bool previousrocketfreeze = false;
        private float localScale = 1.0f; // local timescale, so that coins freeze slowly
        public Vector3 unscaledVelocity = new Vector3(0, 0, 0);
        public Vector3 unscaledAngularVelocity = new Vector3(0, 0, 0);
        public Rigidbody R;
        private float time = 0;

        public void Freeze()
        {
            if (gameObject.GetComponent<Grenade>() != null)
            {
                isRocket = gameObject.GetComponent<Grenade>().rocket;
                //gameObject.AddComponent<FixedUpdateCaller>();
                //GetComponent<FixedUpdateCaller>().enabled = false;
            }
            R = gameObject.GetComponent<Rigidbody>();
            if (gameObject.GetComponent<Coin>() != null)
                isCoin = true;
            if (R != null)
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
            if (GetComponent<Nail>() != null)
            {
                if (GetComponent<Nail>().sawblade || GetComponent<Nail>().chainsaw)
                {
                    if (GetComponent<FixedUpdateCaller>() == null)
                        gameObject.AddComponent<FixedUpdateCaller>();
                }
            }
            if (Timestopper.realTimeScale == 0.0f)
            {
                byDio = true;
            }
        }

        private void UnFreeze()
        {
            if (R != null)
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
                R = gameObject.GetComponent<Rigidbody>();
        }

        public void Update()
        {
            if (Timestopper.TimeStop)
            {
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
            if (isCoin)
            {
                gameObject.GetComponent<SphereCollider>().enabled = true;
                gameObject.GetComponent<BoxCollider>().enabled = true;
                gameObject.GetComponent<Coin>().Invoke("StartCheckingSpeed", 0);
                isCoin = false;
            }
            if (Timestopper.TimeStop && R != null)
            {
                //if (!frozen)
                //{
                //    Freeze();
                //}
                if (R != null)
                {
                    if (!R.isKinematic)
                    {
                        R.useGravity = false;
                        unscaledVelocity += R.GetAccumulatedForce(Time.fixedDeltaTime);
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
                            if (previousrocketfreeze)
                            {
                                previousrocketfreeze = false;
                                MethodInfo MFixedUpdate = gameObject.GetComponent<Grenade>().GetType().GetMethod("FixedUpdate",
                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                                MFixedUpdate.Invoke(gameObject.GetComponent<Grenade>(), null);
                                transform.Find("FreezeEffect").gameObject.SetActive(false);
                            }
                            if (byDio)
                                localScale -= Timestopper.playerDeltaTime / Timestopper.affectSpeed.Value;
                            else
                                localScale -= Timestopper.playerDeltaTime / Timestopper.stopSpeed.Value;
                            if (localScale < 0)
                                localScale = 0.0f;
                        } else
                        {
                            MonoSingleton<WeaponCharges>.Instance.rocketFrozen = false;
                            MethodInfo MFixedUpdate = gameObject.GetComponent<Grenade>().GetType().GetMethod("FixedUpdate",
                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                            MFixedUpdate.Invoke(gameObject.GetComponent<Grenade>(), null);
                            transform.Find("FreezeEffect").gameObject.SetActive(true);
                            MonoSingleton<WeaponCharges>.Instance.rocketFrozen = true;
                            previousrocketfreeze = true;
                            if (localScale < 1)
                                localScale += Timestopper.playerDeltaTime / Timestopper.stopSpeed.Value;
                            if (localScale > 1)
                                localScale = 1.0f;
                        }
                    }
                }
                else
                    R = gameObject.GetComponent<Rigidbody>();
            }
            else if (frozen && R != null)
            {
                UnFreeze();
            }
        }
    }
    public class FixedUpdateCaller : MonoBehaviour
    {
        float time = 0;
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
                        foreach(ScaleNFade C in gameObject.GetComponentsInChildren<ScaleNFade>())
                        {
                            Destroy(C.gameObject);
                        }
                    }
                    foreach(Component C in gameObject.GetComponents(typeof(MonoBehaviour)))
                    {
                        MethodInfo MFixedUpdate = C.GetType().GetMethod("FixedUpdate",
                                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        if (MFixedUpdate != null)
                            MFixedUpdate.Invoke(C, null);
                    }

                }
                Timestopper.UnscaleTimeSince = true;
                Timestopper.fixedCall = false;
            }
        }
    }
    public class Playerstopper : MonoBehaviour   // The other side of the magic
    {
        public float UnscaledDebug = 0.0f;
        public GroundCheck G1;
        public GroundCheck G2;
        public NewMovement movement;
        public float JumpCooldown = 0.0f;
        public bool movementHack = true;
        public GameObject TheCube;
        public bool menuOpen = false;
        public bool frameLaterer = false;
        public PrivateInsideTimer NotJumpingTimer = new PrivateInsideTimer();
        public PrivateInsideTimer JumpReadyTimer = new PrivateInsideTimer();

        public void FixedUpdateFix(Transform target)
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
        public void Awake()
        {
            G1 = gameObject.GetComponentsInChildren<GroundCheck>()[0];
            G2 = gameObject.GetComponentsInChildren<GroundCheck>()[1];
            FixedUpdateFix(transform);
            if (GameObject.Find("GameController").GetComponent<FixedUpdateCaller>() == null)
            {
                GameObject.Find("GameController").gameObject.AddComponent<FixedUpdateCaller>();
            }
            movement = gameObject.GetComponent<NewMovement>();
            JumpReadyTimer.done += () =>
            {
                movement.StartCoroutine("JumpReady");
            };
            NotJumpingTimer.done += () =>
            {
                movement.StartCoroutine("NotJumping");
            };
        }
        public GameObject FindRootGameObject(string name)
        {
            foreach (GameObject G in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (G.name == name)
                    return G;
            }
            return null;
        }
        //public void EndParry()
        //{
        //    Timestopper.mls.LogWarning("Parry Timer Ended! walking: " + GetComponent<NewMovement>().walking);
        //    Timestopper.playerTimeScale = 1;
        //    GetComponent<NewMovement>().walking = false;
        //    //Timestopper.StopTime(0);
        //    Timestopper.MenuCanvas.transform.Find("ParryFlash").gameObject.SetActive(false);
        //    foreach (Transform child in Timestopper.Player.transform.Find("Main Camera/New Game Object").transform)
        //        Destroy(child.gameObject);
        //}
        private void HandleHitstop()
        {
            if ((float)AccessTools.Field(typeof(TimeController), "currentStop").GetValue(MonoSingleton<TimeController>.Instance) <= 0) {
                if (Timestopper.playerTimeScale <= 0)
                {
                    Timestopper.playerTimeScale = 1;
                    //GetComponent<NewMovement>().walking = false;
                    MonoSingleton<TimeController>.Instance.timeScaleModifier = 1;
                    (AccessTools.Field(typeof(TimeController), "parryFlash").GetValue(MonoSingleton<TimeController>.Instance) as GameObject).SetActive(false);
                    foreach (Transform child in Timestopper.Player.transform.Find("Main Camera/New Game Object").transform)
                        Destroy(child.gameObject);
                }
                return;
            }
            else
            {
                frameLaterer = true;
                Timestopper.playerTimeScale = 0;
                Time.timeScale = 0;
                //GetComponent<NewMovement>().walking = false;
            }
        }

        private void HandleMenuPause()
        {
            if (Timestopper.MenuCanvas == null) {
                Timestopper.MenuCanvas = FindRootGameObject("Canvas");
                return;                                 }
            if (menuOpen != (Timestopper.MenuCanvas.transform.Find("PauseMenu").gameObject.activeSelf || Timestopper.MenuCanvas.transform.Find("OptionsMenu").gameObject.activeSelf) )
            {
                menuOpen = (Timestopper.MenuCanvas.transform.Find("PauseMenu").gameObject.activeSelf || Timestopper.MenuCanvas.transform.Find("OptionsMenu").gameObject.activeSelf);
                if (menuOpen == true)
                {
                    Timestopper.playerTimeScale = 0;
                } else
                {
                    Timestopper.playerTimeScale = 1;
                }
            }
        }
        public void FixedUpdate()
        {
            if (Timestopper.TimeStop)
            {
                Time.timeScale = Timestopper.realTimeScale;
                if (Timestopper.playerDeltaTime > 0)
                    Physics.Simulate(Time.fixedDeltaTime*(1-Timestopper.realTimeScale));   // Manually simulate Rigidbody physics
            }
        }
        
        private void Update()
        {
            JumpReadyTimer.Update();
            NotJumpingTimer.Update();
            HandleHitstop();
            if (Timestopper.specialMode.Value)
            {
                if (UnityInput.Current.GetKeyDown(KeyCode.J))
                {
                    GameProgressSaver.AddMoney((int)TimestopperProgress.ArmStatus(TProgress.upgradeCost));
                    Timestopper.mls.LogInfo("MONEY MONEY MONEY");
                }
            }
            if (Timestopper.TimeStop)
            {
                HandleMenuPause();
                if (Timestopper.realTimeScale < Timestopper.blindScale.Value && !ULTRAKILL.Cheats.BlindEnemies.Blind)
                {
                    Timestopper.BlindCheat.Enable(GameObject.Find("Cheat Menu").GetComponent<CheatsManager>());
                }
                if (transform.Find("Main Camera").GetComponent<AudioSource>() != null) // for timestop sound effect to play
                {
                    transform.Find("Main Camera").GetComponent<AudioSource>().pitch = 1;
                    transform.Find("Main Camera").GetComponent<AudioSource>().volume = 1;
                }
                if (!Timestopper.healInTimestop.Value)
                {
                    movement.ForceAddAntiHP(Timestopper.antiHpMultiplier.Value*Time.unscaledDeltaTime*(1-Timestopper.realTimeScale), true, true, true, false);
                }

                foreach (Rigidbody R in FindObjectsOfType<Rigidbody>())  // Freeze anything that has recently been created
                {
                    if (R.gameObject.GetComponent<ConstraintSaver>() == null && R.gameObject.GetComponent<NewMovement>() == null)
                    {
                        R.gameObject.AddComponent<ConstraintSaver>();
                        R.gameObject.GetComponent<ConstraintSaver>().Freeze();
                    }
                }
                // The JumpBug fix \\
                if (movement.jumping && movement.falling && !movement.boost)
                {
                    JumpReadyTimer.SetTimer(0.2f, false);
                    NotJumpingTimer.SetTimer(0.25f, false);
                }
                //if (JumpCooldown > Time.unscaledDeltaTime)  {
                //    movement.StartCoroutine("JumpReady");
                //    JumpCooldown -= Time.unscaledDeltaTime;}
                //else  {
                //    JumpCooldown = 0.0f;
                //    movement.StartCoroutine("JumpReady");
                //    movement.jumping = false;   }
            } else // if time not stop
            {
                if (ULTRAKILL.Cheats.BlindEnemies.Blind)
                {
                    Timestopper.BlindCheat.Disable();
                }
            }
        }


        private void LateUpdate()
        {
            if (Timestopper.TimeStop)
            {
                if (Timestopper.playerDeltaTime > 0)
                {
                    foreach (Animator A in FindObjectsOfType<Animator>())   // Make animations work in stopped time when time is stopped
                        if (A.gameObject.transform.IsChildOf(gameObject.transform) && A.updateMode == AnimatorUpdateMode.Normal)
                            A.updateMode = AnimatorUpdateMode.UnscaledTime;
                } else
                {
                    foreach (Animator A in FindObjectsOfType<Animator>())   // Make animations work in stopped time when time is stopped
                        if (A.gameObject.transform.IsChildOf(gameObject.transform) && A.updateMode == AnimatorUpdateMode.UnscaledTime)
                            A.updateMode = AnimatorUpdateMode.Normal;
                }
                foreach (ParticleSystem A in FindObjectsOfType<ParticleSystem>())   // Make plarticles work in stopped time when time is stopped
                {
                    if (A.gameObject.transform.IsChildOf(gameObject.transform) && A.main.useUnscaledTime == false)
                    {
                        var main = A.main;
                        main.useUnscaledTime = true;
                    }
                }
            }
        }
    }


    // useless experiments that may come in handy
    public class StaticMeshDuplicate : MonoBehaviour
    {
        public GameObject Run()
        {
            return StaticMeshDuplicator.Duplicate(gameObject);
        }

        public void SetAllUV(int channel, Vector2 uv)
        {
            switch (channel) { 
                case 0:
                    GetComponent<MeshFilter>().mesh.uv = StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv, uv);
                    break;
                case 1:
                    GetComponent<MeshFilter>().mesh.uv2 = StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv2, uv);
                    break;
                case 2:
                    GetComponent<MeshFilter>().mesh.uv3 = StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv3, uv);
                    break;
                case 3:
                    GetComponent<MeshFilter>().mesh.uv4 = StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv4, uv);
                    break;
                case 4:
                    GetComponent<MeshFilter>().mesh.uv5 = StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv5, uv);
                    break;
                case 5:
                    GetComponent<MeshFilter>().mesh.uv6 = StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv6, uv);
                    break;
                default: break;
            
            
            }
        }

        public void AddAllUV(int channel, Vector2 uv)
        {
            switch (channel)
            {
                case 0:
                    GetComponent<MeshFilter>().mesh.uv.Zip(StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv, uv), (x, y) => x + y);
                    break;
                case 1:
                    GetComponent<MeshFilter>().mesh.uv2.Zip(StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv2, uv), (x, y) => x + y);
                    break;
                case 2:
                    GetComponent<MeshFilter>().mesh.uv3.Zip(StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv3, uv), (x, y) => x + y);
                    break;
                case 3:
                    GetComponent<MeshFilter>().mesh.uv4.Zip(StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv4, uv), (x, y) => x + y);
                    break;
                case 4:
                    GetComponent<MeshFilter>().mesh.uv5.Zip(StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv5, uv), (x, y) => x + y);
                    break;
                case 5:
                    GetComponent<MeshFilter>().mesh.uv6.Zip(StaticMeshDuplicator.Populate(GetComponent<MeshFilter>().mesh.uv6, uv), (x, y) => x + y);
                    break;
                default: break;


            }
        }
    }

    public static class StaticMeshDuplicator
    {
        public static GameObject Duplicate(GameObject target)
        {
            GameObject G = UnityEngine.Object.Instantiate(target);
            G.name = "Duplicate from Static";
            var extractedMesh = ExtractSubMesh(target.GetComponent<MeshFilter>().sharedMesh, target.GetComponent<MeshRenderer>().subMeshStartIndex);
            Material newMaterial = new Material(target.GetComponent<MeshRenderer>().material.shader);
            //newMaterial.CopyPropertiesFromMaterial(target.GetComponent<MeshRenderer>().material);
            newMaterial.mainTexture = target.GetComponent<MeshRenderer>().material.mainTexture;
            G.GetComponent<MeshFilter>().mesh = extractedMesh;
            Timestopper.mls.LogInfo("loaded shader: " + target.GetComponent<MeshRenderer>().material.shader.name);
            G.GetComponent<MeshRenderer>().material = newMaterial;
            G.transform.position = new Vector3(0, 2, 0);
            //for (int i= 0; i < G.GetComponent<MeshFilter>().mesh.uv.Length; i++)
            //{
            //    G.GetComponent<MeshFilter>().mesh.uv[i].x /= G.GetComponent<MeshRenderer>().material.GetTexture(0).width;
            //    G.GetComponent<MeshFilter>().mesh.uv[i].y /= G.GetComponent<MeshRenderer>().material.GetTexture(0).height; 
            //}
            return G;
        }

        public static T[] ExtractFromArray<T>(T[] array, int start, int length) {
            T[] result = new T[length];
            Array.Copy(array, start, result, 0, length);
            return result;
        }

        public static T[] Populate<T>(T[] array, object value)
        {
            T[] t = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                t[i] = (T)value;
            }
            return t;
        }

        public static Mesh ExtractSubMesh(Mesh originalMesh, int subMeshIndex)
        {
            // Create a readable copy using GPU readback
            Mesh readableMesh = CreateReadableMeshCopy(originalMesh);

            // Extract the submesh
            SubMeshDescriptor descriptor = readableMesh.GetSubMesh(subMeshIndex);

            Mesh newMesh = new Mesh();
            newMesh.vertices = ExtractFromArray(readableMesh.vertices, descriptor.firstVertex, descriptor.vertexCount);

            var triangles = ExtractFromArray(readableMesh.triangles, descriptor.indexStart, descriptor.indexCount);
            // Remap triangle indices to the new vertex array
            for (int i = 0; i < triangles.Length; i++)
                triangles[i] -= descriptor.firstVertex;
            newMesh.triangles = triangles;

            // Copy other attributes if available
            if (readableMesh.normals != null && readableMesh.normals.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.normals = ExtractFromArray(readableMesh.normals, descriptor.firstVertex, descriptor.vertexCount);
            else
                newMesh.RecalculateNormals();

            if (readableMesh.uv != null && readableMesh.uv.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.uv = ExtractFromArray(readableMesh.uv, descriptor.firstVertex, descriptor.vertexCount);
            if (readableMesh.uv2 != null && readableMesh.uv2.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.uv2 = ExtractFromArray(readableMesh.uv2, descriptor.firstVertex, descriptor.vertexCount);
            if (readableMesh.uv3 != null && readableMesh.uv3.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.uv3 = ExtractFromArray(readableMesh.uv3, descriptor.firstVertex, descriptor.vertexCount);
            if (readableMesh.uv4 != null && readableMesh.uv4.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.uv4 = ExtractFromArray(readableMesh.uv4, descriptor.firstVertex, descriptor.vertexCount);
            if (readableMesh.uv5 != null && readableMesh.uv5.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.uv5 = ExtractFromArray(readableMesh.uv5, descriptor.firstVertex, descriptor.vertexCount);
            if (readableMesh.uv6 != null && readableMesh.uv6.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.uv6 = ExtractFromArray(readableMesh.uv6, descriptor.firstVertex, descriptor.vertexCount);
            if (readableMesh.uv7 != null && readableMesh.uv7.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.uv7 = ExtractFromArray(readableMesh.uv7, descriptor.firstVertex, descriptor.vertexCount);
            if (readableMesh.uv8 != null && readableMesh.uv8.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.uv8 = ExtractFromArray(readableMesh.uv8, descriptor.firstVertex, descriptor.vertexCount);


            if (readableMesh.tangents != null && readableMesh.tangents.Length > descriptor.firstVertex + descriptor.vertexCount - 1)
                newMesh.tangents = ExtractFromArray(readableMesh.tangents, descriptor.firstVertex, descriptor.vertexCount);



            newMesh.RecalculateBounds();
            newMesh.name = $"Generated_From_SubMesh_{subMeshIndex}";

            return newMesh;

        }

        private static Mesh CreateReadableMeshCopy(Mesh originalMesh)
        {
            // Method 1: Try using Graphics.CopyTexture equivalent for meshes
            try
            {
                // Create a temporary render texture approach
                var tempGO = new GameObject("TempMeshReader");
                var mf = tempGO.AddComponent<MeshFilter>();
                var mr = tempGO.AddComponent<MeshRenderer>();

                mf.sharedMesh = originalMesh;
                mr.material = new Material(Shader.Find("Standard"));

                // Force Unity to make the mesh readable by accessing it through the renderer
                var bounds = mr.bounds; // This forces some internal processing

                // Create new mesh and copy what we can
                Mesh newMesh = new Mesh();
                newMesh.name = originalMesh.name + "_Readable";

                // Use the SkinnedMeshRenderer approach to extract data
                var tempSkinned = tempGO.AddComponent<SkinnedMeshRenderer>();
                tempSkinned.sharedMesh = originalMesh;

                // Bake the mesh to get readable data
                var bakedMesh = new Mesh();
                tempSkinned.BakeMesh(bakedMesh);

                UnityEngine.Object.DestroyImmediate(tempGO);
                return bakedMesh;
            }
            catch
            {
                // Fallback: try to create a basic copy
                return originalMesh;
            }
        }
    }
}
