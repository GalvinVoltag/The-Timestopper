using System.Collections;
using BepInEx;
using The_Timestopper.Arm;
using The_Timestopper.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace The_Timestopper.Player
{
    public class Playerstopper : MonoBehaviour   // The other side of the magic
    {
        private static Playerstopper _instance;
        public static Playerstopper Instance
        {
            get
            {
                if (_instance) return _instance;
                if (!MonoSingleton<NewMovement>.Instance) return null;
                _instance = MonoSingleton<NewMovement>.Instance.gameObject.AddComponent<Playerstopper>();
                return _instance;
            }
        }
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
            if (timeArm == null)
            {
                // ReSharper disable once MustUseReturnValue
                LoadTimeArm();
            }

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
            Timestopper.FixedUpdateFix(transform);
            StartCoroutine(LoadTimeArm());
            Timestopper.FixedUpdateFix(GameObject.Find("GameController").transform);
            movement = gameObject.GetComponent<NewMovement>();
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
            if (!GunControl.Instance || !FistControl.Instance) return;
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
            if (Timestopper.specialMode.value)
            {
                if (UnityInput.Current.GetKeyDown(KeyCode.J))
                {
                    GameProgressSaver.AddMoney((int)TimestopperProgress.UpgradeCost);
                    Timestopper.Log("MONEY MONEY MONEY", false);
                }
            }
        }
        
    }
}