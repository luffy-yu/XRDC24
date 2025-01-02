using OpenAI;
using UnityEngine;

namespace XRDC24.AI
{
    public interface IAudioResponse: IResponse
    {
        public AudioClip AudioClip { get; set; }
    }
}