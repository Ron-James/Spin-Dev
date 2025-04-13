using System;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public class ItemQuantity : IEquatable<ItemQuantity>
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


    public ItemQuantity()
    {
        item = null;
        itemReference = null;
        quantity = 0;
    }

    public ItemQuantity(Item item, int quantity)
    {
        this.item = item;
        itemReference = new ScriptableObjectReference<Item>(item);
        this.quantity = quantity;
    }

    public bool Equals(ItemQuantity other)
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
        return Equals((ItemQuantity)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(item, itemReference, quantity);
    }
}