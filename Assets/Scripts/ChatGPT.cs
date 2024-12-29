using System;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;

namespace Demo
{
    public class ChatGPT : MonoBehaviour
    {
        public bool debug = false;

        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();

        private string prompt =
            "Act as a random stranger in a chat room and reply to the questions. Don't break character. Don't ever mention that you are an AI model.";

        public System.Action<string> OnResultAvailable;

        public async void SendText(string text)
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = text
            };

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + text;

            messages.Add(newMessage);

            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-4o-mini",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                messages.Add(message);

                print(message.Content);
                if (OnResultAvailable != null)
                {
                    OnResultAvailable(message.Content);
                }
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }

        void Hello()
        {
            SendText("Hello");
        }

        private void OnGUI()
        {
            if (!debug) return;
            GUILayout.BeginVertical();
            if (GUILayout.Button("Hello"))
            {
                Hello();
            }

            GUILayout.EndVertical();
        }
    }
}