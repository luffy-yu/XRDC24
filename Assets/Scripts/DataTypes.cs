using OpenAI;
using UnityEngine;

namespace Demo
{
    #region Text To Speech Data Types

    public class CreateTextToSpeechRequestBase
    {
        public string Input { get; set; }
        public string Voice { get; set; }
        public string Model { get; set; }
    }
    
    public class CreateTextToSpeechRequest: CreateTextToSpeechRequestBase { }
    
    public struct CreateTextToSpeechResponse: IAudioResponse
    {
        public ApiError Error { get; set; }
        public string Warning { get; set; }
        public AudioClip AudioClip { get; set; }
    }
    
    #endregion
}