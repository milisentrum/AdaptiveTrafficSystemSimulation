using MyBox;
using TrafficModule.Waypoints;
using UnityDevKit.Optimization;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    [RequireComponent(typeof(VehicleNavigator))]
    [RequireComponent(typeof(VehicleAccidentHandler))]
    [RequireComponent(typeof(VehicleStoppingState))]
    [RequireComponent(typeof(BoxCollider))]
    public class VehicleMovementAI : CachedMonoBehaviour, IVehicleExtension
    {
        [SerializeField] private Transform sensorStartPos;
        [SerializeField] [PositiveValueOnly] private float increasedColliderModifier = 1.5f;

        private VehicleNavigator _navigator;
        private VehicleController _vehicleController;
        private VehicleMovement _movement;
        private VehicleAccidentHandler _accidentHandler;
        private VehicleStoppingState _stoppingState;

        private VehicleNavigator.Path _navigatorPath;
        private Waypoint _lastWaypointSpeedUpdate;
        private BoxCollider _collider;
        private Vector3 _colliderBaseSize;
        private Vector3 _colliderIncreasedSize;

        private bool _isInitialized;

        #region Init

        protected override void Awake()
        {
            base.Awake();
            _vehicleController = GetComponent<VehicleController>();
            _collider = GetComponent<BoxCollider>();
            _colliderBaseSize = _collider.size;
            _colliderIncreasedSize = _colliderBaseSize * increasedColliderModifier;
        }

        public void Init()
        {
            _navigatorPath = _vehicleController.NavigatorPath;
            LoadExtensions();
            SubscribeOnAccidentEvents();
            SubscribeOnNavigatorEvents();
            _isInitialized = true;
        }

        private void LoadExtensions()
        {
            _navigator = _vehicleController.Navigator;
            _movement = _vehicleController.Movement;
            _accidentHandler = _vehicleController.AccidentHandler;
            _stoppingState = _vehicleController.StoppingStates;
        }

        private void SubscribeOnAccidentEvents()
        {
            _accidentHandler.OnTurnOver.AddListener(DestroyTransport);
            _accidentHandler.OnDeadLock.AddListener(DestroyTransport);
        }

        private void SubscribeOnNavigatorEvents()
        {
            _navigator.OnLeftLaneChangeStart.AddListener(IncreaseCollider);
            _navigator.OnRightLaneChangeStart.AddListener(IncreaseCollider);
            _navigator.OnLeftLaneChangeComplete.AddListener(DecreaseCollider);
            _navigator.OnRightChangeComplete.AddListener(DecreaseCollider);
        }

        #endregion

        #region Main Update

        private void Update()
        {
            if (!_isInitialized) return;

            // after that we check if we are close to Waypoint and we need to update speed and rotate wheels
            if (_navigatorPath.HasPath)
            {
                _movement.WheelsRotation(_navigatorPath.CurrentWaypoint.SelfPosition);

                if (_movement.CalculateDistantView() > Vector3.Distance(
                        sensorStartPos.position,
                        _navigatorPath.CurrentWaypoint.SelfPosition))
                {
                    UpdateSpeed();
                }
                else if (_navigatorPath.CurrentWaypoint != _lastWaypointSpeedUpdate)
                {
                    // this check is necessary cuz when Sensor becomes smaller speed can be set back to maxSpeed
                    // and cant be updated in UpdateSpeed func cuz of "if" statement
                    _movement.SetDefaultMaxSpeed();
                }
            }

            DriveControl();
        }

        #endregion

        #region Movement control

        private void DriveControl()
        {
            var rpmSpeed = _navigatorPath.HasPath ? _movement.GetRpmSpeed() : 0f;

            foreach (var wheelCollider in _movement.PowerWheels)
            {
                if (wheelCollider.rpm < rpmSpeed && _navigatorPath.HasPath &&
                    !_stoppingState._stoppingInfo.NeedExtremeStop)
                {
                    //wheelCollider.motorTorque = _movement.MotorPower;
                    _movement.SetMotorTorqueForWheel(wheelCollider, _movement.MotorPower);
                    wheelCollider.brakeTorque = 0;
                }
                // else if (_states._stoppingInfo.NeedStopByCrossing && !_states._stoppingInfo.NeedExtremeStop)
                // {
                //     wheelCollider.motorTorque = _movement.MotorPower / 3f;
                //     wheelCollider.brakeTorque = _movement.BrakePower / 3f;
                //     Debug.LogWarning($"Low power {gameObject.name}");
                // }
                else
                {
                    //_movement.FullBrakeTorque();
                    wheelCollider.motorTorque = 0;
                    wheelCollider.brakeTorque = _movement.BrakePower;
                }
            }
        }

        // Update waypoints from WaypointNavigator

        // Update speed near waypoint if doesnt update it here before
        // also uses _currentMaxSpeed and maxSpeed vars to avoid troubles with real maxSpeed
        private void UpdateSpeed()
        {
            if (!_navigatorPath.HasPath ||
                _lastWaypointSpeedUpdate == _navigatorPath.CurrentWaypoint ||
                _navigatorPath.FuturePoints.Count <= 0) return;

            var lowestSpeed = VehicleMovement.GetMaxAllowedSpeed();
            var speed = _navigatorPath.CurrentWaypoint.GetWaypointSpeed(
                _navigatorPath.PreviousWaypoint,
                _navigatorPath.FuturePoints[0]);
            var lastWaypointSpeedUpdate = _navigatorPath.CurrentWaypoint; // TODO change here
            if (speed < lowestSpeed && speed != 0)
            {
                lowestSpeed = speed;
                lastWaypointSpeedUpdate = _navigatorPath.CurrentWaypoint; // TODO -- remove?
            }

            if (_navigatorPath.FuturePoints.Count > 1)
            {
                speed = _navigatorPath.FuturePoints[0]
                    .GetWaypointSpeed(_navigatorPath.CurrentWaypoint, _navigatorPath.FuturePoints[1]);
            }

            if (speed < lowestSpeed && speed != 0)
            {
                lowestSpeed = speed;
                lastWaypointSpeedUpdate = _navigatorPath.FuturePoints[0];
            }

            for (var i = 1; i < _navigatorPath.FuturePoints.Count - 2; i++)
            {
                speed = _navigatorPath.FuturePoints[i]
                    .GetWaypointSpeed(_navigatorPath.FuturePoints[i - 1], _navigatorPath.FuturePoints[i + 1]);

                if (speed < lowestSpeed && speed != 0)
                {
                    lowestSpeed = speed;
                    lastWaypointSpeedUpdate = _navigatorPath.FuturePoints[i];
                }
            }

            if (lowestSpeed <= _movement.MaxSpeed)
            {
                _movement.SetCurrentMaxSpeed(lowestSpeed);
                _lastWaypointSpeedUpdate = lastWaypointSpeedUpdate;
            }
            else
            {
                _movement.SetDefaultMaxSpeed();
            }
        }

        #endregion

        #region Actions

        private void DestroyTransport()
        {
            _vehicleController.DestroyTransport();
        }

        private void IncreaseCollider()
        {
            _collider.size = _colliderIncreasedSize;
        }

        private void DecreaseCollider()
        {
            _collider.size = _colliderBaseSize;
        }

        #endregion
    }
}