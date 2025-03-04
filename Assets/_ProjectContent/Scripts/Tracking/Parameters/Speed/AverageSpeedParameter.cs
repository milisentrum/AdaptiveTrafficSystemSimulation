using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class AverageSpeedParameter : BaseSpeedParameter
    {
        public override float GetValue() =>
            speedParametersStorage.IsEmptyStorage ? 0 : speedParametersStorage.Storage.Average();

        public override string GetName() => "Average speed";
    }
}