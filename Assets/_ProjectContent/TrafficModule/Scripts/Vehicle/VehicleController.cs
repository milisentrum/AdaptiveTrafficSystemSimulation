using MyBox;
using TrafficModule.Utils;
using TrafficModule.Vehicle.Data;
using TrafficModule.Vehicle.Extensions;
using TrafficModule.Waypoints;
using UnityEngine;
using UnityEngine.Events;

namespace TrafficModule.Vehicle
{
    [RequireComponent(typeof(VehicleMovement))]
    public class VehicleController : MonoBehaviour
    {
        [SerializeField] private bool handWaypointSet;
        [ConditionalField(nameof(handWaypointSet))] 
        [SerializeField] private Waypoint startWaypoint;

        public UnityEvent carDestroyed;

        public VehicleData vehicleData;

        public VehicleNavigator.Path NavigatorPath { private set; get; }
        public VehicleMovement Movement { get; private set; }
        public VehicleNavigator Navigator { get; private set; }
        public VehicleSensors Sensors { get; private set; }
        public VehicleMovementAI MovementAI { get; private set; }
        public VehicleObstaclesAnalyzer ObstaclesAnalyzer { get; private set; }
        public VehicleTrafficLighterHandler VehicleTrafficLighters { get; private set; }
        public VehiclePriority Priority { get; private set; }
        public VehicleAccidentHandler AccidentHandler { get; private set; }
        public VehicleStoppingState StoppingStates { get; private set; }

        private const float SPAWN_GROUND_OFFSET = 1.5f;

        private void Awake()
        {
            Movement = GetComponent<VehicleMovement>();
            Navigator = GetComponent<VehicleNavigator>();
            Sensors = GetComponent<VehicleSensors>();
            MovementAI = GetComponent<VehicleMovementAI>();
            ObstaclesAnalyzer = GetComponent<VehicleObstaclesAnalyzer>();
            VehicleTrafficLighters = GetComponent<VehicleTrafficLighterHandler>();
            Priority = GetComponent<VehiclePriority>();
            AccidentHandler = GetComponent<VehicleAccidentHandler>();
            StoppingStates = GetComponent<VehicleStoppingState>();
        }

        private void Start()
        {
            Init();
            foreach (var vehicleExtension in GetComponentsInParent<IVehicleExtension>())
            {
                vehicleExtension.Init();
            }

            if (handWaypointSet)
            {
                SetupStartWaypoint(startWaypoint);
            }
        }

        public void Init()
        {
            // Waypoints sync
            NavigatorPath = Navigator.currentPath;
        }

        public void SetupStartWaypoint(Waypoint waypoint)
        {
            transform.position = waypoint.GetPosition() + new Vector3(0, SPAWN_GROUND_OFFSET, 0);
            NavigatorPath.CurrentWaypoint = waypoint;
            Navigator.Init();
            transform.LookAt(NavigatorPath.FutureWaypoint.transform);
        }

        public void SetCarSettings(VehicleData vehicleDataSettings)
        {
            vehicleData = vehicleDataSettings;
        }

        public void DestroyTransport()
        {
            const float destroyDelay = 0.5f;
            
            carDestroyed.Invoke();
            carDestroyed.RemoveAllListeners();
            var cameraHolder = GetComponentInChildren<CameraHolder>();
            cameraHolder.RemoveEvent.Invoke(cameraHolder);
            GameObject carObject;
            (carObject = gameObject).SetActive(false);
            Destroy(carObject, destroyDelay);
        }
    }
}