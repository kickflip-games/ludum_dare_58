using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace LudumDare58
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        PlayableDirector loadedPlayableDirector = null;

        [SerializeField]
        PlayableDirector playableDirector = null;

        private void Start()
        {
            loadedPlayableDirector.Play();
        }

        public void LoadScene(int index)
        {
            StopAllCoroutines();
            StartCoroutine(LoadSceneCoroutine(index));
        }

        public void LoadScene(string name)
        {
            StopAllCoroutines();
            StartCoroutine(LoadSceneCoroutine(name));
        }

        IEnumerator LoadSceneCoroutine(string name)
        {
            yield return PlayTimelineAndWait();

            SceneManager.LoadScene(name);
        }

        IEnumerator LoadSceneCoroutine(int index)
        {
            yield return PlayTimelineAndWait();

            SceneManager.LoadScene(index);
        }

        IEnumerator PlayTimelineAndWait()
        {
            if (playableDirector != null)
            {
                playableDirector.gameObject.SetActive(true);
                playableDirector.Stop();
                playableDirector.time = 0;
                playableDirector.Evaluate();
                playableDirector.Play();

                yield return new WaitForSecondsRealtime((float)playableDirector.duration);
            }
        }
    }
}