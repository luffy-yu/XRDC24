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

    public class BubbleController : MonoBehaviour
    {
        private Rigidbody rbRigidbody;

        public GameObject pokedAnimation;
        public float pokeDuration = 1.0f;
        public GameObject fallenAnimation;
        public float fallenDuration = 1.0f;

        private bool pokedPlaying = false;
        private bool fallenPlaying = false;

        #region event

        public System.Action<GameObject, AnimationType> OnAnimationFinished;
        public System.Action<GameObject, AnimationType> OnBubbleAnimated;

        #endregion

        private MeshRenderer meshRenderer;

        [Header("Settings")] [Tooltip("Gap before showing animation")]
        public float gapSeconds = 1.0f;

        private void Start()
        {
            pokedPlaying = false;
            fallenPlaying = false;

            rbRigidbody = gameObject.GetComponent<Rigidbody>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        // Archived
        public void LetFall()
        {
            // make bubble drop
            rbRigidbody.useGravity = true;
            rbRigidbody.isKinematic = false;

            // disable floating controller
            GetComponent<BubbleFloating>().enabled = false;
        }

        // Archived
        private void OnHit(bool touched)
        {
            if (!touched) return;

            if (!pokedPlaying)
            {
                StartCoroutine(OnPoked());
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // print($"On Trigger Enter: {other.gameObject.name}");

            // Add SphereCollider to XRHand_IndexTip of the Synthetic hand
            // set Radius 0.01 and check Is Trigger

            var name = other.gameObject.name;
            if (name.Equals("XRHand_IndexTip")) // name for poke interaction
            {
                OnBubbleAnimated.Invoke(gameObject, AnimationType.Poke);
                TriggerPokedAnimation();
            }
            else if (name.Contains("FLOOR"))
            {
                OnBubbleAnimated.Invoke(gameObject, AnimationType.Fallen);
                TriggerFallenAnimation();
            }
        }

        IEnumerator OnPoked()
        {
            pokedPlaying = true;
            fallenPlaying = false;

            meshRenderer.enabled = false;
            if (meshRenderer.transform.Find("DarkCenter"))
                meshRenderer.transform.Find("DarkCenter").GetComponent<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(gapSeconds);

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
            
            // make self invisible
            gameObject.SetActive(false);

            yield return null;
        }

        IEnumerator OnFallen()
        {
            fallenPlaying = true;
            pokedPlaying = false;

            meshRenderer.enabled = false;
            if (meshRenderer.transform.Find("DarkCenter"))
                meshRenderer.transform.Find("DarkCenter").GetComponent<MeshRenderer>().enabled = false;

            yield return new WaitForSeconds(gapSeconds);

            // enable
            fallenAnimation.SetActive(true);

            // wait for seconds
            yield return new WaitForSeconds(fallenDuration);

            // disable
            fallenAnimation.SetActive(false);

            // make it re-interactable
            fallenPlaying = false;

            if (OnAnimationFinished != null)
            {
                OnAnimationFinished.Invoke(gameObject, AnimationType.Fallen);
            }
            
            // make self invisible
            gameObject.SetActive(false);

            yield return null;
        }

        public void TriggerFallenAnimation()
        {
            StartCoroutine(OnFallen());
        }

        public void TriggerPokedAnimation()
        {
            StartCoroutine(OnPoked());
        }
    }
}