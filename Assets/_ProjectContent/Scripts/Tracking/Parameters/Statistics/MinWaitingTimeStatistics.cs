using System.Collections.Generic;
using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters.Statistics
{
    public class MinWaitingTimeStatistics : Statistics<MinWaitingTimeParameter>
    {
        protected override float ProcessData(IEnumerable<float> data)
        {
            return data.Min();
        }
    }
}