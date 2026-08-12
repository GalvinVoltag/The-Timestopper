using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace The_Timestopper.Internal
{
    /// <summary>
    /// Use this class to register wanted MonoBehaviors in "targets" or wanted types in "targetTypes" in order
    /// to execute invokes in stopped time.
    /// </summary>
    public class InvokeCaller 
    {
        private static HashSet<MonoBehaviour> targets = new HashSet<MonoBehaviour>();
        private static HashSet<Type> targetTypes = new HashSet<Type>();
        private static Dictionary<Type, HashSet<string>> targetMethods = new Dictionary<Type, HashSet<string>>();
        public static List<InvokeCaller> invokers = new List<InvokeCaller>();
        private MonoBehaviour mono;
        private MethodInfo method;
        private float time;

        public static void Update()
        {
            foreach (InvokeCaller IC in invokers.ToArray())
            {
                IC.Iterate();
            }
        }

        public static void RegisterType(Type type)
        {
            targetTypes.Add(type);
        }
        public static void UnregisterType(Type type)
        {
            targetTypes.Remove(type);
        }
        public static void RegisterMethod(Type type, string methodName)
        {
            if (!targetMethods.ContainsKey(type)) targetMethods.Add(type, new HashSet<string>() { methodName });
            else targetMethods[type].Add(methodName);
        }
        public static void UnregisterMethod(Type type, string methodName)
        {
            if (!targetMethods.ContainsKey(type)) return;
            if (targetMethods[type].Remove(methodName) && targetMethods[type].Count == 0)
                targetMethods.Remove(type);
        }

        public static void ClearDestroyed()
        {
            targets.RemoveWhere(item => item == null);
        }
        public static void ClearMonos()
        {
            targets.Clear();
        }
        public static void RegisterTypes(IEnumerable<Type> values)
        {
            foreach (Type T in values)
            {
                targetTypes.Add(T);
            }
        }
        public static void UnRegisterTypes(IEnumerable<Type> values)
        {
            foreach (Type T in values)
            {
                targetTypes.Remove(T);
            }
        }
        public static void RegisterMethods(Type type, IEnumerable<string> methodNames)
        {
            if (!targetMethods.ContainsKey(type)) targetMethods.Add(type, new HashSet<string>());
            foreach (string methodName in methodNames)
                targetMethods[type].Add(methodName);
        }
        public static void UnregisterMethods(Type type, IEnumerable<string> methodNames)
        {
            if (!targetMethods.ContainsKey(type)) return;
            foreach (string methodName in methodNames)
                if (targetMethods[type].Remove(methodName) && targetMethods[type].Count == 0)
                {
                    targetMethods.Remove(type);
                    break;
                }
        }
        public static void RegisterMonos(IEnumerable<MonoBehaviour> values)
        {
            foreach (MonoBehaviour M in values)
            {
                targets.Add(M);
            }
        }
        public static void UnRegisterMonos(IEnumerable<MonoBehaviour> values)
        {
            foreach (MonoBehaviour M in values)
            {
                targets.Remove(M);
            }
        }

        private InvokeCaller(MonoBehaviour instance, string methodName, float delay)
        {
            time = delay;
            mono = instance;
            try
            {
                method = mono.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod);
            }
            catch
            {
                Debug.LogWarning("InvokeCaller method could not be created, method is null");
                method = null;
            }
        }
        private void Iterate()
        {
            time -= Timestopper.playerDeltaTime;
            if (time < 0)
            {
                if (mono!=null)
                    method?.Invoke(mono, null);
                invokers.Remove(this);
            }
        }
        /// <summary>
        /// returns true when target is successfully added to update list, do not use this method to register
        /// components, types or methods
        /// </summary>
        /// <param name="instance"> the MonoBehavior instance the method is invoked </param>
        /// <param name="methodName"> the method name that will be invoked </param>
        /// <param name="delay"> the delay of which when the method will be called in seconds </param>
        /// <returns></returns>
        public static bool Add(MonoBehaviour instance, string methodName, float delay)
        {
            if (   targets.Contains(instance) || targetTypes.Contains(instance.GetType()) || 
            ( targetMethods.ContainsKey(instance.GetType()) && targetMethods[instance.GetType()].Contains(methodName) )  )
            {
                invokers.Add(new InvokeCaller(instance, methodName, delay));
                return true;
            }
            return false;
        }
    }
    
}