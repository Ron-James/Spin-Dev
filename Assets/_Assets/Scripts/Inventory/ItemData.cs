using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemData : IEquatable<ItemData>
{
    [SerializeField] private Item item;
    [SerializeField, ReadOnly] private ScriptableObjectReference<Item> itemReference;
    [SerializeField] private int quantity;

    public int Quantity => quantity;

    public ScriptableObjectReference<Item> ItemReference => itemReference;
        
        
    public ItemData()
    {
        item = null;
        this.itemReference = null;
        this.quantity = 0;
    }
        
    public ItemData(Item item, int quantity)
    {
        this.item = item;
        itemReference = new ScriptableObjectReference<Item>(item);
        this.quantity = quantity;
    }

    public bool Equals(ItemData other)
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
        return Equals((ItemData)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(item, itemReference, quantity);
    }
}