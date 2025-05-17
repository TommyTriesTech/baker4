using UnityEngine;

public class ItemSlot
{
    private ItemSO itemSO;
    private int quantity = 0;

    public bool isEmpty()
    {
        return itemSO == null || quantity <= 0;
    }

    public void SetItem(ItemSO newItemSO, int amount = 1)
    {
        itemSO = newItemSO;
        quantity = amount;
    }

    public ItemSO GetItem()
    {
        return itemSO;
    }

    public int GetQuantity()
    {
        return quantity;
    }

    public bool CanAddToStack(ItemSO newItem)
    {
        if (isEmpty()) return true;
        if (!itemSO.isStackable) return false;
        if (itemSO != newItem) return false;
        return quantity < itemSO.maxStackSize;
    }

    public int AddToStack(int amountToAdd)
    {
        if (isEmpty() || !itemSO.isStackable) return amountToAdd;

        int spaceAvailable = itemSO.maxStackSize - quantity;
        int amountToActuallyAdd = Mathf.Min(amountToAdd, spaceAvailable);

        quantity += amountToActuallyAdd;
        return amountToAdd - amountToActuallyAdd; // Return leftover amount
    }

    public void Clear()
    {
        itemSO = null;
        quantity = 0;
    }

    // New methods for transferring items
    public bool CanTake(int amountToTake)
    {
        return !isEmpty() && quantity >= amountToTake;
    }

    public bool TakeItems(int amountToTake, out ItemSO takenItem, out int takenQuantity)
    {
        takenItem = null;
        takenQuantity = 0;

        if (!CanTake(amountToTake))
        {
            return false;
        }

        takenItem = itemSO;
        takenQuantity = Mathf.Min(amountToTake, quantity);
        quantity -= takenQuantity;

        // If we took all items, clear the slot
        if (quantity <= 0)
        {
            Clear();
        }

        return true;
    }

    public bool TryAddItems(ItemSO newItem, int amountToAdd, out int leftover)
    {
        leftover = amountToAdd;

        if (isEmpty())
        {
            // Slot is empty, just add the items
            SetItem(newItem, amountToAdd);
            leftover = 0;
            return true;
        }

        if (CanAddToStack(newItem))
        {
            // Can stack with existing items
            leftover = AddToStack(amountToAdd);
            return true;
        }

        // Can't add to this slot
        return false;
    }
}
