using System;
using AdaptiveTrafficSystem.Crossing;
using AdaptiveTrafficSystem.TrafficLighters;
using UnityDevKit.Optimization;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    [RequireComponent(typeof(VehicleSensors))]
    [RequireComponent(typeof(VehicleTrafficLighterHandler))]
    [RequireComponent(typeof(VehiclePriority))]
    [RequireComponent(typeof(VehicleStoppingState))]
    [RequireComponent(typeof(Collider))]
    public class VehicleObstaclesAnalyzer : CachedMonoBehaviour, IVehicleExtension
    {
        [SerializeField] private DynamicFrameFilter dynamicFrameFilter;

        private VehicleController _vehicleController;
        private VehicleSensors _vehicleSensors;
        private VehicleTrafficLighterHandler _vehicleTrafficLighterHandler;
        private VehiclePriority _priority;
        private VehicleStoppingState _stoppingState;

        private Collider _collider;

        private bool _isInitialized;

        protected override void Awake()
        {
            base.Awake();
            _vehicleController = GetComponent<VehicleController>();
            _collider = GetComponent<Collider>();
        }

        public void Init()
        {
            LoadExtensions();
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized) return;
            if (dynamicFrameFilter.IsFilteredFrame())
            {
                AnalyzeDetectedObjects();
                _vehicleSensors.ClearFrontObjects();
            }
        }

        private void LoadExtensions()
        {
            _vehicleSensors = _vehicleController.Sensors;
            _vehicleTrafficLighterHandler = _vehicleController.VehicleTrafficLighters;
            _priority = _vehicleController.Priority;
            _stoppingState = _vehicleController.StoppingStates;
        }

        #region ANALYZE

        private void AnalyzeDetectedObjects()
        {
            foreach (var detectedObject in _vehicleSensors.frontObjects)
            {
                switch (detectedObject.DetectedObjectType)
                {
                    case VehicleSensors.DetectedObjectType.VEHICLE:
                        HandleVehicle(detectedObject);
                        break;
                    case VehicleSensors.DetectedObjectType.TRAFFIC_LIGHT:
                        HandleTrafficLight(detectedObject.DetectedGameObject);
                        break;
                    case VehicleSensors.DetectedObjectType.CROSSING:
                        HandleCrossing(detectedObject.DetectedGameObject);
                        break;
                    case VehicleSensors.DetectedObjectType.HUMAN:
                        HandlePedestrian(detectedObject.DetectedGameObject);
                        break;
                    case VehicleSensors.DetectedObjectType.UNRECOGNIZED:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (_vehicleSensors.frontObjects.Count == 0)
            {
                _stoppingState._stoppingInfo.Reset();
                dynamicFrameFilter.Increase();
            }
            else
            {
                dynamicFrameFilter.SetToMin();
            }
        }

        private void HandleVehicle(VehicleSensors.DetectedObject detectedObject)
        {
            _stoppingState._stoppingInfo.NeedStopByVehicle = true;
            var detectedGameObject = detectedObject.DetectedGameObject;

            if (detectedGameObject == null) return; // if vehicle was destroyed
            var vehicleController = detectedGameObject.GetComponent<VehicleController>();

            if (vehicleController.StoppingStates._stoppingInfo.NeedStopByPedestrian)
            {
                _stoppingState._stoppingInfo.NeedStopByPedestrian = true;
                Debug.Log("Jam by pedestrians");
            }

            if (_vehicleTrafficLighterHandler.IsCrossing)
            {
                if (detectedObject.DetectionDirection is VehicleSensors.DetectionDirection.LEFT
                    or VehicleSensors.DetectionDirection.RIGHT)
                {
                    // TODO
                    _stoppingState._stoppingInfo.HasHighPriority = false;
                    // const float safeDistance = 3f;
                    // var hasHighPriority = _priority.HasHigherPriorityThen(vehicleController.Priority);
                    // _stoppingState._stoppingInfo.HasHighPriority = hasHighPriority && Vector3.Distance(
                    //     SelfPosition, 
                    //     detectedGameObject.transform.position) > safeDistance;
                }

                return;
            }

            if (vehicleController.VehicleTrafficLighters.IsInRedState)
            {
                _vehicleTrafficLighterHandler.ChangeToRedState();
                _stoppingState.IsInJam = true;
            }
            else if (vehicleController.VehicleTrafficLighters.IsCrossing)
            {
                _stoppingState.IsInJam = false;
            }
            else if (_vehicleTrafficLighterHandler.IsInRedState)
            {
                _vehicleTrafficLighterHandler.ChangeToGreenState();
            }
        }

        private void HandleTrafficLight(GameObject detectedObject)
        {
            var trafficLighter = detectedObject.GetComponent<TrafficLighter>();
            _stoppingState._stoppingInfo.NeedStopByTrafficLight =
                _vehicleTrafficLighterHandler.HandleTrafficLight(trafficLighter);
            _stoppingState.IsInJam = false;
        }

        private void HandleCrossing(GameObject detectedObject)
        {
            var crossing = detectedObject.GetComponentInParent<Crossing>();
            _stoppingState._stoppingInfo.NeedStopByCrossing = crossing.HasManyPedestrians; // crossing.HasPedestrians;

            if (!crossing.IsClosed) // IsClosed for vehicles
            {
                var detectedCollider = detectedObject.GetComponent<Collider>();
                if (detectedCollider.bounds.Intersects(_collider.bounds))
                {
                    Debug.LogWarning("Vehicle on crossing!!!");
                    _stoppingState._stoppingInfo.NeedStopByCrossing = false;
                }
            }
        }

        private void HandlePedestrian(GameObject detectedObject)
        {
            _stoppingState._stoppingInfo.NeedStopByPedestrian = true;
        }

        #endregion
    }
}