using System;
using UnityEngine;

namespace XRDC24.GestureDriven
{
    public class AuroraController : MonoBehaviour
    {
        public GameObject leftAurora;
        public GameObject rightAurora;

        private void Start()
        {
            // disable at the beginning
            leftAurora.SetActive(false);
            rightAurora.SetActive(false);
        }

        public void EnableLeftAurora()
        {
            leftAurora.SetActive(true);
        }

        public void EnableRightAurora()
        {
            rightAurora.SetActive(true);
        }
    }
}