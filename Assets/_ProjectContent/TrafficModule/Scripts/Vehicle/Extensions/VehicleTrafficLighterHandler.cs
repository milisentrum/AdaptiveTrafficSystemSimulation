using AdaptiveTrafficSystem.TrafficLighters;
using MyBox;
using UnityDevKit.Events;
using UnityDevKit.Optimization;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleTrafficLighterHandler : CachedMonoBehaviour, IVehicleExtension
    {
        [Separator("Traffic Lighters")] 
        [SerializeField] private string crossingBorderTag = "TrafficLighter";
        [SerializeField] private LightState lightState = LightState.GREEN;
        
        public LightState TrafficState => lightState;
        public bool IsCrossing => lightState == LightState.CROSSING;
        
        public readonly EventHolder<VehicleController> OnTrafficStateChange = new EventHolder<VehicleController>();

        private VehicleController _vehicleController;

        public enum LightState
        {
            RED,
            CROSSING,
            GREEN
        }

        public void Init()
        {
        }
        
        private void Start()
        {
            _vehicleController = GetComponent<VehicleController>();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(crossingBorderTag)) return;
            lightState = LightState.CROSSING;
        }

        // returns vehicle 'need stop'
        public bool HandleTrafficLight(TrafficLighter trafficLighter)
        {
            if (IsCrossing) return false;
            if (trafficLighter.GetMode() == TrafficMode.CLOSE)
            {
                ChangeToRedState();
                return true;
            }
            else
            {
                ChangeToGreenState();
                return false;
            }
        }

        public void ChangeToRedState()
        {
            lightState = LightState.RED;
            OnTrafficStateChange.Invoke(_vehicleController);
        }

        public void ChangeToGreenState()
        {
            lightState = LightState.GREEN;
            OnTrafficStateChange.Invoke(_vehicleController);
        }

        public bool IsInRedState => lightState is LightState.RED;
    }
}