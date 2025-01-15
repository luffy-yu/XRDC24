using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace XRDC24.Helper
{
    public class FullScreenImage : MonoBehaviour
    {
        public Canvas targetCanvas;

        public Image startImage;
        public Image endImage;
        
        public void ShowStart()
        {
            targetCanvas.enabled = true;
            startImage.gameObject.SetActive(true);
            endImage.gameObject.SetActive(false);
        }

        public void ShowEnd()
        {
            targetCanvas.enabled = true;
            startImage.gameObject.SetActive(false);
            endImage.gameObject.SetActive(true);
        }

        public void DisableSplash()
        {
            targetCanvas.enabled = false;
            startImage.gameObject.SetActive(false);
            endImage.gameObject.SetActive(false);
        }

        void Start()
        {
            ShowStart();
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.S))
            {
                ShowStart();
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                ShowEnd();
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                DisableSplash();
            }
        }
    }
}