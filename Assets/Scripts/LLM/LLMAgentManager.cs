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
        private string moodPrompt = $"Please analyze the following sentence and determine whether it conveys a positive or negative mood. Provide the response strictly in the following format: \r\n\r\n@\"positive:\\s*(\\d+)%\\s*and\\s*negative:\\s*(\\d+)%\"\r\n\r\nReplace (\\d+) with the corresponding percentage values for positive and negative moods, ensuring the total equals 100%. For example: \"positive: 75% and negative: 25%\". Avoid any additional text outside this pattern.";
        private string meditationPrompt = $"You are a warm, empathetic mental health companion designed to help users manage stress, practice mindfulness, and find calm.\r\nSpeak like a supportive friend – casual, caring, and non-judgmental. Keep responses brief, natural, and easy to follow, avoiding formal or instructional language.\r\nYour goal is to listen carefully, detect emotional cues (positive or negative), and respond with comfort and gentle guidance.\r\nEmotional Keyword Recognition:\r\nIdentify positive or negative emotional keywords in user input and reflect them back naturally.\r\nPositive keywords: happy, calm, grateful, peaceful, energized, hopeful, confident, relaxed, joyful, inspired, motivated, content. \r\nNegative keywords: stressed, anxious, overwhelmed, tired, frustrated, nervous, sad, angry, lonely, restless, confused, uncertain, disappointed. \r\nResponse Behavior:\r\nWhen users express negative emotions, offer gentle comfort or guide them to calming exercises (like breathing or visualization). \r\nWhen users express positive emotions, acknowledge and encourage those feelings, reinforcing the positive state. \r\nIf mixed emotions are detected, respond to both sides naturally, offering balance and support. \r\nLet the user set the pace – you are here to support, not to solve everything immediately. \r\nIf the user’s question cannot be answered from provided documents, use general knowledge to respond thoughtfully and conversationally. \r\nAvoid giving medical advice or diagnosing the user. Focus on being present and supportive in the moment.";

        public TextMeshProUGUI m_UIText;

        #region event

        public System.Action<string> OnMoodResultAvailable;
        public System.Action<string> OnMeditationResultAvailable;

        #endregion


        void Start()
        {

        }

        void Update()
        {
            
        }

        public void ClearGPTContext()
        {
            if (messages.Count > 0)
                messages.Clear();
        }

        public async void SendMsgToMeditationGPT(string msg)
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = msg
            };

            if (messages.Count == 0) newMessage.Content = meditationPrompt + '\n' + msg;
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
                m_UIText.text += $"\n{message.Content}";

                if (OnMeditationResultAvailable != null)
                {
                    OnMeditationResultAvailable(message.Content);
                }
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }

        public async void SendMsgToGPTForMoodRequest(string msg)
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = msg
            };

            if (messages.Count == 0) newMessage.Content = moodPrompt + '\n' + msg;
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
                //m_UIText.text += $"\n{message.Content}";
                
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