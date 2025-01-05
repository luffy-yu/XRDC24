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
        public Animator animator;
        public float ratio = 0.6f;

        private Random random;
        public Transform bubble;
        private Vector3 bubbleStartPosition;

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
        }

        void StartAnimation()
        {
            // animator.speed = speed;
            animator.Play("BubbleFloating", -1, (float)random.NextDouble(0, 1));
            bubbleStartPosition = bubble.position;
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
                bubble.position = bubbleStartPosition + offset * ratio;
            }
        }
    }
}