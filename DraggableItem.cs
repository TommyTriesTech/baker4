using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public ItemSlotUI sourceSlotUI;

    [SerializeField] private Image image;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Vector2 originalPosition;
    [SerializeField] private Transform originalParent;
    [SerializeField] private int originalSiblingIndex;

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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store the source slot for reference on drop
        sourceSlotUI = GetComponentInParent<ItemSlotUI>();
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Set this to the canvas level for proper dragging
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        // Make it pass through other UI elements
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move with the mouse/touch
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Reset properties
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Return to original parent and position first
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = originalPosition;

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

        // If we dropped on a different slot, trigger the swap
        if (targetSlotUI != null && targetSlotUI != sourceSlotUI)
        {
            // Trigger the item swap event
            if (GameServices.EventManagerService != null)
            {
                GameServices.EventManagerService.OnItemSwap(sourceSlotUI, targetSlotUI);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check for Left click + Shift held
        if (eventData.button == PointerEventData.InputButton.Left &&
            GameServices.GameInputService != null &&
            GameServices.GameInputService.IsShiftHeld())
        {
            sourceSlotUI = GetComponentInParent<ItemSlotUI>();

            if (sourceSlotUI != null && GameServices.EventManagerService != null)
            {
                // Trigger quick transfer event
                GameServices.EventManagerService.OnQuickTransfer(sourceSlotUI);
            }
        }
    }
}