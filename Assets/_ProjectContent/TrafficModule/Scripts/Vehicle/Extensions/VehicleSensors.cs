using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityDevKit.Optimization;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleSensors : CachedMonoBehaviour, IVehicleExtension
    {
        [Separator("Sensors")] 
        [SerializeField] private Transform sensorStartPos;
        [SerializeField] [Range(0, 90)] private int sideSensorAngle = 5;
        [SerializeField] private AnimationCurve sphereRadiusCurve;
        [SerializeField] private bool drawSensorsGizmos = true;

        [Separator("Tracking settings")] 
        [SerializeField] private LayerMask trackingLayer;
        [SerializeField] private bool trackVehicles = true;
        [SerializeField] private bool trackHumans = true;
        [SerializeField] private bool trackTrafficLighters = true;
        [SerializeField] private bool trackCrossingZones = true;

        [SerializeField] private DynamicFrameFilter dynamicFrameFilter;

        public List<DetectedObject> frontObjects = new List<DetectedObject>();

        private VehicleController _vehicleController;
        private VehicleMovement _movement;

        private readonly RaycastHit[] _frontHitsBuffer = new RaycastHit[MAX_FRONT_HITS_BUFFER_COUNT];
        private readonly RaycastHit[] _frontSphereHitsBuffer = new RaycastHit[MAX_FRONT_HITS_BUFFER_COUNT];
        private readonly RaycastHit[] _frontHighSphereHitsBuffer = new RaycastHit[MAX_FRONT_HITS_BUFFER_COUNT];
        private readonly RaycastHit[] _frontLowSphereHitsBuffer = new RaycastHit[MAX_FRONT_HITS_BUFFER_COUNT];
        private readonly RaycastHit[] _frontCloseSphereHitsBuffer = new RaycastHit[MAX_FRONT_HITS_BUFFER_COUNT];
        private readonly RaycastHit[] _leftSphereHitsBuffer = new RaycastHit[MAX_FRONT_HITS_BUFFER_COUNT];
        private readonly RaycastHit[] _rightSphereHitsBuffer = new RaycastHit[MAX_FRONT_HITS_BUFFER_COUNT];

        private readonly Vector3 _offsetVector = new Vector3(0, RAY_OFFSET, 0);
        
        private const int MAX_FRONT_HITS_BUFFER_COUNT = 12;

        // RAYCAST
        private const float SENSOR_SPHERE_RADIUS = 1.5f;
        private const float RAY_OFFSET = SENSOR_SPHERE_RADIUS * 2;
        private const float LOW_ACCURACY_SPEED = 5f;

        public struct DetectedObject
        {
            public DetectedObjectType DetectedObjectType;
            public DetectionDirection DetectionDirection;
            public GameObject DetectedGameObject;
        }

        public enum DetectedObjectType
        {
            TRAFFIC_LIGHT,
            VEHICLE,
            HUMAN,
            CROSSING,
            UNRECOGNIZED
        }

        public enum DetectionDirection
        {
            LEFT,
            RIGHT,
            FRONT
        }

        public void Init()
        {
        }

        private void Start()
        {
            _vehicleController = GetComponent<VehicleController>();
            _movement = _vehicleController.Movement;
        }

        private void Update()
        {
            if (dynamicFrameFilter.IsFilteredFrame())
            {
                CheckForObjectsInFront();
            }
        }

        //casting rays and spheres in front of car
        private void CheckForObjectsInFront()
        {
            const string vehicleTag = "Vehicle";
            const string humanTag = "Human";
            const string trafficLighterTag = "TrafficLighter";
            const string crossingTag = "CrossingZone";
            
            var rayLength = _movement.CalculateDistantView();
            var sensorSphereRadius = CalculateSensorSphereRadius();
            var sensorPosition = sensorStartPos.position;
            var sensorForward = sensorStartPos.forward;
            
            var frontRayObjects = FrontRaycast(
                rayLength,
                sensorSphereRadius,
                sensorPosition,
                sensorForward);
            
            var sidesRayObjects = SidesRaycast(rayLength,
                sensorSphereRadius,
                sensorPosition,
                sensorForward);

            var rayObjects = sidesRayObjects != null 
                ? frontRayObjects.Concat(sidesRayObjects)
                : frontRayObjects;

            foreach (var (hit, direction) in rayObjects)
            {
                if (trackVehicles && hit.transform.CompareTag(vehicleTag) && hit.transform != CachedTransform ||
                    trackHumans && hit.transform.CompareTag(humanTag))
                {
                    AddDetected(new DetectedObject
                    {
                        DetectedObjectType = hit.transform.CompareTag(vehicleTag)
                            ? DetectedObjectType.VEHICLE
                            : DetectedObjectType.HUMAN,
                        DetectionDirection = direction,
                        DetectedGameObject = hit.transform.gameObject
                    });
                }
                else if (trackTrafficLighters && hit.transform.CompareTag(trafficLighterTag))
                {
                    AddDetected(new DetectedObject
                    {
                        DetectedObjectType = DetectedObjectType.TRAFFIC_LIGHT,
                        DetectionDirection = direction,
                        DetectedGameObject = hit.transform.gameObject
                    });
                }
                else if (trackCrossingZones && hit.transform.CompareTag(crossingTag))
                {
                    AddDetected(new DetectedObject
                    {
                        DetectedObjectType = DetectedObjectType.CROSSING,
                        DetectionDirection = direction,
                        DetectedGameObject = hit.transform.gameObject
                    }); 
                }
                else
                {
                    dynamicFrameFilter.Increase();
                }
            }
        }

        private IEnumerable<(RaycastHit, DetectionDirection)> FrontRaycast(
            float rayLength, 
            float sensorSphereRadius,
            Vector3 sensorPosition,
            Vector3 sensorForward)
        {
            var frontRay = new Ray(sensorPosition, sensorForward);
            var highRay = new Ray(sensorPosition + _offsetVector, sensorForward);
            var lowRay = new Ray(sensorPosition - _offsetVector, sensorForward);

            var frontHitsCount = Physics.RaycastNonAlloc(frontRay, _frontHitsBuffer, rayLength);

            var sphereCastFrontHitsCount = Physics.SphereCastNonAlloc(
                frontRay, 
                sensorSphereRadius,
                _frontSphereHitsBuffer, 
                rayLength, 
                trackingLayer);

            var sphereCastHighHitsCount = Physics.SphereCastNonAlloc(
                highRay, 
                sensorSphereRadius,
                _frontHighSphereHitsBuffer, 
                rayLength, 
                trackingLayer);

            var sphereCastLowHitsCount = Physics.SphereCastNonAlloc(
                lowRay, 
                sensorSphereRadius,
                _frontLowSphereHitsBuffer, 
                rayLength, 
                trackingLayer);

            var sphereCastCloseHitsCount = Physics.SphereCastNonAlloc(
                frontRay, 
                _movement.CalculateCloseView(),
                _frontCloseSphereHitsBuffer, 
                0, 
                trackingLayer);
            
            return _frontHitsBuffer
                .Take(frontHitsCount)
                .Concat(_frontSphereHitsBuffer.Take(sphereCastFrontHitsCount))
                .Concat(_frontHighSphereHitsBuffer.Take(sphereCastHighHitsCount))
                .Concat(_frontLowSphereHitsBuffer.Take(sphereCastLowHitsCount))
                .Concat(_frontCloseSphereHitsBuffer.Take(sphereCastCloseHitsCount))
                .Select(hit => (hit, DetectionDirection.FRONT));
        }

        private IEnumerable<(RaycastHit, DetectionDirection)> SidesRaycast(
            float rayLength, 
            float sensorSphereRadius,
            Vector3 sensorPosition,
            Vector3 sensorForward)
        {
            if (!(_movement.currentSpeed > LOW_ACCURACY_SPEED)) return null;
            var sensorStartPosUp = sensorStartPos.up;
            var rayDirection = Quaternion.AngleAxis(sideSensorAngle, sensorStartPosUp) * sensorForward;
            var ray = new Ray(sensorPosition, rayDirection);
            var sphereCastLeftHitsCount = Physics.SphereCastNonAlloc(
                ray, 
                sensorSphereRadius, 
                _leftSphereHitsBuffer,
                rayLength, 
                trackingLayer);

            rayDirection = Quaternion.AngleAxis(-sideSensorAngle, sensorStartPosUp) * sensorForward;
            ray = new Ray(sensorPosition, rayDirection);
            var sphereCastRightHitsCount = Physics.SphereCastNonAlloc(
                ray, 
                sensorSphereRadius,
                _rightSphereHitsBuffer, 
                rayLength, 
                trackingLayer);

            return _leftSphereHitsBuffer
                .Take(sphereCastLeftHitsCount)
                .Select(hit => (hit, DetectionDirection.LEFT))
                .Concat(_rightSphereHitsBuffer
                    .Take(sphereCastRightHitsCount)
                    .Select(hit => (hit, DetectionDirection.RIGHT)));

        }

        private void AddDetected(DetectedObject detectedObject)
        {
            frontObjects.Add(detectedObject);
            dynamicFrameFilter.SetToMin();
        }

        public void ClearFrontObjects()
        {
            frontObjects.Clear();
        }

        private float CalculateSensorSphereRadius()
        {
            var speedRatio = _movement.currentSpeed / _movement.MaxSpeed;
            var radius = SENSOR_SPHERE_RADIUS * sphereRadiusCurve.Evaluate(speedRatio);
            return radius;
        }
            

        // this method visualizes raycasts and spherecasts 
        private void OnDrawGizmos()
        {
            if (!drawSensorsGizmos || !Application.isPlaying) return;

            var rayLength = _movement.CalculateDistantView();
            var sensorSphereRadius = CalculateSensorSphereRadius();
            var position = sensorStartPos.position;
            var forward = sensorStartPos.forward;
            var up = sensorStartPos.up;
            var rightEndpoint = position + Quaternion.AngleAxis(sideSensorAngle, up) * forward * rayLength;
            var leftEndpoint = position + Quaternion.AngleAxis(-sideSensorAngle, up) * forward * rayLength;

            // straight
            Gizmos.DrawLine(position, position + forward * rayLength);
            Gizmos.DrawWireSphere(position + forward * rayLength, sensorSphereRadius);
            Gizmos.DrawWireSphere(position + forward * rayLength - new Vector3(0, RAY_OFFSET, 0),
                sensorSphereRadius);
            Gizmos.DrawWireSphere(position + forward * rayLength + new Vector3(0, RAY_OFFSET, 0),
                sensorSphereRadius);

            if (_movement.currentSpeed > LOW_ACCURACY_SPEED)
            {
                // right
                Gizmos.DrawLine(position, rightEndpoint);
                Gizmos.DrawWireSphere(rightEndpoint, sensorSphereRadius);
                // left
                Gizmos.DrawLine(position, leftEndpoint);
                Gizmos.DrawWireSphere(leftEndpoint, sensorSphereRadius);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, _movement.CalculateCloseView());
        }
    }
}