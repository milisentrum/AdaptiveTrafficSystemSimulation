namespace AdaptiveTrafficSystem.Net.Websocket.Messages
{
    public class Message
    {
        public string Type;
    }

    public class DataPack<T> : Message
    {
        public T[] Data;
    }
}