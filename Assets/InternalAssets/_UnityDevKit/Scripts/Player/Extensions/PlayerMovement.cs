using MyBox;
using UnityEngine;

namespace UnityDevKit.Player.Extensions
{
    [DisallowMultipleComponent]
    public class PlayerMovement : PlayerExtension
    {
        [SerializeField] private CharacterController controller;

        [SerializeField] [PositiveValueOnly] private float speed = 12f;
        [SerializeField] [PositiveValueOnly] private float maxRunModifier = 1.75f;
        [SerializeField] [PositiveValueOnly] private float acceleratingDelta = 0.5f;
        [SerializeField] [PositiveValueOnly] private float slowDownDelta = 1f;

        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        private Vector3 velocity;

        private bool isGrounded;

        public float RunModifier { get; private set; } = DefaultRunModifier;

        private const float DefaultRunModifier = 1f;

        protected override void Update()
        {
            base.Update();

            HandleMovement();
            HandleGravity();
        }

        private void HandleMovement()
        {
            var x = Input.GetAxis("Horizontal");
            var z = Input.GetAxis("Vertical");

            HandleRunModifier();

            var move = CachedTransform.right * x + CachedTransform.forward * z;
            var normalizedMove = NormalizeMoveDelta(move) * RunModifier;

            controller.Move(normalizedMove * (speed * Time.deltaTime));
        }

        private void HandleRunModifier()
        {
            if (Input.GetKey(KeyCode.LeftShift) && RunModifier <= maxRunModifier)
            {
                RunModifier = Mathf.Min(RunModifier + acceleratingDelta * Time.deltaTime, maxRunModifier);
            }
            else if (RunModifier > DefaultRunModifier)
            {
                RunModifier = Mathf.Max(RunModifier - slowDownDelta * Time.deltaTime, DefaultRunModifier);
            }
        }

        private Vector3 NormalizeMoveDelta(Vector3 moveDelta)
        {
            var magnitude = moveDelta.magnitude;
            return magnitude > 1f ? moveDelta / magnitude : moveDelta;
        }

        private void HandleGravity()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
                return;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}