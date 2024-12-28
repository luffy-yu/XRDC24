using System;
using OpenAI;
using Samples.Whisper;
using Unity.Collections;
using UnityEngine;

namespace Demo
{
    public class STT : MonoBehaviour
    {
        private AudioClip clip;

        private int duration = 10;
        private readonly string fileName = "audio.wav";

        private OpenAIApi openai = new OpenAIApi();

        private bool recording = false;

        public System.Action<string> OnResultAvailable;

        private void Start()
        {
            Debug.LogWarning($"[STT] Recording will use {Microphone.devices[0]}.");
        }

        public void StartRecording()
        {
            // var index = PlayerPrefs.GetInt("user-mic-device-index");

            clip = Microphone.Start(Microphone.devices[0], false, duration, 44100);
            recording = true;
        }

        public async void EndRecording()
        {
            print("Transcripting...");

            Microphone.End(null);

            byte[] data = SaveWav.Save(fileName, clip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = fileName },
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            print(res.Text);

            if (OnResultAvailable != null)
            {
                OnResultAvailable(res.Text);
            }

            recording = false;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label($"Device: {Microphone.devices[0]}");

            if (recording)
            {
                if (GUILayout.Button("End Recording"))
                {
                    EndRecording();
                }
            }
            else
            {
                if (GUILayout.Button("Start Recording"))
                {
                    StartRecording();
                }
            }


            GUILayout.EndVertical();
        }
    }
}