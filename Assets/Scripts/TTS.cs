using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using OpenAI;
using Samples.Whisper;
using UnityEngine.Networking;

namespace Demo
{
    public class TTS : MonoBehaviour
    {
        /// OpenAI API base path for requests.
        private const string BASE_PATH = "https://api.openai.com/v1";

        /// <summary>
        ///     Reads and sets user credentials from %User%/.openai/auth.json
        ///     Remember that your API key is a secret! Do not share it with others or expose it in any client-side code (browsers, apps).
        ///     Production requests must be routed through your own backend server where your API key can be securely loaded from an environment variable or key management service.
        /// </summary>
        private Configuration configuration;

        public AudioSource audioSource;

        public bool debug = false;

        public System.Action<AudioClip> OnResultAvailable;

        private Configuration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = new Configuration();
                }

                return configuration;
            }
        }

        /// Used for serializing and deserializing PascalCase request object fields into snake_case format for JSON. Ignores null fields when creating JSON strings.
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CustomNamingStrategy()
            },
            Culture = CultureInfo.InvariantCulture
        };

        private OpenAIApi openai = new OpenAIApi();

        /// <summary>
        ///     Create byte array payload from the given request object that contains the parameters.
        /// </summary>
        /// <param name="request">The request object that contains the parameters of the payload.</param>
        /// <typeparam name="T">type of the request object.</typeparam>
        /// <returns>Byte array payload.</returns>
        private byte[] CreatePayload<T>(T request)
        {
            var json = JsonConvert.SerializeObject(request, jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        ///     Dispatches an HTTP request for an audio file to the specified path with the specified method and optional payload.
        /// </summary>
        /// <param name="path">The path to send the request to.</param>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="payload">An optional byte array of json payload to include in the request.</param>
        /// <typeparam name="T">Response type of the request.</typeparam>
        /// <returns>A Task containing the response from the request as the specified type.</returns>
        private async Task<T> DispatchAudioRequest<T>(string path, string method, byte[] payload = null)
            where T : IAudioResponse
        {
            T data = default;

            using (var request = UnityWebRequest.Put(path, payload))
            {
                request.method = method;
                request.SetHeaders(Configuration, ContentType.ApplicationJson);

                var downloadHandlerAudioClip = new DownloadHandlerAudioClip(string.Empty, AudioType.MPEG);
                request.downloadHandler = downloadHandlerAudioClip;

                var asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone) await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (data != null) data.AudioClip = DownloadHandlerAudioClip.GetContent(request);
                }
                else
                {
                    if (data != null)
                        data.Error = new ApiError
                            { Code = request.responseCode, Message = request.error, Type = request.error };
                }
            }

            if (data?.Error != null)
            {
                ApiError error = data.Error;
                Debug.LogError($"Error Message: {error.Message}\nError Type: {error.Type}\n");
            }

            if (data?.Warning != null)
            {
                Debug.LogWarning(data.Warning);
            }

            return data;
        }

        /// <summary>
        ///     Returns speech audio for the provided text.
        /// </summary>
        /// <param name="request">See <see cref="CreateTextToSpeechRequest"/></param>
        /// <returns>See <see cref="CreateTextToSpeechResponse"/></returns>
        public async Task<CreateTextToSpeechResponse> CreateTextToSpeech(CreateTextToSpeechRequest request)
        {
            var path = $"{BASE_PATH}/audio/speech";
            var payload = CreatePayload(request);

            return await DispatchAudioRequest<CreateTextToSpeechResponse>(path, UnityWebRequest.kHttpVerbPOST, payload);
        }

        public async void SendRequest(string text)
        {
            var request = new CreateTextToSpeechRequest
            {
                Input = text,
                Model = "tts-1",
                Voice = "alloy"
            };

            var response = await CreateTextToSpeech(request);

            if (response.AudioClip)
            {
                // save

                var filename = "Audio.wav";
                var data = SaveWav.Save(filename, response.AudioClip);
                using (var fileStream = File.OpenWrite(filename))
                {
                    fileStream.Write(data);
                }

                if (OnResultAvailable != null)
                {
                    audioSource.clip = response.AudioClip;
                    OnResultAvailable(response.AudioClip);
                }
            }
        }

        void Test()
        {
            var text = "Today is a wonderful day to build something people love!";
            SendRequest(text);
        }

        private void OnGUI()
        {
            if (!debug) return;
            GUILayout.BeginVertical();

            if (GUILayout.Button("Test"))
            {
                Test();
            }

            GUILayout.EndVertical();
        }
    }
}