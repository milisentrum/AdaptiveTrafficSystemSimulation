using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class AverageWaitingTimeParameter : BaseJamParameter
    {
        public override float GetValue() => jamHandler.IsEmptyStorage
            ? 0
            : jamHandler.GetWaitingVehiclesData().Average(vehicleData => vehicleData.WaitingTime);

        public override string GetName() => "Average waiting time";
    }
}