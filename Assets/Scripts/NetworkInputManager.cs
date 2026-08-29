using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkInputManager : NetworkBehaviour
{
    private PlayerInput playerInput;

    private InputAction move;
    private InputAction jump;
    private InputAction look;
    private InputAction pickupCard;
    private InputAction dropCard;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogWarning("[NetworkInputManager] PlayerInput component not found");
        }
    }

    private void GetBindings()
    {
        move = GetAction("Move");
        jump = GetAction("Jump");
        look = GetAction("Look");
        dropCard = GetAction("DropCard");
        pickupCard = GetAction("PickupCard");

        Debug.Log($"Move map enabled: {move?.actionMap.enabled}, DropCard map enabled: {dropCard?.actionMap.enabled}");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        GetBindings();
    }

    public override void OnStartClient()
    {
        Debug.Log($"OnStartClient: {gameObject.name}, isLocalPlayer: {isLocalPlayer}");

        if (!isLocalPlayer)
        {
            playerInput.enabled = false;
            enabled = false;
        }
    }

    private InputAction GetAction(string actionName)
    {
        InputAction action = playerInput?.actions.FindAction(actionName);

        if (action == null)
        {
            Debug.LogWarning($"[NetworkInputAction] Input action '{actionName}' was not found.");
        }

        return action;
    }

    public Vector2 MoveReadValue()
    {
        return move.ReadValue<Vector2>();
    }

    public bool JumpPressed()
    {
        return jump.WasPressedThisFrame();
    }

    public Vector2 LookReadValue()
    {
        return look.ReadValue<Vector2>();
    }

    public bool PickupCardReadValue()
    {
        return pickupCard.WasPressedThisFrame();
    }

    public bool DropCardReadValue()
    {
        return dropCard.WasPressedThisFrame();
    }
}
