using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    [RequireComponent(typeof(SpeedParametersStorage))]
    public abstract class BaseSpeedParameter : MonoBehaviour, ITrackingParameter
    {
        protected SpeedParametersStorage speedParametersStorage;

        private void Awake()
        {
            speedParametersStorage = GetComponent<SpeedParametersStorage>();
        }

        public abstract float GetValue();

        public abstract string GetName();
    }
}