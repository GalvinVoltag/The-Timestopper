using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using System.Collections;
//using Configgy;
//using Configgable.Assets;
using UnityEngine.SceneManagement;
using TMPro;
using SettingsMenu.Models;
using SettingsMenu.Components;
using System.Runtime.InteropServices;
//using TimelessConfig;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using BepInEx.Bootstrap;
using PluginConfig.API;
using PluginConfig.API.Fields;
using UnityEngine.InputSystem.XR.Haptics;

//namespace TimelessConfig //work in progress custom config library: Timeless Config
//{
//    public enum SettingType
//    {
//        Auto,
//        Check,
//        Value,
//        Slider,
//        Dropdown,
//    };

//    public class Config<T>
//    {
//        public static List<object> All = new List<object>();

//        public const int NO_OP = -1;
//        public const int FAILURE = 0;
//        public const int SUCCESS = 1;
//        bool modified = false;  // if the value is modified, needed for when default values are updated
//        bool locked = false;    // if the setting can be changed from the settings menu
//        string _path;           // path of the setting in the menu
//        string _name;           // name of the setting in the menu, \n means locked by default
//        string _tooltip;        // tip of the setting in the menu
//        Type _type = typeof(T); // type of the config
//        T _value;               // current value
//        T _default;             // default value
//        SettingType settingType;// type of the setting used in the menu
//        string[] _list;         // list of available options for if the setting is a dropdown, 0 and 1 are used for min and max slider values if its a slider
//        bool[] _listLocks;      // list of which elements are locked if the setting is a dropdown
//        public event Action<string, object> OnModification;
//        public Config(T defaultvalue, string name = "setting", string path = "", string tooltip = "", SettingType typeOfSetting = SettingType.Auto, string[] list = null, bool islocked = false)
//        {
//            _value = defaultvalue;
//            _default = defaultvalue;
//            _path = path;
//            _name = name.Substring((name[0] == '\n') ? 1 : 0);
//            _tooltip = tooltip;
//            locked = islocked || (name[0] == '\n');
//            _type = typeof(T);
//            All.Add(this);
//            settingType = typeOfSetting;
//            if (list == null)
//                return;
//            _list = new string[list.Length];
//            _listLocks = new bool[list.Length];
//            for (int i = 0; i < list.Length; i++)
//            {
//                _list[i] = list[i].Substring((list[i][0] == '\n') ? 1 : 0);
//                _listLocks[i] = (list[i][0] == '\n');
//            }
//        }

//        public string GetNameTitle()
//        {
//            string slash = "";
//            if (Path != "")
//                slash = "/";
//            return $@"[{Path}{slash}{Name}]";
//        }
//        public string Serialize()
//        {
//            string liststring = "";
//            string lockstring = "";
//            string slash = "";
//            if (Path != "")
//                slash = "/";
//            if (_list != null)
//            {
//                for (int i = 0; i < _list.Length; i++)
//                {
//                    liststring += _list[i] + ", ";
//                    lockstring += (_listLocks[i] ? "true" : "false") + ", ";
//                }
//            }
//            if (_list == null)
//                return $@"
//[{Path}{slash}{Name}]
//type = {type.Name}
//value = {Value.ToString()}
//default = {Default.ToString()}
//locked = {locked}
//modified = {modified}
//tip = {"\""}{Tooltip}{"\""}
//settingType = {SettingType}
//";
//            else
//                return $@"
//[{Path}{slash}{Name}]
//type = {type.Name}
//value = {Value.ToString()}
//default = {Default.ToString()}
//locked = {locked}
//modified = {modified}
//tip = {"\""}{Tooltip}{"\""}
//droplist = [{liststring}]
//settingType = {SettingType}
//droplocks = [{lockstring}]
//";
//        }

//        public void Digest(string serialized)
//        {
//            string[] lines = serialized.Split('\n');
//            Dictionary<string, string> cache = new Dictionary<string, string>();
//            foreach(string s in lines)
//            {
//                if (s.Length >= 1)
//                    cache.Add(s.Split('=')[0].Substring(0, s.Split('=')[0].Length - 1), s.Substring(s.Split('=')[0].Length + 1));
//            }
//            System.ComponentModel.TypeConverter converter;
//            foreach(string s in cache.Keys)
//            {
//                switch(s)
//                {
//                    case "value":
//                        converter = TypeDescriptor.GetConverter(typeof(T));
//                        _value = (T)converter.ConvertFromString(cache["value"]);
//                        break;
//                    case "modified":
//                        converter = TypeDescriptor.GetConverter(typeof(bool));
//                        modified = (bool)converter.ConvertFromString(cache["modified"]);
//                        break;
//                    case "locked":
//                        converter = TypeDescriptor.GetConverter(typeof(bool));
//                        locked = (bool)converter.ConvertFromString(cache["locked"]);
//                        break;
//                    case "droplist":
//                        {
//                            string execution = cache["droplist"].Split('[')[1].Split(']')[0];
//                            _list = execution.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
//                            break;
//                        }
//                    case "droplocks":
//                        {
//                            string[] execution = cache["droplocks"].Split('[')[1].Split(']')[0].Split(',');
//                            converter = TypeDescriptor.GetConverter(typeof(bool));
//                            List<bool> bools = new List<bool>();
//                            foreach (string ent in execution)
//                                if (ent != "")
//                                    bools.Add((bool)converter.ConvertFromString(ent));
//                            _listLocks = bools.ToArray();
//                            break;
//                        }
//                    case "settingType":
//                        converter = TypeDescriptor.GetConverter(typeof(SettingType));
//                        settingType = (SettingType)converter.ConvertFromString(cache["settingType"]);
//                        break;
//                    default:
//                        break;
//                }
//                if (!modified) Restore();
//            }
//        }
//        public void Restore() {     // return to default value
//            Value = _default;
//        }
//        public void Lock() { if (locked == false) OnModification?.Invoke(GetNameTitle(), this);  locked = true; }
//        public void Unlock() { if (locked == true) OnModification?.Invoke(GetNameTitle(), this); locked = false; }
//        public void LockDropdownElement(int id)
//        {
//            if (!_listLocks[id]) OnModification?.Invoke(GetNameTitle(), this);
//            _listLocks[id] = true;
//        }
//        public void LockDropdownElement(string elementName) // not including the \n
//        {   int i = 0;
//            foreach(string s in _list)
//            {
//                if (s == elementName)
//                {
//                    if (!_listLocks[i]) OnModification?.Invoke(GetNameTitle(),this);
//                    _listLocks[i] = true;
//                }
//                i++;
//            }
//        }
//        public void UnlockDropdownElement(int id)
//        {
//            if (_listLocks[id]) OnModification?.Invoke(GetNameTitle(), this);
//            _listLocks[id] = false;
//        }
//        public void UnlockDropdownElement(string elementName) // not including the \n
//        {   int i = 0;
//            foreach (string s in _list)
//            {
//                if (s == "\n" + elementName)
//                    if (_listLocks[i]) OnModification?.Invoke(GetNameTitle(), this);
//                    _listLocks[i] = false;
//                i++;
//            }
//        }
//        public string GetDropdownElement(int id)
//        {
//            return _list[id];
//        }
//        public int GetDropdownElementID(string elementName) // returns -1 in failure, -2 if list is null
//        {
//            if (_list == null) return -2;
//            for (int i = 0; i < _list.Length; i++)
//            {
//                if (_list[i] == elementName)
//                    return i;
//            }
//            return -1;
//        }
//        public int Modify(T newvalue) // use Value if you don't need feedback
//        {
//            if (_value.Equals(newvalue))
//                return NO_OP;
//            Value = newvalue;
//            if (!_value.Equals(newvalue))
//                return FAILURE;
//            OnModification.Invoke(GetNameTitle(), this);
//            return SUCCESS;
//        }
//        public T Value { get { return _value; } set { if (!_value.Equals(value)) OnModification?.Invoke(GetNameTitle(), this); modified = !_value.Equals(_default); _value = value; } }
//        public T Default { get { return _default; } }
//        public Type type { get { return _type; } }
//        public bool Modified { get { return modified; } }
//        public bool Locked { get { return locked; } }
//        public SettingType SettingType { get { return settingType; } }
//        public string Name { get { return _name.Substring((_name[0] == '\n') ? 1 : 0); } }
//        public string Path { get { return _path; } }
//        public string Tooltip { get { return _tooltip; } }
//        public string[] ListElements { get { return _list; } }

//        public static implicit operator T(Config<T> entry) { return entry.value; }
//        //public static implicit operator Config<T>(T value) { return new Config<T>(value); }
//    }

//    public class ConfigManager
//    {
//        Assembly builder;
//        string[] allEntries;
//        string[] metadata;
//        const string version = "1.0.0";
//        public ConfigManager(string ModGUID, string ModName, ManualLogSource mls, string customPath = "") // relative to default config path
//        {
//            mls.LogWarning("TIMELESS CONFIIIIIGGGHHH !!!!!!!!!!!!!!!!!!!!!!!!!!");
//            builder = Assembly.GetCallingAssembly();
//            GUID = (string.IsNullOrEmpty(ModGUID) ? builder.GetName().Name : ModGUID);
//            Name = (string.IsNullOrEmpty(ModName) ? GUID : ModName);
//            metadata = new string[3] {
//                "## Generated by Timeless Config - v" + version,
//                "## Config file created for the mod " + Name,
//                "## GUID " + GUID
//            };
//        }
//        void LoadFromFile()
//        {
//            string path = Path.Combine(Paths.ConfigPath, GUID + ".tcfg");
//            List<string> all = new List<string>();
//            if (!File.Exists(path))
//                return;
//            string[] lines = File.ReadAllLines(path);
//            string entry = "";
//            foreach (string line in lines)
//            {
//                if (line.StartsWith("["))
//                {
//                    if (entry != "")
//                        all.Add(entry);
//                    entry = line;
//                    continue;
//                }
//                if (entry == "")
//                    continue;
//                else
//                    entry += line + "\n";
//            }
//            all.Add(entry);
//            allEntries = all.ToArray();
//        }
//        public string GetEntry(string titleCard)
//        {
//            foreach(string s in allEntries)
//            {
//                if (s.StartsWith(titleCard))
//                    return s;
//            }
//            return null;
//        }
//        public int GetEntryID(string titleCard)
//        {
//            int i = 0;
//            foreach (string s in allEntries)
//            {
//                if (s.StartsWith(titleCard))
//                    return i;
//                i++;
//            }
//            return 0;
//        }
//        public void SaveChangedValue(string titleCard, object instance)
//        {

//            int id = GetEntryID(titleCard);
//            allEntries[id] = instance.GetType().GetMethod("Serialize").Invoke(instance, null) as string;
//            string path = Path.Combine(Paths.ConfigPath, GUID + ".tcfg");
//            File.WriteAllLines(path, metadata.Concat(allEntries));
//        }
//        public void BuildAll(ManualLogSource mls)
//        {
//            mls.LogError("Buildall has been called! ----------------");
//            if ((object)builder != null)
//                builder = Assembly.GetCallingAssembly();


//            List<string> all = new List<string>();

//            mls.LogWarning("loading file");
//            LoadFromFile();

//            Type[] types = builder.GetTypes();
//            foreach(Type type in types)
//            {
//                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
//                foreach (FieldInfo field in fields)
//                {
//                    if (!field.IsStatic)
//                        continue;

//                    if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Config<>))
//                    {
//                        var cInstance = field.GetValue(null);
//                        if (cInstance != null)
//                        {
//                            mls.LogInfo($"Found config: {field.DeclaringType.Name}.{field.Name}, type = {field.DeclaringType.Name}");
//                            string titleCard = cInstance.GetType().GetMethod("GetNameTitle").Invoke(cInstance, null) as string;
//                            string entryString = GetEntry(titleCard);
//                            if (entryString != null)
//                                cInstance.GetType().GetMethod("Digest").Invoke(cInstance, new object[] { entryString });
//                            cInstance.GetType().GetEvent("OnModification").AddEventHandler(cInstance, (Action<string, object>)SaveChangedValue);
//                            all.Add(cInstance.GetType().GetMethod("Serialize").Invoke(cInstance, null) as string);
//                            mls.LogInfo(cInstance.GetType().GetMethod("Serialize").Invoke(cInstance, null) as string);
//                            mls.LogInfo($"config done");
//                        }
//                    }
//                }
//            }
//            allEntries = all.ToArray();
//            string path = Path.Combine(Paths.ConfigPath, GUID + ".tcfg");
//            File.WriteAllLines(path, metadata.Concat(allEntries));
//        }
//        string GUID;
//        string Name;

//    }
//}

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

        public static new string ToString()
        {
            TimestopperProgress progress = Read();
            return $@"Timestopper saved progress:
            - has arm: {progress.hasArm}
            - equipped: {progress.equippedArm}
            - firstwarning: {progress.firstWarning}
            - upgrades: {progress.upgradeCount}
            - max time: {progress.maxTime} ";
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
        public static object ArmStatus(TProgress id)
        {
            TimestopperProgress progress = Read();
            switch (id)
            {
                case TProgress.hasArm:
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
                    return 150000 + progress.upgradeCount * 66000;
                default:
                    return null;
            }
        }
        public static void UpgradeArm()
        {
            TimestopperProgress progress = Read();
            GameProgressSaver.AddMoney(-progress.upgradeCost);
            progress.maxTime += 1 + 1 / (progress.upgradeCount + 0.5f);
            progress.upgradeCount++;
            Write(progress);
        }
        public static void ForceDownngradeArm()
        {
            TimestopperProgress progress = Read();
            while (progress.upgradeCount > Timestopper.maxUpgrades.value)
            {
                progress.upgradeCount--;
                progress.maxTime -= 1 + 1 / (progress.upgradeCount + 0.5f);
            }
            Write(progress);
        }
        public static void AceptWarning()
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
            Timestopper.Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.SetActive(true);
            Timestopper.Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.GetComponent<Animator>().Play("Pickup");
            Timestopper.mls.LogInfo("Received Golden Arm");
            Write(progress);
        }
        public static void ChangeEquipmentStatus()
        {
            Timestopper.mls.LogWarning("Dum dum gumgineashi");
            if (Timestopper.LatestTerminal != null)
                EquipArm(Timestopper.LatestTerminal.transform.Find("Canvas/Background/Main Panel/Weapons/" +
              "Arm Window/Variation Screen/Variations/Arm Panel (Gold)/Equipment/Equipment Status/Text (TMP)").GetComponent<TextMeshProUGUI>().text[0] == 'E');
            else
                Timestopper.mls.LogWarning("LatestTerminal is Null!");
        }
        public static void EquipArm(bool equipped)
        {
            TimestopperProgress progress = Read();
            if (Playerstopper.Instance.GoldArm == null)
                return;
            if (progress.hasArm)
            {
                progress.equippedArm = equipped;
                Playerstopper.Instance.GoldArm.SetActive(equipped);
                Timestopper.mls.LogInfo("Gold Arm Equipment Status changed: " + progress.equippedArm.ToString());
            }
            else
                Timestopper.mls.LogError("Invalid request of arm equipment, user doesn't have the arm!");
            Write(progress);
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
        public const string Version = "1.0.0";

        private readonly Harmony harmony = new Harmony(GUID);
        public static Timestopper Instance;

        //private ConfigBuilder config;
        public static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(Name);
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
        public static AudioClip[] TimestopSounds = new AudioClip[4] { null, null, null, null };
        public static AudioClip[] StoppedTimeAmbiences = new AudioClip[4] { null, null, null, null };
        public static AudioClip[] TimestartSounds = new AudioClip[4] { null, null, null, null };
        public static Texture2D armGoldLogo;
        public static Texture2D armGoldText;
        public static Texture2D armGoldColor;
        public static Texture2D newRoomColor;
        public static GameObject armGoldObj;
        public static GameObject newRoomObj;
        public static GameObject newRoom;
        public static Animator armGoldAnimator = new Animator();
        public static RuntimeAnimatorController armGoldAC;
        public AssetBundle bundle = null;

        // vvvvvvvvvvvvv REFERENCES vvvvvvvvvvvvvvvvvvvvvv\\
        private GameObject[] TimeHUD = new GameObject[3];
        public static GameObject Player;
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
        public static KeyCodeField stopKey;
        public static EnumField<SoundTypeA> stopSound;
        public static EnumField<SoundTypeB> stoppedSound;
        public static EnumField<SoundTypeC> startSound;
        public static FloatField stopSpeed;
        public static FloatField startSpeed;
        public static FloatField affectSpeed;
        public static FloatField animationSpeed;
        public static FloatSliderField soundEffectVolume;
        public static BoolField filterMusic;
        public static FloatSliderField stoppedMusicPitch;
        public static FloatSliderField stoppedMusicVolume;
        public static BoolField grayscale;
        public static FloatSliderField grayscaleAmount;
        public static BoolField exclusiveGrayscale;
        public static BoolField timestopHardDamage;
        public static IntField maxUpgrades;
        public static BoolField forceDowngrade;
        public static BoolField specialMode;
        //---------------------technical stuff--------------------\\
        public static FloatField lowerTreshold; //2.0f
        public static FloatField blindScale; //0.4f
        public static FloatField refillMultiplier; //0.12f
        public static FloatField bonusTimeForParry;
        public static FloatField antiHpMultiplier;
        //-------------------------colors--------------------------\\
        public static ColorField timeJuiceColorNormal;
        public static ColorField timeJuiceColorInsufficient;
        public static ColorField timeJuiceColorUsing;
        public static ColorField timeJuiceColorNoCooldown;

        private PluginConfigurator config;


        // {{{{{{{{{{{ FOR TESTING }}}}}}}}}}}}}}}}}
        //public static Config<float> testfloat = new Config<float>(1, "test one");
        //public static Config<float> testfloat2 = new Config<float>(2, "test two");
        //public static Config<bool> testbool = new Config<bool>(false, "bool option");
        //public static ConfigManager configmanager = new ConfigManager(GUID, Name, mls);


        void Awake()
        {
            if (Instance == null) { Instance = this; }

            mls.LogInfo("The Timestopper has awakened!");
            InitializeConfig();

            harmony.PatchAll();

            base.StartCoroutine(LoadBundle());   //Load all assets

            //***********DEBUG**************\\
            TheCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            TheCube.name = "The Cube";

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        void InitializeConfig()
        {
            if (config == null)
            {
                config = PluginConfigurator.Create(Name, GUID);

                PluginConfig.API.Decorators.ConfigSpace space0 = new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 6);
                PluginConfig.API.Decorators.ConfigHeader general = new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- GENERAL --");

                stopKey = new KeyCodeField(config.rootPanel, "Timestopper Key", "stopkey", KeyCode.V);
                timestopHardDamage = new BoolField(config.rootPanel, "Timestop Hard Damage", "harddamage", true); // reverse input
                stopSpeed = new FloatField(config.rootPanel, "Timestop Speed", "stopspeed", 0.6f);
                startSpeed = new FloatField(config.rootPanel, "Timestart Speed", "startspeed", 0.8f);
                affectSpeed = new FloatField(config.rootPanel, "Interaction Speed", "interactionspeed", 1.0f);
                animationSpeed = new FloatField(config.rootPanel, "Animation Speed", "animationspeed", 1.3f);

                PluginConfig.API.Decorators.ConfigSpace space1 = new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                PluginConfig.API.Decorators.ConfigHeader graphic = new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- GRAPHICS --");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    mls.LogWarning("Linux OS detected, grayscale effects are turned off by default!");
                    grayscale = new BoolField(config.rootPanel, "Grayscale Effect", "doGrayscale", false);
                }  else  {
                    grayscale = new BoolField(config.rootPanel, "Grayscale Effect", "doGrayscale", true); }
                ConfigDivision grayscaleOptions = new ConfigDivision(config.rootPanel, "grayscaleOptions");
                grayscale.onValueChange += (BoolField.BoolValueChangeEvent e) => { grayscaleOptions.interactable = e.value; };
                exclusiveGrayscale = new BoolField(grayscaleOptions, "Exclusive Grayscale", "exclusivegrayscale", true);
                grayscaleAmount = new FloatSliderField(grayscaleOptions, "Grayscale Amount", "grayscaleamount", new Tuple<float, float>(0, 2), 1.0f);

                PluginConfig.API.Decorators.ConfigSpace space2 = new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                PluginConfig.API.Decorators.ConfigHeader audio = new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- AUDIO --");

                soundEffectVolume = new FloatSliderField(config.rootPanel, "Sound Effects Volume", "effectvolume", new Tuple<float, float>(0, 2), 1);
                stoppedMusicPitch = new FloatSliderField(config.rootPanel, "Music Pitch in Stopped Time", "musicpitch", new Tuple<float, float>(0, 1), 0.6f);
                stoppedMusicVolume = new FloatSliderField(config.rootPanel, "Music volume in Stopped Time", "musicvolume", new Tuple<float, float>(0, 1), 0.8f);
                filterMusic = new BoolField(config.rootPanel, "Filter Music in Stopped Time", "filtermusic", false);
                stopSound = new EnumField<SoundTypeA>(config.rootPanel, "Timestop Sound", "timestopprofile", SoundTypeA.Classic);
                stoppedSound = new EnumField<SoundTypeB>(config.rootPanel, "Stopped Time Ambience", "ambienceprofile", SoundTypeB.Classic);
                startSound = new EnumField<SoundTypeC>(config.rootPanel, "Timestart Sound", "timestartprofile", SoundTypeC.Classic);

                PluginConfig.API.Decorators.ConfigSpace space3 = new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                PluginConfig.API.Decorators.ConfigHeader gameplay = new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- GAMEPLAY --");

                maxUpgrades = new IntField(config.rootPanel, "Maximum Number of Upgrades", "maxupgrades", 10);
                refillMultiplier = new FloatField(config.rootPanel, "Passive Income Multiplier", "refillmultiplier", 0.1f);
                bonusTimeForParry = new FloatField(config.rootPanel, "Time Juice Refill Per Parry", "bonustimeperparry", 1.0f);
                specialMode = new BoolField(config.rootPanel, "Special Mode", "specialmode", false);
                specialMode.interactable = false;

                PluginConfig.API.Decorators.ConfigSpace space4 = new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                PluginConfig.API.Decorators.ConfigHeader colors = new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- COLORS --");

                timeJuiceColorNormal = new ColorField(config.rootPanel, "Time Juice Bar Normal Color", "timejuicecolornormal", new Color(1, 1, 0, 1));
                timeJuiceColorInsufficient = new ColorField(config.rootPanel, "Time Juice Bar Insufficient Color", "timejuicecolorinsufficient", new Color(1, 0, 0, 1));
                timeJuiceColorUsing = new ColorField(config.rootPanel, "Time Juice Bar Draining Color", "timejuicecolorusing", new Color(1, 0.6f, 0, 1));
                timeJuiceColorNoCooldown = new ColorField(config.rootPanel, "Time Juice Bar No Cooldown Color", "timejuicecolornocooldown", new Color(0, 1, 1, 1));

                PluginConfig.API.Decorators.ConfigSpace space5 = new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 4);
                PluginConfig.API.Decorators.ConfigHeader advanced = new PluginConfig.API.Decorators.ConfigHeader(config.rootPanel, "-- ADVANCED OPTIONS --");

                ConfigPanel advancedOptions = new ConfigPanel(config.rootPanel, "ADVANCED", "advancedoptions");
                PluginConfig.API.Decorators.ConfigSpace space6 = new PluginConfig.API.Decorators.ConfigSpace(config.rootPanel, 8);
                forceDowngrade = new BoolField(advancedOptions, "Force Downgrade Arm", "forcedowngrade", true);
                lowerTreshold = new FloatField(advancedOptions, "Min Time Juice to Stop Time", "lowertreshold", 2.0f);
                blindScale = new FloatField(advancedOptions, "Blinding TimeScale", "blindscale", 0.2f);
                antiHpMultiplier = new FloatField(advancedOptions, "Hard Damage Buildup Multiplier", "antihpmultiplier", 30);

            }
        }
        void InitializeShaders()
        {
            //.....................SHADERWORK..........................\\
            Camera c = Player.transform.Find("Main Camera/Virtual Camera").GetComponent<Camera>();
            if (c.gameObject.GetComponent<Grayscaler>() == null)
                c.gameObject.AddComponent<Grayscaler>();
            c.gameObject.GetComponent<Grayscaler>().DoIt();
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
            using (var stream = assembler.GetManifestResourceStream("The_Timestopper.timestopper_assets_all.bundle"))
            {
                if (bundle == null)
                    bundle = AssetBundle.LoadFromStream(stream);
                else
                    mls.LogInfo("Bundle is already loaded!");
                if (bundle == null)
                {
                    mls.LogError("AssetBundle failed to load!");
                    yield break;
                }
                mls.LogInfo(">Loading bundle asyncroniously:");

                mls.LogInfo(">All assets in bundle:");
                foreach (string assetName in bundle.GetAllAssetNames())
                {
                    mls.LogInfo("-->" + assetName);
                }
                grayscaleShader = bundle.LoadAsset<Shader>("Assets/BundledAssets/Grayscale.shader");
                if (grayscaleShader == null) mls.LogError("Failed to load Grayscale shader");

                slappShader = bundle.LoadAsset<Shader>("Assets/BundledAssets/Slapp.shader");
                if (slappShader == null) mls.LogError("Failed to load Slapp shader");

                TimestopSounds[(int)SoundTypeA.Classic] = bundle.LoadAsset<AudioClip>("Assets/BundledAssets/Audio/TimeStop-Classic.mp3");
                if (TimestopSounds[(int)SoundTypeA.Classic] == null) mls.LogError("Failed to load TimeStop-Classic");

                TimestopSounds[(int)SoundTypeA.Alternate] = bundle.LoadAsset<AudioClip>("Assets/BundledAssets/Audio/TimeStop-Alternate.mp3");
                if (TimestopSounds[(int)SoundTypeA.Alternate] == null) mls.LogError("Failed to load TimeStop-Alternate");

                TimestopSounds[(int)SoundTypeA.Za_Warudo] = bundle.LoadAsset<AudioClip>("Assets/BundledAssets/Audio/TimeStop-ZaWarudo.mp3");
                if (TimestopSounds[(int)SoundTypeA.Za_Warudo] == null) mls.LogError("Failed to load TimeStop-ZaWarudo");

                StoppedTimeAmbiences[(int)SoundTypeB.Classic] = bundle.LoadAsset<AudioClip>("Assets/BundledAssets/Audio/TimeJuice-Classic.mp3");
                if (StoppedTimeAmbiences[(int)SoundTypeB.Classic] == null) mls.LogError("Failed to load TimeJuice-Classic");

                StoppedTimeAmbiences[(int)SoundTypeB.Alternate] = bundle.LoadAsset<AudioClip>("Assets/BundledAssets/Audio/TimeJuice-Alternate.mp3");
                if (StoppedTimeAmbiences[(int)SoundTypeB.Alternate] == null) mls.LogError("Failed to load TimeJuice-Alternate");

                StoppedTimeAmbiences[(int)SoundTypeB.Ambience] = bundle.LoadAsset<AudioClip>("Assets/BundledAssets/Audio/TimeJuice-Ambience.mp3");
                if (StoppedTimeAmbiences[(int)SoundTypeB.Ambience] == null) mls.LogError("Failed to load TimeJuice-Ambience");

                TimestartSounds[(int)SoundTypeC.Classic] = bundle.LoadAsset<AudioClip>("Assets/BundledAssets/Audio/TimeStart-Classic.mp3");
                if (TimestartSounds[(int)SoundTypeC.Classic] == null) mls.LogError("Failed to load TimeStart-Classic");

                TimestartSounds[(int)SoundTypeC.Alternate] = bundle.LoadAsset<AudioClip>("Assets/BundledAssets/Audio/TimeStart-Alternate.mp3");
                if (TimestartSounds[(int)SoundTypeC.Alternate] == null) mls.LogError("Failed to load TimeStart-Alternate");

                armGoldLogo = bundle.LoadAsset<Texture2D>("Assets/BundledAssets/Timestopper.png");
                if (armGoldLogo == null) mls.LogError("Failed to load Timestopper logo");
                if (config != null) config.icon = Sprite.Create(armGoldLogo, new Rect(0, 0, 512, 512), new Vector2(256, 256));

                //var prefab = bundle.LoadAsset<GameObject>("Assets/BundledAssets/gold arm model.fbx");
                //if (prefab != null) armGoldObj = Instantiate(prefab, transform);
                //else mls.LogError("Failed to load Timestopper prefab");

                armGoldColor = bundle.LoadAsset<Texture2D>("Assets/BundledAssets/GoldArmColor.png");
                if (armGoldColor == null) mls.LogError("Failed to load Gold Arm Model");

                armGoldText = bundle.LoadAsset<Texture2D>("Assets/BundledAssets/TextmodeV1Arm4.png");
                if (armGoldText == null) mls.LogError("Failed to load Gold Arm Textmode");

                armGoldObj = bundle.LoadAsset<GameObject>("Assets/BundledAssets/gold arm model.fbx");
                if (armGoldObj == null) mls.LogError("Failed to load Gold Arm Model");

                armGoldAC = bundle.LoadAsset<RuntimeAnimatorController>("Assets/BundledAssets/AC.controller");
                if (armGoldAC == null) mls.LogError("Failed to load AC controller");

                //ConfigMenuPrefab = Instantiate(bundle.LoadAsset<GameObject>("Assets/BundledAssets/Prefabs/configsmenu.prefab"), transform);
                //if (ConfigMenuPrefab == null) mls.LogError("Failed to load Config Menu Prefab");

                //newRoomColor = bundle.LoadAsset<Texture2D>("Assets/BundledAssets/Texture2D_11_1.png");
                //if (newRoomColor == null) mls.LogError("Failed to load New Room Texture");

                //newRoomObj = bundle.LoadAsset<GameObject>("Assets/BundledAssets/Timestopper Room.fbx");
                //if (newRoomObj == null) mls.LogError("Failed to load New Room Obj");

                yield return null;

                //bundle.Unload(false);       //Release the bundle so it doesn't cause leakage?
                
                mls.LogInfo("Asset extraction status:");
            }
            mls.LogInfo("Bundle extraction done!");
            if (Player == null)
            {
                mls.LogError("Player is null, shaders cannot apply!");
                LoadDone = true;
                LoadStarted = false;
                yield break;
            }

            if (grayscaleShader.isSupported && grayscale.value)
                InitializeShaders();

            TimeColor = Timestopper.timeJuiceColorNormal.value;
            TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
            LoadDone = true;
            LoadStarted = false;
        }
        public IEnumerator EnsureShader()
        {
            if (!grayscale.value)
            {
                mls.LogWarning("Ahader loading interrupted because grayscale effect is off!");
                yield break;
            }
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
            if (ShopComp == null)
            {
                mls.LogError("Shop Component is null, cannot update terminal!");
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
            if (Player == null)  //Never lose track of the player, NEVER!
            {
                Player = MonoSingleton<NewMovement>.Instance.gameObject;
                if (Player == null) return;
                if (Playerstopper.Instance == null) Player.AddComponent<Playerstopper>();
            }
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
            mls.LogInfo("shearching ULTRAKILL shaders for: " + name);
            foreach (Shader shader in Resources.FindObjectsOfTypeAll<Shader>())
            {
                mls.LogInfo("-> " + shader.name);
                if (shader.name == name)
                {
                    mls.LogError("found shader!");
                    return shader;
                }
            }
            mls.LogError("Couldn't find!");
            return null;
        }
        public static Texture2D GrayscaleImage(Texture2D original)
        {
            if (!original.isReadable) {
                mls.LogError("Texture is not readable!");
                return null;
            }
            Texture2D grayscaledTexture = new Texture2D(original.width, original.height, original.format, false);
            Color[] pixels = original.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                float gray = (pixel.r + pixel.g + pixel.b) / 3;
                pixels[i] = new Color(gray, gray, gray, pixel.a);
            }

            grayscaledTexture.SetPixels(pixels);
            grayscaledTexture.Apply();

            return grayscaledTexture;
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
        public IEnumerator ModifySettingsPage() // for experiments, don't read
        {
            mls.LogWarning("creating instances...");
            SettingsPage sp = SettingsPage.CreateInstance<SettingsPage>();
            SettingsCategory sc = SettingsCategory.CreateInstance<SettingsCategory>();
            sc.items = new List<SettingsItem> { };
            //foreach (object C in CM.BuildAll())
            //{
            //    SettingsItem item = SettingsItem.CreateInstance<SettingsItem>();
            //    //Config<float> tester = new Config<float>(0);
            //    item.buttonLabel = (string)C.GetType().GetProperty("Name").GetValue(C) + "button";
            //    item.label = (string)C.GetType().GetProperty("Name").GetValue(C);
            //    item.name = (string)C.GetType().GetProperty("Name").GetValue(C);
            //    item.style = SettingsItemStyle.Normal;
            //    item.itemType = SettingsItemType.Slider;
            //    item.sliderConfig = new SliderConfig();
            //    sc.items.Add(item);
            //    //item.dropdownList = new string[] { "test1", "test2", "test3" };
            //    //item.sideNote = "this is a literal side note, you can change it or set it to any value you want";
            //    //item.dropdownList = new string[] { "one", "two", "three" };
            //}
            mls.LogWarning("Created instances");
            sc.title = "Test Title";
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
            mls.LogInfo(TimestopperProgress.ToString());
            if (scene.name == "b3e7f2f8052488a45b35549efb98d902" /*main menu*/)
            {
                mls.LogWarning("main menu loaded");
                GameObject goldArmText = Instantiate(FindRootGameObject("Canvas").transform.Find("Main Menu (1)/V1/Knuckleblaster").gameObject, FindRootGameObject("Canvas").transform.Find("Main Menu (1)/V1"));
                goldArmText.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(armGoldText, new Rect(0, 0, armGoldText.width, armGoldText.height), Vector2.zero);
                goldArmText.name = "Timestopper";
                goldArmText.transform.localPosition = Vector3.zero;
                goldArmText.transform.localScale = Vector3.one * 0.93f;
                goldArmText.SetActive((bool)TimestopperProgress.ArmStatus(TProgress.hasArm));
                GameObject goldArmText2 = Instantiate(FindRootGameObject("Canvas").transform.Find("Difficulty Select (1)/Info Background/V1/Knuckleblaster").gameObject, FindRootGameObject("Canvas").transform.Find("Difficulty Select (1)/Info Background/V1"));
                goldArmText2.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(armGoldText, new Rect(0, 0, armGoldText.width, armGoldText.height), Vector2.zero);
                goldArmText2.GetComponent<UnityEngine.UI.Image>().color = new Color(0.125f, 0.125f, 0.125f, 1);
                goldArmText2.transform.localPosition = Vector3.zero;
                goldArmText2.transform.localScale = Vector3.one * 0.93f;
                goldArmText2.SetActive((bool)TimestopperProgress.ArmStatus(TProgress.hasArm));
            }
            TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
            if (scene.name != "b3e7f2f8052488a45b35549efb98d902" /*main menu*/ &&
                scene.name != "Bootstrap" && LoadDone &&
                scene.name != "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/)
            {
                if (Playerstopper.Instance == null)
                    MonoSingleton<NewMovement>.Instance.gameObject.GetComponent<Playerstopper>();
                if (forceDowngrade.value)
                    TimestopperProgress.ForceDownngradeArm();
                StartCoroutine(LoadHUD());
                StartCoroutine(EnsureShader());
                StatsManager.checkpointRestart += ResetGoldArm;
                if (firstLoad && !(bool)TimestopperProgress.ArmStatus(TProgress.hasArm)) //display the message for newcomers
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(ARM_NEW_MESSAGE, "", "", 2);
                    messageTimer.done += () =>
                    {
                        MonoSingleton<HudMessageReceiver>.Instance.Invoke("Done", 0);
                        firstLoad = false;
                    };
                    messageTimer.SetTimer(6, true);
                }
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem("timestopper.timestop", TIMESTOP_STYLE); // register timestop style
                //newRoom = Instantiate(newRoomObj, transform);
                //Shader s = FindShader("ULTRAKILL/Master");
                //if (s != null)
                //{
                //    newRoom.GetComponent<MeshRenderer>().material = new Material(s);
                //    newRoom.GetComponent<MeshRenderer>().material.mainTexture = newRoomColor;
                //}
            }
            else
            {
                timeStopper = CStopTime(0);
                timeStarter = CStartTime(0);
            }
            // Update the Level
            if (ConfirmLevel("VIOLENCE /// FIRST")) // Add the door to the level
            {
                mls.LogInfo("violence level detected");
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
                itemArm.GetComponentInChildren<SkinnedMeshRenderer>().material = new Material(FindShader("ULTRAKILL/Master"));
                itemArm.GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = armGoldColor;
                itemArm.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                itemArm.transform.GetChild(1).GetComponentInChildren<SkinnedMeshRenderer>().rayTracingMode = UnityEngine.Experimental.Rendering.RayTracingMode.DynamicGeometry;
                itemArm.transform.localScale = Vector3.one * 5;
                itemArm.transform.localPosition = new Vector3(0f, 1f, 0f);
                itemArm.transform.GetChild(0).localPosition = new Vector3(0f, 0f, 0f);
                itemArm.transform.localEulerAngles = new Vector3(0.0f, 0f, 0f);
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
            if (scene.name == "9240e656c89994d44b21940f65ab57da")
            {
                cybergrind = true;
                if (Chainloader.PluginInfos.ContainsKey("dev.flazhik.jukebox"))
                {
                    Compatability_JukeBox = true;
                    Type Comp = Type.GetType("Jukebox.Components.NowPlayingHud, Jukebox");
                    if (Comp != null)
                    {
                        UnityEngine.Component C = FindObjectOfType(Comp) as UnityEngine.Component;
                        if (C != null) C.gameObject.transform.localPosition += new Vector3(0, 60, 0);
                        else mls.LogError("Component C is null!");
                    }
                    else mls.LogError("Could not get Jukebox.Components.NowPlayingHud");
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
            if (scene.name != "b3e7f2f8052488a45b35549efb98d902" /*main menu*/ &&
                scene.name != "Bootstrap" && LoadDone &&
                scene.name != "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/ &&
                realTimeScale < 1)
            {
                StartTime(0, true);
            }
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
            //Player.transform.Find("Main Camera/Punch/Arm Gold").gameObject.GetComponent<Animator>().speed = animationSpeed.value;
            if (filterMusic.value)
                MonoSingleton<MusicManager>.Instance.FilterMusic();
            Physics.simulationMode = SimulationMode.Script;
            foreach (Rigidbody R in FindObjectsOfType<Rigidbody>())  //Save everyone's states and freeze them except Player
            {
                GameObject G = R.gameObject;
                if (Playerstopper.Instance.gameObject != G)
                {
                    if (G.GetComponent<RigidbodyStopper>() == null)
                        G.AddComponent<RigidbodyStopper>();
                    G.GetComponent<RigidbodyStopper>().enabled = true;
                    G.GetComponent<RigidbodyStopper>().Freeze();
                }
            }
            foreach (AudioSource A in FindObjectsOfType<AudioSource>())  //Add the pitching effect to all AudioSources except the ones on Player
                if (A.gameObject.GetComponent<Audiopitcher>() == null && A.gameObject.transform.parent != Player.transform)
                    A.gameObject.AddComponent<Audiopitcher>();
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
            yield break;
        }
        public IEnumerator CStartTime(float speed, bool preventStyle = false)
        {
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
                TimeColor = timeJuiceColorUsing.value;
            }
            else
            {
                if (realTimeScale <= 0.3f)
                    Player.GetComponent<NewMovement>().walking = false;
                TimeLeft += Time.deltaTime * refillMultiplier.value;
                if (TimeLeft > (float)TimestopperProgress.ArmStatus(TProgress.maxTime))
                    TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
                if (TimeLeft < lowerTreshold.value)
                    TimeColor = timeJuiceColorInsufficient.value;
                else
                    TimeColor = timeJuiceColorNormal.value;
            }
            if (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown)
            {
                TimeLeft = (float)TimestopperProgress.ArmStatus(TProgress.maxTime);
                TimeColor = timeJuiceColorNoCooldown.value;
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
                    }
                    else if (TimeHUD[2].transform.Find("Text (TMP)").gameObject.activeInHierarchy)
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
        public static bool frameLaterer = false;
        private void HandleHitstop()
        {
            if ((float)AccessTools.Field(typeof(TimeController), "currentStop").GetValue(MonoSingleton<TimeController>.Instance) <= 0)
            {
                if (playerTimeScale <= 0)
                {
                    playerTimeScale = 1;
                    Time.timeScale = 0;
                    MonoSingleton<TimeController>.Instance.timeScaleModifier = 1;
                    (AccessTools.Field(typeof(TimeController), "parryFlash").GetValue(MonoSingleton<TimeController>.Instance) as GameObject).SetActive(false);
                    foreach (Transform child in Player.transform.Find("Main Camera/New Game Object").transform)
                        Destroy(child.gameObject);
                }
                return;
            }
            else
            {
                frameLaterer = true;
                playerTimeScale = 0;
                Time.timeScale = 0;
            }
        }
        private bool menuOpenLastFrame = false;
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
        float time = 0;
        private void LateUpdate()
        {
            if (SceneManager.GetActiveScene().name == "b3e7f2f8052488a45b35549efb98d902" /*main menu*/ ||
            SceneManager.GetActiveScene().name == "Bootstrap" ||
            SceneManager.GetActiveScene().name == "241a6a8caec7a13438a5ee786040de32" /*newblood screen*/)
            {
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
            UpdateTimeJuice();
            UpdateHUD();
            if (MonoSingleton<NewMovement>.Instance.dead && TimeStop)
            {
                StartTime(0);
            }
            if (TimeStop)
            {
                foreach (Rigidbody R in FindObjectsOfType<Rigidbody>())  // Freeze anything that has recently been created
                {
                    if (R.gameObject.GetComponent<RigidbodyStopper>() == null && R.gameObject != MonoSingleton<NewMovement>.Instance.gameObject)
                    {
                        R.gameObject.AddComponent<RigidbodyStopper>();
                        R.gameObject.GetComponent<RigidbodyStopper>().Freeze();
                    }
                }
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

    public class Audiopitcher : MonoBehaviour
    {
        AudioSource audio;
        float originalPitch = 1;
        bool music = false;
        bool awake = true;
        public void LateUpdate()
        {
            if (GetComponent<AudioSource>() == null)
                return;
            if (awake)
            {
                audio = GetComponent<AudioSource>();
                originalPitch = audio.pitch;
                if ((Timestopper.cybergrind && gameObject == GameObject.Find("MusicChanger/Battle Theme")) || (audio == MonoSingleton<MusicManager>.Instance.targetTheme))
                    music = true;
                awake = false;
            }
            if (Timestopper.realTimeScale == 1.0f)
            {
                audio.pitch = originalPitch;
                originalPitch = audio.pitch;
                return;
            }
            if (!music || audio == MonoSingleton<MusicManager>.Instance.targetTheme)
            {
                audio.pitch = originalPitch * Timestopper.realTimeScale;
            } else
            {
                audio.pitch = Timestopper.realTimeScale * (1 - Timestopper.stoppedMusicPitch.value) + Timestopper.stoppedMusicPitch.value;
                MonoSingleton<MusicManager>.Instance.volume = Timestopper.realTimeScale * (1 - Timestopper.stoppedMusicVolume.value) + Timestopper.stoppedMusicVolume.value;
            }


            //if (GetComponent<AudioSource>() != null && Timestopper.realTimeScale <= 1.0f)
            //{
            //    if (transform.parent != null)
            //    {
            //        if (GetComponent<AudioSource>() == MonoSingleton<MusicManager>.Instance.targetTheme)
            //            if (Timestopper.realTimeScale > 0.0f)
            //            {
            //                GetComponent<AudioSource>().pitch = Timestopper.realTimeScale * (1 - Timestopper.stoppedMusicPitch.value) + Timestopper.stoppedMusicPitch.value;
            //                MonoSingleton<MusicManager>.Instance.volume = Timestopper.realTimeScale * (1 - Timestopper.stoppedMusicVolume.value) + Timestopper.stoppedMusicVolume.value;
            //            }
            //            else
            //            {
            //                GetComponent<AudioSource>().pitch = Timestopper.stoppedMusicPitch.value;
            //                MonoSingleton<MusicManager>.Instance.volume = Timestopper.stoppedMusicVolume.value;
            //            }
            //    }
            //    else
            //    {
            //        if (Timestopper.realTimeScale > 0.0f)
            //        {
            //            GetComponent<AudioSource>().pitch = Timestopper.realTimeScale;
            //        }
            //        else
            //            GetComponent<AudioSource>().pitch = 0;
            //    }
            //}
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
            if ((int)TimestopperProgress.ArmStatus(TProgress.upgradeCount) < Timestopper.maxUpgrades.value)
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
            }
            else
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
            if (!Timestopper.grayscale.value || !Timestopper.grayscaleShader.isSupported)
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
                    Timestopper.grayscale.value = false;
                }
            if (Timestopper.grayscale.value)
                transform.parent.Find("HUD Camera").gameObject.GetComponent<Camera>().enabled = false;
            else
                transform.parent.Find("HUD Camera").gameObject.GetComponent<Camera>().enabled = true;
        }
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material != null && _slapper != null && Timestopper.grayscale.value)
            {
                if (Timestopper.exclusiveGrayscale.value)
                    Grayscale = (1 - Timestopper.realTimeScale);
                else
                    Grayscale = 1 - Time.timeScale;

                RenderTexture rt = RenderTexture.active;  //Clear R to empty
                RenderTexture.active = R;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = rt;

                _material.SetFloat("_Grayscale", Grayscale * Timestopper.grayscaleAmount.value);
                camera.CopyFrom(transform.parent.Find("HUD Camera").gameObject.GetComponent<Camera>());
                camera.targetTexture = R;
                camera.Render();
                _slapper.SetTexture("_SlappedTex", R);
                Graphics.Blit(source, T, _material);
                Graphics.Blit(T, destination, _slapper);

            }
            else
            {
                if (!Timestopper.grayscale.value || !Timestopper.grayscaleShader.isSupported)
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
    public class RigidbodyStopper : MonoBehaviour    // Added to all Rigidbodies when time stops
    {
        public float localScale = 1.0f; // local timescale, so that coins and stuff freeze slowly
        bool gravity = false;
        bool frozen = false;
        bool byDio = false;
        bool isCoin = false;
        bool isRocket = false;
        public Vector3 unscaledVelocity = new Vector3(0, 0, 0);
        public Vector3 unscaledAngularVelocity = new Vector3(0, 0, 0);
        public Rigidbody R;
        private float time = 0;

        public void Freeze()
        {
            if (GetComponent<Landmine>() != null)
                transform.Find("Trigger").gameObject.SetActive(false);
            if (GetComponent<Chainsaw>() != null)
                if (GetComponent<FixedUpdateCaller>() == null)
                    gameObject.AddComponent<FixedUpdateCaller>();
            if (gameObject.GetComponent<Grenade>() != null)
            {
                isRocket = gameObject.GetComponent<Grenade>().rocket;
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
            else
            {
                this.enabled = false;
                return;
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
            if (GetComponent<Landmine>() != null)
                transform.Find("Trigger").gameObject.SetActive(true);
            if (GetComponent<Chainsaw>() != null)
                R.isKinematic = false;
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
            if (GetComponent<EnemyIdentifier>() != null)
            {
                if (GetComponent<EnemyIdentifier>().hooked && ((List<EnemyType>)AccessTools.Field(typeof(HookArm), "lightEnemies").GetValue(MonoSingleton<HookArm>.Instance)).Contains(GetComponent<EnemyIdentifier>().enemyType))
                {
                    R.isKinematic = false;
                    localScale = 1;
                    unscaledVelocity = R.velocity;
                    return;
                }
            }
            if (isCoin)
            {
                gameObject.GetComponent<SphereCollider>().enabled = true;
                gameObject.GetComponent<BoxCollider>().enabled = true;
                gameObject.GetComponent<Coin>().Invoke("StartCheckingSpeed", 0);
                isCoin = false;
            }
            if (Timestopper.TimeStop && R != null)
            {
                if (GetComponent<Chainsaw>() != null && localScale == 0)
                    R.isKinematic = true;
                if (R.isKinematic || !gameObject.activeInHierarchy)
                {
                    return;
                }
                if (isRocket)
                {
                    R.isKinematic = !GetComponent<Grenade>().frozen;
                    GetComponent<Grenade>().rideable = true;
                    MethodInfo MFixedUpdate = GetComponent<Grenade>().GetType().GetMethod("FixedUpdate",
                                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (MFixedUpdate != null)
                        MFixedUpdate.Invoke(GetComponent<Grenade>(), null);
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
                        if (byDio)
                            localScale -= Timestopper.playerDeltaTime / Timestopper.affectSpeed.value;
                        else
                            localScale -= Timestopper.playerDeltaTime / Timestopper.stopSpeed.value;
                        if (localScale < 0)
                            localScale = 0.0f;
                    }
                    else
                    {
                        MonoSingleton<WeaponCharges>.Instance.rocketFrozen = false;
                        GetComponent<Grenade>().rideable = true;
                        MethodInfo MFixedUpdate = gameObject.GetComponent<Grenade>().GetType().GetMethod("FixedUpdate",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        MFixedUpdate.Invoke(gameObject.GetComponent<Grenade>(), null);
                        transform.Find("FreezeEffect").gameObject.SetActive(true);
                        MonoSingleton<WeaponCharges>.Instance.rocketFrozen = true;
                        if (localScale < 1)
                            localScale += Timestopper.playerDeltaTime / Timestopper.stopSpeed.value;
                        if (localScale > 1)
                            localScale = 1.0f;
                    }
                }
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
                        foreach (ScaleNFade C in gameObject.GetComponentsInChildren<ScaleNFade>())
                        {
                            Destroy(C.gameObject);
                        }
                    }
                    foreach (UnityEngine.Component C in gameObject.GetComponents(typeof(MonoBehaviour)))
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
        public static Playerstopper Instance;
        public GameObject GoldArm;
        public AudioSource audio;
        public float UnscaledDebug = 0.0f;
        public float JumpCooldown = 0.0f;
        public GameObject TheCube;
        public bool frameLaterer = false;
        public PrivateInsideTimer NotJumpingTimer = new PrivateInsideTimer();
        public PrivateInsideTimer JumpReadyTimer = new PrivateInsideTimer();
        NewMovement movement;

        public IEnumerator LoadGoldArm()
        {
            yield return Timestopper.armGoldObj;
            yield return transform.Find("Main Camera/Punch");
            GameObject GoldArm = Instantiate(Timestopper.armGoldObj, transform.Find("Main Camera/Punch"));
            Timestopper.mls.LogInfo("Loaded gold arm: " + GoldArm);
            GoldArm.GetComponentInChildren<SkinnedMeshRenderer>().material = new Material(Timestopper.FindShader("ULTRAKILL/Master"));
            GoldArm.GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = Timestopper.armGoldColor;
            GoldArm.GetComponentInChildren<SkinnedMeshRenderer>().material.EnableKeyword("VERTEX_LIGHTING");
            GoldArm.GetComponentInChildren<SkinnedMeshRenderer>().material.EnableKeyword("_FOG_ON");
            GoldArm.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = true;
            GoldArm.name = "Arm Gold";
            GoldArm.layer = 13;
            foreach (Transform t in GoldArm.transform)
                t.gameObject.layer = 13;
            GoldArm.AddComponent<Animator>();
            GoldArm.GetComponent<Animator>().runtimeAnimatorController = Timestopper.armGoldAC;
            //GoldArm.transform.localEulerAngles = new Vector3(0f, 235f, 20f);
            GoldArm.transform.localEulerAngles = new Vector3(0f, 250f, 12f);
            GoldArm.transform.localScale = new Vector3(10, 10, 10);
            GoldArm.transform.localPosition = new Vector3(-0.6029f, -1.04f, -1.2945f);
            GoldArm.AddComponent<WalkingBob>();
            GoldArm.transform.GetChild(1).gameObject.SetActive(false);
            //GoldArm.SetActive((bool)TimestopperProgress.ArmStatus(TProgress.equippedArm));
            Timestopper.mls.LogInfo("Golden Arm created successfully.");
            yield break;
        }
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
            Instance = this;
            FixedUpdateFix(transform);
            StartCoroutine(LoadGoldArm());
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

        public void PlayRespectiveSound(bool istimestopped)
        {
            if (audio == null)
            {
                audio = transform.Find("Main Camera").gameObject.AddComponent<AudioSource>();
                audio.clip = Timestopper.StoppedTimeAmbiences[(int)Timestopper.stoppedSound.value];
            }
            if (istimestopped != Timestopper.TimeStop)
                if (istimestopped)
                {
                    if (Timestopper.TimestopSounds[(int)Timestopper.stopSound.value] != null)
                        audio.PlayOneShot(Timestopper.TimestopSounds[(int)Timestopper.stopSound.value]);
                    if (Timestopper.StoppedTimeAmbiences[(int)Timestopper.stoppedSound.value] != null)
                    {
                        audio.clip = Timestopper.StoppedTimeAmbiences[(int)Timestopper.stoppedSound.value];
                        audio.Play();
                        audio.loop = true;
                    }
                }
                else
                {
                    audio.Stop();
                    if (Timestopper.TimestartSounds[(int)Timestopper.startSound.value] != null)
                        audio.PlayOneShot(Timestopper.TimestartSounds[(int)Timestopper.startSound.value]);
                }
        }

        private void Update()
        {
            JumpReadyTimer.Update();
            NotJumpingTimer.Update();
            if (Timestopper.specialMode.value)
            {
                if (UnityInput.Current.GetKeyDown(KeyCode.J))
                {
                    GameProgressSaver.AddMoney((int)TimestopperProgress.ArmStatus(TProgress.upgradeCost));
                    Timestopper.mls.LogInfo("MONEY MONEY MONEY");
                }
            }
            //GoldArm.transform.localEulerAngles = new Vector3(0f, 235f, 20f);
            if (GoldArm == null) GoldArm = transform.Find("Main Camera/Punch/Arm Gold").gameObject;
            Vector3 newRot = new Vector3(0f, 250f - (32 * MonoSingleton<FistControl>.Instance.fistCooldown), 12f + (16 * MonoSingleton<FistControl>.Instance.fistCooldown));
            GoldArm.transform.localEulerAngles = (newRot*Timestopper.playerDeltaTime*20 + GoldArm.transform.localEulerAngles) / (1 + Timestopper.playerDeltaTime*20);
            // trigger function
            if (((UnityInput.Current.GetKeyDown(Timestopper.stopKey.value)) && (bool)TimestopperProgress.ArmStatus(TProgress.equippedArm))
                || (Timestopper.TimeStop && Timestopper.TimeLeft <= 0.0f)
                || (movement.dead && Timestopper.TimeStop))
            {
                if (MonoSingleton<OptionsManager>.Instance.paused) //if game paused
                    return;
                if (!Timestopper.TimeStop && !MonoSingleton<FistControl>.Instance.shopping && Timestopper.TimeLeft > Timestopper.lowerTreshold.value)
                {
                    PlayRespectiveSound(true);
                    Timestopper.StopTime(Timestopper.stopSpeed.value);
                    GoldArm.transform.GetChild(1).gameObject.SetActive(true);
                    GoldArm.GetComponent<Animator>().Play("Stop");
                    FixedUpdateFix(transform);
                    Timestopper.mls.LogInfo("Time stops!");
                }
                else
                {
                    PlayRespectiveSound(false);
                    Timestopper.StartTime(Timestopper.startSpeed.value);
                    GoldArm.transform.GetChild(1).gameObject.SetActive(true);
                    GoldArm.GetComponent<Animator>().Play("Release");
                    Timestopper.BlindCheat.Disable();
                    Timestopper.mls.LogInfo("Time resumes.");
                }
            }
            if (movement.dead)
            {
                GoldArm.GetComponent<Animator>().Play("Pickup");
                GoldArm.transform.GetChild(1).gameObject.SetActive(false);
            }
            if (Timestopper.TimeStop)
            {
                if (Timestopper.realTimeScale < Timestopper.blindScale.value && !ULTRAKILL.Cheats.BlindEnemies.Blind)
                {
                    Timestopper.BlindCheat.Enable(GameObject.Find("Cheat Menu").GetComponent<CheatsManager>());
                }
                if (audio != null) // for timestop sound effect to be able to play
                {
                    audio.pitch = 1;
                    transform.Find("Main Camera").GetComponent<AudioSource>().volume = 1;
                }
                if (Timestopper.timestopHardDamage.value)
                {
                    movement.ForceAddAntiHP(Timestopper.antiHpMultiplier.value * Time.unscaledDeltaTime * (1 - Timestopper.realTimeScale), true, true, true, false);
                }
                // The JumpBug fix \\
                if (movement.jumping && movement.falling && !movement.boost)
                {
                    JumpReadyTimer.SetTimer(0.2f, false);
                    NotJumpingTimer.SetTimer(0.25f, false);
                }
            }
            else // if time is not stopped
            {
                if (ULTRAKILL.Cheats.BlindEnemies.Blind)
                {
                    Timestopper.BlindCheat.Disable();
                }
            }
        }


        private void LateUpdate() // make animations work
        {
            if (Timestopper.TimeStop)
            {
                if (Timestopper.playerDeltaTime > 0)
                {
                    foreach (Animator A in FindObjectsOfType<Animator>())   // Make animations work in stopped time when time is stopped
                        if (A.gameObject.transform.IsChildOf(gameObject.transform) && A.updateMode == AnimatorUpdateMode.Normal)
                            A.updateMode = AnimatorUpdateMode.UnscaledTime;
                }
                else
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
                Timestopper.TimeLeft += Timestopper.bonusTimeForParry.value;
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

    [HarmonyPatch(typeof(WeaponCharges), "Update")] public class TranspileWeaponCharges0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[WeaponCharges]=> Update"); }
    [HarmonyPatch(typeof(WeaponCharges), "Charge")] public class TranspileWeaponCharges1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[WeaponCharges]=> Charge"); }
    [HarmonyPatch(typeof(GunControl), "Update")] public class TranspileGunControl { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[GunControl]=> Update"); }
    [HarmonyPatch(typeof(Revolver), "Update")] public class TranspileRevolver0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Revolver]=> Update"); }
    [HarmonyPatch(typeof(Revolver), "LateUpdate")] public class TranspileRevolver1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[Revolver]=> LateUpdate"); }
    [HarmonyPatch(typeof(ShotgunHammer), "UpdateMeter")] public class TranspileShotgunHammer0 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> UpdateMeter"); }
    [HarmonyPatch(typeof(ShotgunHammer), "Update")] public class TranspileShotgunHammer1 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> Update"); }
    [HarmonyPatch(typeof(ShotgunHammer), "LateUpdate")] public class TranspileShotgunHammer2 { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[ShotgunHammer]=> LateUpdate"); }
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
    [HarmonyPatch(typeof(CameraController), "Update")] public class TranspileCameraController { [HarmonyTranspiler] static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => DeltaTimeReplacer.Transpiler(instructions, "[CameraController]=> Update"); }
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