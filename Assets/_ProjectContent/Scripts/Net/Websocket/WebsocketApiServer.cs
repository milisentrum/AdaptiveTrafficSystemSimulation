using System.Net;
using AdaptiveTrafficSystem.Api;
using AdaptiveTrafficSystem.Net.Websocket.Behaviors;
using AdaptiveTrafficSystem.Paths;
using AdaptiveTrafficSystem.Tracking.Parameters;
using UnityEngine;
using WebSocketSharp.Server;

namespace AdaptiveTrafficSystem.Net.Websocket
{
    public class WebsocketApiServer
    {
        // https://github.com/sta/websocket-sharp
        private readonly WebSocketServer _server;

        public WebsocketApiServer(IPEndPoint endPoint)
        {
            _server = new WebSocketServer($"ws://{endPoint.Address}:{endPoint.Port}");
        }

        public void InitTrafficLightersApi(ControlledPath[] controlledPaths)
        {
            const string path = "/Phases";
            _server.AddWebSocketService<TrafficLightersApiBehavior>(path, 
                trafficLightersBehavior =>
                {
                    trafficLightersBehavior.TrafficLightersPhases = new TrafficLightersPhases(controlledPaths);
                });
        }
        
        public void InitTrafficParametersApi(ParametersHolder[] parametersHolders)
        {
            const string path = "/Traffic";
            _server.AddWebSocketService<TrafficParametersApiBehavior>(path,
                parametersBehavior =>
                {
                    parametersBehavior.ParametersHolder = parametersHolders;

                }
            );
        }
        
        public void InitTrafficSpawnApi(SpawnSetupController spawnSetupController)
        {
            const string path = "/Spawn";
            _server.AddWebSocketService<TrafficSpawnApiBehavior>(path,
                spawnBehavior =>
                {
                    spawnBehavior.SpawnSetupController = spawnSetupController;
                }
            );
        }

        public void Start()
        {
            _server.Start();

            if (_server.IsListening)
            {
                Debug.Log($"Listening on port {_server.Port}, and providing WebSocket services:");

                foreach (var path in _server.WebSocketServices.Paths)
                {
                    Debug.Log($" - {path}");
                }
            }
        }

        public void Stop()
        {
            _server.Stop();
        }

        public bool IsListening => _server.IsListening;
    }
}