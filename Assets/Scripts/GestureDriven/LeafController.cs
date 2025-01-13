using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRDC24.GestureDriven
{
    public class LeafController : MonoBehaviour
    {
        public List<GameObject> leaves;

        public float gap = 1f; // gap before enable next leaf dropping animation

        public Dictionary<GameObject, bool> enabled = new Dictionary<GameObject, bool>();

        private bool running = false;

        private void Start()
        {
            // disable at the beginning
            foreach (var leaf in leaves)
            {
                leaf.SetActive(false);
            }
        }

        public void StartFalling()
        {
            // now new to enable
            if (enabled.Count == leaves.Count) return;
            
            if(running) return;

            StartCoroutine(Fall());
        }

        IEnumerator Fall()
        {
            running = true;
            foreach (var leaf in leaves)
            {
                if (enabled.ContainsKey(leaf)) continue;

                leaf.SetActive(true);
                enabled.Add(leaf, true);
                yield return new WaitForSeconds(gap);
                // fall all leaves
                // break;
            }

            running = false;

            yield return null;
        }
    }
}