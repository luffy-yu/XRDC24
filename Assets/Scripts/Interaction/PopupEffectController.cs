using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace XRDC24.Interaction
{
    public class PopupEffectController : MonoBehaviour
    {
        public RectTransform parentCanvas; // Reference to the parent canvas
        public RectTransform popupWindow; // Reference to the popup window

        public RectTransform mainWindow;

        public float riseDuration = 1.0f; // Time to rise to the center
        public float displayDuration = 3.0f; // Time to stay at the center
        public float fadeDuration = 0.5f; // Time to fade out

        private Vector2 bottomPosition;
        private Vector2 centerPosition;

        private void Start()
        {

        }

        // trigger when enabled
        private void OnEnable()
        {
            StartCoroutine(InitEffect());
        }

        IEnumerator InitEffect()
        {
            popupWindow.gameObject.SetActive(false);
            // hide manin window
            mainWindow.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.5f);

            bottomPosition = new Vector2(0, -Math.Abs(parentCanvas.rect.height) / 2);
            centerPosition = Vector2.zero;
            yield return new WaitForSeconds(0.1f);
            ShowPopup();
        }

        public void ShowPopup()
        {
            StartCoroutine(PopupRoutine());
        }

        private IEnumerator PopupRoutine()
        {
            // Activate the popup and set its initial position
            popupWindow.gameObject.SetActive(true);
            popupWindow.anchoredPosition = bottomPosition;

            // Animate rising to the center
            float elapsedTime = 0;
            while (elapsedTime < riseDuration)
            {
                popupWindow.anchoredPosition = Vector2.Lerp(bottomPosition, centerPosition, elapsedTime / riseDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            popupWindow.anchoredPosition = centerPosition;

            // Wait for the display duration
            yield return new WaitForSeconds(displayDuration);

            // Animate fading out
            CanvasGroup canvasGroup = popupWindow.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = popupWindow.gameObject.AddComponent<CanvasGroup>();
            }

            elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = 0;

            // Deactivate the popup
            popupWindow.gameObject.SetActive(false);

            // enable the main window
            mainWindow.gameObject.SetActive(true);
        }

        #region Debug

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Show Popup"))
            {
                ShowPopup();
            }

            GUILayout.EndVertical();
        }

        #endregion
    }
}