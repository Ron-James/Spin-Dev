using UnityEngine;

[CreateAssetMenu(fileName = "ItemQuantityAction", menuName = "Actions/Item Quantity Action")]
public class ItemQuantityAction : ActionSO<ItemQuantity>
{
    public void Raise(Item item, int quantity)
    {
        Raise(new ItemQuantity(item, quantity));
    }
}