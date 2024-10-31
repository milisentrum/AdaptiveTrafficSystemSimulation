using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class IntensityParameter : MonoBehaviour, ITrackingParameter
    {
        [SerializeField] private TrackerDataSourceBase tracker;

        private float _currentValue;

        private void Start()
        {
            Track();
        }

        public void Track()
        {
            tracker.OnDataTracked.AddListener(HandleLoseEvent);
        }

        private void HandleLoseEvent(GameObject lostObject)
        {
            const float trackedDynamicPeriod = 60; // tracking for last minute (Vehicles per minute) 
            _currentValue++;
            Invoke(nameof(Clear), trackedDynamicPeriod);
        }

        private void Clear()
        {
            _currentValue--;
        }

        public float GetValue()
        {
            return _currentValue;
        }

        public string GetName() => "Intensity";
    }
}