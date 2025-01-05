using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using XRDC24.AI;

public class ModuleManager : MonoBehaviour
{
    [SerializeField] OVRCameraRig m_OVRCameraRig;
    [SerializeField] BubblesManager m_BubbleManager;
    [SerializeField] LLMAgentManager m_LLMAgentManager;
    [SerializeField] STT m_SpeechToText;
    [SerializeField] TTS m_TextToSpeech;

    public GameObject m_3DUIPanel;

    private float positivePercentage;
    private float negativePercentage;
    private int videoClipLength;
    private int totalBubbleN;

    public TextMeshProUGUI m_UIText;

    private void OnEnable()
    {
        m_TextToSpeech.OnResultAvailable += PlayAIAgentAudioClip;
        m_SpeechToText.OnResultAvailable += SendTextToLLM;
        m_LLMAgentManager.OnMoodResultAvailable += ReceiveMoodResult;
    }

    private void OnDisable()
    {
        m_TextToSpeech.OnResultAvailable -= PlayAIAgentAudioClip;
        m_SpeechToText.OnResultAvailable -= SendTextToLLM;
        m_LLMAgentManager.OnMoodResultAvailable -= ReceiveMoodResult;
    }

    private void PlayAIAgentAudioClip(AudioClip clip)
    {
        m_TextToSpeech.audioSource.Play();
    }

    private void SendTextToLLM(string res)
    {
        // TODO: determine which type of communication to send
        m_LLMAgentManager.SendMsgToGPT(res);
    }

    private void ReceiveMoodResult(string res)
    {
        Debug.Log($"Receive mood result: {res}");

        ProcessResultForBubbleSpawn(res);

        int positiveBubbleN = (int)(totalBubbleN * positivePercentage / 100);
        int negativeBubbleN = totalBubbleN - positiveBubbleN;
        m_BubbleManager.SpawnBubbles(positiveBubbleN, negativeBubbleN);

        m_UIText.text = $"positve bubbles: {positiveBubbleN}\nnegative bubbles: {negativeBubbleN}";
    }

    void Start()
    {
        // after scan the room, start agent audio to user
        m_TextToSpeech.SendRequest("Hi, How do you feel today?");

    }

    void Update()
    {
        AdjustUIPose();
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
}
