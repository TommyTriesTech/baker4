using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryBase : MonoBehaviour
{
    [SerializeField] protected List<ItemSlot> itemSlotList;
    [SerializeField] protected int itemSlotCount;

    public virtual void Initialise()
    {
        itemSlotList = new List<ItemSlot>();
        for (int i = 0; i < itemSlotCount; i++)
        {
            itemSlotList.Add(new ItemSlot());
        }
    }

    public virtual bool TryAddItemToInventory(Item item)
    {
        ItemSO itemToAdd = item.GetItemSO();
        Debug.Log($"Adding {itemToAdd.itemName} to inventory");

        // First try to stack with existing items
        if (itemToAdd.isStackable)
        {
            for (int i = 0; i < itemSlotList.Count; i++)
            {
                if (!itemSlotList[i].isEmpty() && itemSlotList[i].CanAddToStack(itemToAdd))
                {
                    itemSlotList[i].AddToStack(1);
                    UpdateUI(i, itemToAdd, itemSlotList[i].GetQuantity());
                    return true;
                }
            }
        }

        // If stacking failed or item isn't stackable, find empty slot
        for (int i = 0; i < itemSlotList.Count; i++)
        {
            if (itemSlotList[i].isEmpty())
            {
                itemSlotList[i].SetItem(itemToAdd, 1);
                UpdateUI(i, itemToAdd, 1);
                return true;
            }
        }

        return false;
    }

    public virtual bool TrySwapItems(int sourceIndex, int targetIndex)
    {
        // Check if indices are valid
        if (sourceIndex < 0 || sourceIndex >= itemSlotList.Count ||
            targetIndex < 0 || targetIndex >= itemSlotList.Count)
        {
            return false;
        }

        // Handle stacking if items are the same and stackable
        if (!itemSlotList[sourceIndex].isEmpty() && !itemSlotList[targetIndex].isEmpty())
        {
            ItemSO sourceItem = itemSlotList[sourceIndex].GetItem();
            ItemSO targetItem = itemSlotList[targetIndex].GetItem();

            if (sourceItem == targetItem && sourceItem.isStackable)
            {
                // Try to stack items
                int leftover = itemSlotList[targetIndex].AddToStack(itemSlotList[sourceIndex].GetQuantity());

                if (leftover == 0)
                {
                    // All items stacked, clear source slot
                    itemSlotList[sourceIndex].Clear();
                    UpdateUI(sourceIndex, null, 0);
                }
                else
                {
                    // Some items couldn't stack, update source slot
                    itemSlotList[sourceIndex].SetItem(sourceItem, leftover);
                    UpdateUI(sourceIndex, sourceItem, leftover);
                }

                // Update target slot UI
                UpdateUI(targetIndex, targetItem, itemSlotList[targetIndex].GetQuantity());

                return true;
            }
        }

        ItemSO sourceSO = itemSlotList[sourceIndex].GetItem();
        int sourceQuantity = itemSlotList[sourceIndex].GetQuantity();

        ItemSO targetSO = itemSlotList[targetIndex].GetItem();
        int targetQuantity = itemSlotList[targetIndex].GetQuantity();

        if (targetSO == null)
        {
            // Target slot is empty, just move the item
            itemSlotList[targetIndex].SetItem(sourceSO, sourceQuantity);
            itemSlotList[sourceIndex].Clear();
        }
        else
        {
            // Swap the items
            itemSlotList[sourceIndex].SetItem(targetSO, targetQuantity);
            itemSlotList[targetIndex].SetItem(sourceSO, sourceQuantity);
        }

        // Update UI
        UpdateUI(sourceIndex, itemSlotList[sourceIndex].GetItem(), itemSlotList[sourceIndex].GetQuantity());
        UpdateUI(targetIndex, itemSlotList[targetIndex].GetItem(), itemSlotList[targetIndex].GetQuantity());

        return true;
    }

    // New method for transferring items between different inventories
    public virtual bool TryTransferItemTo(int sourceIndex, InventoryBase targetInventory, int targetIndex)
    {
        // Validate indices
        if (sourceIndex < 0 || sourceIndex >= itemSlotList.Count ||
            !itemSlotList[sourceIndex].CanTake(1))
        {
            return false;
        }

        if (targetIndex < 0 || targetIndex >= targetInventory.itemSlotList.Count)
        {
            return false;
        }

        int sourceQuantity = itemSlotList[sourceIndex].GetQuantity();

        // Get the item from source slot
        if (!itemSlotList[sourceIndex].TakeItems(sourceQuantity, out ItemSO itemToTransfer, out int quantityToTransfer))
        {
            return false;
        }

        // Try to add it to target slot
        if (!targetInventory.itemSlotList[targetIndex].TryAddItems(itemToTransfer, quantityToTransfer, out int leftover))
        {
            // Can't add to target, put back in source
            itemSlotList[sourceIndex].TryAddItems(itemToTransfer, quantityToTransfer, out _);
            return false;
        }

        // Update UIs
        UpdateUI(sourceIndex, itemSlotList[sourceIndex].GetItem(), itemSlotList[sourceIndex].GetQuantity());
        targetInventory.UpdateUI(targetIndex, targetInventory.itemSlotList[targetIndex].GetItem(),
                                 targetInventory.itemSlotList[targetIndex].GetQuantity());

        return true;
    }

    // Method to try adding an item to any available slot
    public virtual bool TryAddItem(ItemSO itemToAdd, int quantity, out int leftover)
    {
        leftover = quantity;

        // First try to stack with existing items
        if (itemToAdd.isStackable)
        {
            for (int i = 0; i < itemSlotList.Count; i++)
            {
                if (!itemSlotList[i].isEmpty() && itemSlotList[i].CanAddToStack(itemToAdd))
                {
                    if (itemSlotList[i].TryAddItems(itemToAdd, leftover, out leftover))
                    {
                        UpdateUI(i, itemToAdd, itemSlotList[i].GetQuantity());
                        if (leftover == 0) return true;
                    }
                }
            }
        }

        // Find empty slots for remaining items
        for (int i = 0; i < itemSlotList.Count && leftover > 0; i++)
        {
            if (itemSlotList[i].isEmpty())
            {
                if (itemSlotList[i].TryAddItems(itemToAdd, leftover, out leftover))
                {
                    UpdateUI(i, itemToAdd, itemSlotList[i].GetQuantity());
                }
            }
        }

        return leftover == 0;
    }

    public abstract void UpdateUI(int index, ItemSO itemSO, int quantity);

    public List<ItemSlot> GetItemSlotList()
    {
        return itemSlotList;
    }

    public int GetItemSlotCount()
    {
        return itemSlotCount;
    }

    public int GetSlotIndexFromUI(ItemSlotUI slotUI, InventoryUI inventoryUI)
    {
        if (inventoryUI == null)
        {
            return -1;
        }

        return inventoryUI.GetSlotIndex(slotUI);
    }
}