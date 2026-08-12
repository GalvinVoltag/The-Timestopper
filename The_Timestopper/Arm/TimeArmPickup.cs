using The_Timestopper.Arm;
using The_Timestopper.Player;
using UnityEngine;

namespace The_Timestopper.Arm
{
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
                MonoSingleton<HudMessageReceiver>.Instance?.SendHudMessage(string.Format(Timestopper.ARM_PICKUP_MESSAGE, Timestopper.stopKey.value), "", "", 2);
                // gameObject.SetActive(false);
                onPickup?.Invoke();
                enabled = false;
            }
        }
    }
}