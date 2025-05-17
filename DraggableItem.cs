using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public ItemSlotUI sourceSlotUI;
    [HideInInspector] public int dragQuantity = 0;
    [HideInInspector] public bool isPartialDrag = false;

    [SerializeField] private GameObject itemPrefab; // Reference to the item prefab
    [SerializeField] private Image image;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector2 originalPosition;
    [SerializeField] private Transform originalParent;
    [SerializeField] private int originalSiblingIndex;

    // Track shift state separately
    private bool isShiftCurrentlyHeld = false;

    private GameObject quantityTextObj;
    private TMPro.TextMeshProUGUI quantityText;
    private ItemSO currentItemSO;

    // New field for temporary drag object
    private GameObject tempDragObject;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Find quantity text component
        quantityTextObj = transform.Find("QuantityText")?.gameObject;
        if (quantityTextObj != null)
        {
            quantityText = quantityTextObj.GetComponent<TMPro.TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        // Subscribe to shift state changes
        if (GameServices.GameInputService != null)
        {
            GameServices.GameInputService.OnShiftHoldChanged += GameInput_OnShiftHoldChanged;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (GameServices.GameInputService != null)
        {
            GameServices.GameInputService.OnShiftHoldChanged -= GameInput_OnShiftHoldChanged;
        }
    }

    private void GameInput_OnShiftHoldChanged(object sender, bool isHeld)
    {
        isShiftCurrentlyHeld = isHeld;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store the source slot for reference on drop
        sourceSlotUI = GetComponentInParent<ItemSlotUI>();
        if (sourceSlotUI == null) return;

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Save the current item before drag
        currentItemSO = sourceSlotUI?.GetCurrentItemSO();

        // Get inventory info
        var inventoryInfo = GetInventoryInfo(sourceSlotUI);
        if (inventoryInfo.inventory == null || currentItemSO == null) return;

        int slotIndex = sourceSlotUI.GetIndex();
        var itemSlot = inventoryInfo.inventory.GetItemSlotList()[slotIndex];
        int totalQuantity = itemSlot.GetQuantity();

        // Right-click to split stack
        if (eventData.button == PointerEventData.InputButton.Right && totalQuantity > 1)
        {
            // Calculate drag quantity
            dragQuantity = Mathf.CeilToInt(totalQuantity / 2f);
            isPartialDrag = true;

            // Create temp drag object from prefab
            tempDragObject = Instantiate(itemPrefab, canvas.transform);
            tempDragObject.name = "TempDragItem";

            // Get and set up the components
            Image dragImage = tempDragObject.transform.Find("ItemSprite").GetComponent<Image>();
            dragImage.sprite = currentItemSO.sprite;

            // Get quantity text component
            TMPro.TextMeshProUGUI quantText = tempDragObject.transform.Find("QuantityText").GetComponent<TMPro.TextMeshProUGUI>();
            if (dragQuantity > 1)
            {
                quantText.text = dragQuantity.ToString();
                quantText.gameObject.SetActive(true);
            }
            else
            {
                quantText.gameObject.SetActive(false);
            }

            // Set position and size
            RectTransform dragRect = tempDragObject.GetComponent<RectTransform>();
            dragRect.sizeDelta = new Vector2(70, 70);
            dragRect.position = eventData.position;

            // Add transparency
            CanvasGroup dragCanvasGroup = tempDragObject.GetComponent<CanvasGroup>();
            if (dragCanvasGroup == null)
                dragCanvasGroup = tempDragObject.AddComponent<CanvasGroup>();
            dragCanvasGroup.alpha = 0.6f;
            dragCanvasGroup.blocksRaycasts = false;

            // Update source slot visual to show remaining quantity
            int remainingQuantity = totalQuantity - dragQuantity;
            sourceSlotUI.UpdateVisual(currentItemSO, remainingQuantity);

            return;
        }

        // Standard drag logic for left-click
        dragQuantity = totalQuantity;
        isPartialDrag = false;

        // Set this to the canvas level for proper dragging
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        // Make it pass through other UI elements
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    private void UpdateDragVisual(ItemSO itemSO, int quantity)
    {
        if (image != null)
        {
            image.sprite = itemSO.sprite;
            image.enabled = true;
        }

        if (quantityText != null && itemSO.isStackable)
        {
            if (quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityTextObj.SetActive(true);
            }
            else
            {
                quantityTextObj.SetActive(false);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move with the mouse/touch
        if (tempDragObject != null)
        {
            tempDragObject.GetComponent<RectTransform>().position = eventData.position;
        }
        else
        {
            rectTransform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool usedTempDrag = tempDragObject != null;

        // Check what we dropped on
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        ItemSlotUI targetSlotUI = null;

        if (dropTarget != null)
        {
            // Look for ItemSlotUI in the drop target or its parents
            targetSlotUI = dropTarget.GetComponent<ItemSlotUI>();
            if (targetSlotUI == null)
            {
                targetSlotUI = dropTarget.GetComponentInParent<ItemSlotUI>();
            }
        }

        // Clean up temporary drag object if it exists
        if (usedTempDrag)
        {
            Destroy(tempDragObject);
            tempDragObject = null;
        }
        else
        {
            // Reset transparency and raycast blocking
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // Return to original parent and position
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;
        }

        // If we dropped on a different slot, trigger the swap
        if (targetSlotUI != null && targetSlotUI != sourceSlotUI)
        {
            // Trigger the item swap event with quantity info
            if (GameServices.EventManagerService != null)
            {
                GameServices.EventManagerService.OnItemSwap(sourceSlotUI, targetSlotUI, dragQuantity, isPartialDrag);
            }
        }
        else if (isPartialDrag && sourceSlotUI != null)
        {
            // Dropped on invalid location, restore the source slot UI
            var inventoryInfo = GetInventoryInfo(sourceSlotUI);
            if (inventoryInfo.inventory != null)
            {
                int slotIndex = sourceSlotUI.GetIndex();
                var itemSlot = inventoryInfo.inventory.GetItemSlotList()[slotIndex];

                // Just restore the UI to match the actual data model
                if (currentItemSO != null)
                {
                    sourceSlotUI.UpdateVisual(currentItemSO, itemSlot.GetQuantity());
                }
            }
        }

        // Reset drag info
        dragQuantity = 0;
        isPartialDrag = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");

        // Direct check for shift key state instead of using tracked variable
        bool isShiftDown = GameServices.GameInputService != null &&
                           GameServices.GameInputService.IsShiftDirectlyPressed();

        if (eventData.button == PointerEventData.InputButton.Left && isShiftDown)
        {
            Debug.Log("Shift + Clicked - Direct check");
            sourceSlotUI = GetComponentInParent<ItemSlotUI>();
            if (sourceSlotUI != null && GameServices.EventManagerService != null)
            {
                GameServices.EventManagerService.OnQuickTransfer(sourceSlotUI);
            }
        }
    }

    private InventoryInfo GetInventoryInfo(ItemSlotUI slotUI)
    {
        InventoryInfo info = new InventoryInfo();

        if (slotUI == null)
        {
            return info;
        }

        // Traverse up the hierarchy to find either InventoryUI or ToolbarUI
        Transform current = slotUI.transform;
        while (current != null)
        {
            // Check for InventoryUI first
            InventoryUI inventoryUI = current.GetComponent<InventoryUI>();
            if (inventoryUI != null)
            {
                info.inventoryUI = inventoryUI;
                info.inventory = inventoryUI.GetInventory();
                info.isToolbar = false;
                break;
            }

            // Check for ToolbarUI (for Toolbar inventory)
            ToolbarUI toolbarUI = current.GetComponent<ToolbarUI>();
            if (toolbarUI != null)
            {
                info.toolbarUI = toolbarUI;
                // For toolbar, we need to get the Toolbar component (which is the InventoryBase)
                Toolbar toolbar = current.GetComponentInParent<Toolbar>();
                if (toolbar != null)
                {
                    info.inventory = toolbar;
                    info.isToolbar = true;
                }
                break;
            }

            current = current.parent;
        }

        return info;
    }

    private struct InventoryInfo
    {
        public InventoryBase inventory;
        public InventoryUI inventoryUI;
        public ToolbarUI toolbarUI;
        public bool isToolbar;
    }
}