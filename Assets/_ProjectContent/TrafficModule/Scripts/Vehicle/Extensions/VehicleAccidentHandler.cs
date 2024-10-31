using UnityDevKit.Events;
using UnityDevKit.Optimization;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleAccidentHandler : CachedMonoBehaviour, IVehicleExtension
    {
        public readonly EventHolderBase OnTurnOver = new EventHolderBase();
        public readonly EventHolderBase OnDeadLock = new EventHolderBase();

        private VehicleController _controller;
        private VehicleMovement _movement;
        private VehicleTrafficLighterHandler _vehicleTrafficLighterHandler;
        private VehicleStoppingState _stoppingStates;
        
        private TurnoverState _turnoverState;
        private DeadLockState _deadLockState;

        private Vector3 _deadlockPosition;
        private int _deadlockCheckFrames;

        private enum TurnoverState
        {
            OK,
            TURNOVER
        }

        private enum DeadLockState
        {
            OK,
            LOCKED
        }

        protected override void Awake()
        {
            base.Awake();
            _controller = GetComponent<VehicleController>();
        }

        public void Init()
        {
            const int deadlockBaseCheckFrames = 3 * 59;
            const int deadlockDeltaCheckFrames = 30;
            LoadExtensions();
            
            _turnoverState = TurnoverState.OK;
            _deadLockState = DeadLockState.OK;
            _deadlockCheckFrames = deadlockBaseCheckFrames + Random.Range(-deadlockDeltaCheckFrames, deadlockDeltaCheckFrames);
        }
        
        private void LoadExtensions()
        {
            _movement = _controller.Movement;
            _vehicleTrafficLighterHandler = _controller.VehicleTrafficLighters;
            _stoppingStates = _controller.StoppingStates;
        }

        private void CheckForTurnover()
        {
            const float maxTurnoverAngle = 45f;
            var angle = Vector3.SignedAngle(transform.up, Vector3.up, Vector3.up);
            if (angle > maxTurnoverAngle)
            {
                switch (_turnoverState)
                {
                    case TurnoverState.OK:
                        _turnoverState = TurnoverState.TURNOVER;
                        break;
                    case TurnoverState.TURNOVER:
                        OnTurnOver.Invoke();
                        break;
                }
            }
            else
            {
                _turnoverState = TurnoverState.OK;
            }
        }
        
        private void CheckForDeadLock()
        {
            const float deaLockAllowedTransitionDistance = 0.5f;
            const float deaLockAllowedSpeed = 0.9f;

            if (!_vehicleTrafficLighterHandler.IsCrossing) return;

            if (_vehicleTrafficLighterHandler.IsInRedState ||
                _stoppingStates.IsInJam ||
                _stoppingStates._stoppingInfo.NeedStopByCrossing ||
                _stoppingStates._stoppingInfo.NeedStopByPedestrian ||
                _movement.currentSpeed > deaLockAllowedSpeed
               )
            {
                _deadLockState = DeadLockState.OK;
            }
            else if (_deadLockState == DeadLockState.OK)
            {
                _deadlockPosition = SelfPosition;
                _deadLockState = DeadLockState.LOCKED;
            }
            else if (_deadLockState == DeadLockState.LOCKED)
            {
                if (Vector3.Distance(_deadlockPosition, SelfPosition) > deaLockAllowedTransitionDistance)
                {
                    _deadLockState = DeadLockState.OK;
                }
                else
                {
                    OnDeadLock.Invoke();
                }
            }
        }
        
        private void StateCheck()
        {
            const int turnOverCheckFrames = 4 * 59;
            if (Time.frameCount % turnOverCheckFrames == 0)
            {
                CheckForTurnover();
            }

            if (Time.frameCount % _deadlockCheckFrames == 0)
            {
                CheckForDeadLock();
            }
        }

        private void Update()
        {
            StateCheck();
        }
    }
}