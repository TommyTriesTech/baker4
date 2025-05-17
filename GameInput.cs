using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    // Events
    public event EventHandler OnInteractAction;
    public event EventHandler OnAlternateInteractAction;
    public event EventHandler OnJumpAction;
    public event EventHandler OnTestAction;
    public event EventHandler OnPauseAction;
    public event EventHandler<bool> OnShiftHoldChanged; // New event for shift state

    // Input values
    private Vector2 movementInput;
    private bool isShiftHeld = false;
    private bool isPointerOverUI = false;


    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        GameServices.RegisterInput(this);
        // Initialize input system
        playerInputActions = new PlayerInputActions();

        // Subscribe to input events
        playerInputActions.Player.Enable();
        playerInputActions.Player.Move.performed += Move_performed;
        playerInputActions.Player.Move.canceled += Move_canceled;
        playerInputActions.Player.Jump.performed += Jump_performed;
        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.AlternateInteract.performed += AlternateInteract_performed;
        playerInputActions.Player.Test.performed += Test_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;

        // Subscribe to shift input
        playerInputActions.Player.HoldShift.performed += HoldShift_performed;
        playerInputActions.Player.HoldShift.canceled += HoldShift_canceled;
    }

    private void Update()
    {
        isPointerOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private void OnDestroy()
    {
        // Clean up subscriptions
        playerInputActions.Player.Move.performed -= Move_performed;
        playerInputActions.Player.Move.canceled -= Move_canceled;
        playerInputActions.Player.Jump.performed -= Jump_performed;
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.AlternateInteract.performed -= AlternateInteract_performed;
        playerInputActions.Player.Test.performed -= Test_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;
        playerInputActions.Player.HoldShift.performed -= HoldShift_performed;
        playerInputActions.Player.HoldShift.canceled -= HoldShift_canceled;

        playerInputActions.Dispose();
    }

    // Input callbacks
    private void Move_performed(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>().normalized;
    }

    private void Move_canceled(InputAction.CallbackContext context)
    {
        movementInput = Vector2.zero;
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        OnJumpAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void AlternateInteract_performed(InputAction.CallbackContext context)
    {

        // Skip world interactions if mouse is over UI
        if (isPointerOverUI)
        {
            return;
        }

        OnAlternateInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void Test_performed(InputAction.CallbackContext context)
    {
        OnTestAction?.Invoke(this, EventArgs.Empty);
    }

    private void Pause_performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void HoldShift_performed(InputAction.CallbackContext context)
    {
        isShiftHeld = true;
        OnShiftHoldChanged?.Invoke(this, true);
    }

    private void HoldShift_canceled(InputAction.CallbackContext context)
    {
        isShiftHeld = false;
        OnShiftHoldChanged?.Invoke(this, false);
    }

    // Public accessor for movement values
    public Vector2 GetMovementVectorNormalized()
    {
        return movementInput;
    }

    // Public accessor for shift state
    public bool IsShiftHeld()
    {
        return isShiftHeld;
    }

    private void OnDisable()
    {
        if (playerInputActions != null)
            playerInputActions.Player.Disable();
    }

    public bool IsShiftDirectlyPressed()
    {
        if (playerInputActions == null) return false;
        return playerInputActions.Player.HoldShift.ReadValue<float>() > 0;
    }
}