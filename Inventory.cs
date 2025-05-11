using System.Collections.Generic;
using UnityEngine;

public class Inventory : InventoryBase
{
    public override void Initialise()
    {
        base.Initialise();
    }

    public override void UpdateUI(int index, ItemSO itemSO, int quantity)
    {
        if (GameServices.InventoryUIManagerService != null)
        {
            GameServices.InventoryUIManagerService.UpdateInventoryUI(this, index, itemSO, quantity);
        }
    }
}
