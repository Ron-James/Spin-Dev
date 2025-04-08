using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class InterfaceReferenceOdinDrawer<TInterface, TObject> : OdinValueDrawer<InterfaceReference<TInterface, TObject>>
    where TObject : Object
    where TInterface : class
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var reference = this.ValueEntry.SmartValue;
        var underlying = reference?.UnderlyingValue;

        EditorGUI.BeginChangeCheck();
        var assignedObject = EditorGUILayout.ObjectField(label, underlying, typeof(TObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            if (assignedObject == null)
            {
                reference.UnderlyingValue = null;
            }
            else if (assignedObject is GameObject go)
            {
                var component = go.GetComponent(typeof(TInterface));
                if (component != null)
                {
                    reference.UnderlyingValue = component as TObject;
                }
                else
                {
                    Debug.LogWarning($"GameObject '{go.name}' does not implement interface '{typeof(TInterface).Name}'.");
                    reference.UnderlyingValue = null;
                }
            }
            else if (assignedObject is TInterface)
            {
                reference.UnderlyingValue = assignedObject as TObject;
            }
            else
            {
                Debug.LogWarning($"Assigned object does not implement required interface '{typeof(TInterface).Name}'.");
                reference.UnderlyingValue = null;
            }

            this.ValueEntry.SmartValue = reference;
        }
    }
}


