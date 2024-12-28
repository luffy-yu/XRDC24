using OpenAI;
using UnityEngine;

namespace Demo
{
    public interface IAudioResponse: IResponse
    {
        public AudioClip AudioClip { get; set; }
    }
}