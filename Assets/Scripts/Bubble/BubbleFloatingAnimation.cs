using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace XRDC24.Bubble
{
    public class BubbleFloatingAnimation : MonoBehaviour
    {
        private Animator animator;
        // private List<GameObject> followees;

        public float speed = 0.6f;

        private Vector3 startPosition;

        private Dictionary<GameObject, Vector3> bubblePositions = new Dictionary<GameObject, Vector3>();

        public GameObject pos;
        public GameObject neg;

        private Random random;

        private void Start()
        {
            animator = GetComponent<Animator>();
            // backup speed
            speed = animator.speed;
            random = new Random();
        }

        IEnumerator StartAnimation()
        {
            animator.speed = speed;
            animator.Play("BubbleFloating", -1, 0);

            yield return new WaitForSeconds(0.1f);
            // backup position
            startPosition = transform.position;

            yield return null;
        }

        private void StopAnimation()
        {
            animator.speed = 0f;
        }

        public void AddBubble(GameObject bubble)
        {
            bubblePositions.Add(bubble, bubble.transform.position);
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Start Animation"))
            {
                StartCoroutine(StartAnimation());
            }

            if (GUILayout.Button("Add pos"))
            {
                AddBubble(pos);
            }

            if (GUILayout.Button("Add neg"))
            {
                AddBubble(neg);
            }

            GUILayout.EndVertical();
        }

        private void Update()
        {
            var pos = transform.position;
            foreach (var (k, v) in bubblePositions)
            {
                var offset = pos - startPosition;
                var r = random.NextDouble();
                offset.y = offset.y * (float)r;
                // add randomness
                k.transform.position = v + offset;
            }
        }
    }
}