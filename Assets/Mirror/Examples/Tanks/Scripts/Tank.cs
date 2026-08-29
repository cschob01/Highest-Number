using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Mirror.Examples.Tanks
{
    public class Tank : NetworkBehaviour
    {
        [Header("Components")]
        public NavMeshAgent agent;
        public Animator  animator;
        public TextMesh  healthBar;
        public Transform turret;

        [Header("Movement")]
        public float rotationSpeed = 100;

        [Header("Firing")]
        public KeyCode shootKey = KeyCode.Space;
        public GameObject projectilePrefab;
        public Transform  projectileMount;

        [Header("Stats")]
        [SyncVar] public int health = 4;

        void Update()
        {
            // always update health bar.
            // (SyncVar hook would only update on clients, not on server)
            healthBar.text = new string('-', health);
            
            // take input from focused window only
            if(!Application.isFocused) return; 

            // movement for local player
            if (isLocalPlayer)
            {
                // Rotate
                float horizontal = 0f;

                if (Keyboard.current.aKey.isPressed)
                    horizontal -= 1f;

                if (Keyboard.current.dKey.isPressed)
                    horizontal += 1f;

                transform.Rotate(
                    0,
                    horizontal * rotationSpeed * Time.deltaTime,
                    0
                );

                // Move
                float vertical = 0f;

                if (Keyboard.current.wKey.isPressed)
                    vertical += 1f;

                if (Keyboard.current.sKey.isPressed)
                    vertical -= 1f;

                Vector3 forward = transform.TransformDirection(Vector3.forward);

                agent.velocity =
                    forward * Mathf.Max(vertical, 0) * agent.speed;

                animator.SetBool(
                    "Moving",
                    agent.velocity != Vector3.zero
                );

                // Shoot
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    CmdFire();
                }

                RotateTurret();
            }
        }

        // this is called on the server
        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, projectileMount.rotation);
            NetworkServer.Spawn(projectile);
            RpcOnFire();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }

        [ServerCallback]
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Projectile>() != null)
            {
                --health;
                if (health == 0)
                    NetworkServer.Destroy(gameObject);
            }
        }

        void RotateTurret()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                Debug.DrawLine(ray.origin, hit.point);

                Vector3 lookRotation = new Vector3(
                    hit.point.x,
                    turret.transform.position.y,
                    hit.point.z
                );

                turret.transform.LookAt(lookRotation);
            }
        }
    }
}
