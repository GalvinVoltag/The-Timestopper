using UnityEngine;

namespace The_Timestopper
{
    public class ParticleSystemUpdater : MonoBehaviour
    {

        public ParticleSystem PS;
        private void Awake()
        {
            PS = GetComponent<ParticleSystem>();
            if (PS == null) Destroy(this);
        }

        private void OnEnable()
        {
            UpdateState();
        }

        public void UpdateState()
        {
            if (PS == null) Destroy(this);
            var main = PS.main;
            main.useUnscaledTime = (Timestopper.playerDeltaTime > 0 && !(MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused));
        }
    }
}