using System.Collections.Generic;
using MyBox;
using UnityDevKit.Events;
using UnityDevKit.Triggers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TrafficModule.Waypoints
{
    [RequireComponent(typeof(Waypoint))]
    public class VehicleSpawnPoint : MonoBehaviour
    {
        [SerializeField] private bool instantMode;
        [SerializeField] [PositiveValueOnly] private FloatTriggerEvent intensityTrigger;
        [SerializeField] [PositiveValueOnly] [Range(0, 1f)] private float deltaPercentage = 0.2f; 

        public bool VehicleInside { get; private set; }
        public EventHolder<Waypoint> SpawnEvent { get; } = new EventHolder<Waypoint>();
        public Waypoint Waypoint { get; private set; }

        private bool _isInitialized;
        private float _spawnPeriod;
        private readonly List<GameObject> _vehiclesInCollider = new();

        private float _spawnRemainingTime;

        private const string VEHICLE_TAG = "Vehicle";

        private void Awake()
        {
            Waypoint = GetComponent<Waypoint>();
        }

        private void Update()
        {
            const float oneMinuteInSeconds = 60f;
            if (!_isInitialized || instantMode) return;
            if (_spawnRemainingTime > 0)
            {
                _spawnRemainingTime -= Time.deltaTime;
            }
            else
            {
                _spawnPeriod = oneMinuteInSeconds / intensityTrigger.GetValue();
                _spawnRemainingTime = _spawnPeriod * (1 + Random.Range(-deltaPercentage, deltaPercentage));
                Spawn();
            }
        }

        public void Init()
        {
            _isInitialized = true;
        }

        public void Setup(bool isInstantMode, float intensity = 0)
        {
            SetupMode(isInstantMode);
            if (isInstantMode)
            {
                InstantSpawn();
            }
            else
            {
                SetupIntensity(intensity);
            }
        }

        private void SetupMode(bool isInstantMode)
        {
            instantMode = isInstantMode;
        }

        private void SetupIntensity(float intensity)
        {
            intensityTrigger.SetValue(intensity);
        }

        private void InstantSpawn()
        {
            if (!instantMode) return;
            Spawn();
        }

        private void Spawn()
        {
            if (!VehicleInside)
            {
                SpawnEvent.Invoke(Waypoint);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(VEHICLE_TAG)) return;
            _vehiclesInCollider.Add(other.gameObject);
            VehicleInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.CompareTag(VEHICLE_TAG)) return;
            if (_vehiclesInCollider.Contains(other.gameObject))
            {
                _vehiclesInCollider.Remove(other.gameObject);
            }

            if (HasNoVehicles())
            {
                VehicleInside = false;
            }
        }

        private bool HasNoVehicles()
        {
            for (var i = _vehiclesInCollider.Count - 1; i >= 0; i--)
            {
                if (_vehiclesInCollider[i] == null)
                {
                    _vehiclesInCollider.Remove(_vehiclesInCollider[i]);
                }
            }

            return _vehiclesInCollider.Count == 0;
        }
    }
}