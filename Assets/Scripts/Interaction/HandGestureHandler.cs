using Oculus.Interaction;
using UnityEngine;

namespace XRDC24.Interaction
{
    [RequireComponent(typeof(ActiveStateUnityEventWrapper))]
    public class HandGestureHandler : MonoBehaviour
    {
        public System.Action OnGestureActivated;


        private ActiveStateUnityEventWrapper _activeStateUnityEventWrapper;


        void Start()
        {
            _activeStateUnityEventWrapper = GetComponent<ActiveStateUnityEventWrapper>();
            _activeStateUnityEventWrapper.WhenActivated.AddListener(GestureActivated);
        }

        public void GestureActivated()
        {
            print($"GestureActivated: {gameObject.name}");
            if (OnGestureActivated != null)
            {
                OnGestureActivated();
            }
        }

        void Update()
        {

        }
    }
}
