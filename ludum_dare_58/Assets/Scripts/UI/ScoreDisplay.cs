using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LudumDare58.Game
{
    public class ScoreAndComboUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI comboText;
        [SerializeField] Slider comboSlider; // <- assign a UI Slider in the inspector

        [Header("Combo Settings")]
        [SerializeField] float comboTimeout = 1.5f;  // Time before combo resets
        [SerializeField] AnimationCurve comboPopScale = AnimationCurve.EaseInOut(0, 1, 0.3f, 1.3f);
        [SerializeField] int baseScorePerCollectable = 100;  // Base points per collectable
        [SerializeField] Color normalBarColor = Color.green;
        [SerializeField] Color warningBarColor = Color.red;
        [SerializeField] float warningThreshold = 0.3f; // below this % bar turns red

        private int score = 0;
        private int comboCount = 0;
        private float comboTimer = 0f;
        private int comboMultiplier = 1;
        private bool comboActive = false;

        private Vector3 baseScale;
        private Coroutine popRoutine;
        private Image comboFillImage;

        private void Start()
        {
            if (scoreText != null)
                scoreText.text = "Score: 0";

            if (comboText != null)
                comboText.text = "";

            baseScale = comboText != null ? comboText.transform.localScale : Vector3.one;

            if (comboSlider != null)
            {
                comboSlider.gameObject.SetActive(false);
                comboFillImage = comboSlider.fillRect?.GetComponent<Image>();
            }
        }

        private void Update()
        {
            if (comboActive)
            {
                comboTimer -= Time.deltaTime;

                if (comboSlider != null)
                {
                    comboSlider.value = comboTimer / comboTimeout;

                    // Change color to red near timeout
                    if (comboFillImage != null)
                    {
                        comboFillImage.color = comboSlider.value <= warningThreshold
                            ? warningBarColor
                            : normalBarColor;
                    }
                }

                if (comboTimer <= 0)
                    ResetCombo();
            }
        }

        /// <summary>
        /// Called when the player collects a collectable.
        /// </summary>
        public void OnCollectableCollected()
        {
            // --- Combo logic ---
            comboCount++;
            comboTimer = comboTimeout;
            comboActive = true;
            comboMultiplier = CalculateMultiplier(comboCount);

            // --- Score logic ---
            int points = baseScorePerCollectable * comboMultiplier;
            score += points;
            UpdateScoreUI();
            UpdateComboUI();

            if (comboSlider != null)
            {
                comboSlider.gameObject.SetActive(true);
                comboSlider.maxValue = 1;
                comboSlider.value = 1;
            }

            // --- Pop animation ---
            if (popRoutine != null)
                StopCoroutine(popRoutine);
            popRoutine = StartCoroutine(PopText(comboText.transform));
        }

        public void OnMissedAction()
        {
            ResetCombo();
        }

        private void ResetCombo()
        {
            comboCount = 0;
            comboMultiplier = 1;
            comboActive = false;

            if (comboText != null)
                comboText.text = "";

            if (comboSlider != null)
                comboSlider.gameObject.SetActive(false);
        }

        private int CalculateMultiplier(int currentCombo)
        {
            // Increase multiplier every 3 combos
            return 1 + (currentCombo / 3);
        }

        private void UpdateScoreUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }

        private void UpdateComboUI()
        {
            if (comboText != null)
                comboText.text = $"Combo {comboCount}  x{comboMultiplier}";
        }

        private IEnumerator PopText(Transform textTransform)
        {
            float duration = 0.3f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float scale = comboPopScale.Evaluate(t / duration);
                textTransform.localScale = baseScale * scale;
                yield return null;
            }
            textTransform.localScale = baseScale;
        }
    }
}
