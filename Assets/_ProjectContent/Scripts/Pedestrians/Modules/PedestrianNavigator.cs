using UnityDevKit.Events;
using UnityEngine;

namespace AdaptiveTrafficSystem.Pedestrians.Modules
{
    [RequireComponent(typeof(Pedestrian))]
    [RequireComponent(typeof(PedestrianMovement))]
    public class PedestrianNavigator : MonoBehaviour, IPedestrianModule
    {
        public EventHolder<PedestrianNavigator> OnDestinationReached { get; } = new EventHolder<PedestrianNavigator>();

        private PedestrianMovement _movement;

        private bool _isInitialized;

        private void Awake()
        {
            _movement = GetComponent<PedestrianMovement>();
        }

        private void Update()
        {
            const int frameFilterCount = 30;
            if (Time.frameCount % frameFilterCount != 0 || !_isInitialized) return;

            if (_movement.IsOnDestination)
            {
                EndUpNavigation();
            }
        }

        public void Init()
        {
            _isInitialized = true;
        }

        public void NavigateToPoint(Vector3 targetPoint)
        {
            _movement.GoTo(targetPoint);
        }

        private void EndUpNavigation()
        {
            OnDestinationReached.Invoke(this);
        }
    }
}