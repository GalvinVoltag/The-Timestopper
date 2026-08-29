using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
        private static readonly List<IFixedUpdateReceiver> FixedUpdateRegisters = new List<IFixedUpdateReceiver>();
        private static readonly HashSet<Type> IgnoredTypes = new HashSet<Type>();

        public static bool isInLoop { get; private set; } = false;

        private readonly Dictionary<Component, Action> delegates = new Dictionary<Component, Action>();
        private bool overrideTargets = false;
        public bool isRegistered { get; set; }
        public bool destroyed;

        public static void RegisterFixedUpdate(IFixedUpdateReceiver receiver)
        {
            receiver.isRegistered = true;
            if (isInLoop)
                FixedUpdateRegisters.Add(receiver);
            else
                FixedUpdateCallers.Add(receiver);
        }
        public static void UnregisterFixedUpdate(IFixedUpdateReceiver receiver)
        {
            receiver.isRegistered = false;
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
        private void Awake()
        {
            UpdateTargetList();
            RegisterFixedUpdate(this);
        }
        public void FakeFixedUpdate()
        {
            if (destroyed || targets == null) return;
            if (!overrideTargets && gameObject.GetComponentCount() != targets.Length)
                UpdateTargetList();
            bool error = false;
            foreach (Component C in targets)
            {
                if (error)
                {
                    Timestopper.mls.LogWarning("successfully dodged error block");
                    error = false;
                }
                try
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
                catch (Exception e)
                {
                    error = true;
                    Timestopper.mls.LogError("An error occurred while calling a fake fixed update at " + C.GetType() + " of gameobject: " + C.gameObject.name );
                    Timestopper.mls.LogError(e);
                }
            }
        }

        public void RealFakeFixedUpdate()
        {
            isInLoop = true;
            FakeFixedUpdate();
            isInLoop = false;
        }

        private static void ClearUnregisteredCallers()
        {
            if (FixedUpdateUnregisters.Count != 0)
            {
                foreach (var v in FixedUpdateUnregisters) FixedUpdateCallers.Remove(v);
                FixedUpdateUnregisters.Clear();
            }
            if (FixedUpdateRegisters.Count != 0)
            {
                foreach (var v in FixedUpdateRegisters) FixedUpdateCallers.Add(v);
                FixedUpdateRegisters.Clear();
            }
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
            bool error = false;
            foreach (var t in FixedUpdateCallers)
            {
                if (t is FixedUpdateCaller FUC && (FUC.destroyed || !FUC.isRegistered)) continue;
                t?.FakeFixedUpdate();
            }
            isInLoop = false;
        }

        private void OnEnable()
        {
            if (!isRegistered) RegisterFixedUpdate(this);
        }

        private void OnDisable()
        {
            if (isRegistered) UnregisterFixedUpdate(this);
        }

        private void OnDestroy()
        {
            destroyed = true;
            if (isRegistered) UnregisterFixedUpdate(this);
        }
    }
}