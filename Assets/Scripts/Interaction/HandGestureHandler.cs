using System;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace XRDC24.Interaction
{
    [RequireComponent(typeof(ActiveStateUnityEventWrapper))]
    public class HandGestureHandler : MonoBehaviour
    {
        // public System.Action OnGestureActivated;


        private ActiveStateUnityEventWrapper _activeStateUnityEventWrapper;
        
        public UnityEvent whenGestureActivated;
        
        [HideInInspector] public bool Triggered = false;


        private void OnEnable()
        {
            Triggered = false;
        }

        void Start()
        {
            _activeStateUnityEventWrapper = GetComponent<ActiveStateUnityEventWrapper>();
            _activeStateUnityEventWrapper.WhenActivated.AddListener(GestureActivated);
        }

        public void GestureActivated()
        {
            print($"GestureActivated: {gameObject.name}");
            // if (GestureActivated != null)
            // {
            //     OnGestureActivated();
            // }
            Triggered = true;
            whenGestureActivated?.Invoke();
        }

        public void TriggerGesture()
        {
            GestureActivated();
        }

        void Update()
        {

        }
    }
}
