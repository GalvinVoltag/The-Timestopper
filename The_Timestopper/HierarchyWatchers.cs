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
                if (oldRootGameObjects.Contains(GO)) continue;
                ExecuteOnTreeChange.ExecuteOnNewGameObject(GO);
            }
            oldRootGameObjects = rootGameObjects.ToHashSet();
        }
    }
    public class ExecuteOnTreeChange : MonoBehaviour
    {
        public static event Action<GameObject> onNewGameObject;
        
        private HashSet<Transform> oldChildren = new HashSet<Transform>();
        List<Transform> newChildren = new List<Transform>();

        public static void ExecuteOnNewGameObject(GameObject go)
        {
            onNewGameObject?.Invoke(go);
            if (go.GetComponent<ExecuteOnTreeChange>()) return;
            ExecuteOnTreeChange eotc = go.AddComponent<ExecuteOnTreeChange>();
            eotc.OnTransformChildrenChanged();
        }
        

        private void OnEnable()
        {
            OnTransformChildrenChanged();
        }

        private void OnTransformChildrenChanged()
        {
            newChildren.Clear();
            foreach (Transform t in transform)
            {
                newChildren.Add(t);
                if (oldChildren.Contains(t)) continue;
                ExecuteOnNewGameObject(t.gameObject);
            }
            oldChildren = new HashSet<Transform>(newChildren);
        }
    }
}