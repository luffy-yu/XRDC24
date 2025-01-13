using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRDC24.Interaction
{
    [Serializable]
    public struct GuidedGesture
    {
        // name for identification
        public string name;

        // recorded animations
        public List<GameObject> demos;

        // call backs
        public List<HandGestureHandler> handlers;

        // wait time before enter to next state
        public int waitTime;
    }

    public class GuidedGestureController : MonoBehaviour
    {
        public FigmaFrameController figmaFrameController;

        public List<GuidedGesture> gestures;

        private int current = -1;


        private bool AtFirst => gestures != null && gestures.Count > 0 && current == 0;
        private bool AtLast => gestures != null && gestures.Count > 0 && current == gestures.Count - 1;

        public bool inDebug = false;

        private void OnEnable()
        {
            
        }

        private void Start()
        {
            ResetGestures();
        }

        public void EnableGuidance()
        {
            // reset gestures
            ResetGestures();

            // only enable when it's current frame
            if (figmaFrameController != null && figmaFrameController.CurrentFrame != null &&
                figmaFrameController.CurrentFrame == gameObject)
            {
                // enable first state
                current = 0;

                StartGuidance();
            }
        }

        void ResetGestures()
        {
            foreach (var gesture in gestures)
            {
                // disable demos
                gesture.demos.ForEach(item => item.SetActive(false));
                // disable handlers
                gesture.handlers.ForEach(h => { h.enabled = false; });
            }
        }

        void StartGuidance()
        {
            var gesture = gestures[current];
            print($"Start guidance: {gesture.name} waittime: {gesture.waitTime}");
            // set visible
            gesture.demos.ForEach(item => item.SetActive(true));
            // play animation automatically
            // enable handler
            gesture.handlers.ForEach(h => { h.enabled = true; });
        }

        void UpdateLoop()
        {
            if (current > 0)
            {
                // disable current
                gestures[current - 1].demos.ForEach(item => item.SetActive(false));
            }

            StartCoroutine(UpdateLoopImpl());
        }

        private void Update()
        {
            if (current < gestures.Count && current >= 0)
            {
                // must all triggered before entering next stage
                var satisfied = true;

                gestures[current].handlers.ForEach(item => { satisfied = satisfied && item.Triggered; });

                if (satisfied)
                {
                    // update
                    current += 1;
                    UpdateLoop();
                }
            }
        }

        IEnumerator UpdateLoopImpl()
        {
            if (current > 0)
            {
                // wait
                yield return new WaitForSeconds(gestures[current - 1].waitTime);
            }

            if (current >= gestures.Count)
            {
                // disable gestures
                ResetGestures();

                // enter next frame
                figmaFrameController.NextFrame();
                yield return null;
            }

            // update guidance
            StartGuidance();
        }

        #region Debug

        private void OnGUI()
        {
            if (!inDebug) return;

            GUILayout.BeginVertical();
            if (GUILayout.Button("Reset gestures"))
            {
                ResetGestures();
            }

            if (GUILayout.Button("First"))
            {
                FirstGuidance();
            }

            if (GUILayout.Button("Next"))
            {
                NextGuidance();
            }

            GUILayout.EndVertical();
        }

        void FirstGuidance()
        {
            current = 0;
            StartGuidance();
        }

        void NextGuidance()
        {
            UpdateLoop();
        }

        #endregion
    }
}