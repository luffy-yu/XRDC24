using Meta.XR.MRUtilityKit;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using XRDC24.AI;
using System.Collections.Generic;
using XRDC24.Bubble;
using UnityEngine.VFX;
using System.Collections;
using XRDC24.Interaction;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class ModuleManager : MonoBehaviour
{
    [SerializeField] OVRCameraRig m_OVRCameraRig;
    [SerializeField] OVRManager m_OVRManager;
    [SerializeField] BubblesManager m_BubbleManager;
    [SerializeField] LLMAgentManager m_LLMAgentManager;
    [SerializeField] STT m_SpeechToText;
    [SerializeField] TTS m_TextToSpeech;
    [SerializeField] MRUK m_MRUK;
    [SerializeField] PortalManager m_PortalManager;

    // UI
    [Header("User Interfaces")]
    public GameObject m_3DUIPanel;
    public GameObject m_ButtonNext;
    public GameObject m_AIAvatar;
    public VisualEffect m_AIAvatarVFX;
    public GameObject m_AvatarBackground;
    public GameObject m_TitleLogo;
    public GameObject m_3DButton;

    private int frameIndex = 0;
    private int audioFrameIndex = 0;
    private TextMeshProUGUI textAgent;
    private TextMeshProUGUI textUser;

    // bubble
    private float positivePercentage;
    private float negativePercentage;
    private int videoClipLength;
    private int totalBubbleN;

    // agent avatar
    private float[] spectrumData = new float[256];
    [Header("AI Avatar Setting")]
    public Vector3 originalScale = Vector3.one;
    public float originalRate = 1f;
    public float originalAnimatedSpeed = 1f;
    public float scaleMultiplier = 100f;
    public float minFactor = 0.8f;
    public float maxFactor = 1.2f;
    public float amplitudeSmoothTime = 0.1f; 
    private float currentAmplitude;        
    private float amplitudeVelocity;
    private bool isPoseInit = false;

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
        m_BubbleManager.OnBubbleInteractionFinished += ToNext;
        m_3DButton.transform.Find("RecordingButton").GetChild(0).GetComponent<TriggerForwarder>().OnRecordingTriggerEnter += StartRecording;
        m_3DButton.transform.Find("RecordingButton").GetChild(0).GetComponent<TriggerForwarder>().OnRecordingTriggerExit += EndRecording;
    }

    private void OnDisable()
    {
        m_TextToSpeech.OnResultAvailable -= PlayAIAgentAudioClip;
        m_SpeechToText.OnResultAvailable -= SendTextToLLM;
        m_LLMAgentManager.OnMoodResultAvailable -= ReceivedMoodResult;
        m_LLMAgentManager.OnMeditationResultAvailable -= ReceivedMeditationResult;
        m_BubbleManager.OnBubbleAnimated -= SpawnPortalBasedOnBubble;
        m_BubbleManager.OnBubbleInteractionFinished -= ToNext;
        m_3DButton.transform.Find("RecordingButton").GetChild(0).GetComponent<TriggerForwarder>().OnRecordingTriggerEnter -= StartRecording;
        m_3DButton.transform.Find("RecordingButton").GetChild(0).GetComponent<TriggerForwarder>().OnRecordingTriggerExit -= EndRecording;
    }

    private void PlayAIAgentAudioClip(AudioClip clip, string text)
    {
        m_TextToSpeech.audioSource.Play();

        StartCoroutine(ShowAgentText(clip.length, text));
    }

    private IEnumerator ShowAgentText(float second, string text)
    {
        GameObject dialog = m_3DUIPanel.transform.Find("DialogAgentUser").gameObject;

        // show text
        dialog.SetActive(true);
        dialog.transform.Find("Dialog_Agent").gameObject.SetActive(true);
        textAgent.text = text;

        yield return new WaitForSeconds(second);

        // hide text
        dialog.SetActive(false);
        dialog.transform.Find("Dialog_Agent").gameObject.SetActive(false);

        if (m_ModuleState == ModuleState.OnBoarding)
        {
            if (audioFrameIndex == 0)
            {
                yield return new WaitForSeconds(0.5f);

                m_3DButton.SetActive(true);
                m_3DUIPanel.transform.Find("FigmaCanvas02").gameObject.SetActive(true);

                frameIndex = 3;
                audioFrameIndex++;
            }
            else if (audioFrameIndex == 1)
            {
                frameIndex = 4;
                audioFrameIndex++;
            }
            else if (audioFrameIndex == 2)
            {
                // to moode detection mode
                ToNext();
            }
        }
    }

    private IEnumerator ShowUserText(string text)
    {
        GameObject dialog = m_3DUIPanel.transform.Find("DialogAgentUser").gameObject;

        // show text
        dialog.SetActive(true);
        dialog.transform.Find("Dialog_User").gameObject.SetActive(true);
        textUser.text = text;

        yield return new WaitForSeconds(3);
    }


    private void SendTextToLLM(string res)
    {
        switch (m_ModuleState)
        {
            case ModuleState.OnBoarding:
                break;

            case ModuleState.MoodDetection:
                StartCoroutine(ShowUserText(res));
                m_LLMAgentManager.SendMsgToGPTForMoodRequest(res);
                break;

            case ModuleState.FreeSpeech:
                m_LLMAgentManager.SendMsgToMeditationGPT(res);
                break;

            default:
                break;
        }
    }

    private void PlayOnBoardingFrames()
    {
        if (m_ModuleState != ModuleState.OnBoarding)
            return;

        switch (frameIndex)
        {
            case 0:
                break;

            case 1:
                StartCoroutine(ShowTitle());
                frameIndex = 0;
                break;

            case 2:
                StartCoroutine(ShowOnboardPanel());
                frameIndex = 0;
                break;

            case 3:
                m_TextToSpeech.SendRequest("Take your time. As you speak, I’ll reflect your words as bubbles.");
                frameIndex = 0;
                break;

            case 4:
                m_TextToSpeech.SendRequest("Your positive emotions will form soft, glowing bubbles. If there are heavier \r\nfeelings, they might appear darker or sharper – and that’s okay.");
                frameIndex = 0;
                break;

        }
    }

    private IEnumerator ShowOnboardPanel()
    {
        m_3DUIPanel.transform.Find("FigmaCanvasI01")?.gameObject.SetActive(true);

        yield return new WaitForSeconds(Constants.ONBOARD_PANEL_SHOW_SECONDS);

        m_3DUIPanel.transform.Find("FigmaCanvasI01")?.gameObject.SetActive(false);

        // to next onboarding frame
        m_TextToSpeech.SendRequest("Hello, my name is Flo, your personal mental health assistant, I’m here to listen. How are you feeling today?");
    }

    private IEnumerator ShowTitle()
    {
        m_TitleLogo.SetActive(true);
        m_AvatarBackground.SetActive(true);

        yield return new WaitForSeconds(Constants.TITLE_SHOW_SECONDS);

        float time = 0f;
        float startAlpha = 1f;

        while (time <= Constants.TITLE_DISAPPEAR_DURATION)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, time / Constants.TITLE_DISAPPEAR_DURATION);

            // Update the material’s color
            m_TitleLogo.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_OverallAlpha", alpha);
            m_TitleLogo.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_OverallAlpha", alpha);

            yield return null;
        }

        m_TitleLogo.SetActive(false);
        frameIndex = 2;
    }

    private void ReceivedMoodResult(string res)
    {
        Debug.Log($"Receive mood result: {res}");

        ProcessResultForBubbleSpawn(res);

        int positiveBubbleN = (int)(totalBubbleN * positivePercentage / 100);
        int negativeBubbleN = totalBubbleN - positiveBubbleN;
        m_BubbleManager.SpawnBubbles(positiveBubbleN, negativeBubbleN);

        Debug.Log($"positve bubbles: {positiveBubbleN}\nnegative bubbles: {negativeBubbleN}");
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

    private void StartRecording(TriggerType type)
    {
        if (type != TriggerType.Recording)
            return;

        m_3DUIPanel.transform.Find("FigmaCanvas02").gameObject.SetActive(false);
        m_SpeechToText.StartRecording();
    }

    private void EndRecording(TriggerType type)
    {
        if (type != TriggerType.Recording)
            return;

        Debug.Log("inside the end recording");
        m_SpeechToText.EndRecording();
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
        m_ModuleState = ModuleState.OnBoarding;
        frameIndex = 1;
        m_3DUIPanel.SetActive(true);
        m_AvatarBackground.SetActive(false);
        m_3DButton.SetActive(false);

        textAgent = m_3DUIPanel.transform.Find("DialogAgentUser").Find("Dialog_Agent").GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        textUser = m_3DUIPanel.transform.Find("DialogAgentUser").Find("Dialog_User").GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        AdjustUIPose();

        UpdateAIAvatarScale();
        UpdateAIAvatarParticleRate();

        PlayOnBoardingFrames();
    }

    private void AdjustUIPose()
    {
        Vector3 headPos = m_OVRCameraRig.centerEyeAnchor.position;
        if (Vector3.Distance(headPos, m_3DUIPanel.transform.position) < 1f)
            return;

        Vector3 forward = m_OVRCameraRig.transform.forward;

        m_3DUIPanel.transform.position = headPos + forward * 0.5f;
        m_3DUIPanel.transform.rotation = m_OVRCameraRig.transform.rotation;

        m_AIAvatar.transform.position = headPos + forward * 1.2f;
        m_AIAvatar.transform.rotation = m_OVRCameraRig.transform.rotation;
    }

    private void UpdateAIAvatarScale()
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
                targetAmplitude * 0.1f,
                ref amplitudeVelocity,
                amplitudeSmoothTime
        );

        m_AIAvatar.transform.localScale = originalScale * (1f + currentAmplitude);
    }

    private void UpdateAIAvatarParticleRate()
    {
        if (!m_TextToSpeech.audioSource.isPlaying)
        {
            m_AIAvatarVFX.playRate = originalRate;
            m_AIAvatar.GetComponent<Animator>().speed = originalAnimatedSpeed;
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

        m_AIAvatarVFX.playRate = originalRate * (1f + currentAmplitude * 10);
        m_AIAvatar.GetComponent<Animator>().speed = originalAnimatedSpeed * (1f + currentAmplitude);
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
            if (videoClipLength > 0 && videoClipLength < 15)
                totalBubbleN = 5;
            else if (videoClipLength >= 16 && videoClipLength < 30)
                totalBubbleN = 16;
            else
                totalBubbleN = 20;
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
                break;

            case ModuleState.FreeSpeech:
                m_LLMAgentManager.ClearGPTContext();
                m_BubbleManager.ClearAllBubbles();
                m_PortalManager.ClearPortals();
                TriggerPassthrough(false);
                break;

            default:
                break;
        }
       
    }

    public void TriggerPassthrough(bool isEnabled)
    {
        if (isEnabled)
        {
            m_OVRManager.isInsightPassthroughEnabled = true;
            m_OVRCameraRig.centerEyeAnchor.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        }
        else
        {
            m_OVRManager.isInsightPassthroughEnabled = false;
            m_OVRCameraRig.centerEyeAnchor.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
        }
    }

}
