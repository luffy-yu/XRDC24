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

        private void OnTriggerEnter(Collider other)
        {
            // if it's hand
            var name = other.gameObject.name;
            // Add box collider to LeftHand/RightHand which has a SkinnedMeshRender
            if (name.Contains("Hand"))
            {
                OnTriggerTriggered?.Invoke(triggerType);
            }
        }
    }
}