using AdaptiveTrafficSystem.Paths;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Crossing
{
    public class CrossingSetupTest : MonoBehaviour
    {
        [SerializeField] [MustBeAssigned] private Crossing crossing;
        [SerializeField] [PositiveValueOnly] private float width = 5;

        private void Start()
        {
            crossing.Setup(new RoadData {Width = width});
        }
    }
}