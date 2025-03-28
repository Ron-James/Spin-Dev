#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Object = UnityEngine.Object;


public abstract class PersistentMonoBehaviour<T> : SerializedMonoBehaviour where T : MonoBehaviour
{
    
    protected static void Initialize()
    {
        SceneManager.sceneLoaded += (scene, mode) => EnsureExists();
        EnsureExists();
    }

    private static void EnsureExists()
    {
        if (FindFirstObjectByType<T>() == null)
        {
            Type type = typeof(T);
            string typeName = type.Name;
            GameObject go = new GameObject(typeName);
            go.AddComponent<T>();
        }
    }
}

[ExecuteAlways]
public class SceneTracker : PersistentMonoBehaviour<SceneTracker>
{
    
    
    private static SceneInitializer CurrentSceneInitializer;
    // Static events for scene callbacks
    public static event Action OnStart;
    public static event Action OnStop;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        Initialize();
    }

    void Awake()
    {
        // (Optional) Additional setup if needed.
    }

    async void OnEnable()
    {
        CurrentSceneInitializer = FindAnyObjectByType<SceneInitializer>();
        
        if(CurrentSceneInitializer == null)
        {
            Debug.LogError("SceneInitializer not found in scene.");
        }
        else
        {
            await CurrentSceneInitializer.RunSetup();
        }
        
        OnStart?.Invoke();
        CurrentSceneInitializer?.CompleteSetup();
        
    }

    void OnDisable()
    {
        
        OnStop?.Invoke();
    }

    
}