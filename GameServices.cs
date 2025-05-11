using UnityEngine;

public static class GameServices
{
    public static GameInput GameInputService { get; private set; }
    public static InventoryUIManager InventoryUIManagerService { get; private set; }
    public static EventManager EventManagerService { get; private set; }

    public static void RegisterInput(GameInput input) => GameInputService = input;
    public static void RegisterInventoryUI(InventoryUIManager ui) => InventoryUIManagerService = ui;
    public static void RegisterEvents(EventManager events) => EventManagerService = events;
}