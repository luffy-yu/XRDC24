using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRDC24.Interaction
{
    public class FigmaFrameController : MonoBehaviour
    {
        public GameObject rootCanvas;

        public List<GameObject> frames;
        private int currentFrame = -1;

        private bool AtFirstFrame => frames != null && frames.Count > 0 && currentFrame == 0;
        private bool AtLastFrame => frames != null && frames.Count > 0 && currentFrame == frames.Count - 1;

        private void Start()
        {
            currentFrame = -1;
            // disable all 
            SetVisible();
        }

        public void ShowFirst()
        {
            currentFrame = 0;
            PlaceCenter();
        }

        public void PreviousFrame()
        {
            if (AtFirstFrame) return;

            currentFrame--;

            PlaceCenter();
        }

        public void NextFrame()
        {
            if (AtLastFrame) return;
            currentFrame++;
            PlaceCenter();
        }

        void SetVisible()
        {
            for (int i = 0; i < frames.Count; i++)
            {
                if (i == currentFrame)
                {
                    frames[i].gameObject.SetActive(true);
                }
                else
                {
                    frames[i].gameObject.SetActive(false);
                }
            }
        }

        void PlaceCenter()
        {
            // set visibility
            SetVisible();
            
            // place children
            var childCanvas = frames[currentFrame].GetComponent<RectTransform>();
            var parentCanvas = rootCanvas.GetComponent<RectTransform>();

            if (childCanvas.parent == null)
            {
                childCanvas.SetParent(parentCanvas);
            }

            childCanvas.localPosition = Vector3.zero;
            childCanvas.localRotation = Quaternion.identity;
            childCanvas.localScale = Vector3.one;

            childCanvas.anchoredPosition = Vector2.zero;
            // childCanvas.sizeDelta = parentCanvas.sizeDelta;
            childCanvas.pivot = new Vector2(0.5f, 0.5f);
        }

        #region Debug

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Show First"))
            {
                ShowFirst();
            }

            if (GUILayout.Button("Previous Frame"))
            {
                PreviousFrame();
            }

            if (GUILayout.Button("Next Frame"))
            {
                NextFrame();
            }

            GUILayout.EndVertical();
        }

        #endregion
    }
}