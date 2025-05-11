using UnityEngine;

public class InventoryMediator : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to inventory-related events
        if (GameServices.EventManagerService != null)
        {
            GameServices.EventManagerService.OnItemSlotSwap += HandleItemSlotSwap;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (GameServices.EventManagerService != null)
        {
            GameServices.EventManagerService.OnItemSlotSwap -= HandleItemSlotSwap;
        }
    }

    private void HandleItemSlotSwap(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI)
    {
        // Check if both slots are valid
        if (sourceSlotUI == null || targetSlotUI == null)
        {
            Debug.LogWarning("Invalid slot UI references for swapping");
            return;
        }

        // Get the inventory components for both source and target
        var sourceInfo = GetInventoryInfo(sourceSlotUI);
        var targetInfo = GetInventoryInfo(targetSlotUI);

        if (sourceInfo.inventory == null || targetInfo.inventory == null)
        {
            Debug.LogWarning("Could not find inventory references for swapping");
            return;
        }

        // Check if we're swapping within the same inventory or between different inventories
        if (sourceInfo.inventory == targetInfo.inventory)
        {
            // Same inventory swap
            HandleSameInventorySwap(sourceSlotUI, targetSlotUI, sourceInfo, targetInfo);
        }
        else
        {
            // Different inventory swap/transfer
            HandleDifferentInventorySwap(sourceSlotUI, targetSlotUI, sourceInfo, targetInfo);
        }
    }

    private void HandleSameInventorySwap(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI, InventoryInfo sourceInfo, InventoryInfo targetInfo)
    {
        InventoryBase inventory = sourceInfo.inventory;

        int sourceIndex = GetSlotIndex(sourceSlotUI, sourceInfo);
        int targetIndex = GetSlotIndex(targetSlotUI, targetInfo);

        if (sourceIndex == -1 || targetIndex == -1)
        {
            Debug.LogWarning($"Could not determine slot indices. Source: {sourceIndex}, Target: {targetIndex}");
            return;
        }

        bool success = inventory.TrySwapItems(sourceIndex, targetIndex);

        // Notify that the swap is complete
        if (GameServices.EventManagerService != null)
        {
            GameServices.EventManagerService.ItemSlotSwapComplete(sourceSlotUI, targetSlotUI, success);
        }
    }

    private void HandleDifferentInventorySwap(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI, InventoryInfo sourceInfo, InventoryInfo targetInfo)
    {
        InventoryBase sourceInventory = sourceInfo.inventory;
        InventoryBase targetInventory = targetInfo.inventory;

        int sourceIndex = GetSlotIndex(sourceSlotUI, sourceInfo);
        int targetIndex = GetSlotIndex(targetSlotUI, targetInfo);

        if (sourceIndex == -1 || targetIndex == -1)
        {
            Debug.LogWarning($"Could not determine slot indices. Source: {sourceIndex}, Target: {targetIndex}");
            return;
        }

        bool success = false;

        // Get source item info
        var sourceSlot = sourceInventory.GetItemSlotList()[sourceIndex];
        var targetSlot = targetInventory.GetItemSlotList()[targetIndex];

        if (!sourceSlot.isEmpty())
        {
            ItemSO sourceItem = sourceSlot.GetItem();
            int sourceQuantity = sourceSlot.GetQuantity();

            if (targetSlot.isEmpty())
            {
                // Target slot is empty, try to transfer the item
                success = sourceInventory.TryTransferItemTo(sourceIndex, targetInventory, targetIndex);
            }
            else
            {
                // Target slot has an item, try to swap
                ItemSO targetItem = targetSlot.GetItem();
                int targetQuantity = targetSlot.GetQuantity();

                // Check if items are the same and stackable
                if (sourceItem == targetItem && sourceItem.isStackable)
                {
                    // Try to stack items
                    if (targetSlot.TryAddItems(sourceItem, sourceQuantity, out int leftover))
                    {
                        // Successfully stacked, remove from source
                        sourceSlot.Clear();

                        // If there's leftover, put it back in source
                        if (leftover > 0)
                        {
                            sourceSlot.SetItem(sourceItem, leftover);
                        }

                        // Update UIs
                        sourceInventory.UpdateUI(sourceIndex, sourceSlot.GetItem(), sourceSlot.GetQuantity());
                        targetInventory.UpdateUI(targetIndex, targetSlot.GetItem(), targetSlot.GetQuantity());

                        success = true;
                    }
                }
                else
                {
                    // Different items, perform a swap
                    // Take all items from source
                    if (sourceSlot.TakeItems(sourceQuantity, out ItemSO takenSource, out int takenSourceQuantity))
                    {
                        // Take all items from target
                        if (targetSlot.TakeItems(targetQuantity, out ItemSO takenTarget, out int takenTargetQuantity))
                        {
                            // Put source item in target slot
                            targetSlot.TryAddItems(takenSource, takenSourceQuantity, out _);
                            // Put target item in source slot
                            sourceSlot.TryAddItems(takenTarget, takenTargetQuantity, out _);

                            // Update UIs
                            sourceInventory.UpdateUI(sourceIndex, sourceSlot.GetItem(), sourceSlot.GetQuantity());
                            targetInventory.UpdateUI(targetIndex, targetSlot.GetItem(), targetSlot.GetQuantity());

                            success = true;
                        }
                        else
                        {
                            // Failed to take from target, put back in source
                            sourceSlot.TryAddItems(takenSource, takenSourceQuantity, out _);
                        }
                    }
                }
            }
        }

        // Notify that the swap/transfer is complete
        if (GameServices.EventManagerService != null)
        {
            GameServices.EventManagerService.ItemSlotSwapComplete(sourceSlotUI, targetSlotUI, success);
        }
    }

    private struct InventoryInfo
    {
        public InventoryBase inventory;
        public InventoryUI inventoryUI;
        public ToolbarUI toolbarUI;
        public bool isToolbar;
    }

    private InventoryInfo GetInventoryInfo(ItemSlotUI slotUI)
    {
        InventoryInfo info = new InventoryInfo();

        if (slotUI == null)
        {
            return info;
        }

        // Traverse up the hierarchy to find either InventoryUI or ToolbarUI
        Transform current = slotUI.transform;
        while (current != null)
        {
            // Check for InventoryUI first
            InventoryUI inventoryUI = current.GetComponent<InventoryUI>();
            if (inventoryUI != null)
            {
                info.inventoryUI = inventoryUI;
                info.inventory = inventoryUI.GetInventory();
                info.isToolbar = false;
                break;
            }

            // Check for ToolbarUI (for Toolbar inventory)
            ToolbarUI toolbarUI = current.GetComponent<ToolbarUI>();
            if (toolbarUI != null)
            {
                info.toolbarUI = toolbarUI;
                // For toolbar, we need to get the Toolbar component (which is the InventoryBase)
                Toolbar toolbar = current.GetComponentInParent<Toolbar>();
                if (toolbar != null)
                {
                    info.inventory = toolbar;
                    info.isToolbar = true;
                }
                break;
            }

            current = current.parent;
        }

        return info;
    }

    private int GetSlotIndex(ItemSlotUI slotUI, InventoryInfo info)
    {
        if (info.isToolbar && info.toolbarUI != null)
        {
            // For toolbar, we need to find the index manually
            Transform slotsParent = info.toolbarUI.transform.Find("SlotManager");
            if (slotsParent != null)
            {
                for (int i = 0; i < slotsParent.childCount; i++)
                {
                    if (slotsParent.GetChild(i).GetComponent<ItemSlotUI>() == slotUI)
                    {
                        return i;
                    }
                }
            }
        }
        else if (!info.isToolbar && info.inventoryUI != null)
        {
            // For regular inventory UI, use the existing method
            return info.inventoryUI.GetSlotIndex(slotUI);
        }

        return -1;
    }
}