using UnityEngine;
using System.Collections.Generic;

public class BubbleGenerator : MonoBehaviour
{
    [SerializeField] OVRCameraRig m_OVRCameraRig;
    
    public GameObject m_PositiveBubblePrefab;
    public GameObject m_NegativeBubblePrefab;

    private List<GameObject> spawnBubbleList;
    private int positiveN = 0;
    private int negativeN = 0;
    private float spawnDist = 0.6f;
    private float spawnRange = 0.5f;

    void Start()
    {

    }

    void Update()
    {

    }

    public void SpawnBubbles(int posiN, int negN)
    {
        positiveN = posiN;
        negativeN = negN;
        spawnBubbleList = new List<GameObject>();

        Vector3 headPos = m_OVRCameraRig.centerEyeAnchor.position;
        Vector3 forward = m_OVRCameraRig.transform.forward;

        // spawn positive bubbles
        for (int i = 0; i < positiveN; i++)
        {
            SpawnBubble(m_PositiveBubblePrefab, headPos, forward);
        }

        // spawn negative bubbles
        for (int i = 0; i < negativeN; i++)
        {
            SpawnBubble(m_NegativeBubblePrefab, headPos, forward);
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

        GameObject bubble = Instantiate(prefab, spawnPoint, Quaternion.identity);
        spawnBubbleList.Add(bubble);
    }
}
