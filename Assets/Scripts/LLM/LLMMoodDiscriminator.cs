using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using OpenAI;
using TMPro;

namespace XRDC24.AI
{
    public class LLMMoodDiscriminator : MonoBehaviour
    {
        [SerializeField] private STT stt;
        // [SerializeField] private BubbleGenerator bubbleGenerator;

        private OpenAIApi openai = new OpenAIApi();
        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = $"Please determine whether the below sentence should be in positve or negative mood. The reply should contain 'positive' or 'negative'\n";

        public TextMeshProUGUI m_UIText;

        private void OnEnable()
        {
            stt.OnResultAvailable += SendMsgToGPT;
        }

        private void OnDisable()
        {
            stt.OnResultAvailable -= SendMsgToGPT;

        }

        void Start()
        {

        }

        void Update()
        {
            
        }

        private async void SendMsgToGPT(string msg)
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = msg
            };

            if (messages.Count == 0) newMessage.Content = prompt + msg;
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
                m_UIText.text = message.Content;
                // InitBubble();
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
    }
}