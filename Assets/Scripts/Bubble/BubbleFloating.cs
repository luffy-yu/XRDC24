using UnityEngine;

namespace XRDC24.Bubble
{

    public class BubbleFloating : MonoBehaviour
    {
        public float riseSpeed = 1.0f; // Speed of rising
        public float driftSpeed = 0.5f; // Horizontal drift speed
        public float rotationSpeed = 30f; // Rotation speed
        public float maxY = 0.05f; // Maximum height before shaking starts
        public float shakeIntensity = 0.1f; // Intensity of the shake
        public int shakeFrequency = 10; // Shake every n frames
        public bool isShaking = false;

        private Vector3 originalPosition;
        private Vector3 driftDirection;
        private int frameCounter = 0; // Frame counter for triggering shake
        
        private SphereCollider sphereCollider;

        void Awake()
        {
            // Initialize a random horizontal drift direction
            driftDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            sphereCollider = GetComponent<SphereCollider>();
            originalPosition = transform.position;
            sphereCollider.enabled = true;
            frameCounter = 0;
        }

        void Update()
        {
            if (!isShaking)
            {
                // Move the bubble upwards
                transform.Translate(Vector3.up * riseSpeed * Time.deltaTime, Space.Self);

                // Add horizontal drifting
                transform.Translate(driftDirection * driftSpeed * Time.deltaTime, Space.Self);

                // Rotate the bubble for a natural effect
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

                // Check if the bubble reaches the height limit
                if (transform.position.y >= maxY)
                {
                    originalPosition = transform.position;
                    isShaking = true;
                    frameCounter = 0; // Reset the frame counter
                    // enable collider
                    sphereCollider.enabled = true;
                }
            }
            else
            {
                // Increment the frame counter
                frameCounter++;

                // Check if it's time to shake
                if (frameCounter % shakeFrequency == 0)
                {
                    ShakeBubble();
                }
            }
        }

        private void ShakeBubble()
        {
            // Apply random shaking within the defined intensity
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity)
            );

            transform.position = originalPosition + shakeOffset;

            // Reset position to avoid permanent displacement
            Invoke(nameof(ResetPosition), 1f); // Reset after a short delay to create the shake effect
        }

        private void ResetPosition()
        {
            transform.position = originalPosition;
        }
    }

}