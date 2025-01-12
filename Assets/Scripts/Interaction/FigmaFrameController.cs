using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.VersionControl;
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

        public AudioSource audioSource;

        // frame name (first 4 characters) and audio clip source
        Dictionary<string, AudioClip> frameClips = new Dictionary<string, AudioClip>();

        private string lastName = null;

        private void Start()
        {
            currentFrame = -1;
            // init sounds
            InitSounds();
            // verify names
            Verify();
            // disable all 
            SetVisible();

            lastName = "";
        }

        void InitSounds()
        {
            foreach (var frame in frames)
            {
                var key = frame.name.Substring(0, 4);
                if(frameClips.ContainsKey(key)) continue;
                
                var clip = LoadSound(frame.name);
                frameClips.Add(key, clip);
            }
        }

        void Verify()
        {
            // sometimes some object is added more than once.
            var set = new HashSet<GameObject>();
            frames.ForEach(frame =>
            {
                if (set.Contains(frame))
                {
                    Debug.LogError($"{frame} is already added.");
                }

                set.Add(frame);
            });
        }

        public void ShowFirst()
        {
            currentFrame = 0;
            PlaceCenter();
            PlaySound();
        }

        public void PreviousFrame()
        {
            if (AtFirstFrame) return;

            currentFrame--;

            PlaceCenter();
            PlaySound();
        }

        public void NextFrame()
        {
            if (AtLastFrame) return;
            currentFrame++;
            PlaceCenter();
            PlaySound();
        }

        void SetVisible()
        {
            for (int i = 0; i < frames.Count; i++)
            {
                if (i == currentFrame)
                {
                    frames[i].SetActive(true);
                }
                else
                {
                    frames[i].SetActive(false);
                }
            }
        }

        string GetFrameName()
        {
            return frames[currentFrame].name.Substring(0, 4);
        }

        void PlaySound()
        {
            var name = GetFrameName();

            if (name != lastName)
            {
                // play
                audioSource.PlayOneShot(frameClips[name]);
                lastName = name;
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

        AudioClip LoadSound(string name)
        {
            //
            string path = $"Sounds/{name.Substring(0, 4)}_0";
            AudioClip clip = Resources.Load<AudioClip>(path);

            if (clip != null)
            {
                Debug.Log($"Loaded {path}");
            }
            else
            {
                Debug.LogError($"Failed to load {path}");
            }

            return clip;
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