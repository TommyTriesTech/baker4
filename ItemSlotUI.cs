using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject itemObject;
    [SerializeField] private Image itemSprite;
    [SerializeField] private TextMeshProUGUI quantityText;

    private ItemSO currentItemSO;
    private int slotIndex = -1;
    private int originalQuantity = 0; // Track original quantity for drag operations

    private void Awake()
    {
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
        originalQuantity = 0;

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
        // DraggableItem handles event triggering
    }

    public void UpdateVisual(ItemSO itemSO, int quantity = 1)
    {
        currentItemSO = itemSO;
        originalQuantity = quantity;

        if (itemSO != null && quantity > 0)
        {
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
            ClearSlot();
        }
    }

    // Update visual during drag for partial stacks
    public void UpdateDragVisual(int dragQuantity)
    {
        if (quantityText != null && currentItemSO != null && currentItemSO.isStackable)
        {
            if (dragQuantity > 1)
            {
                quantityText.text = dragQuantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
    }

    // Restore original visual after drag
    public void RestoreOriginalVisual()
    {
        if (quantityText != null && currentItemSO != null && currentItemSO.isStackable)
        {
            if (originalQuantity > 1)
            {
                quantityText.text = originalQuantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
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