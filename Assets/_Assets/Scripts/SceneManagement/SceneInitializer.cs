using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;


public interface IInitializable
{
    Task Init();
}

public class SceneInitializer : SerializedMonoBehaviour
{
    [SerializeField] SaveDataManagerSO _saveDataManager;
    [SerializeField] private GameObject _loadingScreen;
    [Title("Scene Components")]
    [SerializeField] private SceneComponent[] _sceneComponents;

    private void Awake()
    {
        if(_loadingScreen) _loadingScreen.SetActive(true);
    }
    public async Task RunSetup()
    {
        try
        {
            await _saveDataManager.Init();
        }
        catch
        {
            Debug.LogError("SaveDataManager failed to initialize");
        }
        if (_loadingScreen) _loadingScreen?.SetActive(true);
        foreach (var sceneComponent in _sceneComponents)
        {
            sceneComponent.Init(this);
            await sceneComponent.Init();
        }
    }


    public void CompleteSetup()
    {
        if (_loadingScreen) _loadingScreen?.SetActive(false);
    }
}


[Serializable]
public class SceneComponent
{
    [SerializeField, ReadOnly, ShowIf("_sceneInitializer")]
    private SceneInitializer _sceneInitializer;

    [SerializeField, ReadOnly] private InterfaceReference<IInitializable>[] _initializables;
    [Title("Prefab"), Tooltip("The level prefab to instantiate or initialize. Can Be gameobject or IInitializable asset.")]
    [SerializeField] private Object _prefab;

    [SerializeField, ReadOnly, ShowIf("ValidatePrefabGameObject")]
    private GameObject _instance;

    public void Init(SceneInitializer sceneInitializer)
    {
        _sceneInitializer = sceneInitializer;
    }

    private bool ValidatePrefabGameObject()
    {
        if (_prefab == null)
        {
            return false;
        }

        return _prefab is GameObject;
    }

    public virtual async Task Init()
    {
        List<InterfaceReference<IInitializable>> initializables = new();
        if (ValidatePrefabGameObject())
        {
            GameObject go = _prefab as GameObject;

            _instance = Object.Instantiate(go);
            _instance.name = _prefab.name;
        }
        else if (_prefab is IInitializable initializable)
        {
            InterfaceReference<IInitializable> interfaceReference = new InterfaceReference<IInitializable>();
            interfaceReference.Value = initializable;
            interfaceReference.UnderlyingValue = _prefab;
            initializables.Add(interfaceReference);
            await interfaceReference.Value.Init();
            _initializables = initializables.ToArray();
            return;
        }
        else
        {
            Debug.LogError("Prefab is not a GameObject or IInitializable assset");
            return;
        }

        if (_instance.GetComponentsInChildren<MonoBehaviour>().Length != 0)
        {
            foreach (var mono in _instance.GetComponentsInChildren<MonoBehaviour>())
            {
                if (mono is not IInitializable) continue;
                InterfaceReference<IInitializable> interfaceReference = new InterfaceReference<IInitializable>();
                interfaceReference.Value = mono as IInitializable;
                interfaceReference.UnderlyingValue = mono;
                initializables.Add(interfaceReference);
                await interfaceReference.Value.Init();
            }
        }

        _initializables = initializables.ToArray();
    }
}