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
    
    [Button]
    public ScriptableObject GetObjectbyGUID(string guid)
    {
        return ScriptableObjectManager.GetAssetByGuid<ScriptableObject>(guid);
    }
}