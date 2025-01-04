using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using XRDC24.AI;

public class ModuleManager : MonoBehaviour
{
    [SerializeField] BubblesManager m_BubbleManager;
    [SerializeField] LLMMoodDiscriminator m_LLMMoodDiscriminator;
    [SerializeField] STT m_SpeakToText;

    private float positivePercentage;
    private float negativePercentage;
    private int videoClipLength;
    private int totalBubbleN;

    public TextMeshProUGUI m_UIText;

    private void OnEnable()
    {
        m_LLMMoodDiscriminator.OnLLMResultAvailable += ReceiveMoodResult;
    }

    private void OnDisable()
    {
        m_LLMMoodDiscriminator.OnLLMResultAvailable -= ReceiveMoodResult;
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
        
    }

    void Update()
    {
        
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

            videoClipLength = m_SpeakToText.GetVideoClipLength();
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
