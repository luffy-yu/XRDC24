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

        private void Awake()
        {
            // reset gestures
            ResetGestures();

            // enable first state
            current = 0;

            StartGuidance();
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
            gesture.handlers.ForEach(h =>
            {
                h.enabled = true;
                h.whenGestureActivated.AddListener(UpdateLoop);
            });
        }

        void UpdateLoop()
        {
            // must all triggered before entering next stage
            var satisfied = true;
            
            gestures[current].handlers.ForEach(item => satisfied = satisfied && item.Triggered);
            
            if(!satisfied) return;
            
            if (AtLast)
            {
                // enter next frame
                figmaFrameController.NextFrame();
                return;
            }

            // disable current
            gestures[current].demos.ForEach(item => item.SetActive(false));

            StartCoroutine(UpdateLoopImpl());
        }

        IEnumerator UpdateLoopImpl()
        {
            // wait
            var time = gestures[current].waitTime;
            yield return new WaitForSeconds(time);
            // update
            current += 1;
            // update guidance
            StartGuidance();
        }

        #region Debug

        private void OnGUI()
        {
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