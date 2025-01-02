using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SwitchToURP : MonoBehaviour
{
    public UniversalRenderPipelineAsset urpAsset; // 在 Inspector 中赋值

    void Start()
    {
        if (urpAsset != null)
        {
            GraphicsSettings.defaultRenderPipeline = urpAsset;
            Debug.Log("Switched to URP!");
        }
        else
        {
            Debug.LogError("URP Asset is not assigned!");
        }
    }
}