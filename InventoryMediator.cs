using UnityEngine;

public class InventoryMediator : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to inventory-related events
        if (GameServices.EventManagerService != null)
        {
            GameServices.EventManagerService.OnItemSlotSwap += HandleItemSlotSwap;
            GameServices.EventManagerService.OnQuickItemTransfer += HandleQuickTransfer;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (GameServices.EventManagerService != null)
        {
            GameServices.EventManagerService.OnItemSlotSwap -= HandleItemSlotSwap;
            GameServices.EventManagerService.OnQuickItemTransfer -= HandleQuickTransfer;
        }
    }

    private void HandleQuickTransfer(ItemSlotUI sourceSlotUI)
    {
        Debug.Log("=== QUICK TRANSFER START ===");

        // Add this line to see if multiple quick transfers are being triggered
        Debug.Log($"Quick Transfer triggered for slot {sourceSlotUI?.GetIndex()} at time {Time.time}");

        // Check if the source slot is valid
        if (sourceSlotUI == null)
        {
            Debug.LogError("Invalid source slot for quick transfer");
            return;
        }

        // Get the inventory info for the source slot
        var sourceInfo = GetInventoryInfo(sourceSlotUI);

        if (sourceInfo.inventory == null)
        {
            Debug.LogError("Could not find source inventory for quick transfer");
            return;
        }

        // Determine the target inventory (opposite of source)
        InventoryBase targetInventory = null;

        // Check if source is toolbar, if so transfer to currently open inventory
        if (sourceInfo.isToolbar)
        {
            Debug.Log("Source is toolbar, looking for open inventory");
            if (GameServices.InventoryUIManagerService != null)
            {
                var currentlyOpen = GameServices.InventoryUIManagerService.GetCurrentlyOpenInventory();
                if (currentlyOpen != null)
                {
                    targetInventory = currentlyOpen;
                    Debug.Log($"Found target inventory: {currentlyOpen.name}");
                }
                else
                {
                    Debug.LogWarning("No open inventory found");
                }
            }
        }
        else
        {
            Debug.Log("Source is inventory, looking for toolbar");
            // Source is an inventory UI, transfer to toolbar
            // Find the toolbar (player's toolbar)
            Toolbar toolbar = FindAnyObjectByType<Toolbar>();
            if (toolbar != null)
            {
                targetInventory = toolbar;
                Debug.Log($"Found target toolbar: {toolbar.name}");
            }
            else
            {
                Debug.LogWarning("No toolbar found");
            }
        }

        if (targetInventory == null)
        {
            Debug.LogError("Could not find target inventory for quick transfer");
            return;
        }

        // Get source slot index
        int sourceIndex = GetSlotIndex(sourceSlotUI, sourceInfo);
        if (sourceIndex == -1)
        {
            Debug.LogError("Could not determine source slot index");
            return;
        }

        Debug.Log($"Source slot index: {sourceIndex}");

        // Get source item info
        var sourceSlot = sourceInfo.inventory.GetItemSlotList()[sourceIndex];
        if (sourceSlot.isEmpty())
        {
            Debug.LogWarning("Source slot is empty");
            return;
        }

        ItemSO sourceItem = sourceSlot.GetItem();
        int sourceQuantity = sourceSlot.GetQuantity();

        Debug.Log($"Transferring {sourceQuantity}x {sourceItem.itemName}");

        // Try to add to target inventory (handles stacking automatically)
        if (targetInventory.TryAddItem(sourceItem, sourceQuantity, out int leftover))
        {
            // Successfully transferred, update source slot
            if (leftover == 0)
            {
                // All items transferred
                sourceSlot.Clear();
                Debug.Log("All items transferred, clearing source slot");
            }
            else
            {
                // Some items couldn't transfer
                sourceSlot.SetItem(sourceItem, leftover);
                Debug.Log($"Partial transfer, {leftover} items remain in source");
            }

            // Update source UI
            sourceInfo.inventory.UpdateUI(sourceIndex, sourceSlot.GetItem(), sourceSlot.GetQuantity());

            Debug.Log($"Quick transferred {sourceQuantity - leftover} {sourceItem.itemName}(s)");
        }
        else
        {
            Debug.LogWarning($"Could not transfer {sourceItem.itemName} - no space available");
        }

        Debug.Log("=== QUICK TRANSFER END ===");
    }

    private void HandleItemSlotSwap(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI, int dragQuantity, bool isPartialDrag)
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
            HandleSameInventorySwap(sourceSlotUI, targetSlotUI, sourceInfo, targetInfo, dragQuantity, isPartialDrag);
        }
        else
        {
            // Different inventory swap/transfer
            HandleDifferentInventorySwap(sourceSlotUI, targetSlotUI, sourceInfo, targetInfo, dragQuantity, isPartialDrag);
        }
    }

    private void HandleSameInventorySwap(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI, InventoryInfo sourceInfo, InventoryInfo targetInfo, int dragQuantity, bool isPartialDrag)
    {
        InventoryBase inventory = sourceInfo.inventory;

        int sourceIndex = GetSlotIndex(sourceSlotUI, sourceInfo);
        int targetIndex = GetSlotIndex(targetSlotUI, targetInfo);

        if (sourceIndex == -1 || targetIndex == -1)
        {
            Debug.LogWarning($"Could not determine slot indices. Source: {sourceIndex}, Target: {targetIndex}");
            return;
        }

        bool success = false;

        if (isPartialDrag && dragQuantity > 0)
        {
            // Handle partial drag within same inventory
            success = HandlePartialStackSplit(inventory, sourceIndex, targetIndex, dragQuantity);
        }
        else
        {
            // Full swap/move
            success = inventory.TrySwapItems(sourceIndex, targetIndex);
        }

        // Notify that the swap is complete
        if (GameServices.EventManagerService != null)
        {
            GameServices.EventManagerService.ItemSlotSwapComplete(sourceSlotUI, targetSlotUI, success);
        }
    }

    private void HandleDifferentInventorySwap(ItemSlotUI sourceSlotUI, ItemSlotUI targetSlotUI, InventoryInfo sourceInfo, InventoryInfo targetInfo, int dragQuantity, bool isPartialDrag)
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
        var sourceSlot = sourceInventory.GetItemSlotList()[sourceIndex];
        var targetSlot = targetInventory.GetItemSlotList()[targetIndex];

        if (!sourceSlot.isEmpty())
        {
            ItemSO sourceItem = sourceSlot.GetItem();
            int quantityToTransfer = isPartialDrag && dragQuantity > 0 ? dragQuantity : sourceSlot.GetQuantity();

            if (targetSlot.isEmpty())
            {
                // Target is empty - transfer the specified quantity
                if (sourceSlot.TakeItems(quantityToTransfer, out ItemSO takenItem, out int takenQuantity))
                {
                    if (targetSlot.TryAddItems(takenItem, takenQuantity, out int leftover))
                    {
                        // Put any leftover back in source
                        if (leftover > 0)
                        {
                            sourceSlot.TryAddItems(takenItem, leftover, out _);
                        }

                        sourceInventory.UpdateUI(sourceIndex, sourceSlot.GetItem(), sourceSlot.GetQuantity());
                        targetInventory.UpdateUI(targetIndex, targetSlot.GetItem(), targetSlot.GetQuantity());
                        success = true;
                    }
                    else
                    {
                        // Failed to add to target, put back in source
                        sourceSlot.TryAddItems(takenItem, takenQuantity, out _);
                    }
                }
            }
            else
            {
                // Target has item - check if same and stackable
                ItemSO targetItem = targetSlot.GetItem();

                if (sourceItem == targetItem && sourceItem.isStackable)
                {
                    // Same item - try to stack
                    if (sourceSlot.TakeItems(quantityToTransfer, out ItemSO takenItem, out int takenQuantity))
                    {
                        if (targetSlot.TryAddItems(takenItem, takenQuantity, out int leftover))
                        {
                            // Put any leftover back in source
                            if (leftover > 0)
                            {
                                sourceSlot.TryAddItems(takenItem, leftover, out _);
                            }

                            sourceInventory.UpdateUI(sourceIndex, sourceSlot.GetItem(), sourceSlot.GetQuantity());
                            targetInventory.UpdateUI(targetIndex, targetSlot.GetItem(), targetSlot.GetQuantity());
                            success = true;
                        }
                        else
                        {
                            // Failed to stack, put back in source
                            sourceSlot.TryAddItems(takenItem, takenQuantity, out _);
                        }
                    }
                }
                else if (!isPartialDrag)
                {
                    // Different items and full drag - swap them
                    int sourceQuantity = sourceSlot.GetQuantity();
                    int targetQuantity = targetSlot.GetQuantity();

                    if (sourceSlot.TakeItems(sourceQuantity, out ItemSO takenSource, out int takenSourceQuantity) &&
                        targetSlot.TakeItems(targetQuantity, out ItemSO takenTarget, out int takenTargetQuantity))
                    {
                        sourceSlot.TryAddItems(takenTarget, takenTargetQuantity, out _);
                        targetSlot.TryAddItems(takenSource, takenSourceQuantity, out _);

                        sourceInventory.UpdateUI(sourceIndex, sourceSlot.GetItem(), sourceSlot.GetQuantity());
                        targetInventory.UpdateUI(targetIndex, targetSlot.GetItem(), targetSlot.GetQuantity());
                        success = true;
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

    private bool HandlePartialStackSplit(InventoryBase inventory, int sourceIndex, int targetIndex, int quantityToMove)
    {
        var sourceSlot = inventory.GetItemSlotList()[sourceIndex];
        var targetSlot = inventory.GetItemSlotList()[targetIndex];

        if (sourceSlot.isEmpty() || quantityToMove <= 0) return false;

        ItemSO sourceItem = sourceSlot.GetItem();

        if (targetSlot.isEmpty())
        {
            // Target is empty - move partial stack
            if (sourceSlot.TakeItems(quantityToMove, out ItemSO takenItem, out int takenQuantity))
            {
                targetSlot.TryAddItems(takenItem, takenQuantity, out _);
                inventory.UpdateUI(sourceIndex, sourceSlot.GetItem(), sourceSlot.GetQuantity());
                inventory.UpdateUI(targetIndex, targetSlot.GetItem(), targetSlot.GetQuantity());
                return true;
            }
        }
        else if (targetSlot.GetItem() == sourceItem && sourceItem.isStackable)
        {
            // Same item - try to stack partial amount
            if (sourceSlot.TakeItems(quantityToMove, out ItemSO takenItem, out int takenQuantity))
            {
                if (targetSlot.TryAddItems(takenItem, takenQuantity, out int leftover))
                {
                    // Put any leftover back in source
                    if (leftover > 0)
                    {
                        sourceSlot.TryAddItems(takenItem, leftover, out _);
                    }

                    inventory.UpdateUI(sourceIndex, sourceSlot.GetItem(), sourceSlot.GetQuantity());
                    inventory.UpdateUI(targetIndex, targetSlot.GetItem(), targetSlot.GetQuantity());
                    return true;
                }
                else
                {
                    // Failed to add, put back in source
                    sourceSlot.TryAddItems(takenItem, takenQuantity, out _);
                }
            }
        }

        return false;
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
        // Use the index stored in the ItemSlotUI itself - much more reliable!
        if (slotUI != null)
        {
            return slotUI.GetIndex();
        }

        return -1;
    }
}