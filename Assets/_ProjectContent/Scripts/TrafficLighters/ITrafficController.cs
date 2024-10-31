namespace AdaptiveTrafficSystem.TrafficLighters
{
    public interface ITrafficController
    {
        void SwitchToOpen();
        void SwitchToClose();
        TrafficMode GetMode();
    }
}