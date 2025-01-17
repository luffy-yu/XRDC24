﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
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

        Dictionary<int, Vector2> frameSizeDelta = new Dictionary<int, Vector2>();

        private string lastName = null;

        [HideInInspector] public bool inDebugMode = true;

        public System.Action OnFinalFrame;

        // whether switching is available
        private bool switchingAvailable = true;

        private void Start()
        {
            currentFrame = -1; //24

            // change font size
            ReformatFont();

            // backup framesize delta
            BackupFrameSizeDelta();

            // bind breathing event
            BindBreathingTimer();
            // init sounds
            InitSounds();
            // verify names
            Verify();
            // disable all 
            SetVisible();

            lastName = "";

            // disable agent animator at the start
            m_AIAvatarVFX.GetComponent<Animator>().enabled = false;

            originalScale = m_AIAvatar.transform.localScale;
        }

        void ReformatFont()
        {
            // Dialog_Agent
            int size = 26;
            foreach (var frame in frames)
            {
                var count = frame.transform.childCount;
                for (var i = 0; i < count; i++)
                {
                    var child = frame.transform.GetChild(i).gameObject;
                    if (child.name.Equals("Dialog_Agent"))
                    {
                        // font
                        var tmp = child.GetComponentInChildren<TextMeshProUGUI>();
                        tmp.enableAutoSizing = false;
                        tmp.fontSize = size;
                        tmp.alignment = TextAlignmentOptions.Center;

                        break;
                    }
                }
            }
        }

        void BackupFrameSizeDelta()
        {
            var count = frames.Count;

            for (int i = 0; i < count; i++)
            {
                var size = frames[i].GetComponent<RectTransform>().sizeDelta;
                frameSizeDelta.Add(i, size);
            }
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
                if (clip != null)
                {
                    frameClips.Add(key, clip);   
                }
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


        void TakeActions()
        {
            PlaceCenter();
            // pop up window
            var pec = GetPopupWindow();
            if (pec != null)
            {
                pec.ShowPopupWindow();
                // wait for seconds to play sound
                StartCoroutine(PlaySound(pec));
            }
            else
            {
                PlaySound();
            }

            Breathe();
            ShowGuidance();
            SetAutoJumper();
        }

        void SetAutoJumper()
        {
            FrameAutoJumper ftj;
            if (CurrentFrame != null && CurrentFrame.TryGetComponent<FrameAutoJumper>(out ftj))
            {
                ftj.Monitor();
            }
        }

        public void ShowFirst()
        {
            currentFrame = 0;
            TakeActions();
        }

        void Breathe()
        {
            AgentStageBreathing asb;
            if (CurrentFrame != null && CurrentFrame.TryGetComponent<AgentStageBreathing>(out asb))
            {
                asb.StartBreathing();
            }
            else
            {
                var animator = m_AIAvatarVFX.gameObject.GetComponent<Animator>();
                animator.enabled = false;
            }
        }

        void ShowGuidance()
        {
            GuidedGestureController ggc;
            if (CurrentFrame != null && CurrentFrame.TryGetComponent<GuidedGestureController>(out ggc))
            {
                ggc.EnableGuidance();
            }
        }

        GuidedGestureController GetGuidedGestureController()
        {
            GuidedGestureController ggc;
            if (CurrentFrame != null && CurrentFrame.TryGetComponent<GuidedGestureController>(out ggc))
            {
                return ggc;
            }

            return null;
        }

        void PopupWindow()
        {
            PopupEffectController pec;
            if (CurrentFrame != null && CurrentFrame.TryGetComponent<PopupEffectController>(out pec))
            {
                pec.ShowPopupWindow();
            }
        }

        PopupEffectController GetPopupWindow()
        {
            PopupEffectController pec;
            if (CurrentFrame != null && CurrentFrame.TryGetComponent<PopupEffectController>(out pec))
            {
                return pec;
            }

            return null;
        }

        public void PreviousFrame()
        {
            if (!switchingAvailable) return;

            if (AtFirstFrame) return;

            currentFrame--;

            TakeActions();
        }

        public void NextFrame()
        {
            if (!switchingAvailable) return;

            if (AtLastFrame)
            {
                OnFinalFrame?.Invoke();
                return;
            }

            var ggc = GetGuidedGestureController();
            if (ggc != null && ggc.AutoLoop && !ggc.AutoLoopDone)
            {
                // return if it's in auto loop mode and the auto loop is not done.
                return;
            }

            currentFrame++;

            TakeActions();
        }

        public void ResetFrame()
        {
            currentFrame = -1;
            lastName = string.Empty;
            SetVisible();
        }

        void SetVisible()
        {
            for (int i = 0; i < frames.Count; i++)
            {
                if (i == currentFrame)
                {
                    // frames[i].SetActive(true);
                    frames[i].GetComponent<RectTransform>().sizeDelta = frameSizeDelta[i];
                }
                else
                {
                    // frames[i].SetActive(false);
                    frames[i].GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                }
            }
        }

        string GetFrameName()
        {
            return frames[currentFrame].name.Substring(0, 4);
        }

        IEnumerator PlaySound(PopupEffectController pec)
        {
            yield return new WaitForSecondsRealtime(pec.TimeBeforeSound);
            PlaySound();
        }

        void PlaySound()
        {
            var name = GetFrameName();

            print($"Enter frame: {name}");

            if (name != lastName && frameClips.ContainsKey(name))
            {
                // disable action
                switchingAvailable = false;
                // play
                audioSource.PlayOneShot(frameClips[name]);
                lastName = name;
                var seconds = frameClips[name].length;
                StartCoroutine(WaitSoundOver(seconds));
            }
        }

        IEnumerator WaitSoundOver(float sec)
        {
            yield return new WaitForSeconds(sec);
            switchingAvailable = true;
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

            // Canvas.ForceUpdateCanvases();
            childCanvas.hasChanged = true;
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
                Debug.LogWarning($"Failed to load {path}");
            }

            return clip;
        }

        #region Speaking Animation

        public GameObject m_AIAvatar;
        public VisualEffect m_AIAvatarVFX;

        private Vector3 originalScale; // = Vector3.one * 0.35416f;

        [Header("Breathing Animation Parameters")]
        public float originalRate = 0.5f;

        public float originalAnimatedSpeed = 1f;
        public float scaleMultiplier = 100f;
        public float amplitudeSmoothTime = 0.1f;

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
            if (!inDebugMode) return;

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

            if (GUILayout.Button("Reset"))
            {
                ResetFrame();
            }

            GUILayout.EndVertical();
        }

        #endregion
    }
}