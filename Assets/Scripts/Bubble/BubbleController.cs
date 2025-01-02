using System;
using System.Collections;
// using Unity.VisualScripting;
using XRDC24.Interaction;
using UnityEngine;

namespace XRDC24.Bubble
{
    public enum AnimationType
    {
        Poke,
        Fallen
    }

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
        private bool fallenPlaying = false;

        public System.Action<GameObject, AnimationType> OnAnimationFinished;

        private void Start()
        {
            pokedPlaying = false;
            fallenPlaying = false;

            sphereColliderSurface = GetComponent<SphereColliderSurface>();
            rbRigidbody = gameObject.GetComponent<Rigidbody>();
            sphereColliderSurface.OnHit += OnHit;
        }

        public void LetFall()
        {
            // make bubble drop
            rbRigidbody.useGravity = true;
            rbRigidbody.isKinematic = false;
            
            // disable floating controller
            GetComponent<BubbleFloating>().enabled = false;
        }

        private void OnHit(bool touched)
        {
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

        private void OnTriggerEnter(Collider other)
        {
            print("trigger"); // works
        }

        IEnumerator OnPoked()
        {
            pokedPlaying = true;
            fallenPlaying = false;
            // enable
            pokedAnimation.SetActive(true);

            // wait for seconds
            yield return new WaitForSeconds(pokeDuration);

            // disable
            pokedAnimation.SetActive(false);

            // make it re-interactable
            pokedPlaying = false;

            if (OnAnimationFinished != null)
            {
                OnAnimationFinished.Invoke(gameObject, AnimationType.Poke);
            }

            yield return null;
        }

        IEnumerator OnFallen()
        {
            fallenPlaying = true;
            pokedPlaying = false;
            // enable
            fallenAnimation.SetActive(true);

            // wait for seconds
            yield return new WaitForSeconds(pokeDuration);

            // disable
            fallenAnimation.SetActive(false);

            // make it re-interactable
            fallenPlaying = false;

            if (OnAnimationFinished != null)
            {
                OnAnimationFinished.Invoke(gameObject, AnimationType.Fallen);
            }

            yield return null;
        }

        public void TriggerFallenAnimation()
        {
            StartCoroutine(OnFallen());
        }
    }
}