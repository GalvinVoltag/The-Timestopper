using UnityEngine;

namespace The_Timestopper.Internal
{
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
            second = seconds;
        }
    }
}