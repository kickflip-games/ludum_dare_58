using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace LudumDare58.Audio
{
    public class MusicManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] AudioMixerGroup audioMixerGroup;
        [SerializeField] AudioClip backgroundMusic = null;
        [SerializeField] bool playOnAwake = true;

        [Header("Bloom / Color Pulse Settings")]
        [SerializeField] Volume postProcessVolume;
        [SerializeField] float baseIntensity = 1f;
        [SerializeField] float maxIntensity = 6.5f;
        [SerializeField] float decaySpeed = 7f;
        [SerializeField] float beatSensitivity = 1.6f;   // lower = more reactive
        [SerializeField] float minBeatInterval = 0.9f;   // half-frequency beats
        [SerializeField] int sampleSize = 512;
        [SerializeField] FFTWindow fftWindow = FFTWindow.BlackmanHarris;

        [Header("Chromatic / Hue Options")]
        [SerializeField] bool enableColorPulse = true;
        [SerializeField] float chromaticMax = 0.6f;      // how far it splits colors
        [SerializeField] float hueShiftSpeed = 0.5f;     // slow rotation of hues

        private MusicPlayer musicPlayer;
        private AudioSource activeSource;
        private Bloom bloom;
        private ChromaticAberration chromatic;
        private ColorAdjustments colorAdj;

        private float[] spectrumData;
        private float prevEnergy = 0f;
        private float beatTimer = 0f;
        private int frameCounter = 0;

        private void Awake()
        {
            musicPlayer = FindOrCreateMusicPlayer();
            spectrumData = new float[sampleSize];
        }

        private void Start()
        {
            activeSource = musicPlayer.AudioSource;

            if (!activeSource.isPlaying || activeSource.clip != backgroundMusic)
            {
                if (playOnAwake)
                {
                    activeSource.clip = backgroundMusic;
                    activeSource.outputAudioMixerGroup = audioMixerGroup;
                    activeSource.loop = true;
                    activeSource.Play();
                }
            }

            if (postProcessVolume != null)
            {
                postProcessVolume.profile.TryGet(out bloom);
                postProcessVolume.profile.TryGet(out chromatic);
                postProcessVolume.profile.TryGet(out colorAdj);
            }

            Debug.Log($"[MusicManager] Using AudioSource '{activeSource.gameObject.name}' for beat analysis.");
        }

        private void Update()
        {
            if (activeSource == null || bloom == null)
                return;

            activeSource.GetSpectrumData(spectrumData, 0, fftWindow);

            float energy = 0f;
            int bassBins = Mathf.Min(80, spectrumData.Length);
            for (int i = 0; i < bassBins; i++)
                energy += spectrumData[i];
            energy /= bassBins;

            beatTimer += Time.deltaTime;

            if (energy > prevEnergy * beatSensitivity && beatTimer > minBeatInterval)
            {
                TriggerBloomPulse(energy);
                beatTimer = 0f;
            }

            prevEnergy = Mathf.Lerp(prevEnergy, energy, Time.deltaTime * 4f);

            // Smooth bloom fade
            bloom.intensity.value = Mathf.Lerp(bloom.intensity.value, baseIntensity, Time.deltaTime * decaySpeed * 2f);

            // Subtle rhythmic baseline pulse
            float bpm = 100f;
            float secondsPerBeat = 60f / bpm;
            float phase = Mathf.Sin((Time.time % secondsPerBeat) / secondsPerBeat * Mathf.PI * 2f);
            bloom.intensity.value += Mathf.InverseLerp(0f, 1f, phase) * 0.05f;

            // Slow hue rotation for ambience
            if (enableColorPulse && colorAdj != null)
                colorAdj.hueShift.value = Mathf.Sin(Time.time * hueShiftSpeed) * 30f;

            frameCounter++;
            // if (frameCounter % 15 == 0)
            //     Debug.Log($"[MusicManager] Bass Energy: {energy:E4} | Prev: {prevEnergy:E4}");
        }

        private void TriggerBloomPulse(float energy)
        {
            float normalized = Mathf.Clamp01((energy - prevEnergy) * 100f);
            float target = Mathf.Lerp(baseIntensity + 1.5f, maxIntensity, normalized);
            bloom.intensity.value = target;

            if (enableColorPulse && chromatic != null)
                StartCoroutine(ChromaticKick());

            StartCoroutine(BloomKick());
            // Debug.Log($"💥 Beat detected! Energy={energy:F5}, Bloom={target:F2}");
        }

        private IEnumerator BloomKick()
        {
            float t = 0f;
            float start = bloom.intensity.value;
            while (t < 0.25f)
            {
                t += Time.deltaTime * 2f;
                bloom.intensity.value = Mathf.Lerp(start, baseIntensity, t);
                yield return null;
            }
        }

        private IEnumerator ChromaticKick()
        {
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime * 4f;
                if (chromatic != null)
                    chromatic.intensity.value = Mathf.Lerp(chromaticMax, 0f, t);
                yield return null;
            }
        }

        private MusicPlayer FindOrCreateMusicPlayer()
        {
            musicPlayer = FindAnyObjectByType<MusicPlayer>();
            if (musicPlayer == null)
            {
                musicPlayer = new GameObject("Music Player").AddComponent<MusicPlayer>();
                DontDestroyOnLoad(musicPlayer);
            }
            return musicPlayer;
        }
    }
}
