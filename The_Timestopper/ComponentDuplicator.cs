using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Todo: robot generated, needs heavy revision
/// </summary>
public static class ComponentDuplicator
{
    public static T CopyComponent<T>(this GameObject destination, T original) where T : Component
    {
        System.Type type = original.GetType();
        T copy = destination.AddComponent(type) as T;

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Instance | BindingFlags.Default);
        foreach (FieldInfo field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(copy, field.GetValue(original));
        }

        PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                                  BindingFlags.Instance | BindingFlags.Default);
        foreach (PropertyInfo prop in props)
        {
            if (!prop.CanWrite || !prop.CanRead) continue;
            if (prop.GetIndexParameters().Length > 0) continue;

            // Skip obsolete properties (e.g. AudioSource.minVolume/maxVolume/rolloffFactor/pan)
            if (prop.GetCustomAttributes(typeof(System.ObsoleteAttribute), true).Any()) continue;

            try
            {
                prop.SetValue(copy, prop.GetValue(original, null), null);
            }
            catch
            {
                // ignore properties that throw on set
            }
        }

        return copy;
    }
    
    public static T CopyComponent<T>(this GameObject host, T original, GameObject target) where T : Component
    {
        System.Type type = original.GetType();
        T copy = target.AddComponent(type) as T;

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                            BindingFlags.Instance | BindingFlags.Default);
        foreach (FieldInfo field in fields)
        {
            if (field.IsStatic) continue;
            field.SetValue(copy, field.GetValue(original));
        }

        PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                                  BindingFlags.Instance | BindingFlags.Default);
        foreach (PropertyInfo prop in props)
        {
            if (!prop.CanWrite || !prop.CanRead) continue;
            if (prop.GetIndexParameters().Length > 0) continue;

            // Skip obsolete properties (e.g. AudioSource.minVolume/maxVolume/rolloffFactor/pan)
            if (prop.GetCustomAttributes(typeof(System.ObsoleteAttribute), true).Any()) continue;

            try
            {
                prop.SetValue(copy, prop.GetValue(original, null), null);
            }
            catch
            {
                // ignore properties that throw on set
            }
        }

        return copy;
    }
}