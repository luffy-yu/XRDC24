﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.VFX;
using XRDC24.Environment;

namespace XRDC24.Interaction
{
    public class FigmaFrameController : MonoBehaviour
    {
        public GameObject rootCanvas;

        public List<GameObject> frames;
        private int currentFrame = -1;
        public GameObject CurrentFrame => frames != null && currentFrame >= 0 ? frames[currentFrame] : null;

        private bool AtFirstFrame => frames != null && frames.Count > 0 && currentFrame == 0;
        private bool AtLastFrame => frames != null && frames.Count > 0 && currentFrame == frames.Count - 1;

        public AudioSource audioSource;

        // frame name (first 4 characters) and audio clip source
        Dictionary<string, AudioClip> frameClips = new Dictionary<string, AudioClip>();

        private string lastName = null;

        private void Start()
        {
            currentFrame = -1; //24
            // bind breathing event
            BindBreathingTimer();
            // init sounds
            InitSounds();
            // verify names
            Verify();
            // disable all 
            SetVisible();

            lastName = "";
        }

        void BindBreathingTimer()
        {
            frames.ForEach(item =>
            {
                // bind breathing animation
                AgentStageBreathing asb;
                if (item.TryGetComponent<AgentStageBreathing>(out asb))
                {
                    asb.TimeOut += OnTimeOut;
                }
            });
        }

        void InitSounds()
        {
            foreach (var frame in frames)
            {
                var key = frame.name.Substring(0, 4);
                if (frameClips.ContainsKey(key)) continue;

                var clip = LoadSound(frame.name);
                frameClips.Add(key, clip);
            }
        }

        private void OnTimeOut()
        {
            NextFrame();
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

        #region Speaking Animation

        public GameObject m_AIAvatar;
        public VisualEffect m_AIAvatarVFX;

        private Vector3 originalScale = Vector3.one * 0.5f;
        private float originalRate = 0.5f;
        private float originalAnimatedSpeed = 1f;
        private float scaleMultiplier = 1000f;
        private float amplitudeSmoothTime = 0.1f;

        private float currentAmplitude;
        private float amplitudeVelocity;
        private float[] spectrumData = new float[256];

        private void Update()
        {
            UpdateAIAvatarScale();
            UpdateAIAvatarParticleRate();
        }

        private void UpdateAIAvatarScale()
        {
            if (!audioSource.isPlaying)
            {
                m_AIAvatar.transform.localScale = originalScale;
                return;
            }

            // get the current spectrum data
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

            float sum = 0f;
            for (int i = 0; i < spectrumData.Length; i++)
            {
                sum += spectrumData[i];
            }

            // compute an amplitube
            float targetAmplitude = Mathf.Clamp01(sum / spectrumData.Length);
            targetAmplitude *= scaleMultiplier;
            //Debug.Log(targetAmplitude);

            currentAmplitude = Mathf.SmoothDamp(
                currentAmplitude,
                targetAmplitude * 0.1f,
                ref amplitudeVelocity,
                amplitudeSmoothTime
            );

            m_AIAvatar.transform.localScale = originalScale * (1f + currentAmplitude);
        }

        private void UpdateAIAvatarParticleRate()
        {
            if (!audioSource.isPlaying)
            {
                m_AIAvatarVFX.playRate = originalRate;
                m_AIAvatar.GetComponent<Animator>().speed = originalAnimatedSpeed;
                return;
            }

            // get the current spectrum data
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

            float sum = 0f;
            for (int i = 0; i < spectrumData.Length; i++)
            {
                sum += spectrumData[i];
            }

            // compute an amplitube
            float targetAmplitude = Mathf.Clamp01(sum / spectrumData.Length);
            targetAmplitude *= scaleMultiplier;
            //Debug.Log(targetAmplitude);

            currentAmplitude = Mathf.SmoothDamp(
                currentAmplitude,
                targetAmplitude,
                ref amplitudeVelocity,
                amplitudeSmoothTime
            );

            m_AIAvatarVFX.playRate = originalRate * (1f + currentAmplitude * 10);
            m_AIAvatar.GetComponent<Animator>().speed = originalAnimatedSpeed * (1f + currentAmplitude);
        }


        #endregion

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