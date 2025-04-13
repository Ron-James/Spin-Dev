using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;



[CreateAssetMenu(fileName = "InventoryDataManager", menuName = "InventoryDataManager")]
public class InventoryDataManager : SaveableSO<InventoryDataManager>, ISceneCycleListener, IInitializable
{
    [SerializeField] Dictionary<Item, int> inventory = new();

    [SerializeField, Save, ReadOnly] private List<ItemQuantity> _savedStock = new();

    [Title("Variable References")] [SerializeField]
    private ActionReference<ItemQuantity> lastAddedItemData = new();
    private ActionReference<Consumable> lastConsumableUsed = new();
    
    public Item[] allItems => inventory.Keys.ToArray();
    public Dictionary<Item, int> Inventory => inventory;

    /// <summary>
    /// Add an item to the inventory.
    /// Notify to listeners that an item was added.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="amount">the quantity you want to add</param>
    [Button]
    public void AddItem(Item item, int amount)
    {
        if(inventory == null)
        {
            inventory = new();
        }
        item.SetInventoryToBeAddedTo(this);
        if(!inventory.TryAdd(item, amount))
        {
            inventory[item] += amount;
        }

        _savedStock = GetSavedStock();
        lastAddedItemData.Value = new ItemQuantity(item, amount);
        
    }
    
    
    [Button]
    public void AddItem(ItemQuantity itemQuantity)
    {
        AddItem(itemQuantity.ItemReference.Value, itemQuantity.Quantity);
    }
    
    /// <summary>
    /// Get quanitity of a specific item in the inventory.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int GetQuantity(Item item)
    {
        if(inventory.ContainsKey(item))
        {
            return inventory[item];
        }
        return 0;
        
    }
    /// <summary>
    /// Check if the inventory has a specific item and if it has enough quantity.
    /// </summary>
    /// <param name="item">The item you want to check</param>
    /// <param name="amount">The quantity you want to validate</param>
    /// <returns></returns>
    public bool ValidateQuantity(Item item, int amount = 0)
    {
        return inventory.ContainsKey(item) && inventory[item] >= amount;
    }
    

    /// <summary>
    /// Getsx the inventory stock in an easily serializable format for saving.
    /// </summary>
    /// <returns></returns>
    private List<ItemQuantity> GetSavedStock()
    {
        List<ItemQuantity> list = new();
        foreach (var kvp in inventory)
        {
            Item item = kvp.Key;
            int quantity = kvp.Value;
            ItemQuantity itemQuantity = new ItemQuantity(item, quantity);
            list.Add(itemQuantity);
            
        }
        
        return list;
    }
    
    
    /// <summary>
    /// Sync the saved stock data with the current inventory.
    /// </summary>
    [Button]
    private void SyncStockData()
    {
        foreach (var itemTransfer in _savedStock)
        {
            Item item = itemTransfer.ItemReference.Value;
            int amount = itemTransfer.Quantity;
            item.SetInventoryToBeAddedTo(this);
            if(!inventory.TryAdd(item, amount))
            {
                inventory[item] += amount;
            }
        }
    }
    [Button]
    private void PopulateEmpty()
    {
        Item[] registryItems = Resources.LoadAll<Item>("");
        inventory = new();
        foreach (var item in registryItems)
        {
            if(inventory.ContainsKey(item))
            {
                inventory[item] = 0;
            }
            else
            {
                inventory.Add(item, 0);
            }
        }

    }
    private void AddMissingItems()
    {
        Item[] registryItems = ScriptableObjectManager.GetAssetsByType<Item>();
        foreach (var item in registryItems)
        {
            inventory.TryAdd(item, 0);
        }
    }
    
    

    public void OnSceneStarted(Scene scene)
    {
        
    }

    public void OnSceneStopped(Scene scene)
    {
        
    }

    public void OnEditorStopped()
    {
        PopulateEmpty();
        _savedStock.Clear();
    }
    

    public Task Init()
    {
        AddMissingItems();
        return Task.CompletedTask;
    }
    
    

    public override void OnSave()
    {
        _savedStock = GetSavedStock();
    }

    public override void OnLoad()
    {
        SyncStockData();
    }
}

