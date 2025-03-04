using AdaptiveTrafficSystem.NavMesh;
using AdaptiveTrafficSystem.Paths;
using UnityEngine;
using UnityEngine.AI;

namespace AdaptiveTrafficSystem.Crossing
{
    [RequireComponent(typeof(NavMeshModifierVolume))]
    public class CrossingNavMesh : MonoBehaviour
    {
        private NavMeshModifierVolume _navMeshModifier;

        private void Awake()
        {
            _navMeshModifier = GetComponent<NavMeshModifierVolume>();
        }

        public void Setup(RoadData data, CrossingLinesParameters crossingParameters)
        {
            const float xSizeModifier = 1.2f;
            const float zSizeModifier = 1.1f;
            const float defaultYSize = 5f;
            const float zOffset = 0.5f;
            
            var navSize = new Vector3(
                data.Width * xSizeModifier,
                defaultYSize,
                crossingParameters.Width * zSizeModifier);
            _navMeshModifier.size = navSize;

            var centerOffset = new Vector3(data.Width / 2f, 0, zOffset);
            _navMeshModifier.center = centerOffset;

            NavMeshUpdater.Instance.UpdateNavMesh();
        }
    }
}