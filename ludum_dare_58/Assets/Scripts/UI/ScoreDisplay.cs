using System.Collections;
using UnityEngine;
using TMPro;

namespace LudumDare58.Game
{
    public class ScoreAndComboUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI comboText;

        [Header("Combo Settings")]
        [SerializeField] float comboTimeout = 1.5f; // Time before combo fades
        [SerializeField] AnimationCurve comboPopScale = AnimationCurve.EaseInOut(0, 1, 0.3f, 1.3f);
        [SerializeField] float comboFadeDuration = 0.5f;
        [SerializeField] int baseScorePerCollectable = 100;
        [SerializeField] AnimationCurve scorePulseScale = AnimationCurve.EaseInOut(0, 1, 0.3f, 1.3f);

        private int score = 0;
        private int comboCount = 0;
        private int comboMultiplier = 1;
        private float comboTimer = 0f;
        private bool comboActive = false;

        private Vector3 baseScoreScale;
        private Vector3 baseComboScale;

        private Coroutine comboFadeRoutine;
        private Coroutine scorePulseRoutine;

        private void Start()
        {
            if (scoreText != null)
            {
                scoreText.text = "0000";
                baseScoreScale = scoreText.transform.localScale;
            }

            if (comboText != null)
            {
                comboText.text = "";
                baseComboScale = comboText.transform.localScale;
            }
        }

        private void Update()
        {
            if (comboActive)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0)
                    StartComboFade();
            }
        }

        public void OnCollectableCollected()
        {
            comboCount++;
            comboTimer = comboTimeout;
            comboActive = true;
            comboMultiplier = 1 + (comboCount / 2); // increase faster

            int points = baseScorePerCollectable * comboMultiplier;
            score += points;

            UpdateScoreUI();
            UpdateComboUI();

            // --- Pulse animations ---
            if (scorePulseRoutine != null) StopCoroutine(scorePulseRoutine);
            scorePulseRoutine = StartCoroutine(PulseText(scoreText.transform, scorePulseScale));

            if (comboFadeRoutine != null) StopCoroutine(comboFadeRoutine);
            comboText.alpha = 1f;
            comboText.transform.localScale = baseComboScale;
            StartCoroutine(PulseText(comboText.transform, comboPopScale));
        }

        public void OnMissedAction()
        {
            ResetCombo();
        }

        private void StartComboFade()
        {
            if (comboFadeRoutine != null) StopCoroutine(comboFadeRoutine);
            comboFadeRoutine = StartCoroutine(FadeOutCombo());
        }

        private IEnumerator FadeOutCombo()
        {
            comboActive = false;
            float t = 0f;
            float startAlpha = comboText.alpha;

            while (t < comboFadeDuration)
            {
                t += Time.deltaTime;
                comboText.alpha = Mathf.Lerp(startAlpha, 0f, t / comboFadeDuration);
                yield return null;
            }

            comboText.text = "";
            comboCount = 0;
            comboMultiplier = 1;
        }

        private void UpdateScoreUI()
        {
            if (scoreText != null)
                scoreText.text = score.ToString("D4"); // cleaner numeric format
        }

        private void UpdateComboUI()
        {
            if (comboText != null)
                comboText.text = $"x{comboMultiplier}";
        }

        private void ResetCombo()
        {
            if (comboFadeRoutine != null) StopCoroutine(comboFadeRoutine);
            comboText.text = "";
            comboText.alpha = 0f;
            comboActive = false;
            comboCount = 0;
            comboMultiplier = 1;
        }

        private IEnumerator PulseText(Transform textTransform, AnimationCurve scaleCurve)
        {
            float duration = 0.3f;
            float t = 0f;
            Vector3 baseScale = Vector3.one;

            while (t < duration)
            {
                t += Time.deltaTime;
                float scale = scaleCurve.Evaluate(t / duration);
                textTransform.localScale = baseScale * scale;
                yield return null;
            }

            textTransform.localScale = Vector3.one;
        }
    }
}
