using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using XRDC24.AI;

namespace XRDC24.Helper
{
    [RequireComponent(typeof(TTS))]
    [RequireComponent(typeof(ChatGPT))]
    public class TTSFile : MonoBehaviour
    {
        private ChatGPT chatGPT;
        public bool conversationMode = false;
        private TTS tts;
        private string folder = "../FileSounds";
        public string filename = "";

        private int gap = 1; // sleep 1 seconds

        private Queue<string> sentences;

        private void Start()
        {
            chatGPT = GetComponent<ChatGPT>();
            tts = GetComponent<TTS>();
            folder = Path.Combine(Application.dataPath, folder);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            print($"Folder: {folder}");

            chatGPT.OnResultAvailable += OnResultAvailable;

            sentences = new Queue<string>();
        }

        private void OnResultAvailable(string result)
        {
            print($"Get ChatGPT response: {result}");
            // enquence
            sentences.Enqueue(result);
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("Run"))
            {
                Generate();
            }

            GUILayout.EndVertical();
        }

        public async void Generate()
        {
            sentences.Clear();
            var fullname = Path.Combine(folder, filename);
            if (!File.Exists(fullname))
            {
                Debug.LogError("File {fullname} does not exist.");
                return;
            }

            int number = 0;

            File.ReadLines(fullname).ToList().ForEach(item => sentences.Enqueue(item));

            // input count
            int inputCount = sentences.Count;

            while (sentences.Count > 0)
            {
                var sentence = sentences.Dequeue();
                var suffix = number < inputCount ? $"_input_{number + 1}" : $"_output_{number - inputCount + 1}";
                var filename = fullname.Replace(".txt", $"{suffix}.wav");
                print($"{filename}: {sentence}");
                var result = await tts.SendRequest(sentence, filename);
                if (result)
                {
                    print($"Save to {filename}");
                }
                else
                {
                    Debug.LogError($"Failed to TTS sentence: {sentence}.");
                }
                
                if (conversationMode && number < inputCount)
                {
                    Thread.Sleep(gap);
                    chatGPT.SendText(sentence);
                }
                
                number++;

                Thread.Sleep(gap);
            }

            print("Done");
        }
    }
}