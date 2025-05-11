using UnityEngine;

public class Fridge : MonoBehaviour, IInventory
{
    [SerializeField] private Inventory inventory;
    [SerializeField] protected InventoryType inventoryType;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<Inventory>();
        }

        inventory.Initialise();
    }

    public void OpenInventory()
    {
        Debug.Log("interacting with Fridge");

        if (GameServices.InventoryUIManagerService != null)
        {
            GameServices.InventoryUIManagerService.ShowInventory(inventoryType, inventory);
        }
    }
}
