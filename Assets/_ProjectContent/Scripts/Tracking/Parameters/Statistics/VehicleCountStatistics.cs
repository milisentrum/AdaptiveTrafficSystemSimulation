using System.Collections.Generic;
using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters.Statistics
{
    public class VehicleCountStatistics : Statistics<VehicleCountParameter>
    {
        protected override float ProcessData(IEnumerable<float> data)
        {
            return data.Average();
        }
    }
}