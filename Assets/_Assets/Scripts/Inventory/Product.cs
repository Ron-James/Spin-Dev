using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Product", menuName = "Inventory/Product")]
public class Product : SerializableScriptableObject
{
    [SerializeField] private InventoryDataManager inventoryDataManager;
    [SerializeReference] private List<ItemQuantity> itemsToTrade;
    [SerializeReference] private List<ItemQuantity> itemsToReceive;


    public async Task TradeAsync()
    {
        bool validateTrade = itemsToTrade.TrueForAll(item =>
            inventoryDataManager.ValidateQuantity(item.ItemReference.Value, item.Quantity));
        if (!validateTrade)
        {
#if Unity_EDITOR
            Debug.LogError("Trade not valid");
#endif
            return;
        }

        foreach (var item in itemsToTrade)
        {
            Item itemToRemove = item.ItemReference.Value;
            int amount = Mathf.Abs(item.Quantity) * -1;
            inventoryDataManager.AddItem(itemToRemove, amount);
            await Awaitable.NextFrameAsync();
        }

        foreach (var item in itemsToReceive)
        {
            Item itemToAdd = item.ItemReference.Value;
            int amount = Mathf.Abs(item.Quantity);
            inventoryDataManager.AddItem(itemToAdd, amount);
            await Awaitable.NextFrameAsync();
        }
    }

    [Button]
    public void TryTrade()
    {
        AsyncCommand cmd = new AsyncCommand("Trade Items " + name, () => TradeAsync());
        CommandRunner.ExecuteCommand(cmd);
    }
}