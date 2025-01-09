using UnityEngine;

namespace XRDC24.GestureDriven
{
    public class GoodbyeController : MonoBehaviour
    {
        public void SayGoodbye()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}