#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using System;
using Sirenix.Serialization;


/// <summary>
/// Keeps track of the current scene and its initializers.
/// Responsible for calling scene cycle events.
/// </summary>
[ExecuteAlways]
public class SceneTracker : PersistentMonoBehaviour<SceneTracker>
{
    private static SceneInitializer CurrentSceneInitializer;


    [SerializeField] MonoBehaviour[] sceneSerializableMonoBehaviours;

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
        if (CurrentSceneInitializer == null)
        {
            Debug.LogError("SceneInitializer not found in scene.");
        }
        else
        {
            await CurrentSceneInitializer.RunSetup();
        }

        MonoBehaviouRegistry.FindSerializableMonoBehaviours();
        sceneSerializableMonoBehaviours = MonoBehaviouRegistry.All;
        OnStart?.Invoke();
        CurrentSceneInitializer?.CompleteSetup();
    }

    void OnDisable()
    {
        OnStop?.Invoke();
        MonoBehaviouRegistry.ClearRegistry();
    }
}