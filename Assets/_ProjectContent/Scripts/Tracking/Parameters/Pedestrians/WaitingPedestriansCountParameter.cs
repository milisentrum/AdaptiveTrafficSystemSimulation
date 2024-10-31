using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class WaitingPedestriansCountParameter : MonoBehaviour, ITrackingParameter
    {
        [SerializeField] private Crossing.Crossing crossing;
        
        public float GetValue() => crossing.WaitingPedestriansCount;

        public string GetName() => "Waiting pedestrians count";
    }
}