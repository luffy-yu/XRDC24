﻿using System;
using UnityEngine;

namespace XRDC24.Environment
{
    public class AgentStageBreathing : MonoBehaviour
    {
        public GameObject agent;
        public Transform target;
        public float duration = 60.0f; // in second
        public Animator animator;

        private float timerRemaining;

        private bool timerRunning = false;

        public System.Action TimeOut;
        
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        private void OnEnable()
        {
            
        }

        public void StartBreathing()
        {
            // BackupAgent();

            animator.enabled = true;

            // play it
            animator.Play("Agent", 0, 0.0f);
            // start timer
            StartTimer();
        }

        public void StopBreathing()
        {
            animator.enabled = false;
        }

        void BackupAgent()
        {
            var t = agent.transform;
            originalPosition = t.position;
            originalRotation = t.rotation;
            // move agent
            agent.transform.position = target.position;
            agent.transform.rotation = target.rotation;
        }

        private void Update()
        {
            if (timerRunning)
            {
                if (timerRemaining > 0)
                {
                    timerRemaining -= Time.deltaTime;
                }
                else
                {
                    // Timer finished
                    timerRemaining = 0;
                    timerRunning = false;
                    
                    // stop animator
                    StopBreathing();
                    
                    // // restore agent
                    // agent.transform.position = originalPosition;
                    // agent.transform.rotation = originalRotation;
                    
                    print("Time out");

                    TimeOut?.Invoke();
                }
            }
        }

        public void StartTimer()
        {
            print("StartTimer");
            timerRemaining = duration;
            timerRunning = true;
        }

        #region Debug

        private void OnGUI()
        {
            // GUILayout.BeginVertical();
            // if (GUILayout.Button("Start"))
            // {
            //     OnEnable();
            // }
            // GUILayout.EndVertical();
        }

        #endregion
    }
}