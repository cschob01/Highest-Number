using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkInputManager))]
public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private NetworkInputManager inputManager;
    private Rigidbody rb;

    [SerializeField] private Transform head;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private CardPhysics cardPhysics;
    [SerializeField] private GameObject[] enableLocally;

    [SerializeField] private CardPhysics card;

    private float headPitch;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("[NetworkPlayer] Rigidbody component not found");
        }

        inputManager = GetComponent<NetworkInputManager>();
    }

    public void SetCardPlayer(bool isPlayer1)
    {
        if (card != null)
        {
            card.isPlayer1 = isPlayer1;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Camera.main.gameObject.SetActive(false);

        EnableLocally();
    }

    private void EnableLocally()
    {
        foreach (GameObject obj in enableLocally)
        {
            obj.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        HandleMovement();
        HandleJump();
        HandleLook();
        HandleCard();
    }

    private void HandleMovement()
    {
        Vector2 input = inputManager.MoveReadValue();

        Vector3 movement =
            transform.right * input.x +
            transform.forward * input.y;

        // Normalize so diagonal movement isn't faster
        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        // Set horizontal velocity
        Vector3 velocity = movement * moveSpeed;

        // Preserve vertical velocity (gravity/jumping)
        rb.linearVelocity = new Vector3(
            velocity.x,
            rb.linearVelocity.y,
            velocity.z
        );
    }

    private void HandleJump()
    {
        if (inputManager.JumpPressed())
        {
            Debug.Log("Jump!");
        }
    }

    private void HandleLook()
    {
        Vector2 look = inputManager.LookReadValue();

        // Horizontal: rotate the body
        transform.Rotate(Vector3.up, look.x * lookSensitivity);

        // Vertical: rotate the head around its own pivot
        headPitch -= look.y * lookSensitivity;
        headPitch = Mathf.Clamp(headPitch, -80f, 80f);

        head.localRotation = Quaternion.Euler(headPitch, 0f, 0f);
    }

    private void HandleCard()
    {
        if (inputManager.DropCardReadValue()) cardPhysics.Drop();
        if (inputManager.PickupCardReadValue()) cardPhysics.Pickup();
    }
}