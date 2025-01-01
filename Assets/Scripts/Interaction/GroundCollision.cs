using System;
using UnityEngine;
using XRDC24.Bubble;

namespace XRDC24.Interaction
{
    public class GroundCollision : MonoBehaviour
    {
        private string bubbleTag = "Bubble";

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.gameObject.tag == bubbleTag)
            {
                var rb = other.gameObject.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                // trigger animation
                other.gameObject.GetComponent<BubbleController>().TriggerFallenAnimation();
            }
        }
    }
}