using UnityEngine;

namespace UnityDevKit.Player.Extensions
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerFootstepsSound : PlayerExtension
    {
        [Header("Main settings")]
        [SerializeField] private AudioClip[] walkSounds;
        [SerializeField] private float footstepDelay = 0.45f;
        [SerializeField] private float volumeScale = 0.7f;

        [SerializeField] private PlayerMovement playerMovement;

        private AudioSource audioSource;
        private float nextFootstep;

        private float footStepDelayError;
        private float volumeScaleError;

        private const float FootStepDelayMAXError = 0.2f;
        private const float VolumeScaleMAXError = 0.1f;

        protected override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
        }

        protected override void Start()
        {
            base.Start();
            Init();
        }

        private void Init()
        {
            footStepDelayError = footstepDelay * FootStepDelayMAXError;
            volumeScaleError = volumeScale * VolumeScaleMAXError;
        }

        protected override void Update()
        {
            base.Update();
            nextFootstep -= Time.deltaTime;
            if (nextFootstep > 0) return;
            if (Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.W))
            {
                audioSource.PlayOneShot(RandomWalkSound, RandomVolumeScale);
                nextFootstep = RandomStepDelay / playerMovement.RunModifier;
            }
        }

        private AudioClip RandomWalkSound => walkSounds[Random.Range(0, walkSounds.Length)];

        private float RandomVolumeScale => volumeScale + volumeScaleError * Random.Range(-1f, 1f);

        private float RandomStepDelay => footstepDelay + footStepDelayError * Random.Range(-1f, 1f);
    }
}