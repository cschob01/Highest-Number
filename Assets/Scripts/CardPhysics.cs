using DG.Tweening;
using Mirror;
using UnityEngine;

public class CardPhysics : NetworkBehaviour
{
    private Rigidbody rb;
    private bool pickedUp = false;
    private bool inPickupZone;
    private Collider parentCol;
    private Transform parentTrans;

    [SyncVar] public bool isPlayer1;

    [SyncVar] public bool locked = false;

    [SerializeField] private float dropSpin = .2f;
    [SerializeField] private float dropPush = .2f;
    [SerializeField] private CardInput input;

    private void Update()
    {
        if (pickedUp && parentTrans != null)
        {
            transform.position = parentTrans.position;
            transform.rotation = parentTrans.rotation;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("[CardPhysics] Rigidbody component not found");
        }

        parentTrans = transform.parent;
        if (parentTrans == null)
        {
            Debug.LogWarning("[CardPhysics] Expected parent transform not found");
        }

        parentCol = transform.parent?.GetComponent<Collider>();
        if (parentCol == null || !parentCol.isTrigger)
        {
            Debug.LogWarning("[CardPhysics] Trigger collider component not found in immediate parent");
        }

        transform.SetParent(null, true);
    }

    public void Drop()
    {
        if (!pickedUp) return;

        if (rb != null)
        {
            rb.isKinematic = false;

            rb.AddTorque(transform.right * dropSpin, ForceMode.Impulse);
            rb.AddForce(transform.forward * dropPush, ForceMode.Impulse);
        }

        if (input != null)
        {
            input.SetInputEnabled(false);
        }
        else
        {
            Debug.LogWarning("[CardPhysics] No CardInput component assigned");
        }

            pickedUp = false;
    }

    public void Pickup()
    {
        if (pickedUp) return;
        if (!inPickupZone) return;
        if (locked) return;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
        }

        if (input != null)
        {
            input.SetInputEnabled(true);
        }
        else
        {
            Debug.LogWarning("[CardPhysics] No CardInput component assigned");
        }

        pickedUp = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == parentCol)
        {
            inPickupZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == parentCol)
        {
            inPickupZone = false;
        }
    }
}
