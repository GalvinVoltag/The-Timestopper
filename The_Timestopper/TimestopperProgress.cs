using System;
using System.IO;
using The_Timestopper.Internal;
using The_Timestopper.Player;
using TMPro;
using UnityEngine;

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
        private static TimestopperProgress _instance;

        public static TimestopperProgress Instance
        {
            set
            {
                _instance = value;
                Write(_instance);
            }
            get
            {
                if (_instance == null) _instance = Read();
                return _instance;
            }
        }

        public static bool HasArm => Instance.hasArm;
        public static bool EquippedArm => Instance.equippedArm;
        public static bool FirstWarning => Instance.firstWarning;
        
        public static int UpgradeCount => Instance.upgradeCount;
        public static string UpgradeText {
            get { return "<align=\"center\"><color=#FFFF42>" + GenerateTextBar('▮', Instance.upgradeCount) + "</color>"; }
        }
        public static float MaxTime => Instance.maxTime;
        public static float UpgradeCost => 150000 + Instance.upgradeCount * 66000; 
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
        public int upgradeCost => 150000 + upgradeCount * 66000; 
        
        private const string PROGRESS_FILE = "timestopper.state";
        
        private static string GenerateTextBar(char c, int b)
        {
            string s = "";
            for (int i = 0; i < b; i++)
                s += c;
            return s;
        }
        public static void UpgradeArm()
        {
            GameProgressSaver.AddMoney(-Instance.upgradeCost);
            Instance.maxTime += 1 + 1 / (Instance.upgradeCount + 0.5f);
            Instance.upgradeCount++;
            Write(Instance);
        }
        public static void ForceDowngradeArm()
        {
            if (Timestopper.maxUpgrades.value < 0)
                Timestopper.maxUpgrades.value = 1;
            while (Instance.upgradeCount > Timestopper.maxUpgrades.value)
            {
                Instance.upgradeCount--;
                Instance.maxTime -= 1 + 1 / (Instance.upgradeCount + 0.5f);
            }
            Write(Instance);
        }
        public static void AcceptWarning()
        {
            Instance.firstWarning = true;
            Write(Instance);

        }
        public static void GiveArm()
        {
            Instance.hasArm = true;
            Instance.equippedArm = true;
            Write(Instance);
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
            Timestopper.Log("Changed equipment status", true, ErrorLevel.Info);
        }
        public static void EquipArm(bool equipped)
        {
            if (Playerstopper.Instance.timeArm == null)
                return;
            if (Instance.hasArm)
            {
                Instance.equippedArm = equipped;
                Playerstopper.Instance.timeArm.SetActive(equipped);
                Timestopper.Log("Gold Arm Equipment Status changed: " + Instance.equippedArm.ToString(), true, ErrorLevel.Info);
            }
            else
            {
                Timestopper.Log("Invalid request of arm equipment, user doesn't have the arm yet!", true, ErrorLevel.Warning);
                GiveArm();
                return;
            }
            Write(Instance);
        }

        public static void Reset()
        {
            string filePath = Path.Combine(GameProgressSaver.SavePath, PROGRESS_FILE);
            if (File.Exists(filePath))
                File.Delete(filePath);
            Timestopper.mls.LogWarning("Deleting save file at: " + filePath);
            Instance = new TimestopperProgress();
        }

        public static TimestopperProgress Read()
        {
            try
            {
                string filePath = Path.Combine(GameProgressSaver.SavePath, PROGRESS_FILE);
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    Instance = JsonUtility.FromJson<TimestopperProgress>(jsonData);
                    if (Instance == null)
                        _instance = new TimestopperProgress();
                }
                else
                {
                    _instance = new TimestopperProgress();
                }
            }
            catch (Exception e)
            {
                Timestopper.mls.LogError($"Failed to read progress: {e.Message}, resetting save file {GameProgressSaver.currentSlot}");
                Instance = new TimestopperProgress();
            }
            return Instance;
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
}