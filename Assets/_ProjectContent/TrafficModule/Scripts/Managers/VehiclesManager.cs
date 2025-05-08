using System.Collections.Generic;
using MyBox;
using TrafficModule.Vehicle;
using TrafficModule.Waypoints;
using UnityDevKit.Triggers;
using UnityEngine;
//using global::AgentController;  // <— add this line

namespace TrafficModule.Managers
{
    public class VehiclesManager : MonoBehaviour
    {
        [Separator("Initialization")]
        [SerializeField] private bool initVehicles = true;
        
        [Separator("Triggers")]
        [SerializeField] private BoolTriggerEvent waypointsInited;
        [SerializeField] private BoolTriggerEvent vehiclesSpawned;
        
        [Separator("Vehicles settings")]
        [SerializeField] private GameObject vehicleHolder;
        [SerializeField] private List<GameObject> vehiclesPool;
        
        [Separator("Spawn settings")]
        [SerializeField] private bool respawnNewVehicles;
        [SerializeField] private List<VehicleSpawnPoint> spawnPoints;

        private readonly List<GameObject> _vehiclesList = new List<GameObject>();
        private int _vehiclesLastIndex = 1;
        
        private void Start()
        {
            if (initVehicles)
            {
                InitSubscription();
            }
        }

        private void InitSubscription()
        {
            waypointsInited.SubscribeToTrueValueSet(Init);
        }

        private void Init()
        {
            if (vehicleHolder == null)
            {
                const string defaultHolderName = "VehiclesHolder";
                vehicleHolder = new GameObject(defaultHolderName);
                vehicleHolder.transform.SetParent(transform);
            }
            
            StartVehiclesTraffic();
            vehiclesSpawned.SetValue(true);
        }

        private void StartVehiclesTraffic()
        {
            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.SpawnEvent.AddListener(CreateVehicle);
                spawnPoint.Init();
            }
        }

        private void CreateVehicle(Waypoint newCarSpawnWaypoint)
        {
            var prototype = vehiclesPool[Random.Range(0, vehiclesPool.Count)];
            var newVehicle = Instantiate(prototype, vehicleHolder.transform, false);
            newVehicle.SetActive(true);
            // 1) На лету цепляем AgentController
            //if (newVehicle.GetComponent<AgentController>() == null)
            //    newVehicle.AddComponent<AgentController>();

            newVehicle.name = $"{prototype.name}_{_vehiclesLastIndex}";
            _vehiclesLastIndex++;
            _vehiclesList.Add(newVehicle);

            var newVehicleController = newVehicle.GetComponent<VehicleController>();
            newVehicleController.Init();
            newVehicleController.SetupStartWaypoint(newCarSpawnWaypoint);
            if (respawnNewVehicles)
            {
                newVehicleController.carDestroyed.AddListener(RespawnNewCar);
            }
        }

        //private void CreateVehicle(Waypoint newCarSpawnWaypoint)
        //{
        //    // 1) Выбираем случайный prototype и создаём его под holder
        //    var prototype = vehiclesPool[Random.Range(0, vehiclesPool.Count)];
        //    var newVehicle = Instantiate(prototype, vehicleHolder.transform, false);

        //    // 2) Делаем объект активным
        //    newVehicle.SetActive(true);

        //    // 3) На лету цепляем AgentController, если его нет
        //    if (newVehicle.GetComponent<AgentController>() == null)
        //    {
        //        newVehicle.AddComponent<AgentController>();
        //    }

        //    // 4) Присваиваем имя, заносим в список
        //    newVehicle.name = $"{prototype.name}_{_vehiclesLastIndex}";
        //    _vehiclesLastIndex++;
        //    _vehiclesList.Add(newVehicle);

        //    // 5) Старая логика инициализации VehicleController
        //    var newVehicleController = newVehicle.GetComponent<VehicleController>();
        //    newVehicleController.Init();
        //    newVehicleController.SetupStartWaypoint(newCarSpawnWaypoint);
        //    if (respawnNewVehicles)
        //    {
        //        newVehicleController.carDestroyed.AddListener(RespawnNewCar);
        //    }
        //}


        private void RespawnNewCar()
        {
            const float respawnDelay = 0.75f;
            var spawnWaypoint = spawnPoints.GetRandom();
            if (spawnWaypoint.VehicleInside)
            {
                Invoke(nameof(RespawnNewCar), respawnDelay);
            }
            else
            {
                CreateVehicle(spawnWaypoint.Waypoint);
            }
        }
    }
}