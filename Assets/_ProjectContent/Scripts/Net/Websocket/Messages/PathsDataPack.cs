namespace AdaptiveTrafficSystem.Net.Websocket.Messages
{
    public class PathsDataPack : DataPack<Path>
    {
    }

    public class Path
    {
        public string ID;
        public string Name;
        public float Phase;
        public Direction[] Directions;
    }

    public class Direction
    {
        public string ID;
        public string PathId;
    }
}