using HarmonyLib;
using The_Timestopper.Arm;

namespace The_Timestopper.Player
{
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
}