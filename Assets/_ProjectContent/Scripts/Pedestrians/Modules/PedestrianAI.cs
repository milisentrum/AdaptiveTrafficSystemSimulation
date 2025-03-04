using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Pedestrians.Modules
{
    [RequireComponent(typeof(Pedestrian))]
    [RequireComponent(typeof(PedestrianStates))]
    [RequireComponent(typeof(PedestrianMovement))]
    public class PedestrianAI : MonoBehaviour, IPedestrianModule
    {
        [SerializeField] private bool isDebug = true;
        [SerializeField] [ConditionalField(nameof(isDebug))] private GameObject crossingFlag;
        [SerializeField] [ConditionalField(nameof(isDebug))] private GameObject closedWayFlag;
        [SerializeField] [ConditionalField(nameof(isDebug))] private GameObject hurryUpFlag;

        private bool _isCrossing;
        private bool _isWaiting;

        private PedestrianStates _states;
        private PedestrianMovement _movement;
        
        private const string CROSSING_ZONE = "CrossingZone";

        public void Init()
        {
            _states = GetComponent<PedestrianStates>();
            _movement = GetComponent<PedestrianMovement>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(CROSSING_ZONE)) return;
            var crossing = other.GetComponentInParent<Crossing.Crossing>();
            if (crossing.IsClosed)
            {
                Wait();
                crossing.AddWaitingPedestrian(_states);
            }
            else
            {
                StartCrossing(crossing);
                if (isDebug)
                {
                    crossingFlag.SetActive(true);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(CROSSING_ZONE)) return;
            var crossing = other.GetComponentInParent<Crossing.Crossing>();

            if (!crossing.IsClosed && !_isCrossing)
            {
                Accelerate();
                StartCrossing(crossing);
            }

            if (crossing.IsClosed && _isCrossing)
            {
                HurryUp();
            }

            if (isDebug)
            {
                crossingFlag.SetActive(!crossing.IsClosed);
                closedWayFlag.SetActive(crossing.IsClosed);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(CROSSING_ZONE)) return;
            var crossing = other.GetComponentInParent<Crossing.Crossing>();
            if (!_isCrossing)
            {
                Accelerate();
            }
            else
            {
                EndUpCrossing(crossing);
            }
          
            if (isDebug)
            {
                crossingFlag.SetActive(false);
                closedWayFlag.SetActive(false);
            }
        }
        
        private void Wait()
        {
            _movement.SlowDown();
            _isWaiting = true;
        }

        private void Accelerate()
        {
            _movement.Accelerate();
        }
        
        private void HurryUp()
        {
            _movement.HurryUp();
            if (isDebug)
            {
                hurryUpFlag.SetActive(true);
            }
        }

        private void SetNormalSpeed()
        {
            _movement.SetNormalSpeed();
            if (isDebug)
            {
                hurryUpFlag.SetActive(false);
            }
        }

        private void StartCrossing(Crossing.Crossing crossing)
        {
            if (_isWaiting)
            {
                crossing.RemoveWaitingPedestrian(_states);
                _isWaiting = false;
            }
            
            crossing.AddCrossingPedestrian(_states);
            _isCrossing = true;
        }

        private void EndUpCrossing(Crossing.Crossing crossing)
        {
            SetNormalSpeed();
            crossing.RemoveCrossingPedestrian(_states);
            _isCrossing = false;
        }
    }
}