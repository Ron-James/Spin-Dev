
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class Item : SerializableScriptableObject
{
    [SerializeField, ReadOnly] InventoryDataManager _inventoryToBeAddedTo;
    public virtual void AddToInventory(int quantity)
    {
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
}


