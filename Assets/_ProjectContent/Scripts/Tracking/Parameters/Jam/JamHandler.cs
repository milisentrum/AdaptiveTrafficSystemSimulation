using System.Collections.Generic;
using System.Linq;
using TrafficModule.Vehicle;
using TrafficModule.Vehicle.Extensions;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class JamHandler : MonoBehaviour
    {
        [SerializeField] private TrackerDataSourceBase detectedVehiclesSources;
        [SerializeField] private TrackerDataSourceBase lostVehiclesSources;

        private Dictionary<VehicleController, JamData> _trackedVehicles;

        private List<JamData> _waitingVehiclesData;

        public bool IsEmptyStorage => !GetWaitingVehiclesData().Any();
        
        public IEnumerable<JamData> GetWaitingVehiclesData() => _waitingVehiclesData
            .Where(vehicleData => vehicleData.WasStopped);

        public class JamData
        {
            public float InTime;
            public float OutTime;
            public float TransitionTime;

            public bool WasStopped;
            public bool IsWaiting;
            public float LastWaitStartTime;
            public float WaitingTime;
        }

        private void Start()
        {
            Init();
            Track();
        }

        private void Init()
        {
            _trackedVehicles = new Dictionary<VehicleController, JamData>();
            _waitingVehiclesData = new List<JamData>();
        }

        public void Track()
        {
            detectedVehiclesSources.OnDataTracked.AddListener(HandleDetectData);
            lostVehiclesSources.OnDataTracked.AddListener(HandleLoseData);
        }

        private void HandleDetectData(GameObject trackedObject)
        {
            var vehicleController = trackedObject.GetComponent<VehicleController>();
            if (_trackedVehicles.ContainsKey(vehicleController)) return;

            _trackedVehicles.Add(vehicleController, new JamData {InTime = Time.time});
            vehicleController.VehicleTrafficLighters.OnTrafficStateChange.AddListener(HandleVehicleJamStateChange);
        }

        private void HandleLoseData(GameObject trackedObject)
        {
            var vehicleController = trackedObject.GetComponent<VehicleController>();
            if (!_trackedVehicles.ContainsKey(vehicleController)) return;

            var vehicleJamData = _trackedVehicles[vehicleController];

            vehicleJamData.OutTime = Time.time;
            vehicleJamData.TransitionTime = vehicleJamData.OutTime - vehicleJamData.InTime;

            _waitingVehiclesData.Add(vehicleJamData);
            vehicleController.VehicleTrafficLighters.OnTrafficStateChange.RemoveListener(HandleVehicleJamStateChange);
        }

        private void HandleVehicleJamStateChange(VehicleController vehicleController)
        {
            var data = _trackedVehicles[vehicleController];
            if (vehicleController.VehicleTrafficLighters.TrafficState == VehicleTrafficLighterHandler.LightState.RED &&
                !data.IsWaiting)
            {
                data.LastWaitStartTime = Time.time;
                data.IsWaiting = true;
                data.WasStopped = true;
            }
            else if (data.IsWaiting)
            {
                data.WaitingTime += Time.time - data.LastWaitStartTime;
                data.IsWaiting = false;
            }
        }
    }
}