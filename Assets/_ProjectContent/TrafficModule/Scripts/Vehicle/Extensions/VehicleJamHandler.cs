using MyBox;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleJamHandler : MonoBehaviour, IVehicleExtension
    {
        [ReadOnly] public float WaitingTime;

        public bool IsTracking { get; private set; } = false;
        public static int JamLength { get; private set; } = 0;
        public static bool IsActivated;

        private VehicleController _controller;

        private void Awake()
        {
            _controller = GetComponent<VehicleController>();
        }

        public void Init()
        {
            _controller.VehicleTrafficLighters.OnTrafficStateChange.AddListener(TrafficRedStateTrack);
        }

        public void OnTriggerExit(Collider other)
        {
            const string trafficLighterTag = "TrafficLighter";
            
            if (!other.CompareTag(trafficLighterTag)) return;

            if (IsTracking)
            {
                Untrack();
            }
        }

        private void TrafficRedStateTrack(VehicleController vehicleController)
        {
            if (vehicleController.VehicleTrafficLighters.IsInRedState)
            {
                Track();
            }
        }

        public void Track()
        {
            const float trackingSaveDelay = 0.5f;
            
            if (IsTracking || !IsActivated) return;
            IsTracking = true;
            JamLength++;
            InvokeRepeating(nameof(SaveTrackedData), trackingSaveDelay, trackingSaveDelay);
        }

        public void Untrack()
        {
            if (!IsTracking || !IsActivated) return;
            SaveTrackedData();
            Reset();
            JamLength--;
            CancelInvoke();
        }

        public static void ClearJam()
        {
            JamLength = 0;
        }

        private void SaveTrackedData()
        {
            //ParametersManager.Instance.AddWaitingTiming(WaitingTime);
        }

        private void Reset()
        {
            IsTracking = false;
            WaitingTime = 0;
        }

        private void Update()
        {
            if (IsTracking)
            {
                WaitingTime += Time.deltaTime;
            }
        }
    }
}