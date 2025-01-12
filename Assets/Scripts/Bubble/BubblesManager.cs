using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRDC24.Bubble;

public class BubblesManager : MonoBehaviour
{
    // public
    [SerializeField] OVRCameraRig m_OVRCameraRig;

    public List<GameObject> positiveTemplateList;
    public List<GameObject> negativeTemplateList;

    public int pokeCount = 0;
    public int fallCount = 0;
    public AudioSource m_AudioSource;
    public AudioClip m_BubbleBurstSound;
    public AudioClip m_BubbleGenSound;
    public AudioClip m_BubbleAgentSound1;
    public AudioClip m_BubbleAgentSound2;

    // private 
    private List<GameObject> spawnedBubbles;

    private int positiveNum;
    private int negativeNum;
    private int totalBubbles;

    private float spawnDist = 0.6f;
    private float spawnRange = 1.0f;

    #region event

    public System.Action<Vector3, Vector3, AnimationType> OnBubbleAnimated;
    public System.Action OnBubbleInteractionFinished;

    #endregion

    public enum BubbleType
    {
        Positive,
        Negative
    };

    private void Start()
    {
        spawnedBubbles = new List<GameObject>();
        pokeCount = 0;
        fallCount = 0;
    }

    public void SpawnBubbles(int positive, int negative)
    {
        positiveNum = positive;
        negativeNum = negative;
        totalBubbles = positiveNum + negativeNum;

        if (spawnedBubbles.Count > 0)
        {
            // clear previous
            foreach (var bubble in spawnedBubbles)
            {
                Destroy(bubble);
            }
            spawnedBubbles.Clear();
        }

        Vector3 headPos = m_OVRCameraRig.centerEyeAnchor.position;
        Vector3 forward = m_OVRCameraRig.transform.forward;

        for (var i = 0; i < positiveNum; i++)
        {
            var template = GetRandomTemplate(BubbleType.Positive);
            SpawnBubble(template, headPos, forward);
        }

        for (var i = 0; i < negativeNum; i++)
        {
            var template = GetRandomTemplate(BubbleType.Negative);
            SpawnBubble(template, headPos, forward);
        }

        StartCoroutine(PlayBubbleGenerateSound());

        Debug.Log($"Bubbles spawned: positive = {positiveNum}, negative = {negativeNum}, total = {totalBubbles}");
    }

    private IEnumerator PlayBubbleGenerateSound()
    {
        m_AudioSource.clip = m_BubbleGenSound;
        m_AudioSource.Play();

        yield return new WaitForSeconds(2);

        m_AudioSource.Stop();
    }

    public void ClearAllBubbles()
    {
        if (spawnedBubbles.Count > 0)
        {
            foreach (var bubble in spawnedBubbles)
            {
                GameObject.Destroy(bubble);
            }
            spawnedBubbles.Clear();
        }
    }

    private void SpawnBubble(GameObject prefab, Vector3 headPos, Vector3 forward)
    {
        Vector3 spawnPoint = headPos + forward * spawnDist;
        spawnPoint += new Vector3(
            Random.Range(-spawnRange, spawnRange),
            Random.Range(-spawnRange, spawnRange),
            Random.Range(-spawnRange / 2, spawnRange / 2)
        );

        GameObject bubbleAnimation = Instantiate(prefab, spawnPoint, Quaternion.identity);

        // binding event
        bubbleAnimation.transform.GetChild(0).GetComponent<BubbleController>().OnAnimationFinished += OnAnimationFinished;
        bubbleAnimation.transform.GetChild(0).GetComponent<BubbleController>().OnBubbleAnimated += BubbleAnimated;

        spawnedBubbles.Add(bubbleAnimation);
    }

    private GameObject GetRandomTemplate(BubbleType type)
    {
        switch (type)
        {
            case BubbleType.Positive:
                return positiveTemplateList[Random.Range(0, positiveTemplateList.Count)];

            case BubbleType.Negative:
                return negativeTemplateList[Random.Range(0, negativeTemplateList.Count)];

            default:
                Debug.LogError("Please assign a correct type");
                return null;
        }
    }

    private void OnAnimationFinished(GameObject go, AnimationType obj)
    {
        // might be removed before
        if (!spawnedBubbles.Contains(go)) return;

        if (obj == AnimationType.Poke)
        {
            pokeCount++;
        }
        else
        {
            fallCount++;
        }

        go.SetActive(false);

        Debug.Log($"Poke count: {pokeCount}, Fall count: {fallCount} Remaining: {totalBubbles - pokeCount - fallCount}");

        if (totalBubbles - pokeCount - fallCount <= 0)
            OnBubbleInteractionFinished();
    }

    private void BubbleAnimated(GameObject obj, AnimationType type)
    {
        // play sound effect
        if (pokeCount > 1)
        {
            m_AudioSource.clip = m_BubbleBurstSound;
            m_AudioSource.Play();
        }
        else if (type == AnimationType.Poke)
        {
            if (pokeCount == 0)
                m_AudioSource.clip = m_BubbleAgentSound1;
            else
                m_AudioSource.clip = m_BubbleAgentSound2;

            m_AudioSource.Play();
        }

        // ray cast to get the portal position and sent back to module manager
        OnBubbleAnimated(m_OVRCameraRig.centerEyeAnchor.position, obj.transform.position, type);
    }

    void LetAllFall()
    {
        foreach (var go in spawnedBubbles)
        {
            go.GetComponent<BubbleController>().LetFall();
        }
    }

    //private void OnGUI()
    //{
    //    GUILayout.BeginVertical();

    //    GUILayout.Label($"Ratio (pos/neg) = {positiveNum}/{negativeNum}\n" +
    //                    $"Total Bubbles = {totalBubbles}\n" +
    //                    $"Poke count = {pokeCount}\n" +
    //                    $"Fall count = {fallCount}\n");

    //    if (GUILayout.Button("Fall"))
    //    {
    //        LetAllFall();
    //    }

    //    GUILayout.EndVertical();
    //}
}