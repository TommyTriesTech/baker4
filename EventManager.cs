using UnityEngine;

public class EventManager : MonoBehaviour
{
    // Convert from static class to MonoBehaviour
    public event System.Action<Item> OnItemPickup;
    public event System.Action<Item> OnItemPickupSuccess;
    public event System.Action<ItemSlotUI, ItemSlotUI> OnItemSlotSwap;
    public event System.Action<ItemSlotUI, ItemSlotUI, bool> OnItemSlotSwapComplete;

    private void Awake()
    {
        GameServices.RegisterEvents(this);
    }

    // Method to trigger item pickup event
    public void ItemPickedUp(Item item)
    {
        Debug.Log("ItemPickedUp triggered");
        OnItemPickup?.Invoke(item);
    }

    // Method to trigger successful pickup event
    public void ItemPickupSuccessful(Item item)
    {
        OnItemPickupSuccess?.Invoke(item);
    }

    // Method to trigger item swap event
    public void OnItemSwap(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI)
    {
        Debug.Log($"Item swap triggered from slot to slot");
        OnItemSlotSwap?.Invoke(sourceSlotUI, targetSlotUI);
    }

    // Method to trigger swap completion event
    public void ItemSlotSwapComplete(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI, bool success)
    {
        OnItemSlotSwapComplete?.Invoke(sourceSlotUI, targetSlotUI, success);
    }
}