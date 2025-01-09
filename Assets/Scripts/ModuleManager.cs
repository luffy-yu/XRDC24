using Meta.XR.MRUtilityKit;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using XRDC24.AI;
using System.Collections.Generic;
using XRDC24.Bubble;

public class ModuleManager : MonoBehaviour
{
    [SerializeField] OVRCameraRig m_OVRCameraRig;
    [SerializeField] BubblesManager m_BubbleManager;
    [SerializeField] LLMAgentManager m_LLMAgentManager;
    [SerializeField] STT m_SpeechToText;
    [SerializeField] TTS m_TextToSpeech;
    [SerializeField] MRUK m_MRUK;
    [SerializeField] PortalManager m_PortalManager;

    // UI
    public GameObject m_3DUIPanel;
    public GameObject m_ButtonNext;
    public GameObject m_AIAvatar;
    public TextMeshProUGUI m_UIText;

    // bubble
    private float positivePercentage;
    private float negativePercentage;
    private int videoClipLength;
    private int totalBubbleN;

    // agent avatar
    private float[] spectrumData = new float[256];
    [Header("AI Avatar Setting")]
    public Vector3 originalScale = Vector3.one;
    public float scaleMultiplier = 100f;
    public float minFactor = 0.8f;
    public float maxFactor = 1.2f;
    public float amplitudeSmoothTime = 0.1f; 
    private float currentAmplitude;        
    private float amplitudeVelocity;

    // scene
    List<MRUKAnchor> walls = new List<MRUKAnchor>();
    MRUKAnchor ground;

    public enum ModuleState
    {
        OnBoarding,  // TODO
        MoodDetection,
        FreeSpeech
    }

    public ModuleState m_ModuleState = ModuleState.OnBoarding;

    private void OnEnable()
    {
        m_TextToSpeech.OnResultAvailable += PlayAIAgentAudioClip;
        m_SpeechToText.OnResultAvailable += SendTextToLLM;
        m_LLMAgentManager.OnMoodResultAvailable += ReceivedMoodResult;
        m_LLMAgentManager.OnMeditationResultAvailable += ReceivedMeditationResult;
        m_BubbleManager.OnBubbleAnimated += SpawnPortalBasedOnBubble;
    }

    private void OnDisable()
    {
        m_TextToSpeech.OnResultAvailable -= PlayAIAgentAudioClip;
        m_SpeechToText.OnResultAvailable -= SendTextToLLM;
        m_LLMAgentManager.OnMoodResultAvailable -= ReceivedMoodResult;
        m_LLMAgentManager.OnMeditationResultAvailable -= ReceivedMeditationResult;
        m_BubbleManager.OnBubbleAnimated -= SpawnPortalBasedOnBubble;
    }

    private void PlayAIAgentAudioClip(AudioClip clip)
    {
        m_TextToSpeech.audioSource.Play();
    }

    private void SendTextToLLM(string res)
    {
        switch (m_ModuleState)
        {
            case ModuleState.OnBoarding:
                break;

            case ModuleState.MoodDetection:
                m_LLMAgentManager.SendMsgToGPTForMoodRequest(res);
                break;

            case ModuleState.FreeSpeech:
                m_LLMAgentManager.SendMsgToMeditationGPT(res);
                break;

            default:
                break;
        }
    }

    private void ReceivedMoodResult(string res)
    {
        Debug.Log($"Receive mood result: {res}");

        ProcessResultForBubbleSpawn(res);

        int positiveBubbleN = (int)(totalBubbleN * positivePercentage / 100);
        int negativeBubbleN = totalBubbleN - positiveBubbleN;
        m_BubbleManager.SpawnBubbles(positiveBubbleN, negativeBubbleN);

        m_UIText.text = $"positve bubbles: {positiveBubbleN}\nnegative bubbles: {negativeBubbleN}";
    }

    private void ReceivedMeditationResult(string res)
    {
        Debug.Log($"receive meditation result: {res}");

        m_TextToSpeech.SendRequest(res);
    }

    private void SpawnPortalBasedOnBubble(Vector3 origin, Vector3 target, AnimationType type)
    {
        Ray ray = new Ray(origin, target - origin);
        Debug.Log("invoke in module manager ++");

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << LayerMask.NameToLayer("RoomMesh"))) 
        {
            Debug.Log("hit the room mesh layer ++");
            
            Vector3 hitPoint = hit.point;
            GameObject hitObject = hit.collider.gameObject;

            m_PortalManager.SpawnPortal(hitPoint, hitObject.transform.rotation);
        }
    }

    public void SceneInitialized()
    {
        Debug.Log("scene loaded and created Successfully");

        foreach (MRUKRoom room in MRUK.Instance.Rooms)
        {
            foreach (MRUKAnchor childAnchor in room.Anchors)
            {
                if (childAnchor.HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE))
                {
                    walls.Add(childAnchor);
                }
                else if (childAnchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR))
                {
                    ground = childAnchor;
                }

            }
        }

        Debug.Log($"wall num: {walls.Count}");
    }

    private void SetupSceneMeshes()
    {
        // walls
        foreach (var anchor in walls)
        {
            if (anchor.transform.GetChild(0))
                anchor.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("RoomMesh");
            else
                Debug.LogError("No scene meshes were created! Please check!");
        }

        // ground
        if (ground.transform.GetChild(0))
            ground.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("RoomMesh");
        else
            Debug.LogError("No scene meshes were created! Please check!");

    }

    void Start()
    {
        // TODO: specific trigger to do that (after onboarding)
        m_ModuleState = ModuleState.OnBoarding;
    }

    void Update()
    {
        AdjustUIPose();

        UpdateAIAvatar();
    }

    private void AdjustUIPose()
    {
        Vector3 headPos = m_OVRCameraRig.centerEyeAnchor.position;
        if (Vector3.Distance(headPos, m_3DUIPanel.transform.position) < 1f)
            return;

        Vector3 forward = m_OVRCameraRig.transform.forward;

        m_3DUIPanel.transform.position = headPos + forward * 0.5f;
        m_3DUIPanel.transform.rotation = m_OVRCameraRig.transform.rotation;
    }

    private void UpdateAIAvatar()
    {
        if (!m_TextToSpeech.audioSource.isPlaying)
        {
            m_AIAvatar.transform.localScale = originalScale;
            return;
        }

        // get the current spectrum data
        m_TextToSpeech.audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        float sum = 0f;
        for (int i = 0; i < spectrumData.Length; i++)
        {
            sum += spectrumData[i];
        }

        // compute an amplitube
        float targetAmplitude = Mathf.Clamp01(sum / spectrumData.Length);
        targetAmplitude *= scaleMultiplier;
        //Debug.Log(targetAmplitude);

        currentAmplitude = Mathf.SmoothDamp(
                currentAmplitude,
                targetAmplitude,
                ref amplitudeVelocity,
                amplitudeSmoothTime
        );

        m_AIAvatar.transform.localScale = originalScale * (1f + currentAmplitude);
    }

    public void ProcessResultForBubbleSpawn(string input)
    {
        string pattern = @"positive:\s*(\d+)%\s*and\s*negative:\s*(\d+)%";

        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            positivePercentage = int.Parse(match.Groups[1].Value);
            negativePercentage = int.Parse(match.Groups[2].Value);

            Debug.Log($"Parsed Positive: {positivePercentage}%");
            Debug.Log($"Parsed Negative: {negativePercentage}%");

            videoClipLength = m_SpeechToText.GetVideoClipLength();
            if (videoClipLength > 0 && videoClipLength < 8)
                totalBubbleN = 5;
            else if (videoClipLength >= 8 && videoClipLength < 15)
                totalBubbleN = 10;
            else
                totalBubbleN = 16;
        }
        else
        {
            Debug.LogError("Failed to parse mood result.");
        }
    }

    public void ToNext()
    {
        m_ModuleState++;

        // to handle the pipline
        switch (m_ModuleState)
        {
            case ModuleState.OnBoarding:
                break;

            case ModuleState.MoodDetection:
                SetupSceneMeshes();
                m_TextToSpeech.SendRequest("Hi, How do you feel today?");
                break;

            case ModuleState.FreeSpeech:
                m_LLMAgentManager.ClearGPTContext();
                m_BubbleManager.ClearAllBubbles();
                break;

            default:
                break;
        }
       
    }
}
