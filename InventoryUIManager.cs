using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private List<InventoryUIPrefab> inventoryUIPrefabs;
    [SerializeField] private Transform canvasTransform;

    private Dictionary<InventoryType, GameObject> inventoryUIPrefabDict = new Dictionary<InventoryType, GameObject>();
    private Dictionary<Inventory, GameObject> inventoryUIInstances = new Dictionary<Inventory, GameObject>();

    private Inventory currentlyOpenInventory = null;

    // Reference to the player script
    [SerializeField] private Player player;

    [System.Serializable]
    public class InventoryUIPrefab
    {
        public InventoryType type;
        public GameObject prefab;
    }

    private void Awake()
    {
        GameServices.RegisterInventoryUI(this);

        foreach (InventoryUIPrefab prefabInfo in inventoryUIPrefabs)
        {
            inventoryUIPrefabDict[prefabInfo.type] = prefabInfo.prefab;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("InventoryUIManager couldn't find Player reference!");
        }
    }

    private void OnEnable()
    {
        if (GameServices.GameInputService != null)
        {
            GameServices.GameInputService.OnPauseAction += GameInput_OnPauseAction;
        }
    }

    private void OnDisable()
    {
        if (GameServices.GameInputService != null)
        {
            GameServices.GameInputService.OnPauseAction -= GameInput_OnPauseAction;
        }
    }

    private void GameInput_OnPauseAction(object sender, System.EventArgs e)
    {
        if (currentlyOpenInventory != null)
        {
            HideInventory(currentlyOpenInventory);
        }
    }

    public InventoryUI ShowInventory(InventoryType type, Inventory inventory)
    {
        // Hide all active inventory UIs
        foreach (var kvp in inventoryUIInstances)
        {
            if (kvp.Key != inventory)
            {
                kvp.Value.SetActive(false);
            }
        }
        // Pause the player
        if (player != null)
        {
            player.SetPauseState(true);
        }

        // Track currently open inventory
        currentlyOpenInventory = inventory;

        // Get or create the inventory UI
        GameObject inventoryUIObject;

        if (inventoryUIInstances.TryGetValue(inventory, out inventoryUIObject))
        {
            inventoryUIObject.SetActive(true);
        }
        else if (inventoryUIPrefabDict.TryGetValue(type, out GameObject prefab))
        {
            inventoryUIObject = Instantiate(prefab, canvasTransform);
            inventoryUIInstances[inventory] = inventoryUIObject;
        }
        else
        {
            Debug.LogError($"No prefab found for inventory type: {type}");
            return null;
        }

        InventoryUI inventoryUI = inventoryUIObject.GetComponent<InventoryUI>();
        if (inventoryUI != null)
        {
            // Set the inventory reference
            inventoryUI.SetInventory(inventory);

            inventoryUI.InitialiseSlots(inventory.GetItemSlotList().Count);

            List<ItemSlot> slots = inventory.GetItemSlotList();
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].isEmpty())
                {
                    inventoryUI.UpdateUI(i, slots[i].GetItem(), slots[i].GetQuantity());
                }
                else
                {
                    // Make sure empty slots are properly cleared
                    inventoryUI.UpdateUI(i, null, 0);
                }
            }

            inventoryUI.Show();
        }

        return inventoryUI;
    }

    public void UpdateInventoryUI(Inventory inventory, int slotIndex, ItemSO itemSO, int quantity = 1)
    {
        // Check if this inventory has a UI instance
        if (inventoryUIInstances.TryGetValue(inventory, out GameObject inventoryUIObject))
        {
            // Get the InventoryUI component
            InventoryUI inventoryUI = inventoryUIObject.GetComponent<InventoryUI>();

            // Update the specific slot
            if (inventoryUI != null)
            {
                inventoryUI.UpdateUI(slotIndex, itemSO, quantity);
            }
        }
    }

    public void HideInventory(Inventory inventory)
    {
        if (inventoryUIInstances.TryGetValue(inventory, out GameObject inventoryUIObject))
        {
            InventoryUI inventoryUI = inventoryUIObject.GetComponent<InventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.Hide();
            }

            // If this is the currently open inventory, unpause the player
            if (inventory == currentlyOpenInventory)
            {
                if (player != null)
                {
                    player.SetPauseState(false);
                }
                currentlyOpenInventory = null;
            }
        }
    }

    public bool HasOpenInventory()
    {
        return currentlyOpenInventory != null;
    }

    public Inventory GetCurrentlyOpenInventory()
    {
        return currentlyOpenInventory;
    }
}