using System.Linq;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class BandwidthParameter : MonoBehaviour, ITrackingParameter
    {
        [SerializeField] private IntensityParameter[] intensityParameters;

        private float _currentValue;

        public float GetValue() => intensityParameters.Sum(intensity => intensity.GetValue());

        public string GetName() => "Bandwidth";
    }
}