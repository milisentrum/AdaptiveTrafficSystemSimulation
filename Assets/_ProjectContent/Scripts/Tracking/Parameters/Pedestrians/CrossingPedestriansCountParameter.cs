using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class CrossingPedestriansCountParameter : MonoBehaviour, ITrackingParameter
    {
        [SerializeField] private Crossing.Crossing crossing;
        
        public float GetValue() => crossing.CrossingPedestriansCount;

        public string GetName() => "Crossing pedestrians count";
    }
}