using UnityEngine;

namespace AdaptiveTrafficSystem.Pedestrians.Modules
{
    [RequireComponent(typeof(Pedestrian))]
    [RequireComponent(typeof(PedestrianMovement))]
    public class PedestrianAnimation : MonoBehaviour, IPedestrianModule
    {
        [SerializeField] private Animator animator;

        private PedestrianMovement _movement;

        private static readonly int Move = Animator.StringToHash("move");
        private static readonly int VelX = Animator.StringToHash("velX");
        private static readonly int VelY = Animator.StringToHash("velY");

        private void Awake()
        {
            _movement = GetComponent<PedestrianMovement>();
        }

        public void Init()
        {
        }

        private void LateUpdate()
        {
            animator.SetBool(Move, _movement.IsMoving);
            animator.SetFloat(VelX, _movement.Velocity.x);
            animator.SetFloat(VelY, _movement.Velocity.y);
        }
    }
}