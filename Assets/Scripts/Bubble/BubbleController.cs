using System;
using System.Collections;
using Unity.VisualScripting;
using XRDC24.Interaction;
using UnityEngine;

namespace XRDC24.Bubble
{
    [RequireComponent(typeof(SphereColliderSurface))]
    public class BubbleController : MonoBehaviour
    {
        private SphereColliderSurface sphereColliderSurface;

        private Rigidbody rbRigidbody;

        public GameObject pokedAnimation;
        public float pokeDuration = 1.0f;
        public GameObject fallenAnimation;
        public float fallenDuration = 1.0f;

        private bool pokedPlaying = false;

        private void Start()
        {
            pokedPlaying = false;

            sphereColliderSurface = GetComponent<SphereColliderSurface>();
            rbRigidbody = gameObject.GetComponent<Rigidbody>();
            sphereColliderSurface.OnHit += OnHit;
        }

        private void OnHit(bool touched)
        {
            // // make bubble drop
            // rbRigidbody.useGravity = true;
            // rbRigidbody.isKinematic = false;

            if (!touched) return;

            if (!pokedPlaying)
            {
                StartCoroutine(OnPoked());
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            print("collision");
        }

        IEnumerator OnPoked()
        {
            pokedPlaying = true;
            // enable
            pokedAnimation.SetActive(true);

            // wait for seconds
            yield return new WaitForSeconds(pokeDuration);

            // disable
            pokedAnimation.SetActive(false);

            // make it re-interactable
            pokedPlaying = false;
            yield return null;
        }

    }
}