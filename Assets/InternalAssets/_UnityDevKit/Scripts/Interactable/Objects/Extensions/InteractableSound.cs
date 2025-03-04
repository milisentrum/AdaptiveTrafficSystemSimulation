using MyBox;
using UnityEngine;

namespace UnityDevKit.Interactables.Extensions
{
    public class InteractableSound : InteractableExtension
    {
        [SerializeField] private bool useOnFocusSound;

        [SerializeField] [ConditionalField(nameof(useOnFocusSound))]
        private AudioSource onFocusAudioSource;

        [SerializeField] private bool useOnInteractSound;

        [SerializeField] [ConditionalField(nameof(useOnInteractSound))]
        private AudioSource onInteractAudioSource;

        protected override void OnFocusAction(InteractionBase source)
        {
            if (!useOnFocusSound) return;
            onFocusAudioSource.Play();
        }

        protected override void OnInteractAction(InteractionBase source)
        {
            if (!useOnInteractSound) return;
            onInteractAudioSource.Play();
        }
    }
}