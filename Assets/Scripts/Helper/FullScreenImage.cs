using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace XRDC24.Helper
{
    public class FullScreenImage : MonoBehaviour
    {
        public Canvas targetCanvas;

        [Header("Image")] public Image startImage;
        public Image endImage;

        [Header("Video")] public VideoPlayer videoPlayer;
        public VideoClip startVideo;
        public VideoClip endVideo;

        public System.Action OnStartVideoEnded;
        public System.Action OnEndVideoEnded;

        public void ShowStart()
        {
            targetCanvas.enabled = true;

            videoPlayer.enabled = true;
            videoPlayer.clip = startVideo;
            videoPlayer.Play();
        }

        public void ShowEnd()
        {
            targetCanvas.enabled = true;

            videoPlayer.enabled = true;
            videoPlayer.clip = endVideo;
            videoPlayer.Play();
        }

        public void DisableSplash()
        {
            targetCanvas.enabled = false;
            videoPlayer.enabled = false;
        }

        void Start()
        {
            videoPlayer.loopPointReached += VideoPlayerOnloopPointReached;
            // disable images
            // startImage.gameObject.SetActive(false);
            // endImage.gameObject.SetActive(false);

            ShowStart();
        }

        private void VideoPlayerOnloopPointReached(VideoPlayer source)
        {
            if (videoPlayer.clip == startVideo)
            {
                OnStartVideoEnded?.Invoke();
            }
            else if (videoPlayer.clip == endVideo)
            {
                OnEndVideoEnded?.Invoke();
            }
        }

        void Update()
        {
            // if (Input.GetKeyUp(KeyCode.S))
            // {
            //     ShowStart();
            // }
            //
            // if (Input.GetKeyUp(KeyCode.E))
            // {
            //     ShowEnd();
            // }
            //
            // if (Input.GetKeyUp(KeyCode.D))
            // {
            //     DisableSplash();
            // }
        }
    }
}