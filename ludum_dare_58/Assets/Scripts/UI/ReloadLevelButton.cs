using LudumDare58.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudumDare58.UI
{
    [AddComponentMenu("Ilumisoft/UI/Reload Level Button")]
    [RequireComponent(typeof(Button))]
    public class ReloadLevelButton : MonoBehaviour
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
            if (sceneLoader != null)
            {
                // GameManager.IsRetry = true;
                sceneLoader.LoadScene(gameObject.scene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(gameObject.scene.buildIndex);
            }
        }
    }
}