using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Filters
{
    public interface IFilter
    {
        GameObject Filter(GameObject trackedObject);
    }
}