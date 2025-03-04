using System.Collections;
using MyBox;
using UnityEngine;

namespace UnityDevKit.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AmbientSound : MonoBehaviour
    {
        [SerializeField] private RangedFloat minMaxValues = new RangedFloat(0, 1);

        private AudioSource audioSource;

        private Coroutine currentCoroutine;

        private const int VolumeChangeIterations = 10;
        private const float VolumeChangeTimeDelta = 0.1f;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void SetVolumeBounds(RangedFloat bounds)
        {
            minMaxValues = bounds;
        }

        public void SetVolume(float volume)
        {
            var normalizedVolume = NormalizeVolume(volume);

            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = StartCoroutine(SmoothVolumeChange(normalizedVolume));
        }

        public void Play()
        {
            audioSource.Play();
        }

        public void Stop()
        {
            audioSource.Stop();
        }

        public float GetClipDuration() => audioSource.clip.length;

        private IEnumerator SmoothVolumeChange(float targetVolume)
        {
            var currentVolume = audioSource.volume;
            var volumeChangeDelta = (targetVolume - currentVolume) / VolumeChangeIterations;

            for (var i = 0; i < VolumeChangeIterations; i++)
            {
                audioSource.volume += volumeChangeDelta;
                yield return new WaitForSeconds(VolumeChangeTimeDelta);
            }
        }

        private float NormalizeVolume(float volume) =>
            Mathf.Abs(volume - minMaxValues.Min) / (minMaxValues.Max - minMaxValues.Min);
    }
}