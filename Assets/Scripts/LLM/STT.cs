using System;
using System.Collections;
using OpenAI;
using Samples.Whisper;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace XRDC24.AI
{
    public class STT : MonoBehaviour
    {
        private OpenAIApi openai = new OpenAIApi();
        private AudioClip clip;
        private int duration = Constants.SPEAK_MAX_DURATION;
        private readonly string fileName = "audio.wav";
        private string microphone;
        private bool recording = false;
        private float countdown;

        #region event
        
        public System.Action<string> OnResultAvailable;

        #endregion

        public TextMeshPro m_StartButtonText;
        public TextMeshProUGUI m_UIText;
        public GameObject m_ButtonStart;
        public GameObject m_ButtonEnd;

        private void Start()
        {
            // init microphone
            if (Microphone.devices.Length > 0)
            {
                Debug.Log($"[STT] Recording will use {Microphone.devices[0]}.");
                microphone = Microphone.devices[0];
                clip = Microphone.Start(microphone, true, duration, 44100);
                while (Microphone.GetPosition(microphone) <= 0) { } 
                Microphone.End(microphone);
                Debug.Log("Microphone initialized");
            }
            else
            {
                Debug.LogError("No microphone detected!");
            }

            countdown = 0;
            m_ButtonEnd.SetActive(false);

        }

        private void Update()
        {
            SetTimer();
        }

        private void SetTimer()
        {
            if (!recording)
                return;
            
            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                if (countdown <= 0)
                {
                    countdown = 0;
                    EndRecording();
                    m_ButtonEnd.SetActive(false);
                }
            }
        }

        public void StartRecording()
        {
            // var index = PlayerPrefs.GetInt("user-mic-device-index");
            
            if (!recording && clip != null)
            {
                clip = Microphone.Start(microphone, false, duration, 44100);
                recording = true;
                m_UIText.text = $"Recording...\nPress end button when you finished";
                m_ButtonEnd.SetActive(true);
                Debug.Log("Recording started...");
            }
        }

        public async void EndRecording()
        {
            if (recording)
            {
                Debug.Log("End Recording...");
                m_StartButtonText.text = "Processing...";

                Microphone.End(microphone);

                byte[] data = SaveWav.Save(fileName, clip);

                var req = new CreateAudioTranscriptionsRequest
                {
                    FileData = new FileData() { Data = data, Name = fileName },
                    Model = "whisper-1",
                    Language = "en"
                };
                var res = await openai.CreateAudioTranscription(req);

                Debug.Log(res.Text);
                m_UIText.text = res.Text;
                m_StartButtonText.text = "Record Again";

                if (OnResultAvailable != null)
                {
                    OnResultAvailable(res.Text);
                }

                recording = false;
            }
        }

        public int GetVideoClipLength()
        {
            if (clip.length >= 0)
                return (int)clip.length;
            return 0;
        }

        //private void OnGUI()
        //{
        //    if (!debug) return;
        //    GUILayout.BeginVertical();
        //    GUILayout.Label($"Device: {Microphone.devices[0]}");

        //    if (recording)
        //    {
        //        if (GUILayout.Button("End Recording"))
        //        {
        //            EndRecording();
        //        }
        //    }
        //    else
        //    {
        //        if (GUILayout.Button("Start Recording"))
        //        {
        //            StartRecording();
        //        }
        //    }


        //    GUILayout.EndVertical();
        //}
    }
}