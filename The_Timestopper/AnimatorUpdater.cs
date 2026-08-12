using UnityEngine;

namespace The_Timestopper
{
    public class AnimatorUpdater : MonoBehaviour
    {
        public Animator animator;
        private bool shouldUseNormalMode = true;
        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null) Destroy(this);
            // animatorUpdaters.Add(this);
        }
        private void Update()
        {
            bool useNormal = Timestopper.playerTimeScale == 0.0f || 
                             (MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused) || 
                             !Timestopper.TimeStop;

            if (useNormal == shouldUseNormalMode) return;
            shouldUseNormalMode = useNormal;
            animator.updateMode = useNormal ? AnimatorUpdateMode.Normal : AnimatorUpdateMode.UnscaledTime;
        }
    }
}