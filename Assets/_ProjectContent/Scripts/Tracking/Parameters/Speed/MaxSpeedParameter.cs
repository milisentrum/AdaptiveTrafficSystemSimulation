using System.Linq;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public class MaxSpeedParameter : BaseSpeedParameter
    {
        public override float GetValue() =>
            speedParametersStorage.IsEmptyStorage ? 0 : speedParametersStorage.Storage.Max();

        public override string GetName() => "Max speed";
    }
}