namespace AdaptiveTrafficSystem.Net.Websocket.Messages
{
    public class PedestrianSpawnDataPack : Message
    {
        public PedestrianSpawnInfo SpawnInfo;
    }
    
    public class PedestrianSpawnInfo
    {
        public int Count;
    }
}