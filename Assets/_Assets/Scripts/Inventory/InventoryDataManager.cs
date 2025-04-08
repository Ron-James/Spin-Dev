using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "InventoryDataManager", menuName = "InventoryDataManager")]
public class InventoryDataManager : SerializableScriptableObject, ISceneCycleListener, IInitializable
{
    [SerializeField] private Dictionary<Item, int> inventory = new();



    [Title("Variable References")] [SerializeField]
    private VariableReference<ItemData> lastAddedItemData;
    
    public Item[] allItems => inventory.Keys.ToArray();
    public Dictionary<Item, int> Inventory => inventory;

    [Button]
    public void AddItem(Item item, int amount)
    {
        if(inventory.ContainsKey(item))
        {
            inventory[item] += amount;
        }
        else
        {
            inventory.Add(item, amount);
        }
        
        lastAddedItemData.CurrentValue = new ItemData(item, amount);
    }
    
    
    [Button]
    public void AddItem(ItemData itemData)
    {
        AddItem(itemData.ItemReference.Value, itemData.Quantity);
    }
    
    
    public int GetQuantity(Item item)
    {
        if(inventory.ContainsKey(item))
        {
            return inventory[item];
        }
        return 0;
        
    }
    
    public bool ValidateQuantity(Item item, int amount = 0)
    {
        return inventory.ContainsKey(item) && inventory[item] >= amount;
    }

    

    

    public void OnSave()
    {
        
    }

    public void OnLoad()
    {
        
    }
    
    private void Initialize()
    {
        
    }

    

    public void OnSceneStarted(Scene scene)
    {
        
    }

    public void OnSceneStopped(Scene scene)
    {
        
    }

    public void OnEditorStopped()
    {
        inventory = null;
    }

    public Task Init()
    {
        Item[] registryItems = ScriptableObjectManager.GetAssetsByType<Item>();
        inventory = new();
        foreach (var item in registryItems)
        {
            AddItem(item, 0);
        }
        
        return Task.CompletedTask;
    }
}

