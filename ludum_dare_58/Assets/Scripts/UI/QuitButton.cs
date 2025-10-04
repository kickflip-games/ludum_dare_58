﻿using UnityEngine;
using UnityEngine.UI;

namespace LudumDare58.UI
{
    [RequireComponent(typeof(Button))]
    public class QuitButton : MonoBehaviour
    {
        Button button = null;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Application.Quit();
        }
    }
}