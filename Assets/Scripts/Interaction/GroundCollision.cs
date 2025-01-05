using System;
using System.Collections;
using UnityEngine;
using XRDC24.Bubble;

namespace XRDC24.Interaction
{
    public class GroundCollision : MonoBehaviour
    {
        private string bubbleTag = "Bubble";

        private float groundY = 0.0f;

        private float animationOffset = 0.1f; // offset on top of the ground
        private void Awake()
        {
            groundY = transform.position.y;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == bubbleTag)
            {
                StartCoroutine(TriggerFallenAnimation(other.gameObject));
            }
        }

        IEnumerator TriggerFallenAnimation(GameObject go)
        {
            var rb = go.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;

            // reset position to make the animation visible
            var postion = go.transform.position;
            postion.y = groundY + animationOffset;
            go.transform.position = postion;

            yield return new WaitForSeconds(0.1f);

            // trigger animation
            go.gameObject.GetComponent<BubbleController>().TriggerFallenAnimation();

            yield return null;
        }
    }
}