using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public abstract class RuntimeSet<T> : SerializableScriptableObject, IEnumerable<T>, ISceneCycleListener
{
    [SerializeField] List<T> items = new List<T>();
    
    
    public virtual void Add(T item)
    {
        if (!items.Contains(item))
        {
            items.Add(item);
        }
    }
    
    public virtual void Remove(T item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
        }
    }
    
    
    public virtual void Clear()
    {
        items.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
       return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        
    }

    public void OnSceneUnload(Scene scene)
    {
        
    }

    public virtual void OnSceneStarted(Scene scene)
    {
        
    }

    public virtual void OnSceneStopped(Scene scene)
    {
        //Clear();
    }

    public void OnEditorStopped()
    {
        
    }
}




public abstract class RuntimeSetInitializer<T> : SerializedMonoBehaviour, IInitializable
{
    [SerializeField] protected RuntimeSet<T> _runtimeSet;
    [SerializeField, HideIf("_useChildrenComponents")] protected T[] _itemsToRegister = Array.Empty<T>();
    [SerializeField] private bool _useChildrenComponents = false;
    
    
    public virtual void OnDisable()
    {
        RegisterItems(false);
    }

    protected virtual void RegisterItems(bool add = true)
    {
        if (_useChildrenComponents && add)
        {
            try
            {
                _itemsToRegister = GetComponentsInChildren<T>();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
        }
        
        foreach (var item in _itemsToRegister)
        {
            if (add)
            {
                _runtimeSet.Add(item);
            }
            else
            {
                _runtimeSet.Remove(item);
            }
        }
    }


    public Task Init()
    {
        RegisterItems();
        return Task.CompletedTask;
    }
}