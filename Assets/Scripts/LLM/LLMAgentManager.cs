using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using OpenAI;
using TMPro;

namespace XRDC24.AI
{
    public class LLMAgentManager : MonoBehaviour
    {
        private OpenAIApi openai = new OpenAIApi();
        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = $"Please analyze the following sentence and determine whether it conveys a positive or negative mood. Provide the response strictly in the following format: \r\n\r\n@\"positive:\\s*(\\d+)%\\s*and\\s*negative:\\s*(\\d+)%\"\r\n\r\nReplace (\\d+) with the corresponding percentage values for positive and negative moods, ensuring the total equals 100%. For example: \"positive: 75% and negative: 25%\". Avoid any additional text outside this pattern.";

        public TextMeshProUGUI m_UIText;

        #region event

        public System.Action<string> OnMoodResultAvailable;

        #endregion


        void Start()
        {

        }

        void Update()
        {
            
        }

        public async void SendMsgToGPT(string msg)
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = msg
            };

            if (messages.Count == 0) newMessage.Content = prompt + '\n' + msg;
            messages.Add(newMessage);

            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-4o-mini",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                // Initialize balls in the scene based on the reply
                messages.Add(message);
                Debug.Log(message.Content);
                m_UIText.text += $"\n{message.Content}";
                
                if (OnMoodResultAvailable != null)
                {
                    OnMoodResultAvailable(message.Content);
                }
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
    }
}