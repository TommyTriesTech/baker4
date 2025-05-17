using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform slotManager;
    [SerializeField] private GameObject itemSlotUI;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private bool isOpen; // Added missing field

    private List<ItemSlotUI> itemSlotUIList = new List<ItemSlotUI>();

    // Reference to the inventory this UI represents
    private InventoryBase inventory;

    private void Awake()
    {
        Hide();
    }

    public void SetInventory(InventoryBase inventory)
    {
        this.inventory = inventory;
    }

    public InventoryBase GetInventory()
    {
        return inventory;
    }

    public void InitialiseSlots(int slotCount)
    {
        foreach (Transform child in slotManager)
        {
            Destroy(child.gameObject);
        }
        itemSlotUIList.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            AddItemSlotUI(i);
        }
    }

    public void AddItemSlotUI(int index = -1)
    {
        GameObject newItemSlot = Instantiate(itemSlotUI, slotManager);
        ItemSlotUI newItemSlotUI = newItemSlot.GetComponent<ItemSlotUI>();

        if (index >= 0)
        {
            newItemSlotUI.SetIndex(index);
        }
        else
        {
            newItemSlotUI.SetIndex(itemSlotUIList.Count);
        }

        itemSlotUIList.Add(newItemSlotUI);
    }

    public void UpdateUI(int index, ItemSO itemSO, int quantity = 1)
    {
        if (index >= 0 && index < itemSlotUIList.Count)
        {
            itemSlotUIList[index].UpdateVisual(itemSO, quantity);
        }
    }

    public void Show()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            isOpen = true;
        }
    }

    public void Hide()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isOpen = false;
        }
    }

    public int GetSlotIndex(ItemSlotUI slotUI)
    {
        return itemSlotUIList.IndexOf(slotUI);
    }

    public ItemSlotUI GetSlotUIByIndex(int index)
    {
        if (index >= 0 && index < itemSlotUIList.Count)
        {
            return itemSlotUIList[index];
        }
        return null;
    }
}