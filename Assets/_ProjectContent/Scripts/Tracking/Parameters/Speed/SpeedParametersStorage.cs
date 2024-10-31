using System.Collections.Generic;
using MyBox;
using TrafficModule.Vehicle.Extensions;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class SpeedParametersStorage : MonoBehaviour
    {
        [Separator("Data")] 
        [SerializeField] private TrackerDataSourceBase dataSource;

        [Separator("Storage settings")] 
        [SerializeField] private bool clearByCapacity;
        [SerializeField] [ConditionalField(nameof(clearByCapacity))] private int maxCapacity = 100;

        [SerializeField] private bool clearByTime = false;
        [SerializeField] [ConditionalField(nameof(clearByTime))] private float storingTime = 180;

        public Queue<float> Storage { get; private set; }

        public bool IsEmptyStorage => Storage.Count == 0;

        private void Start()
        {
            Init();
            Track();
        }
        
        public void Track()
        {
            dataSource.OnDataTracked.AddListener(HandleData);
        }

        private void Init()
        {
            Storage = clearByCapacity
                ? new Queue<float>(maxCapacity)
                : new Queue<float>();
        }

        private void HandleData(GameObject trackedObject)
        {
            var vehicleMovement = trackedObject.GetComponent<VehicleMovement>();
            var speed = vehicleMovement.currentSpeed;
            Storage.Enqueue(speed);

            HandleStorage();
        }

        private void HandleStorage()
        {
            if (clearByCapacity && Storage.Count >= maxCapacity)
            {
                ClearStorage();
            }

            if (clearByTime)
            {
                Invoke(nameof(ClearStorage), storingTime);
            }
        }
        
        private void ClearStorage()
        {
            Storage.Dequeue();
        }
    }
}