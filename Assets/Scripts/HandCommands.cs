using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace Demo
{
    public enum Stage
    {
        None,
        Recording,
        STT,
        ChatGPT,
        TTS,
        Speaking,
    }

    public class HandCommands : MonoBehaviour
    {
        [Header("AIs")]
        public STT stt;
        public ChatGPT chatGPT;
        public TTS tts;

        private Stage stage = Stage.None;
        
        private AudioClip audioClip;
        
        [Space(30)]
        [Header("Text")]
        public TextMeshProUGUI recordingText;
        public TextMeshProUGUI sttText;
        public TextMeshProUGUI chatGPTText;
        public TextMeshProUGUI ttsText;
        public TextMeshProUGUI curText;
        public TextMeshProUGUI nextText;
        
        private void Start()
        {
            stage = Stage.None;
            UpdateText();
            tts.OnResultAvailable += OnTTSResultAvailable;
            chatGPT.OnResultAvailable += OnChatGPTResultAvailable;
            stt.OnResultAvailable += OnSTTResultAvailable;
        }
        
        private void OnSTTResultAvailable(string obj)
        {
            sttText.text = obj;
        }

        private void OnChatGPTResultAvailable(string obj)
        {
            chatGPTText.text = obj;
        }
        
        private void OnTTSResultAvailable(AudioClip obj)
        {
            ttsText.text = "Done";
            audioClip = obj;
        }

        public void OnGesturePerformed()
        {
            print("Gesture Performed");
        }

        public void OnGestureEnded()
        {
            print("Gesture Ended");
            if (stage < Stage.Speaking)
            {
                stage = stage + 1;  
            }
            else
            {
                stage = Stage.None;
            }
            UpdateText();
            StartCoroutine(ProceedAction());
        }

        IEnumerator ProceedAction()
        {
            switch (stage)
            {
                case Stage.Recording:
                    // start recording
                    stt.StartRecording();
                    break;
                case Stage.STT:
                    stt.EndRecording();
                    break;
                case Stage.ChatGPT:
                    chatGPT.SendText(sttText.text);
                    break;
                case Stage.TTS:
                    tts.SendRequest(chatGPTText.text);
                    break;
                case Stage.Speaking:
                    tts.audioSource.Play();
                    break;
                default:
                    break;
            }

            yield return null;
        }

        void UpdateText()
        {
            switch (stage)
            {
                case Stage.Recording:
                    recordingText.text = "True";
                    break;
                case Stage.STT:
                    recordingText.text = "False";
                    break;
                case Stage.ChatGPT:
                    break;
                case Stage.TTS:
                    break;
                case Stage.Speaking:
                    break;
            }

            curText.text = stage.ToString();
            if (stage != Stage.Speaking)
            {
                nextText.text = (stage + 1).ToString();
            }
            else
            {
                nextText.text = Stage.None.ToString();
            }
        }
    }
}