using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Filters
{
    public class LayerFilter : MonoBehaviour, IFilter
    {
        [SerializeField] private bool useLayerFilter = true;
        [SerializeField] [ConditionalField(nameof(useLayerFilter))] private LayerMask detectionLayerMask;

        public GameObject Filter(GameObject trackedObject) =>
            IsValidDetection(trackedObject) ? trackedObject : null;

        private bool IsValidDetection(GameObject detectedObject) =>
            !useLayerFilter || detectionLayerMask == (detectionLayerMask | (1 << detectedObject.layer));
    }
}