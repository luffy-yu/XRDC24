using UnityEngine;

namespace XRDC24.Bubble
{

    public class RigidbodyFallSimulator : MonoBehaviour
    {
        private Rigidbody rb;

        // Gravity scale multiplier
        public float gravityScale = 1.0f;

        // Offset range for randomizing the fall direction
        public float offsetRange = 1.0f;

        private bool isFalling = false;

        void Start()
        {
            rb = GetComponent<Rigidbody>();

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            rb.useGravity = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartFalling();
            }
        }

        public void StartFalling()
        {
            isFalling = true;
            rb.useGravity = true;

            rb.linearVelocity = Vector3.zero;

            Vector3 randomOffset = new Vector3(
                Random.Range(-offsetRange, offsetRange),
                0,
                Random.Range(-offsetRange, offsetRange)
            );

            Vector3 customGravity = Physics.gravity + randomOffset;


            rb.AddForce(customGravity * rb.mass * gravityScale, ForceMode.Acceleration);
        }

        public void ResetFall(Vector3 startPosition)
        {
            isFalling = false;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            transform.position = startPosition;
        }
    }
}