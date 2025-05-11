using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    // References
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Toolbar toolbar;

    // Movement parameters
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private float terminalVelocity = -20f;
    [SerializeField] private float jumpHeight = 2f;

    // State
    [SerializeField] private bool isPaused = false;
    private Vector3 velocity;
    private RaycastHit currentHit;
    private float xRotation = 0f;
    private bool hasHit;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        GameServices.GameInputService.OnInteractAction += GameInput_OnInteractAction;
        GameServices.GameInputService.OnAlternateInteractAction += GameInputService_OnAlternateInteractAction;
        GameServices.GameInputService.OnPauseAction += GameInput_OnPauseAction;
        GameServices.GameInputService.OnJumpAction += GameInput_OnJumpAction;
        GameServices.GameInputService.OnTestAction += GameInput_OnTestAction;
    }


    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameServices.GameInputService != null)
        {
            GameServices.GameInputService.OnInteractAction -= GameInput_OnInteractAction;
            GameServices.GameInputService.OnAlternateInteractAction -= GameInputService_OnAlternateInteractAction;
            GameServices.GameInputService.OnPauseAction -= GameInput_OnPauseAction;
            GameServices.GameInputService.OnJumpAction -= GameInput_OnJumpAction;
            GameServices.GameInputService.OnTestAction -= GameInput_OnTestAction;
        }
    }

    private void GameInput_OnTestAction(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        if (isPaused) return;

        // Unified raycast
        hasHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out currentHit, interactRange);

        HandleLooking();
        HandleGravity();
        HandleMovement();

        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactRange, Color.yellow);
    }

    #region Event Handlers
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (isPaused && GameServices.InventoryUIManagerService != null && !GameServices.InventoryUIManagerService.HasOpenInventory())
            return;

        // If inventory is open, close it and return
        if (GameServices.InventoryUIManagerService != null && GameServices.InventoryUIManagerService.HasOpenInventory())
        {
            GameServices.InventoryUIManagerService.HideInventory(GameServices.InventoryUIManagerService.GetCurrentlyOpenInventory());
            return;
        }

        // Otherwise, interact with world object
        if (!hasHit) return;
        IInteractable interactable = currentHit.transform.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact();
        }
    }

    private void GameInputService_OnAlternateInteractAction(object sender, EventArgs e)
    {
        // If game is paused but no inventory is open, ignore interaction
        if (isPaused && GameServices.InventoryUIManagerService != null && !GameServices.InventoryUIManagerService.HasOpenInventory())
            return;

        if (!hasHit) return;
        IInventory inventory = currentHit.transform.GetComponent<IInventory>();
        if (inventory != null)
        {
            inventory.OpenInventory();
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePause();
    }

    private void GameInput_OnJumpAction(object sender, EventArgs e)
    {
        if (isPaused) return;

        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void Instance_OnNumberKeyPressed(object sender, int e)
    {
        Debug.Log(e);
    }

    #endregion

    #region Movement and Input Handling
    private void HandleLooking()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameServices.GameInputService.GetMovementVectorNormalized();

        Vector3 moveDir = transform.right * inputVector.x + transform.forward * inputVector.y;
        Vector3 horizontalMove = moveDir * moveSpeed;

        Vector3 fullMove = horizontalMove + Vector3.up * velocity.y;

        characterController.Move(fullMove * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            float currentGravity = gravity;
            if (velocity.y < 0)
            {
                currentGravity *= fallMultiplier;
            }

            velocity.y += currentGravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        }
    }
    #endregion

    #region Game State
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // External method to set pause state (used by UI system)
    public void SetPauseState(bool paused)
    {
        if (isPaused != paused)
        {
            isPaused = paused;

            // Update cursor state
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }
    }
    #endregion

}