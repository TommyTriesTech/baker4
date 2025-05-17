using UnityEngine;

public class EventManager : MonoBehaviour
{
    public event System.Action<Item> OnItemPickup;
    public event System.Action<Item> OnItemPickupSuccess;
    public event System.Action<ItemSlotUI, ItemSlotUI, int, bool> OnItemSlotSwap; // Fixed signature to match DraggableItem
    public event System.Action<ItemSlotUI, ItemSlotUI, bool> OnItemSlotSwapComplete;
    public event System.Action<ItemSlotUI> OnQuickItemTransfer;

    private void Awake()
    {
        GameServices.RegisterEvents(this);
    }

    public void ItemPickedUp(Item item)
    {
        Debug.Log("ItemPickedUp triggered");
        OnItemPickup?.Invoke(item);
    }

    public void ItemPickupSuccessful(Item item)
    {
        OnItemPickupSuccess?.Invoke(item);
    }

    public void OnItemSwap(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI, int quantity = -1, bool isPartialDrag = false)
    {
        Debug.Log($"Item swap triggered from slot to slot. Quantity: {quantity}, Partial: {isPartialDrag}");
        OnItemSlotSwap?.Invoke(sourceSlotUI, targetSlotUI, quantity, isPartialDrag);
    }

    public void ItemSlotSwapComplete(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI, bool success)
    {
        OnItemSlotSwapComplete?.Invoke(sourceSlotUI, targetSlotUI, success);
    }

    public void OnQuickTransfer(ItemSlotUI sourceSlotUI)
    {
        Debug.Log($"Quick transfer triggered from slot");
        OnQuickItemTransfer?.Invoke(sourceSlotUI);
    }
}
