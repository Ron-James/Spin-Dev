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
    /// Extracts top-level fields marked with [Save] and recursively captures their data.
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
    /// Applies a saved dictionary to a runtime instance, applying only [Save] fields.
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
                if (current == null && value != null)
                {
                    current = Activator.CreateInstance(field.FieldType);
                    field.SetValue(target, current);
                }

                ApplyNestedValue(current, value);
            }
        }
    }

    
    /// <summary>
    /// extracts the value of a field, handling lists, dictionaries, and complex objects.
    /// </summary>
    /// <param name="value">the object to extract from</param>
    /// <returns></returns>
    private static object ExtractNestedValue(object value)
    {
        if (value == null) return null;
        var type = value.GetType();
        if (IsSimple(type)) return value;

        // List extraction
        if (value is IList list)
        {
            //recursively extract the values from the list
            var newList = new List<object>();
            foreach (var item in list)
                newList.Add(ExtractNestedValue(item));
            return newList;
        }

        // Dictionary extraction
        if (value is IDictionary dict)
        {
            var newDict = new Dictionary<object, object>();
            foreach (DictionaryEntry entry in dict)
                newDict[entry.Key] = ExtractNestedValue(entry.Value);
            return newDict;
        }

        // Complex object
        //Get object type from $type field if available
        var result = new Dictionary<string, object>
        {
            ["$type"] = type.AssemblyQualifiedName // Add type info for rehydration
        };

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            // Skip non-save fields
            if (field.IsDefined(typeof(DontSaveAttribute), true)) continue;
            // Skip events/delegates
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;

            // get the field value
            var nestedValue = field.GetValue(value);
            result[field.Name] = ExtractNestedValue(nestedValue);
        }

        return result;
    }

    // ================================
    // Recursive application
    // ================================

    private static void ApplyNestedValue(object target, object saved)
    {
        if (target == null || saved == null) return;

        // Handle lists
        if (target is IList targetList && saved is IList savedList)
        {
            targetList.Clear();

            var elementType = target.GetType().IsArray
                ? target.GetType().GetElementType()
                : target.GetType().GetGenericArguments().FirstOrDefault() ?? typeof(object);

            foreach (var savedItem in savedList)
            {
                if (IsSimple(elementType))
                {
                    targetList.Add(savedItem);
                }
                else if (savedItem is Dictionary<string, object> savedItemDict)
                {
                    // Try to read $type override for polymorphic support
                    Type actualType = elementType;
                    if (savedItemDict.TryGetValue("$type", out var typeStrObj) && typeStrObj is string typeStr)
                    {
                        var resolvedType = Type.GetType(typeStr);
                        if (resolvedType != null && !resolvedType.IsAbstract)
                            actualType = resolvedType;
                    }

                    var instance = Activator.CreateInstance(actualType);
                    ApplyNestedValue(instance, savedItemDict);
                    targetList.Add(instance);
                }
            }

            return;
        }

        // Handle dictionaries (shallow)
        if (target is IDictionary targetDict && saved is IDictionary savedDict)
        {
            targetDict.Clear();
            foreach (DictionaryEntry entry in savedDict)
            {
                targetDict[entry.Key] = entry.Value;
            }
            return;
        }

        // Custom nested object
        if (saved is Dictionary<string, object> savedFields)
        {
            // If polymorphic, replace instance
            if (savedFields.TryGetValue("$type", out var typeInfoObj) && typeInfoObj is string typeInfo)
            {
                var dynamicType = Type.GetType(typeInfo);
                if (dynamicType != null && dynamicType != target.GetType() && !dynamicType.IsAbstract)
                {
                    var replacement = Activator.CreateInstance(dynamicType);
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
                    var fieldTargetValue = field.GetValue(target);
                    if (fieldTargetValue == null)
                    {
                        fieldTargetValue = Activator.CreateInstance(field.FieldType);
                        field.SetValue(target, fieldTargetValue);
                    }

                    ApplyNestedValue(fieldTargetValue, savedValue);
                }
            }
        }
    }

    /// <summary>
    /// Copies all field values from source to target (same type).
    /// </summary>
    private static void CopyFields(object source, object target)
    {
        var fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            var value = field.GetValue(source);
            field.SetValue(target, value);
        }
    }

    /// <summary>
    /// Returns true if the type can be stored directly without recursion or inspection.
    /// </summary>
    private static bool IsSimple(Type type)
    {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               typeof(UnityEngine.Object).IsAssignableFrom(type);
    }
}
