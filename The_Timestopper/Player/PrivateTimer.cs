using System;
using UnityEngine;

namespace The_Timestopper.Player
{
    public class PrivateTimer : MonoBehaviour
    {
        private float time;
        private bool scaled;
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
}