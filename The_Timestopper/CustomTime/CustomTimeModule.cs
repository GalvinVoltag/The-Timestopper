using System;
using UnityEngine;

namespace The_Timestopper.CustomTimeLibrary
{
    /// <summary>
    /// Custom wrapper for any classes that can't be patched to obey CustomTime time system. Just derive and it will be utilized.
    /// </summary>
    public abstract class CustomTimeModule
    {
        /// <summary>
        /// The CustomTime component this module is attached to.
        /// </summary>
        public CustomTime customTime;
        /// <summary>
        /// Get a list of types this module is meant to wrap.
        /// </summary>
        public abstract Type[] GetValidTypes();
        /// <summary>
        /// This will be called by this module's CustomTime whenever a change occurs in its timeScale value
        /// </summary>
        /// <param name="newTimeScale">current timeScale of this module's CustomTime</param>
        public abstract void OnTimeScaleChange(float newTimeScale);
        /// <summary>
        /// This will be called right after this module has been created and bound to a CustomTime component.
        /// </summary>
        /// <param name="components">a list of all components that match any type defined by GetValidTypes() of this module</param>
        public abstract void Awake(Component[] components);

        /// <summary>
        /// Called when this module is supposed to be destroyed. Use it to release customTime field and anything necessary.
        /// </summary>
        public abstract void OnDestroy();
    }
}