using AdaptiveTrafficSystem.Paths;
using MyBox;
using UnityEngine;
using UnityEngine.AI;

namespace AdaptiveTrafficSystem.Crossing
{
    [RequireComponent(typeof(Crossing))]
    public class CrossingEditorBuilder : MonoBehaviour
    {
        [Header("Editor build settings")] 
        [SerializeField] private float length = 5f;
        [SerializeField] private CrossingBuilder.ColorScheme colorScheme;

#if UNITY_EDITOR
        [ButtonMethod]
        public string BuildCrossing()
        {
            var data = new RoadData {Width = length};
            var crossingBuilder = GetComponent<CrossingBuilder>();
            crossingBuilder.Build(data, colorScheme);
            NavMeshSetup(data, crossingBuilder.Parameters);
            return $"Build {colorScheme} crossing with {length} length";
        }
#endif

        private void NavMeshSetup(RoadData data, CrossingLinesParameters crossingParameters)
        {
            const float xSizeModifier = 1.2f;
            const float zSizeModifier = 1.1f;
            const float defaultYSize = 5f;
            const float zOffset = 0.5f;
            
            var navMeshModifier = GetComponent<NavMeshModifierVolume>();
            var navMeshObstacle = GetComponent<NavMeshObstacle>();
            
            var navSize = new Vector3(
                data.Width * xSizeModifier,
                defaultYSize,
                crossingParameters.Width * zSizeModifier);
            navMeshModifier.size = navSize;
            navMeshObstacle.size = navSize;

            var centerOffset = new Vector3(data.Width / 2f, 0, zOffset);
            navMeshModifier.center = centerOffset;
            navMeshObstacle.center = centerOffset;
        }
    }
}