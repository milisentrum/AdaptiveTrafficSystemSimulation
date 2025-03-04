using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class MinSpeedParameter : BaseSpeedParameter
    {
        public override float GetValue() =>
            speedParametersStorage.IsEmptyStorage ? 0 : speedParametersStorage.Storage.Min();

        public override string GetName() => "Min speed";
    }
}