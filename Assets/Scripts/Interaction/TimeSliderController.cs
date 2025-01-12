using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRDC24.Interaction
{
    [Serializable]
    public struct TimeObject
    {
        public int time;
        public GameObject slider;
    }

    public class TimeSliderController : MonoBehaviour
    {
        public GameObject rootCanvas;
        public List<TimeObject> timeObjects;

        private int current = -1;

        private bool AtFirstFrame => timeObjects != null && timeObjects.Count > 0 && current == 0;
        private bool AtLastFrame => timeObjects != null && timeObjects.Count > 0 && current == timeObjects.Count - 1;

        private void Start()
        {
            // BindEvent();
        }

        void BindEvent()
        {
        }

        void UpdateUI()
        {
            // switch
            // center
            PlaceCenter();
        }

        void PlaceCenter()
        {
            // place children
            var childCanvas = timeObjects[current].slider.GetComponent<RectTransform>();
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

        void Next()
        {
            if (AtLastFrame) return;
            current++;
            UpdateUI();
        }

        void Previous()
        {
            if (AtFirstFrame) return;
            current--;
            UpdateUI();
        }
    }
}