using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LudumDare58.DialogueSystem
{
    public class Dialogue : MonoBehaviour
    {
        [SerializeField]
        public InputAction ContinueAction = null;

        [SerializeField]
        public TMPro.TextMeshProUGUI TextDisplay = null;

        [SerializeField]
        public float CharacterDuration = 0.0f;

        [SerializeField]
        public float LineDelay = 0.0f;

        [SerializeField]
        public UnityAction OnComplete = null;

        DialogueText[] sentences = null;

        public bool FreezeTime = true;

        private int index = 0;

        bool isComplete = false;

        void OnEnable()
        {
            ContinueAction.Enable();
        }

        void OnDisable()
        {
            ContinueAction.Disable();
        }

        private void Awake()
        {
            Time.timeScale = 1.0f;

            ContinueAction.performed += OnContinue;

            sentences = GetComponentsInChildren<DialogueText>();
        }

        private void OnContinue(InputAction.CallbackContext obj)
        {
            Next();
        }

        IEnumerator Start()
        {
            TextDisplay.text = string.Empty;

            yield return null;

            if (FreezeTime)
            {
                Time.timeScale = 0.0f;
            }

            yield return new WaitForSecondsRealtime(.5f);

            StartCoroutine(Type(sentences[index].Text));
        }

        IEnumerator Type(string sentence)
        {
            var characters = sentence.ToCharArray();

            TextDisplay.text = string.Empty;

            foreach (var character in characters)
            {
                TextDisplay.text += character;

                if (CharacterDuration > 0)
                {
                    yield return new WaitForSecondsRealtime(CharacterDuration);
                }

                if (character == '\n')
                {
                    yield return new WaitForSecondsRealtime(LineDelay);
                }
            }

            yield return DisplayAnimatedUnderscore();
        }

        IEnumerator DisplayAnimatedUnderscore()
        {
            bool underscore = true;

            while (true)
            {
                if (underscore)
                {
                    TextDisplay.text += "_";
                }
                else
                {
                    TextDisplay.text = TextDisplay.text.Remove(TextDisplay.text.Length - 1);
                }

                underscore = !underscore;

                yield return new WaitForSecondsRealtime(0.6f);
            }
        }

        public void Next()
        {
            StopAllCoroutines();

            if (isComplete)
            {
                return;
            }

            index++;

            if (index < sentences.Length)
            {
                StartCoroutine(Type(sentences[index].Text));
            }
            else
            {
                if (FreezeTime)
                {
                    Time.timeScale = 1.0f;
                }

                OnComplete?.Invoke();

                isComplete = true;
            }
        }
    }
}