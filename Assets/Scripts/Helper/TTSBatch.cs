using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using XRDC24.AI;

namespace XRDC24.Helper
{
    [RequireComponent(typeof(TTS))]
    public class TTSBatch : MonoBehaviour
    {
        public List<GameObject> texts;

        private TTS tts;

        private int gap = 1;

        private string folder = "Sounds";

        private void Start()
        {
            tts = GetComponent<TTS>();
            folder = Path.Combine(Application.dataPath, folder);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            print($"Folder: {folder}");
        }

        public async void Generate()
        {
            foreach (var t in texts)
            {
                var name = t.name;
                var content = t.GetComponent<TextMeshProUGUI>().text;
                var sentences = content.Split(".");
                for (var i = 0; i < sentences.Length; i++)
                {
                    var sentence = sentences[i] + ".";
                    var filename = Path.Combine(folder, $"{name}_{i}.wav");
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
                    
                    Thread.Sleep(gap);
                }
            }

            print("Done");
        }

        #region Debug

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Generate"))
            {
                Generate();
            }

            GUILayout.EndVertical();
        }

        #endregion
    }
}