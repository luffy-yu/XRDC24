using System;
using XRDC24.Interaction;
using UnityEngine;

namespace XRDC24.Bubble
{
    [RequireComponent(typeof(SphereColliderSurface))]
    public class BubbleController : MonoBehaviour
    {
        private SphereColliderSurface sphereColliderSurface;

        private Rigidbody rbRigidbody;

        private void Start()
        {
            sphereColliderSurface = GetComponent<SphereColliderSurface>();
            rbRigidbody = gameObject.GetComponent<Rigidbody>();
            sphereColliderSurface.OnHit += OnHit;
        }

        private void OnHit()
        {
            // make bubble drop
            rbRigidbody.useGravity = true;
            rbRigidbody.isKinematic = false;
        }
    }
}