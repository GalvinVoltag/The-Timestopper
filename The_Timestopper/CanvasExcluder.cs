using UnityEngine;

namespace The_Timestopper
{
    public class CanvasExcluder : MonoBehaviour
    {
        void OnEnable()
        {
            transform.parent.GetComponent<TerminalExcluder>()?.OverrideInfoMenu();
        }
    }
}