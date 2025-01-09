using System;
using UnityEngine;

namespace XRDC24.Interaction
{
    public enum TriggerType
    {
        Forward,
        Backward,
        Recording,
    }

    public class TriggerForwarder : MonoBehaviour
    {
        public System.Action<TriggerType> OnTriggerTriggered;

        public TriggerType triggerType;

        public Material pressedMaterial;

        private Material originalMaterial;

        private void Start()
        {
            originalMaterial = GetComponent<Renderer>().material;
        }

        private void OnTriggerEnter(Collider other)
        {
            // if it's hand
            var name = other.gameObject.name;
            // Add box collider to LeftHand/RightHand which has a SkinnedMeshRender
            if (name.Contains("Hand"))
            {
                // change material
                GetComponent<Renderer>().material = pressedMaterial;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // if it's hand
            var name = other.gameObject.name;
            // Add box collider to LeftHand/RightHand which has a SkinnedMeshRender
            if (name.Contains("Hand"))
            {
                // revert material
                GetComponent<Renderer>().material = originalMaterial;
                // trigger after trigger exits
                OnTriggerTriggered?.Invoke(triggerType);
            }
        }
    }
}