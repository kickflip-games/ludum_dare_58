using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace LudumDare58.Game
{
    [RequireComponent( typeof(AudioSource))]
    public class GameManager : MonoBehaviour
    {
        public static bool IsRetry = false;

        [SerializeField]
        LevelData levelData = null;

        [SerializeField, Header("Game")]
        Countdown countdown = null;

        [SerializeField, Tooltip("Delay before the countdown starts")]
        float startDelay = 0.0f;

        [SerializeField, Tooltip("Time limit of the countdown")]
        float timeLimit = 10.0f;

        [SerializeField, Header("SFX")]
        AudioClip successSFX = null;

        [SerializeField]
        AudioClip failSFX = null;

        [SerializeField]
        UnityEvent onComplete = null;

        [SerializeField]
        UnityEvent onFail = null;

        AudioSource audioSource;
        CollectableManager collectableManager = null;
        GameObject player;
        bool isGameOver = false;

        public UnityEvent OnComplete { get => this.onComplete; set => this.onComplete = value; }
        public UnityEvent OnFail { get => this.onFail; set => this.onFail = value; }

        private void Awake()
        {
            player = GameObject.FindWithTag("Player");

            collectableManager = GetComponent<CollectableManager>();
            audioSource = GetComponent<AudioSource>();

            // Connect to events
            // collectableManager.OnAllCollected += OnAllCollected;
            countdown.OnTimeOut += OnTimeOut;
        }

        private IEnumerator Start()
        {
            countdown.SetTimeLimit(timeLimit);

            yield return new WaitForSeconds(startDelay);

            countdown.StartCountdown();

            isGameOver = false;
        }

        /// <summary>
        /// Callback invoked when all collectables are collected.
        /// Still supported if used.
        /// </summary>
        private void OnAllCollected()
        {
            EndLevel(won: true);
        }

        /// <summary>
        /// Callback invoked when the player runs out of time.
        /// Now treated as level completion.
        /// </summary>
        private void OnTimeOut()
        {
            EndLevel(won: true); // ✅ Treat timer completion as win
        }

        /// <summary>
        /// Ends the current level.
        /// </summary>
        public void EndLevel(bool won)
        {
            if (isGameOver)
                return;

            isGameOver = true;

            var audioClip = won ? successSFX : failSFX;

            if (audioSource != null && audioClip != null)
            {
                audioSource.PlayOneShot(audioClip);
            }

            var vehicle = player.GetComponent<Vehicle>();

            vehicle.CanMove = false;
            vehicle.Rigidbody.linearDamping = 2.0f;

            if (won)
            {
                OnComplete?.Invoke();
            }
            else
            {
                OnFail?.Invoke();
            }
        }
    }
}
