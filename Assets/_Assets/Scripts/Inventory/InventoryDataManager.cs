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

    [SerializeField, Save, ReadOnly] private List<ItemTransfer> _savedStock = new();

    [Title("Variable References")] [SerializeField]
    private ActionReference<ItemTransfer> lastAddedItemData = new();
    private ActionReference<Consumable> lastConsumableUsed = new();
    
    public Item[] allItems => inventory.Keys.ToArray();
    public Dictionary<Item, int> Inventory => inventory;

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
        lastAddedItemData.Value = new ItemTransfer(item, amount);
        
    }
    
    
    [Button]
    public void AddItem(ItemTransfer itemTransfer)
    {
        AddItem(itemTransfer.ItemReference.Value, itemTransfer.Quantity);
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
    
    private void SetItemInInventory(Item item, int amount)
    {
        if(inventory == null)
        {
            inventory = new();
        }
        inventory.TryAdd(item, amount);
    }

    
    private List<ItemTransfer> GetSavedStock()
    {
        List<ItemTransfer> list = new();
        foreach (var kvp in inventory)
        {
            Item item = kvp.Key;
            int quantity = kvp.Value;
            ItemTransfer itemTransfer = new ItemTransfer(item, quantity);
            list.Add(itemTransfer);
            
        }
        
        return list;
    }
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

