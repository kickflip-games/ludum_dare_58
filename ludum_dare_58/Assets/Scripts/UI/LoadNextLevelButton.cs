using LudumDare58.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudumDare58.UI
{
    [AddComponentMenu("Ilumisoft/UI/Load Next Level Button")]
    [RequireComponent(typeof(Button))]
    public class LoadNextLevelButton : MonoBehaviour
    {
        Button button = null;

        SceneLoader sceneLoader;

        private void Start()
        {
            sceneLoader = FindAnyObjectByType<SceneLoader>();

            button = GetComponent<Button>();

            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            int index = gameObject.scene.buildIndex + 1;

            if (sceneLoader != null)
            {
                GameManager.IsRetry = false;
                sceneLoader.LoadScene(1);
            }
            else
            {
                SceneManager.LoadScene(index);
            }
        }
    }
}