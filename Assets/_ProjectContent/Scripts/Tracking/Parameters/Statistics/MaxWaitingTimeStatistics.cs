using System.Collections.Generic;
using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters.Statistics
{
    public class MaxWaitingTimeStatistics : Statistics<MaxWaitingTimeParameter>
    {
        protected override float ProcessData(IEnumerable<float> data)
        {
            return data.Max();
        }
    }
}