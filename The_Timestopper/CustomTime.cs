using System.Collections.Generic;
using UnityEngine;

namespace The_Timestopper
{
    public class CustomTime : MonoBehaviour
    {
        public struct TimeLayer
        {
            public float _timeScale;
            public float _fixedDeltaTime;
            public TimeLayer(float timeScale = 1f, float fixedDeltaTime = 0.02f)
            {
                this._timeScale = timeScale;
                this._fixedDeltaTime = fixedDeltaTime;
            }
        }

        private static Dictionary<int, TimeLayer> layers = new Dictionary<int, TimeLayer>();
        private static Dictionary<int, int> contributors = new Dictionary<int, int>();
        private static Dictionary<GameObject, CustomTime> gameObjectBindings = new Dictionary<GameObject, CustomTime>();
        private static Dictionary<GameObject, int> gameObjectids = new Dictionary<GameObject, int>();
        private static TimeLayer currentLayer = new TimeLayer();
        private static GameObject currentGameObject;

        /// <summary>
        /// When this is set to true, no time layers will be deleted when they become empty
        /// </summary>
        public static bool allPersistent = false;
        public static float timeScale => currentLayer._timeScale;
        public static float deltaTime => Time.unscaledDeltaTime * currentLayer._timeScale;
        public static float fixedDeltaTime => currentLayer._fixedDeltaTime;
        

        /// <summary>
        /// Used to create time layers for use of different timescales in the same scene,
        /// it is HIGHLY ADVICED TO KEEP TRACK OF LAYERS WITH AN ENUM TO AVOID
        /// NULL EXPRESSION ERRORS, BE WARNED!
        /// </summary>
        /// <param name="id"> id of the layer that is going to be created </param>
        /// <param name="layer"> the time layer to be put in this id </param>
        public static void CreateTimeLayer(int id, TimeLayer layer)
        {
            // if (!layers.TryAdd(id, layer)) throw new Exception("Cannot create a layer in the spot that already exists.");
            contributors[id] = -1;
        }
        public static void SetTimeLayer(int id, TimeLayer timeLayer)
        {
            if (!layers.ContainsKey(id)) contributors[id] = -1;
            layers[id] = timeLayer;
        }
        public static void BindLayer(int bindLayer)
        {
            currentLayer = layers[bindLayer];
        }
        
        
        private int _layer = 0;
        /// <summary>
        /// time layer of the GameObject that is bound with this CustomTime component
        /// CustomTime.deltaTime will not update unless Bind() is called again
        /// </summary>
        public int layer
        {
            get => _layer;
            set {
                contributors[_layer]--;
                if (contributors[_layer] == 0 && !allPersistent) contributors.Remove(_layer);
                _layer = value;
                if (contributors[value] == -1)contributors[value]++;
                contributors[value]++;
                gameObjectids[gameObject] = value;
                if (currentGameObject == gameObject) currentLayer = layers[value];
            }
        }

        public static int Layer
        {
            get => currentGameObject ? gameObjectids[currentGameObject] : 0;
            set
            {
                if (!currentGameObject) return;
                gameObjectBindings[currentGameObject].layer = value;
            }
        }

        public static void Bind(TimeLayer timeLayer)
        {
            currentLayer = timeLayer;
        }
        public static void Bind(GameObject go)
        {
            if (!gameObjectBindings.ContainsKey(go))
                go.AddComponent<CustomTime>();
            currentLayer = layers[gameObjectids[go]];
            currentGameObject = go;
        }

        private void Awake()
        {
            if (!layers.ContainsKey(layer)) CreateTimeLayer(layer, new TimeLayer());
            gameObjectBindings.Add(gameObject, this);
            gameObjectids.Add(gameObject, layer);
            if (contributors[layer] == -1) contributors[layer]++;
            contributors[layer]++;
        }

        private void OnDestroy()
        {
            gameObjectBindings.Remove(gameObject);
            gameObjectids.Remove(gameObject);
            contributors[layer]--;
            if (contributors[layer] == 0 && !allPersistent) contributors.Remove(layer);
        }

        public void Bind()
        {
            currentLayer = layers[layer];
        }
        public void SetCurrentLayerTimeScale(float TimeScale)
        {
            layers[layer] = new TimeLayer(TimeScale, layers[layer]._fixedDeltaTime);
        }
    }
}