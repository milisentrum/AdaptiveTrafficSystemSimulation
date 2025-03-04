using System.Collections.Generic;
using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters.Statistics
{
    public class BandwidthStatistics : Statistics<BandwidthParameter>
    {
        protected override float ProcessData(IEnumerable<float> data)
        {
            return data.Average();
        }
    }
}