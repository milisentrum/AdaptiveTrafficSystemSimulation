namespace AdaptiveTrafficSystem.Net.Websocket.Messages
{
    public class VehicleSpawnDataPack : DataPack<VehicleSpawnInfo>
    {
    }

    public class VehicleSpawnInfo
    {
        public Direction Direction;
        public bool IsInstantMode;
        public float Intensity;
    }
}