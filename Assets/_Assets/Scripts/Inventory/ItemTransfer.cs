using System;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public class ItemTransfer : IEquatable<ItemTransfer>
{
    [SerializeField, DontSave] private Item item;
    [SerializeField, ReadOnly] private ScriptableObjectReference<Item> itemReference;
    [SerializeField] private int quantity;

    public int Quantity => quantity;

    public ScriptableObjectReference<Item> ItemReference => itemReference;
    
    private void UpdateDropIn()
    {
        if (itemReference != null)
        {
            itemReference = new ScriptableObjectReference<Item>(item);
        }
    }
    
        
    public ItemTransfer()
    {
        item = null;
        itemReference = null;
        quantity = 0;
    }
        
    public ItemTransfer(Item item, int quantity)
    {
        this.item = item;
        itemReference = new ScriptableObjectReference<Item>(item);
        this.quantity = quantity;
    }

    public bool Equals(ItemTransfer other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(item, other.item) && Equals(itemReference, other.itemReference) && quantity == other.quantity;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ItemTransfer)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(item, itemReference, quantity);
    }
}