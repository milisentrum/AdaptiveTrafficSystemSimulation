namespace AdaptiveTrafficSystem.Net.Websocket.Messages
{
    public class TrafficParametersDataPack : DataPack<DirectionInfo>
    {
    }

    public class DirectionInfo
    {
        public Direction Direction;
        public DirectionParameters Parameters;
    }

    public class DirectionParameters
    {
        public float Phase;
        public float Intensity;

        public float AvgSpeed;
        public float MinSpeed;
        public float MaxSpeed;

        public float AvgWaitingTime;
        public float MinWaitingTime;
        public float MaxWaitingTime;

        public float WaitingVehicles;
        public float ServiceIntensity;

        public float WaitingPedestriansCount;
        public float CrossingPedestriansCount;
    }
}