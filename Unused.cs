using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
