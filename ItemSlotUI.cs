using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IDropHandler
{
    // References to child objects (set these in the inspector)
    [SerializeField] private GameObject itemObject; // The "Item" GameObject
    [SerializeField] private Image itemSprite; // The ItemSprite child of Item
    [SerializeField] private TextMeshProUGUI quantityText; // The QuantityText child of Item

    // Current item data
    private ItemSO currentItemSO;
    private int slotIndex = -1;

    // If you prefer, the script can find these automatically
    private void Awake()
    {
        // Auto-find child objects if not assigned
        if (itemObject == null)
        {
            itemObject = transform.Find("Background/Item")?.gameObject;
        }

        if (itemObject != null)
        {
            if (itemSprite == null)
            {
                itemSprite = itemObject.transform.Find("ItemSprite")?.GetComponent<Image>();
            }

            if (quantityText == null)
            {
                quantityText = itemObject.transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    public void SetIndex(int index)
    {
        slotIndex = index;
    }

    public int GetIndex()
    {
        return slotIndex;
    }

    public void ClearSlot()
    {
        currentItemSO = null;

        if (itemObject != null)
        {
            itemObject.SetActive(false);
        }

        if (itemSprite != null)
        {
            itemSprite.sprite = null;
            itemSprite.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // The DraggableItem script will handle the event triggering
        // We don't need to do anything here
    }

    public void UpdateVisual(ItemSO itemSO, int quantity = 1)
    {
        currentItemSO = itemSO;

        if (itemSO != null && quantity > 0)
        {
            // Show the item
            if (itemObject != null)
            {
                itemObject.SetActive(true);
            }

            if (itemSprite != null)
            {
                itemSprite.sprite = itemSO.sprite;
                itemSprite.enabled = true;
            }

            if (quantityText != null)
            {
                if (quantity > 1 && itemSO.isStackable)
                {
                    quantityText.text = quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // Clear the slot
            ClearSlot();
        }
    }

    public ItemSO GetCurrentItemSO()
    {
        return currentItemSO;
    }

    public bool HasItem()
    {
        return currentItemSO != null;
    }
}