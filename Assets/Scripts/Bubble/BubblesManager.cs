using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRDC24.Bubble
{
    public class BubblesManager : MonoBehaviour
    {
        public GameObject positiveTemplate;
        public GameObject negativeTemplate;

        private List<GameObject> spawnedBubbles;

        public int positiveRatio;
        public int negativeRatio;

        public int totalBubbles;

        public int pokeCount = 0;
        public int fallCount = 0;

        private void Start()
        {
            spawnedBubbles = new List<GameObject>();
            pokeCount = 0;
            fallCount = 0;

            SpawnBubbles();
        }

        public void SpawnBubbles()
        {
            // clear previous
            foreach (var bubble in spawnedBubbles)
            {
                GameObject.Destroy(bubble);
            }

            spawnedBubbles.Clear();

            var pos = (int)(totalBubbles * positiveRatio / (positiveRatio + negativeRatio * 1.0));
            var neg = totalBubbles - pos;

            for (var i = 0; i < pos; i++)
            {
                var go = Instantiate(positiveTemplate, transform);
                // update transform
                go.transform.position = positiveTemplate.transform.position;
                go.transform.rotation = positiveTemplate.transform.rotation;
                spawnedBubbles.Add(go);
                // binding event
                go.GetComponent<BubbleController>().OnAnimationFinished += OnAnimationFinished;
            }

            for (var i = 0; i < neg; i++)
            {
                var go = Instantiate(negativeTemplate, transform);
                // update transform
                go.transform.position = negativeTemplate.transform.position;
                go.transform.rotation = negativeTemplate.transform.rotation;
                spawnedBubbles.Add(go);
                // binding event
                go.GetComponent<BubbleController>().OnAnimationFinished += OnAnimationFinished;
            }

            print($"Bubbles spawned: positive = {pos}, negative = {neg}, total = {totalBubbles}");
        }

        private void OnAnimationFinished(GameObject go, AnimationType obj)
        {
            if (obj == AnimationType.Poke)
            {
                pokeCount++;
            }
            else
            {
                fallCount++;
            }

            // clear from list
            spawnedBubbles.Remove(go);
            // destroy go
            GameObject.Destroy(go);

            print($"Poke count: {pokeCount}, Fall count: {fallCount} Remaining: {spawnedBubbles.Count}");
        }

        void LetAllFall()
        {
            foreach (var go in spawnedBubbles)
            {
                go.GetComponent<BubbleController>().LetFall();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Label($"Ratio (pos/neg) = {positiveRatio}/{negativeRatio}\n" +
                            $"Total Bubbles = {totalBubbles}\n" +
                            $"Poke count = {pokeCount}\n" +
                            $"Fall count = {fallCount}\n");

            if (GUILayout.Button("Fall"))
            {
                LetAllFall();
            }

            GUILayout.EndVertical();
        }
    }
}