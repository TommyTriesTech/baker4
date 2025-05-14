using System.Collections.Generic;
using UnityEngine;

public class ToolbarUI : MonoBehaviour
{
    [SerializeField] private Transform slotManager;
    [SerializeField] private GameObject itemSlotUI;

    private List<ItemSlotUI> itemSlotUIList = new List<ItemSlotUI>();

    public void AddItemSlotUI()
    {
        GameObject newItemSlot = Instantiate(itemSlotUI, slotManager);
        ItemSlotUI newItemSlotUI = newItemSlot.GetComponent<ItemSlotUI>();

        // Set the index based on the current count
        newItemSlotUI.SetIndex(itemSlotUIList.Count);

        itemSlotUIList.Add(newItemSlotUI);
    }

    public void UpdateUI(int index, ItemSO itemSO, int quantity = 1)
    {
        if (index >= 0 && index < itemSlotUIList.Count)
        {
            itemSlotUIList[index].UpdateVisual(itemSO, quantity);
        }
    }

    // Helper method to get slot index - using the stored index
    public int GetSlotIndex(ItemSlotUI slotUI)
    {
        if (slotUI != null)
        {
            return slotUI.GetIndex();
        }
        return itemSlotUIList.IndexOf(slotUI);
    }
}