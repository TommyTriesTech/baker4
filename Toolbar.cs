using System.Collections.Generic;
using UnityEngine;

public class Toolbar : InventoryBase
{
    [SerializeField] private ToolbarUI toolbarUI;

    private void Awake()
    {
        Initialise();
        InitializeUI();
    }

    private void Start()
    {
        GameServices.EventManagerService.OnItemPickup += HandleItemPickup;
    }

    private void OnDestroy()
    {
        if (GameServices.EventManagerService != null)
        {
            GameServices.EventManagerService.OnItemPickup -= HandleItemPickup;
        }
    }

    private void InitializeUI()
    {
        for (int i = 0; i < itemSlotCount; i++)
        {
            toolbarUI.AddItemSlotUI();
        }
    }

    public override void UpdateUI(int index, ItemSO itemSO, int quantity)
    {
        toolbarUI.UpdateUI(index, itemSO, quantity);
    }

    private void HandleItemPickup(Item item)
    {
        Debug.Log($"Pickup event received for {item.itemSO.itemName}");
        bool wasAdded = TryAddItemToInventory(item);
        if (wasAdded)
        {
            Destroy(item.gameObject);
        }
    }
}
