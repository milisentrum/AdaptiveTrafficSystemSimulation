using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Pedestrians.Data
{
    [CreateAssetMenu(fileName = "SpawningData", menuName = "Pedestrians/SpawningData", order = 0)]
    public class SpawningData : ScriptableObject
    {
        public GameObject[] Prototypes;
        [PositiveValueOnly] public int Amount = 25;
        [PositiveValueOnly] public float StartDelay = 1f;
        [PositiveValueOnly] public float Interval = 0.075f;
    }
}