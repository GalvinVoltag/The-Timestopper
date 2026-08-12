using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace The_Timestopper.Internal
{
    public class FixedUpdateCaller : MonoBehaviour, IFixedUpdateReceiver
    {
        // ReSharper disable once InconsistentNaming
        private Component[] _targets;
        public Component[] targets {
            get => _targets;
            set { overrideTargets = true; _targets = value; }
        }
        private static float time;
        private static readonly List<IFixedUpdateReceiver> FixedUpdateCallers = new List<IFixedUpdateReceiver>();
        private static readonly List<IFixedUpdateReceiver> FixedUpdateUnregisters = new List<IFixedUpdateReceiver>();
        private static readonly HashSet<Type> IgnoredTypes = new HashSet<Type>();

        public static bool isInLoop { get; private set; } = false;

        private readonly Dictionary<Component, Action> delegates = new Dictionary<Component, Action>();
        private bool overrideTargets = false;

        public static void RegisterFixedUpdate(IFixedUpdateReceiver receiver)
        {
            FixedUpdateCallers.Add(receiver);
        }
        public static void UnregisterFixedUpdate(IFixedUpdateReceiver receiver)
        {
            if (isInLoop)
                FixedUpdateUnregisters.Add(receiver);
            else
                FixedUpdateCallers.Remove(receiver);
        }

        private void UpdateTargetList()
        {
            _targets = GetComponents(typeof(MonoBehaviour))
                .Where(T => !IgnoredTypes.Contains(T.GetType()) && !(T is IFixedUpdateReceiver)).ToArray();
        }
        public void Awake()
        {
            UpdateTargetList();
            RegisterFixedUpdate(this);
        }
        public void FakeFixedUpdate()
        {
            if (!overrideTargets && gameObject.GetComponentCount() != targets.Length)
                UpdateTargetList();
            foreach (Component C in targets)
            {
                if (!C) { _targets = _targets.Where(comp => comp).ToArray(); break; }
                Type t = C.GetType();
                if (IgnoredTypes.Contains(t) || !((Behaviour)C).enabled) continue;
                if (delegates.TryGetValue(C, out var @delegate))
                    @delegate();
                else
                {
                    MethodInfo m = t.GetMethod("FixedUpdate",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (m == null)
                    {
                        IgnoredTypes.Add(t);
                        continue;
                    }
                    delegates[C] = (Action)Delegate.CreateDelegate(typeof(Action), C, m);
                }
            }
        }

        public void RealFakeFixedUpdate()
        {
            isInLoop = true;
            FakeFixedUpdate();
            isInLoop = false;
        }

        public static void ClearUnregisteredCallers()
        {
            if (FixedUpdateUnregisters.Count == 0) return;
            foreach (var v in FixedUpdateUnregisters) FixedUpdateCallers.Remove(v);
            FixedUpdateUnregisters.Clear();
        }

        public static void UpdateAll(float deltaTime)
        {
            if (FixedUpdateCallers.Count == 0) return;
            
            time += deltaTime;
            if (time > Time.maximumDeltaTime)
                time = Time.maximumDeltaTime;
            while (time >= Time.fixedDeltaTime)
            {
                time -= Time.fixedDeltaTime;
                CallAllFixedUpdates();
            }
        }
        
        public static void CallAllFixedUpdates()
        {
            if (FixedUpdateCallers.Count == 0) return;
            ClearUnregisteredCallers();

            isInLoop = true;
            foreach (var t in FixedUpdateCallers)
            {
                t.FakeFixedUpdate();
            }
            isInLoop = false;
        }

        private void OnEnable()
        {
            if (!FixedUpdateCallers.Contains(this)) RegisterFixedUpdate(this);
        }

        private void OnDisable()
        {
            UnregisterFixedUpdate(this);
        }

        private void OnDestroy()
        {
            UnregisterFixedUpdate(this);
        }
    }
}