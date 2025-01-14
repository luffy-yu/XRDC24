using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XRDC24.Environment;
using XRDC24.Interaction;

namespace XRDC24.Helper
{
    [RequireComponent(typeof(Camera))]
    public class CustomInputHandler : MonoBehaviour
    {
        public GameObject OVRCameraRig;
        public FigmaFrameController frameController;
        public RoomDissolveController dissolveController;
        public List<GameObject> gestureRecognizers;
        public GuidedGestureController gestureController;
        public Text helpText; // Assign a UI Text element in the Inspector to display help information

        private bool isHelpVisible = false;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool enableRotation = false;

        void Start()
        {
            // disable VR camera
            OVRCameraRig.SetActive(false);
            // disable debug mode
            frameController.inDebugMode = false;
            dissolveController.inDebugMode = false;
            // disable gesture recognizers
            foreach (var gr in gestureRecognizers)
            {
                // gr.SetActive(false);
            }
            // enable auto loop
            gestureController.AutoLoop = true;
            
            // Save the initial position and rotation for reset functionality
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            // Ensure help text is hidden initially
            if (helpText != null)
            {
                helpText.gameObject.SetActive(false);
            }
        }

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleInput();
#endif
        }

        private void HandleInput()
        {
            // Toggle help information with Tab
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                isHelpVisible = !isHelpVisible;
                if (helpText != null)
                {
                    helpText.gameObject.SetActive(isHelpVisible);
                }
            }
            
            // Camera rotation
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // disable
                enableRotation = false;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) ||
                Input.GetKeyDown(KeyCode.Mouse2))
            {
                // enable
                enableRotation = true;
            }

            // WASD for movement
            float moveSpeed = 5f;
            float horizontal = Input.GetAxis("Horizontal"); // A/D
            float vertical = Input.GetAxis("Vertical"); // W/S
            Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
            transform.Translate(movement, Space.Self);

            if (enableRotation)
            {
                // Mouse for camera rotation
                float mouseSensitivity = 2f;
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                transform.Rotate(Vector3.up, mouseX, Space.World);
                transform.Rotate(Vector3.left, mouseY, Space.Self);   
            }

            // N to trigger next function
            if (Input.GetKeyDown(KeyCode.N))
            {
                TriggerNextFunction();
            }

            // P to trigger previous function
            if (Input.GetKeyDown(KeyCode.P))
            {
                TriggerPreviousFunction();
            }

            // Space to reset position and rotation
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResetPositionAndRotation();
            }
        }

        private void TriggerNextFunction()
        {
            Debug.Log("Next function triggered.");
            frameController.NextFrame();
        }

        private void TriggerPreviousFunction()
        {
            Debug.Log("Previous function triggered.");
            frameController.PreviousFrame();
        }

        private void ResetPositionAndRotation()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            // reset frames
            frameController.ResetFrame();
            // reset dissolve
            dissolveController.Revert();
            Debug.Log("Position and rotation reset.");
        }
    }
}