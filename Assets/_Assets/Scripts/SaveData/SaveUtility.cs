using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Utility methods for extracting and applying save data using [Save] filtering.
/// Handles nested structures, lists, dictionaries, abstract types, and skips events/delegates.
/// </summary>
public static class SaveUtility
{
    /// <summary>
    /// Extracts top-level fields marked with [Save] and recursively captures their values into a dictionary.
    /// Supports lists, dictionaries, nested objects, and polymorphic types.
    /// </summary>
    public static Dictionary<string, object> ExtractSaveData(object source)
    {
        var data = new Dictionary<string, object>();
        var fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            if (!field.IsDefined(typeof(SaveAttribute), true)) continue;
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;

            var value = field.GetValue(source);
            data[field.Name] = ExtractNestedValue(value);
        }

        return data;
    }

    /// <summary>
    /// Applies previously saved dictionary data onto a runtime object,
    /// only updating fields marked with [Save] and preserving other field values.
    /// </summary>
    public static void ApplySaveData(object target, Dictionary<string, object> saved)
    {
        if (target == null || saved == null) return;

        var fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            if (!field.IsDefined(typeof(SaveAttribute), true)) continue;
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;

            if (!saved.TryGetValue(field.Name, out var value)) continue;

            if (IsSimple(field.FieldType))
            {
                field.SetValue(target, value);
            }
            else
            {
                var current = field.GetValue(target);
                if (current != null)
                {
                    ApplyNestedValue(current, value);
                }
                else
                {
                    var fallback = Activator.CreateInstance(field.FieldType);
                    ApplyNestedValue(fallback, value);
                    field.SetValue(target, fallback);
                }
            }
        }
    }

    /// <summary>
    /// Recursively extracts nested object data including $type info for polymorphic types.
    /// Skips fields marked with [DontSave] or delegates.
    /// </summary>
    private static object ExtractNestedValue(object value)
    {
        if (value == null) return null;
        var type = value.GetType();
        if (IsSimple(type)) return value;

        if (value is IList list)
        {
            var newList = new List<object>();
            foreach (var item in list)
                newList.Add(ExtractNestedValue(item));
            return newList;
        }

        if (value is IDictionary dict)
        {
            var newDict = new Dictionary<object, object>();
            foreach (DictionaryEntry entry in dict)
                newDict[entry.Key] = ExtractNestedValue(entry.Value);
            return newDict;
        }

        var result = new Dictionary<string, object>
        {
            ["$type"] = type.AssemblyQualifiedName
        };

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            if (field.IsDefined(typeof(DontSaveAttribute), true)) continue;
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;

            var nestedValue = field.GetValue(value);
            result[field.Name] = ExtractNestedValue(nestedValue);
        }

        return result;
    }

    /// <summary>
    /// Applies nested dictionary data to a complex object.
    /// Includes polymorphic $type support and recursive restoration.
    /// </summary>
    private static void ApplyNestedValue(object target, object saved)
    {
        if (target == null || saved == null) return;

        if (target is IList targetList && saved is IList savedList)
        {
            MergeList(targetList, savedList);
            return;
        }

        if (target is IDictionary targetDict && saved is IDictionary savedDict)
        {
            targetDict.Clear();
            foreach (DictionaryEntry entry in savedDict)
                targetDict[entry.Key] = entry.Value;
            return;
        }

        if (saved is Dictionary<string, object> savedFields)
        {
            if (savedFields.TryGetValue("$type", out var typeObj) && typeObj is string typeName)
            {
                var actualType = Type.GetType(typeName);
                if (actualType != null && actualType != target.GetType() && !actualType.IsAbstract)
                {
                    var replacement = Activator.CreateInstance(actualType);
                    ApplyNestedValue(replacement, savedFields);
                    CopyFields(replacement, target);
                    return;
                }
            }

            var fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.IsDefined(typeof(DontSaveAttribute), true)) continue;
                if (typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;
                if (!savedFields.TryGetValue(field.Name, out var savedValue)) continue;

                if (IsSimple(field.FieldType))
                {
                    field.SetValue(target, savedValue);
                }
                else
                {
                    var nested = field.GetValue(target);
                    if (nested != null)
                    {
                        ApplyNestedValue(nested, savedValue);
                    }
                    else
                    {
                        var created = Activator.CreateInstance(field.FieldType);
                        ApplyNestedValue(created, savedValue);
                        field.SetValue(target, created);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reuses or rebuilds list contents with new values, respecting $type metadata for polymorphic deserialization.
    /// </summary>
    private static void MergeList(IList targetList, IList savedList)
    {
        int count = Mathf.Min(targetList.Count, savedList.Count);

        var resultList = (IList)Activator.CreateInstance(targetList.GetType());
    
        for (int i = 0; i < savedList.Count; i++)
        {
            var savedItem = savedList[i];

            if (savedItem == null)
            {
                resultList.Add(null);
                continue;
            }

            if (IsSimple(savedItem.GetType()))
            {
                resultList.Add(savedItem);
                continue;
            }

            if (savedItem is Dictionary<string, object> savedItemDict &&
                savedItemDict.TryGetValue("$type", out var typeObj) &&
                typeObj is string typeName)
            {
                var actualType = Type.GetType(typeName);
                if (actualType == null || actualType.IsAbstract)
                {
                    Debug.LogWarning($"[SaveUtility] Unable to resolve polymorphic type: {typeName}");
                    continue;
                }

                object instance = null;

                // Try reuse existing instance at same index if type matches
                if (i < targetList.Count && targetList[i]?.GetType() == actualType)
                {
                    instance = targetList[i];
                    ApplyNestedValue(instance, savedItemDict);
                }
                else
                {
                    instance = Activator.CreateInstance(actualType);
                    ApplyNestedValue(instance, savedItemDict);
                }

                resultList.Add(instance);
            }
            else
            {
                Debug.LogWarning($"[SaveUtility] Unexpected list item format: {savedItem?.GetType()}");
            }
        }

        // Replace contents without reallocating the list reference
        targetList.Clear();
        foreach (var item in resultList)
            targetList.Add(item);
    }



    /// <summary>
    /// Copies all public/private fields from one object to another of the same type.
    /// </summary>
    private static void CopyFields(object source, object target)
    {
        var fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            field.SetValue(target, field.GetValue(source));
        }
    }

    /// <summary>
    /// Determines whether a type is a primitive, enum, string, or UnityEngine.Object.
    /// </summary>
    private static bool IsSimple(Type type)
    {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               typeof(UnityEngine.Object).IsAssignableFrom(type);
    }
}
