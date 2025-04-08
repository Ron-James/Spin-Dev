using Sirenix.OdinInspector;
using UnityEngine;

public class ScriptableObjectLoadDebug : SerializedMonoBehaviour
{
    [SerializeField] private ScriptableObject[] _scriptableObjects;
    private void Start()
    {
        UpdateList();
    }

    [Button]
    public void UpdateList()
    {
        _scriptableObjects = ScriptableObjectManager.All;
    }
    
}