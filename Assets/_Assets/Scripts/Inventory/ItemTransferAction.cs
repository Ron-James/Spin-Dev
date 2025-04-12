using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataEvent", menuName = "Custom Events/ItemDataEvent")]
public class ItemTransferAction : ActionSO<ItemTransfer>
{
    public void Raise(Item item, int quantity)
    {
        Raise(new ItemTransfer(item, quantity));
    }
}