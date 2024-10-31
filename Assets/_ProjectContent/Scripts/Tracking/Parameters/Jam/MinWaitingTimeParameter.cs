using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class MinWaitingTimeParameter : BaseJamParameter
    {
        public override float GetValue() => jamHandler.IsEmptyStorage
            ? 0
            : jamHandler.GetWaitingVehiclesData().Min(vehicleData => vehicleData.WaitingTime);

        public override string GetName() => "Min waiting time";
    }
}