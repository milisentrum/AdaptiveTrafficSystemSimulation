using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class MaxWaitingTimeParameter : BaseJamParameter
    {
        public override float GetValue() => jamHandler.IsEmptyStorage
            ? 0
            : jamHandler.GetWaitingVehiclesData().Max(vehicleData => vehicleData.WaitingTime);

        public override string GetName() => "Max waiting time";
    }
}