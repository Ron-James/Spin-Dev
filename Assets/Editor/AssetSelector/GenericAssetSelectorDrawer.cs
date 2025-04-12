using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
public class GenericAssetSelectorDrawer<T> : OdinAttributeDrawer<GenericAssetSelectorAttribute, T> where T : UnityEngine.Object
{
    private List<ScriptableObject> matchingAssets;
    private string[] assetNames;
    private bool initialized;

    protected override void DrawPropertyLayout(GUIContent label)
    {
        if (this.ValueEntry == null)
        {
            SirenixEditorGUI.ErrorMessageBox("Odin: ValueEntry is null. Skipping drawing.");
            return;
        }

        if (!initialized || matchingAssets == null || assetNames == null)
        {
            InitializeMatchingAssets();
            initialized = true;
        }

        if (matchingAssets == null || assetNames == null || assetNames.Length == 0)
        {
            SirenixEditorGUI.WarningMessageBox($"No matching assets found for '{typeof(T).Name}' in Resources.");
            CallNextDrawer(label);
            return;
        }

        ScriptableObject current = this.ValueEntry.SmartValue as ScriptableObject;
        int currentIndex = current != null ? matchingAssets.IndexOf(current) : 0;
        currentIndex = Mathf.Clamp(currentIndex, 0, assetNames.Length - 1);

        string dropdownLabel = label?.text ?? "";

        int selectedIndex = EditorGUILayout.Popup(dropdownLabel, currentIndex, assetNames);

        if (selectedIndex != currentIndex)
        {
            this.ValueEntry.SmartValue = (T)(object)matchingAssets[selectedIndex];
        }
    }


    private void InitializeMatchingAssets()
    {
        matchingAssets = new List<ScriptableObject>();
        assetNames = Array.Empty<string>(); // Prevent null ref fallback

        Type expectedType = typeof(T);
        Type filterType = this.Attribute?.TargetGenericType;

        var allSOs = Resources.LoadAll<ScriptableObject>("");

        foreach (var so in allSOs)
        {
            if (so == null) continue;

            Type soType = so.GetType();
            if (!expectedType.IsAssignableFrom(soType))
                continue;

            bool include = false;

            if (filterType == null)
            {
                include = true;
            }
            else if (filterType.IsGenericTypeDefinition)
            {
                include = InheritsFromGenericBase(soType, filterType, expectedType);
            }
            else if (filterType.IsAssignableFrom(soType))
            {
                include = true;
            }

            if (include)
            {
                matchingAssets.Add(so);
            }
        }

        // Add a "None" entry for clearing
        matchingAssets.Insert(0, null);
        assetNames = matchingAssets.Select(x => x != null ? x.name : "<None>").ToArray();
    }

    private bool InheritsFromGenericBase(Type type, Type genericBaseType, Type expectedGenericArg)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBaseType)
            {
                var args = type.GetGenericArguments();
                if (args.Length == 1 && args[0] == expectedGenericArg)
                    return true;
            }
            type = type.BaseType;
        }
        return false;
    }
}
