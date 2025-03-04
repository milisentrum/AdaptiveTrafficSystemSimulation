using System;
using System.Linq;
using AdaptiveTrafficSystem.Net.Websocket.Messages;
using AdaptiveTrafficSystem.Paths;
using AdaptiveTrafficSystem.Pedestrians;
using TrafficModule.Waypoints;
using UnityEngine;

namespace AdaptiveTrafficSystem.Api
{
    public class SpawnSetupController : MonoBehaviour
    {
        [SerializeField] private DirectionSpawnData[] directionSpawnData;
        [SerializeField] private PedestriansSpawningSystem pedestriansSpawningSystem;

        [Serializable]
        public struct DirectionSpawnData
        {
            public PathDirection Direction;
            public VehicleSpawnPoint SpawnPoint;
        }

        public void SetupVehicleSpawn(Direction direction, bool isInstantMode, float intensity = 0)
        {
            var spawnPoint = directionSpawnData.FirstOrDefault(data => data.Direction.Id == direction.ID).SpawnPoint;
            if (spawnPoint == null)
            {
                Debug.LogError($"Unknown direction with id {direction.ID}");
            }
            else
            {
                spawnPoint.Setup(isInstantMode, intensity);
                Debug.Log($"Setup {direction.ID}");
            }
        }

        public void SetupPedestrianSpawn(int count)
        {
            pedestriansSpawningSystem.Spawn(count);
        }
    }
}