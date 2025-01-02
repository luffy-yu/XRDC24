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
        private bool recording = false;
        private int countdown;

        #region event
        
        public System.Action<string> OnResultAvailable;

        #endregion

        public TextMeshPro m_ButtonText;
        public TextMeshProUGUI m_UIText;

        private void Start()
        {
            Debug.Log($"[STT] Recording will use {Microphone.devices[0]}.");
            countdown = duration;
        }

        private void Update()
        {
            
            
        }

        public void StartRecording()
        {
            // var index = PlayerPrefs.GetInt("user-mic-device-index");

            clip = Microphone.Start(Microphone.devices[0], false, duration, 44100);
            recording = true;
            Debug.Log("Recording started...");

            StartCoroutine(EndRecordingAfterDuration());
        }

        private IEnumerator EndRecordingAfterDuration()
        {
            while (countdown > 0)
            {
                m_ButtonText.text = $"{countdown}";
                yield return new WaitForSeconds(1f);
                countdown--;
            }

            m_ButtonText.text = "Transcripting...";
            EndRecording();
        }

        public async void EndRecording()
        {
            Debug.Log("End Recording...");

            Microphone.End(null);

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
            m_ButtonText.text = "Start Recording";

            if (OnResultAvailable != null)
            {
                OnResultAvailable(res.Text);
            }

            recording = false;
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