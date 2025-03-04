using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    [RequireComponent(typeof(JamHandler))]
    public abstract class BaseJamParameter : MonoBehaviour, ITrackingParameter
    {
        protected JamHandler jamHandler;

        private void Awake()
        {
            jamHandler = GetComponent<JamHandler>();
        }

        public abstract float GetValue();
        public abstract string GetName();
    }
}