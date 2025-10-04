using LudumDare58.Game;
using UnityEngine;
using UnityEngine.UI;

namespace LudumDare58.UI
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI text = null;

        [SerializeField]
        Image lockImage = null;

        int sceneIndex = 0;

        Button button;

        SceneLoader sceneLoader;

        int levelIndex;

        private void Awake()
        {
            sceneLoader = FindAnyObjectByType<SceneLoader>();

            button = GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        private void Start()
        {
            bool isLocked = LevelLockManager.IsLocked(levelIndex);

            text.gameObject.SetActive(!isLocked);
            lockImage.gameObject.SetActive(isLocked);

            button.interactable = !isLocked;

        }

        private void OnClick()
        {
            GameManager.IsRetry = false;
            sceneLoader.LoadScene(sceneIndex);
        }

        public void SetText(string buttonText)
        {
            this.text.text = buttonText;
        }

        public void SetLevelIndex(int levelIndex)
        {
            this.levelIndex = levelIndex;
        }

        public void SetSceneIndex(int index)
        {
            sceneIndex = index;
        }
    }
}