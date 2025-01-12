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
        public System.Action<TriggerType> OnRecordingTriggerEnter;
        public System.Action<TriggerType> OnRecordingTriggerExit;

        public TriggerType triggerType;
        public Material pressedMaterial;
        public AudioSource clickSound;

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

                clickSound.Play();

                // trigger
                OnRecordingTriggerEnter?.Invoke(triggerType);
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

                if (triggerType == TriggerType.Recording)
                    clickSound.Play();

                // trigger after trigger exits
                OnTriggerTriggered?.Invoke(triggerType);
                OnRecordingTriggerExit?.Invoke(triggerType);
            }
        }
    }
}