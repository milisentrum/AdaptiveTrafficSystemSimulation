using UnityEngine;

namespace AdaptiveTrafficSystem.Crossing
{
    [CreateAssetMenu(fileName = "CrossingLines", menuName = "Crossing/CrossingLines", order = 0)]
    public class CrossingLinesParameters : ScriptableObject
    {
        public float Step = 0.6f;
        public float Width = 5f;
    }
}