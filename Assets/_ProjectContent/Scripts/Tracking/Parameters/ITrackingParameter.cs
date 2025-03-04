using System.Globalization;

namespace AdaptiveTrafficSystem.Tracking.Parameters
{
    public interface ITrackingParameter
    {
        float GetValue();
        string GetName();

        public string LogString() => $"{GetName()}: {GetValue().ToString(CultureInfo.InvariantCulture)}";
    }
}