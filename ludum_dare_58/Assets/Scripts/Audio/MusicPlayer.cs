using UnityEngine;

namespace LudumDare58.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        AudioSource audioSource;

        public AudioSource AudioSource { get => this.audioSource; set => this.audioSource = value; }

        private void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
        }
    }
}