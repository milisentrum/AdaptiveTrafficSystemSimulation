using AdaptiveTrafficSystem.TrafficControllers;

namespace AdaptiveTrafficSystem.Crossing
{
    public class ControlledCrossing : Crossing, IControlledEntity
    {
        public void OnTrafficOpen()
        {
            Close();
        }

        public void OnTrafficClose()
        {
            Open();
        }
    }
}