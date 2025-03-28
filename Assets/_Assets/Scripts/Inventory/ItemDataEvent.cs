using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataEvent", menuName = "Custom Events/ItemDataEvent")]
public class ItemDataEvent : EventSO<ItemData>
{
    public void Raise(Item item, int quantity)
    {
        Raise(new ItemData(item, quantity));
    }
}