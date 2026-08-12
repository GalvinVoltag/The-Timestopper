using System;
using System.Collections.Generic;
using System.Globalization;
using The_Timestopper.Arm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace The_Timestopper.Player
{
    public class TimeHUD : MonoBehaviour
    {
        public static List<TimeHUD> instances = new List<TimeHUD>();
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
            if (instances == null || instances.Count < 3)
                Timestopper.LoadHUDIfAppropriate();
            if (instances == null) return;
            foreach (TimeHUD T in instances)
            {
                T.Reconsider();
            }
        }
        public void Reconsider()
        {
            if (TimestopperProgress.EquippedArm && TimestopperProgress.HasArm)
                gameObject.SetActive(true);
            else
                gameObject.SetActive(false);
        }
        public void Update()
        {
            if (!TimestopperProgress.HasArm || !TimestopperProgress.EquippedArm) return;
            if (!TimeArm.Instance) return;
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
}