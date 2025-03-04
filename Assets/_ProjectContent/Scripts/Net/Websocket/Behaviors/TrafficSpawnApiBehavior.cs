using AdaptiveTrafficSystem.Api;
using AdaptiveTrafficSystem.Net.Websocket.Messages;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

namespace AdaptiveTrafficSystem.Net.Websocket.Behaviors
{
    public class TrafficSpawnApiBehavior : ExtendedWebSocketBehavior
    {
        public SpawnSetupController SpawnSetupController;

        private const string VEHICLES_TYPE = "Vehicles";
        private const string PEDESTRIANS_TYPE = "Pedestrians";

        protected override void OnMessage(MessageEventArgs e)
        {
            var type = JsonConvert.DeserializeObject<Message>(e.Data).Type;
            Debug.Log($"Msg ({type}): {e.Data}");

            switch (type)
            {
                case VEHICLES_TYPE:
                    LoadVehiclesSpawn(e.Data);
                    break;
                case PEDESTRIANS_TYPE:
                    LoadPedestriansSpawn(e.Data);
                    break;
            }
        }

        private void LoadVehiclesSpawn(string msg)
        {
            var packet = JsonConvert.DeserializeObject<VehicleSpawnDataPack>(msg);
            ExecuteOnMainTread(SetupVehiclesSpawn, packet.Data);
        }

        private void SetupVehiclesSpawn(VehicleSpawnInfo[] spawnData)
        {
            foreach (var spawnInfo in spawnData)
            {
                SpawnSetupController.SetupVehicleSpawn(
                    spawnInfo.Direction,
                    spawnInfo.IsInstantMode,
                    spawnInfo.Intensity);
            }
        }
        
        private void LoadPedestriansSpawn(string msg)
        {
            var packet = JsonConvert.DeserializeObject<PedestrianSpawnDataPack>(msg);
            ExecuteOnMainTread(SetupPedestriansSpawn, packet.SpawnInfo);
        }

        private void SetupPedestriansSpawn(PedestrianSpawnInfo spawnInfo)
        {
            SpawnSetupController.SetupPedestrianSpawn(spawnInfo.Count);
        }
    }
}