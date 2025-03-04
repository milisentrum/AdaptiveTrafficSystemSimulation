using System.Linq;
using AdaptiveTrafficSystem.Api;
using AdaptiveTrafficSystem.Net.Websocket.Messages;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

namespace AdaptiveTrafficSystem.Net.Websocket.Behaviors
{
    public class TrafficLightersApiBehavior : ExtendedWebSocketBehavior
    {
        public TrafficLightersPhases TrafficLightersPhases;

        private const string INIT_TYPE = "Init";
        private const string PATHS_TYPE = "Paths";

        protected override void OnMessage(MessageEventArgs e)
        {
            var type = JsonConvert.DeserializeObject<Message>(e.Data).Type;
            Debug.Log($"Msg ({type}): {e.Data}");

            switch (type)
            {
                case INIT_TYPE:
                    SendPaths();
                    break;
                case PATHS_TYPE:
                    LoadPaths(e.Data);
                    break;
            }
        }

        private void SendPaths()
        {
            var data = TrafficLightersPhases.ControlledPaths
                .Select(path => new Path
                {
                    ID = path.Id,
                    Name = path.PathName,
                    Phase = TrafficLightersPhases.GetPhase(path.Id),
                    Directions = path.PathDirections
                        .Select(direction => new Direction
                        {
                            ID = direction.Id,
                            PathId = path.Id
                        }).ToArray()
                })
                .ToArray();
            var msg = new PathsDataPack
            {
                Type = INIT_TYPE,
                Data = data
            };
            var json = JsonConvert.SerializeObject(msg);
            SendAsync(json);
        }

        private void LoadPaths(string msg)
        {
            var packet = JsonConvert.DeserializeObject<PathsDataPack>(msg);

            ExecuteOnMainTread(ProcessPaths, packet.Data);
        }

        private void ProcessPaths(Path[] paths)
        {
            foreach (var path in paths)
            {
                TrafficLightersPhases.SetPhase(path.ID, path.Phase);
            }
        }
    }
}