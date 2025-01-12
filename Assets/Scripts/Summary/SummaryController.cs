using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace XRDC24.Summary
{
    public class SummaryController : MonoBehaviour
    {
        public List<TextMeshProUGUI> positives;
        public List<TextMeshProUGUI> negatives;
        public List<TextMeshProUGUI> durations;

        public List<TextMeshProUGUI> sentences;

        public int positiveCount;
        public int negativeCount;
        public int duration;

        private void Start()
        {
            // Update UI
            UpdateUI();
        }

        string GenerateSentence()
        {
            var s = $"" +
                    $"You gathered {positiveCount} positive thoughts " +
                    $"and overcame {negativeCount} negative ones, " +
                    $"and finished {duration} minutes meditation process today.";
            return s;
        }

        public void UpdateSummary(int pos, int neg, int duration)
        {
            positiveCount = pos;
            negativeCount = neg;
            this.duration = duration;

            // Update UI
            UpdateUI();
        }

        void UpdateUI()
        {
            positives.ForEach(item => item.text = $"{positiveCount}");
            negatives.ForEach(item => item.text = $"{negativeCount}");
            durations.ForEach(item => item.text = $"{duration}");

            var s = GenerateSentence();
            sentences.ForEach(item => item.text = s);
        }
    }
}