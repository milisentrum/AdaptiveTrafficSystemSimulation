using System.Linq;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Filters
{
    public class TagFilter : MonoBehaviour, IFilter
    {
        [SerializeField] private bool useAllowedTags = true;
        [SerializeField] [ConditionalField(nameof(useAllowedTags))] private string[] allowedTags;

        [SerializeField] private bool useRestrictedTags = false;
        [SerializeField] [ConditionalField(nameof(useRestrictedTags))] private string[] restrictedTags;

        public GameObject Filter(GameObject trackedObject) => IsValidDetection(trackedObject) ? trackedObject : null;

        private bool IsValidDetection(GameObject detectedObject) =>
            (!useAllowedTags || allowedTags.Contains(detectedObject.tag)) &&
            (!useRestrictedTags || !restrictedTags.Contains(detectedObject.tag));
    }
}