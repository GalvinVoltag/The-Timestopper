using The_Timestopper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace The_Timestopper
{
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
}