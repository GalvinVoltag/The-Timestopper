using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace The_Timestopper
{
    public class SceneTreeChangeWatcher : MonoBehaviour
    {
        private static HashSet<GameObject> oldRootGameObjects = new HashSet<GameObject>();
        private static List<GameObject> rootGameObjects = new List<GameObject>();
        public static void StartWatchingSceneForTreeChanges()
        {
            StatsManager.Instance?.gameObject.AddComponent<SceneTreeChangeWatcher>();
        }
        
        private void Update()
        {
            SceneManager.GetActiveScene().GetRootGameObjects(rootGameObjects);
            foreach (GameObject GO in rootGameObjects)
            {
                if (!oldRootGameObjects.Add(GO)) continue;
                GO.GetOrAddComponent<ExecuteOnTreeChange>();
            }
            oldRootGameObjects.RemoveWhere(t => !t || t.transform.parent != null);
        }
    }
    public class ExecuteOnTreeChange : MonoBehaviour
    {
        /// <summary>
        /// This is called whenever a gameobject is enabled or instantiated, and will include all hierarchy of said object.
        /// </summary>
        public static event Action<GameObject> onNewGameObject;
        public static bool executed { get; private set; } = false;
        
        private HashSet<Transform> oldChildren = new HashSet<Transform>();
        

        private void OnEnable()
        {
            if (executed) return;
            onNewGameObject?.Invoke(gameObject);
            OnTransformChildrenChanged();
            executed = true;
        }

        private void OnTransformChildrenChanged()
        {
            oldChildren.RemoveWhere(t => !t || !t.IsChildOf(transform));
            foreach (Transform t in transform)
            {
                if (!oldChildren.Add(t)) continue;
                t.gameObject.GetOrAddComponent<ExecuteOnTreeChange>();
            }
        }
    }
}