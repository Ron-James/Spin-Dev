using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    [SerializeField] private ScriptableObjectReference<Item> item;
    [SerializeField] private int quantity;

    public int Quantity => quantity;

    public ScriptableObjectReference<Item> Item => item;
        
        
    public ItemData()
    {
        this.item = null;
        this.quantity = 0;
    }
        
    public ItemData(Item item, int quantity)
    {
        this.item = new ScriptableObjectReference<Item>(item);
        this.quantity = quantity;
    }
}