using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private InventoryUIManager inventoryUI;
    [SerializeField] private EventManager eventManager;

    private void Awake()
    {
        // Use newer, non-deprecated methods
        if (gameInput == null) gameInput = FindAnyObjectByType<GameInput>();
        if (inventoryUI == null) inventoryUI = FindAnyObjectByType<InventoryUIManager>();
        if (eventManager == null) eventManager = FindAnyObjectByType<EventManager>();

        // Optional: Log missing services
        if (gameInput == null) Debug.LogError("GameInput missing!");
        if (inventoryUI == null) Debug.LogError("InventoryUIManager missing!");
        if (eventManager == null) Debug.LogError("EventManager missing!");
    }
}