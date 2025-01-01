using UnityEngine;

namespace XRDC24.Bubble
{

    public class BubbleFloating : MonoBehaviour
    {
        public float riseSpeed = 1.0f; // Speed of rising
        public float driftSpeed = 0.5f; // Horizontal drift speed
        public float rotationSpeed = 30f; // Rotation speed
        public float maxY = 10.0f; // Maximum height before shaking starts
        public float shakeIntensity = 0.1f; // Intensity of the shake
        public int shakeFrequency = 10; // Shake every n frames

        private Vector3 originalPosition;
        private Vector3 driftDirection;
        private bool isShaking = false;
        private int frameCounter = 0; // Frame counter for triggering shake

        void Awake()
        {
            // Initialize a random horizontal drift direction
            driftDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        }

        void Update()
        {
            if (!isShaking)
            {
                // Move the bubble upwards
                transform.Translate(Vector3.up * riseSpeed * Time.deltaTime, Space.World);

                // Add horizontal drifting
                transform.Translate(driftDirection * driftSpeed * Time.deltaTime, Space.World);

                // Rotate the bubble for a natural effect
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

                // Check if the bubble reaches the height limit
                if (transform.position.y >= maxY)
                {
                    originalPosition = transform.position;
                    isShaking = true;
                    frameCounter = 0; // Reset the frame counter
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
            Invoke(nameof(ResetPosition), 0.05f); // Reset after a short delay to create the shake effect
        }

        private void ResetPosition()
        {
            transform.position = originalPosition;
        }
    }

}