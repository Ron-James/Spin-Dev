
using System;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public abstract class Item : SerializableScriptableObject
{
    [SerializeField, ReadOnly] protected InventoryDataManager _inventoryToBeAddedTo;
    public virtual void AddToInventory(int quantity)
    {
        if(_inventoryToBeAddedTo == null)
        {
            _inventoryToBeAddedTo = ScriptableObjectManager.LoadFirstScriptableObject<InventoryDataManager>();
        }
        try
        {
            _inventoryToBeAddedTo.AddItem(this, quantity);
        }
        catch
        {
            Debug.LogError("InventoryDataManager not found");
        }
    }
    
    

    
    public void SetInventoryToBeAddedTo(InventoryDataManager inventoryDataManager)
    {
        _inventoryToBeAddedTo = inventoryDataManager;
    }


    public virtual bool ValidateQuantity(int quantity = 1)
    {
        try
        {
            return _inventoryToBeAddedTo.ValidateQuantity(this, quantity);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
    }
    

    
    
    
}



public abstract class Consumable : Item
{
    [SerializeField] private UnityEvent<int> OnConsumed;
    public virtual Task Consume(int quantity = 1)
    {
        int amount = -1 * Mathf.Abs(quantity);
        AddToInventory(amount);
        OnConsumed?.Invoke(quantity);
        return Task.CompletedTask;
    }

    [Button]
    public void TryConsume(int quantity)
    {
        if (!ValidateQuantity(quantity))
        {
            Debug.LogError("Not enough items in inventory");
            return;
        }

        AsyncCommand cmd = new AsyncCommand("Consume " + this.name, () => Consume(quantity));
        CommandRunner.ExecuteCommand(cmd);
    }
    
    [Button]
    public void ConsumeFreely(int quantity = 1)
    {
        AsyncCommand cmd = new AsyncCommand("Consume " + this.name, () => Consume(quantity));
        CommandRunner.ExecuteCommand(cmd);
    }
}