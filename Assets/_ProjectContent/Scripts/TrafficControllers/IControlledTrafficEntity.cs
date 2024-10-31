namespace AdaptiveTrafficSystem.TrafficControllers
{
    public interface IControlledEntity
    {
        void OnTrafficOpen();
        void OnTrafficClose();
    }
}