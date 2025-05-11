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
        itemSlotUIList.Add(newItemSlotUI);
    }

    public void UpdateUI(int index, ItemSO itemSO, int quantity = 1)
    {
        if (index >= 0 && index < itemSlotUIList.Count)
        {
            itemSlotUIList[index].UpdateVisual(itemSO, quantity);
        }
    }
}