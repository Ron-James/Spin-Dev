using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
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