using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using CustomSceneReference;
using JetBrains.Annotations;
using Sirenix.Serialization;
using UnityEngine.Serialization;


public interface ISceneCycleListener
{

    void OnSceneStarted(Scene scene);
    void OnSceneStopped(Scene scene);
    void OnEditorStopped();
}

[CreateAssetMenu(fileName = "SceneLoadManager", menuName = "Scene Load Manager")]
public class SceneLoadManager : SerializedScriptableObject
{
    [ShowInInspector, ReadOnly] private static List<ISceneCycleListener> _sceneLoadListeners = new();
    [SerializeReference, TableList] private List<SceneReference> _scenes = new();


    [FormerlySerializedAs("_sceneStopwatch")] [SerializeField, ReadOnly] private SceneTracker sceneTracker;


    private void OnEnable()
    {
#if UNITY_EDITOR
        _scenes.Clear();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            var sceneReference = new SceneReference(scene.path);
            _scenes.Add(sceneReference);
        }
        UnityEditor.EditorApplication.playModeStateChanged += OnEditorPlayModeStateChanged;
#endif
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitSceneLoadListeners()
    {
        
        foreach (var so in ScriptableObjectManager.All)
        {
            if(so is ISceneCycleListener listener)
            {
                _sceneLoadListeners.Add(listener);
            }
        }
        SceneTracker.OnStart += OnSceneStarted;
        SceneTracker.OnStop += OnSceneStopped;
    }
    
    
    private void OnDisable()
    {
        
        SceneTracker.OnStart -= OnSceneStarted;
        SceneTracker.OnStop -= OnSceneStopped;

        _sceneLoadListeners.Clear();

#if UNITY_EDITOR
        _scenes.Clear();
        UnityEditor.EditorApplication.playModeStateChanged -= OnEditorPlayModeStateChanged;
#endif
    }

    


    static void OnSceneStarted()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log("Stopwatch started from scene manager, calling on " + _sceneLoadListeners.Count + " listeners");
        foreach (var listener in _sceneLoadListeners)
        {
            listener.OnSceneStarted(activeScene);
        }
        
    }
#if UNITY_EDITOR
    private void OnEditorPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            foreach (var listener in _sceneLoadListeners)
            {
                listener.OnEditorStopped();
            }
        }
    }
#endif
    static void OnSceneStopped()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log("Stopwatch stopped from scene manager");
        foreach (var listener in _sceneLoadListeners)
        {
            listener.OnSceneStopped(activeScene);
        }
        _sceneLoadListeners.Clear();
    }

    


    

    
}