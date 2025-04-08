using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sirenix.Serialization;

/// <summary>
/// Utility methods for creating and restoring POCO save data using [Save] filtered fields.
/// </summary>
public static class SaveUtility
{
    /// <summary>
    /// Creates a filtered POCO deep copy of the source object using Odin and [Save] field filtering.
    /// </summary>
    public static T GetFilteredState<T>(object source) where T : class, new()
    {
        var clone = SerializationUtility.CreateCopy(source); // Odin deep copy
        var result = new T();
        CopyFilteredFields(clone, result);
        return result;
    }

    /// <summary>
    /// Applies [Save] fields from the POCO object into the target runtime instance.
    /// </summary>
    public static void RestoreFilteredState(object saved, object target)
    {
        CopyFilteredFields(saved, target);
    }

    /// <summary>
    /// Recursively copies only [Save]-marked fields from source to target, including collections and polymorphic types.
    /// </summary>
    private static void CopyFilteredFields(object source, object target)
    {
        if (source == null || target == null) return;

        var type = source.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            if (!field.IsDefined(typeof(SaveAttribute), true)) continue;

            var sourceValue = field.GetValue(source);
            var fieldType = field.FieldType;

            if (IsSimple(fieldType))
            {
                field.SetValue(target, sourceValue);
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                var clonedList = CloneList(sourceValue as IList);
                field.SetValue(target, clonedList);
            }
            else if (typeof(IDictionary).IsAssignableFrom(fieldType))
            {
                var clonedDict = CloneDictionary(sourceValue as IDictionary);
                field.SetValue(target, clonedDict);
            }
            else
            {
                object targetValue = field.GetValue(target);

                if (sourceValue == null)
                {
                    field.SetValue(target, null);
                    continue;
                }

                if (targetValue == null)
                {
                    targetValue = Activator.CreateInstance(fieldType);
                    field.SetValue(target, targetValue);
                }

                CopyFilteredFields(sourceValue, targetValue); // Recursive
            }
        }
    }

    private static IList CloneList(IList source)
    {
        if (source == null) return null;

        var elementType = source.GetType().IsArray
            ? source.GetType().GetElementType()
            : source.GetType().GetGenericArguments().FirstOrDefault() ?? typeof(object);

        var listType = typeof(List<>).MakeGenericType(elementType);
        var clonedList = (IList)Activator.CreateInstance(listType);

        foreach (var item in source)
        {
            clonedList.Add(CloneValue(item));
        }

        return clonedList;
    }

    private static IDictionary CloneDictionary(IDictionary source)
    {
        if (source == null) return null;

        var keyType = source.GetType().GetGenericArguments()[0];
        var valueType = source.GetType().GetGenericArguments()[1];
        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var clonedDict = (IDictionary)Activator.CreateInstance(dictType);

        foreach (DictionaryEntry entry in source)
        {
            clonedDict.Add(CloneValue(entry.Key), CloneValue(entry.Value));
        }

        return clonedDict;
    }

    /// <summary>
    /// Clones a single object using Odin for polymorphic types and [Save] reflection otherwise.
    /// </summary>
    private static object CloneValue(object value)
    {
        if (value == null) return null;

        var type = value.GetType();

        if (IsSimple(type))
            return value;

        // Step 1: Deep clone the object
        var deepClone = SerializationUtility.CreateCopy(value);

        // Step 2: Create filtered POCO based on [Save]
        var filtered = Activator.CreateInstance(type);
        CopyFilteredFields(deepClone, filtered);

        return filtered;
    }

    /// <summary>
    /// Determines if a type is simple and can be copied directly.
    /// </summary>
    private static bool IsSimple(Type type)
    {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               typeof(UnityEngine.Object).IsAssignableFrom(type);
    }
}
