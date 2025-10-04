using UnityEngine;

namespace LudumDare58.DialogueSystem
{
    public class ReplaceInputDeviceText : MonoBehaviour
    {
        [SerializeField]
        DialogueText dialogueText;

        [SerializeField]
        string keyword = string.Empty;

        [SerializeField]
        string keyboardText = string.Empty;

        [SerializeField]
        string xboxControllerText = string.Empty;

        bool isControllerConnected = false;

        private void Awake()
        {
            var gamepads = UnityEngine.InputSystem.Gamepad.all;

            if (gamepads.Count > 0)
            {
                isControllerConnected = true;
            }

            ReplaceText();
        }

        void ReplaceText()
        {
            var replacement = isControllerConnected ? xboxControllerText : keyboardText;

            dialogueText.Text = dialogueText.Text.Replace(keyword, replacement);
        }
    }
}