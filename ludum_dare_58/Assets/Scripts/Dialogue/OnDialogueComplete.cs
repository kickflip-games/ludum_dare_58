using UnityEngine;
using UnityEngine.Events;

namespace LudumDare58.DialogueSystem
{
    public class OnDialogueComplete : MonoBehaviour
    {
        [SerializeField]
        Dialogue dialogue;

        public UnityEvent actions = new UnityEvent();

        private void Reset()
        {
            dialogue = GetComponentInParent<Dialogue>();
        }

        private void OnEnable()
        {
            if (dialogue != null)
            {
                dialogue.OnComplete += OnComplete;
            }
        }

        private void OnDisable()
        {
            if (dialogue != null)
            {
                dialogue.OnComplete -= OnComplete;
            }
        }

        private void OnComplete()
        {
            actions?.Invoke();
        }
    }
}