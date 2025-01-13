using System;
using UnityEngine;

namespace XRDC24.Interaction
{
    public class FrameAutoJumper : MonoBehaviour
    {
        private FigmaFrameController figmaFrameController;

        private AudioSource audioSource => figmaFrameController.audioSource;

        public float gap = 3.0f; // in seconds
        private float duration;

        [HideInInspector] public bool playing = false;
        private bool monitoring = false;

        private void Start()
        {
            figmaFrameController = transform.parent.gameObject.GetComponent<FigmaFrameController>();
        }

        bool IsSelf()
        {
            return figmaFrameController.CurrentFrame == gameObject;
        }

        public void Monitor()
        {
            playing = true;
            monitoring = true;
        }

        void Update()
        {
            if(!IsSelf()) return;
            
            if (monitoring)
            {
                playing = audioSource.isPlaying;
                if (!playing)
                {
                    // start timer
                    monitoring = false;
                    duration = gap;
                }
            }
            else
            {
                duration -= Time.deltaTime;

                if (duration <= 0)
                {
                    // auto jump
                    figmaFrameController.NextFrame();
                }
            }
        }
    }
}