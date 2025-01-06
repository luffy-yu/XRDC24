using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace XRDC24.Bubble
{
    public static class RandomExtensions
    {
        public static double NextDouble(
            this Random random,
            double minValue,
            double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
    }

    public class BubbleFloatingAnimation : MonoBehaviour
    {
        [Header("Floating Animation Settings")]
        public Animator animator;
        public float ratio = 0.6f;

        [Header("Fall Settings")]
        public bool isFalling = false;
        public float fallSpeed = 0.001f;
        public float minYPosition = 0.0f;
        private float fallingSpeedOffset = 0.01f;

        [Header("Flicker Settings")]
        public bool isFlickering = false;
        public float flickerSpeed;      // How fast the bubble flickers left/right
        public float flickerMagnitude; // How far the bubble moves left/right
        private float originalX;
        private float flickerSpeedOffset = 0.5f;
        private float flickerMagtitubeOffset = 0.05f;

        private Random random;
        public Transform bubble;
        private Vector3 currBubblePosition;

        private Vector3 startPosition;
        private bool startPositionSet;

        private bool following;


        private void Start()
        {
            // backup speed
            // speed = animator.speed;
            random = new Random();
            startPositionSet = false;
            following = false;

            // test
            StartFollowing();

            // flicker
            originalX = currBubblePosition.x;

            fallSpeed += UnityEngine.Random.Range(-fallingSpeedOffset, fallingSpeedOffset);
            flickerSpeed += UnityEngine.Random.Range(-flickerSpeedOffset, flickerSpeedOffset);
            flickerMagnitude += UnityEngine.Random.Range(-flickerMagtitubeOffset, flickerMagtitubeOffset);
        }

        void StartAnimation()
        {
            // animator.speed = speed;
            animator.Play("BubbleFloating", -1, (float)random.NextDouble(0, 1));
            currBubblePosition = bubble.position;
        }

        public void StartFollowing()
        {
            following = true;
            StartAnimation();
        }

        public void StopFollowing()
        {
            following = false;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Start Following"))
            {
                StartFollowing();
            }

            GUILayout.EndVertical();
        }

        private void Update()
        {
            if (!startPositionSet)
            {
                var time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (time > 0)
                {
                    // set start position
                    startPosition = animator.transform.position;
                    startPositionSet = true;
                }

                return;
            }

            if (following && startPositionSet)
            {
                var offset = animator.transform.position - startPosition;

                // set fall
                if (isFalling)
                {
                    currBubblePosition.y -= fallSpeed * Time.deltaTime;
                    if (currBubblePosition.y < minYPosition)
                    {
                        currBubblePosition.y = minYPosition;
                        animator.enabled = false;
                        isFalling = false;
                        isFlickering = false;
                    }
                }

                if (isFlickering)
                {
                    float offsetX = flickerMagnitude * Mathf.Sin(Time.time * flickerSpeed);

                    // Apply the offset to the bubble
                    currBubblePosition = new Vector3(
                        originalX + offsetX,
                        currBubblePosition.y,
                    currBubblePosition.z
                    );
                }

                bubble.position = currBubblePosition + offset * ratio;
            }
        }
    }
}