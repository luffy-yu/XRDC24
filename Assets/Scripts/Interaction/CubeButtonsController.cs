using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRDC24.Interaction
{
    public class CubeButtonsController : MonoBehaviour
    {
        public List<TriggerForwarder> forwarders;

        private void Start()
        {
            // bind event
            foreach (var tf in forwarders)
            {
                tf.OnTriggerTriggered += OnTriggerTriggered;
            }
        }

        private void OnTriggerTriggered(TriggerType obj)
        {
            switch (obj)
            {
                case TriggerType.Forward:
                    OnForward();
                    break;
                case TriggerType.Backward:
                    OnBackward();
                    break;
                case TriggerType.Recording:
                    OnRecording();
                    break;
                default:
                    break;
            }
        }

        private void OnForward()
        {
            print("OnForward");
        }

        private void OnBackward()
        {
            print("OnBackward");
        }

        private void OnRecording()
        {
            print("OnRecording");
        }
    }
}