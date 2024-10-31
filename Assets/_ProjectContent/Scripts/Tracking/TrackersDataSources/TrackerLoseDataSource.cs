using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace AdaptiveTrafficSystem.Tracking
{
    public class TrackerLoseDataSource : TrackerDataSourceBase
    {
        public override IEnumerable<UnityEvent<GameObject>> GetDataInputEvent() =>
            trackers.Select(tracker => tracker.OnLoseEvent);
    }
}