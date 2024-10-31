using System;
using System.Collections;
using System.Linq;
using MyBox;
using TrafficModule.Vehicle.Data;
using UnityDevKit.Optimization;
using UnityEngine;

namespace TrafficModule.Vehicle.Extensions
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleMovement : CachedMonoBehaviour, IVehicleExtension
    {
        [Separator("Data")] 
        [SerializeField] private VehicleMovementData movementData;
        [ReadOnly] public float currentSpeed;

        [Separator("Wheels")] 
        [SerializeField] private WheelCollider[] backWheels;
        [SerializeField] private WheelCollider[] frontWheels;

        [Separator("Misc")] 
        [SerializeField] private Transform wheelTurnerSmooth;
        [SerializeField] private Transform wheelTurner;

        private Rigidbody _rigidbody;
        private VehicleController _controller;
        private VehicleMovementSettings _settings;
        
        private float _targetSteerAngle;
        private bool _reverse;
        private float _zVelocity;

        private float _wheelScaleCoefficient;
        private int _currentMaxSpeed;

        private Coroutine _currentTorqueCoroutine;

        // WHEELS
        private const float PERFECT_NUMBER = 0.3f;
        private const float WHEEL_SCALE_MODIFIER = 3.25f;

        // SPEED
        private const float RPM_SPEED_MODIFIER = 30f;
        private const int MAX_SPEED = 300;

        private const float KM_MODIFIER = 3.6f;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            _controller = GetComponent<VehicleController>();
        }

        public void Init()
        {
            SetSettings();

            PowerWheels = _settings.transmissionType switch
            {
                TransmissionType.FORWARD => frontWheels,
                TransmissionType.BACKWARD => backWheels,
                TransmissionType.FULL => backWheels.Concat(frontWheels).ToArray(),
                _ => throw new ArgumentOutOfRangeException()
            };

            _wheelScaleCoefficient = PowerWheels[0].transform.parent.localScale.x 
                                     * PowerWheels[0].radius 
                                     / PERFECT_NUMBER 
                                     * WHEEL_SCALE_MODIFIER;
        }

        private void SetSettings()
        {
            var carSettings = _controller.vehicleData;
            movementData = carSettings.MovementData;
            _settings = movementData.GetSettings();
        }

        private void Update()
        {
            _zVelocity = transform.InverseTransformDirection(_rigidbody.velocity).z;
            currentSpeed = _rigidbody.velocity.magnitude * KM_MODIFIER; // speed in km/h
        }

        #region TORQUE

        public void SetMotorTorque(float targetPower)
        {
            foreach (var wheelCollider in PowerWheels.Where(wheelCollider => wheelCollider.rpm < GetRpmSpeed()))
            {
                wheelCollider.motorTorque = targetPower;
                wheelCollider.brakeTorque = 0;
            }
        }

        public void SetMotorTorqueForWheel(WheelCollider wheelCollider, float targetPower)
        {
            if (_currentTorqueCoroutine != null)
            {
                StopCoroutine(_currentTorqueCoroutine);
            }

            _currentTorqueCoroutine = StartCoroutine(SetMotorTorqueForWheelProcess(wheelCollider, targetPower));
        }

        private IEnumerator SetMotorTorqueForWheelProcess(WheelCollider wheelCollider, float targetPower)
        {
            const float accelerationMaxTime = 10;
            const int accelerationMaxSteps = 50;

            //Debug.Log("Motor power process");

            var torqueDelta = targetPower - wheelCollider.motorTorque;
            var torqueModifier = torqueDelta / _settings.motorPower;
            var accelerationSteps = (int) Math.Ceiling(torqueModifier * accelerationMaxSteps);
            var accelerationTime = torqueDelta * accelerationMaxTime;
            var torqueStep = torqueDelta / accelerationSteps;
            var timeStep = accelerationTime / accelerationSteps;

            while (wheelCollider.motorTorque < targetPower)
            {
                wheelCollider.motorTorque += torqueStep;
                yield return new WaitForSeconds(timeStep);
            }
            wheelCollider.motorTorque = targetPower;
        }

        public void FullMotorTorque()
        {
            SetMotorTorque(_settings.motorPower);
        }

        public void SetBrakeTorque(float targetPower)
        {
            foreach (var wheelCollider in PowerWheels)
            {
                wheelCollider.motorTorque = 0;
                wheelCollider.brakeTorque = targetPower;
            }
        }

        public void FullBrakeTorque()
        {
            SetBrakeTorque(_settings.brakePower);
        }

        public void AdaptiveTorque()
        {
            foreach (var wheelCollider in PowerWheels)
            {
                if (wheelCollider.rpm < GetRpmSpeed())
                {
                    wheelCollider.motorTorque = _settings.motorPower;
                    wheelCollider.brakeTorque = 0;
                }
                else
                {
                    wheelCollider.motorTorque = 0;
                    wheelCollider.brakeTorque = _settings.brakePower;
                }
            }
        }

        #endregion

        #region WHEELS

        public void WheelsRotation(Vector3 destinationPosition)
        {
            // turn wheels
            wheelTurner.LookAt(destinationPosition);
            var pos = wheelTurnerSmooth.position;
            pos.y = destinationPosition.y;
            wheelTurnerSmooth.position = pos;
            var rot = wheelTurner.localEulerAngles;
            rot.x = 0;
            wheelTurner.localEulerAngles = rot;
            LerpToSteerAngle(destinationPosition);
        }

        //turn wheels
        private void LerpToSteerAngle(Vector3 destinationPosition)
        {
            foreach (var wheelCollider in frontWheels)
            {
                wheelCollider.steerAngle = GetNewSteerAngle(destinationPosition);
            }
        }

        //calculate angle for wheels, looking at currentWaypoint
        private float GetNewSteerAngle(Vector3 destinationPosition)
        {
            var relativeVector = CachedTransform.InverseTransformPoint(destinationPosition);
            var newSteer = relativeVector.x / relativeVector.magnitude * _settings.maxSteerAngle;
            return newSteer;
        }

        #endregion

        public void SetCurrentMaxSpeed(int newMaxSpeed)
        {
            _currentMaxSpeed = newMaxSpeed;
        }

        public void SetDefaultMaxSpeed()
        {
            _currentMaxSpeed = _settings.maxSpeed;
        }

        public int MaxSpeed => _settings.maxSpeed;
        public float MotorPower => _settings.motorPower;

        public float BrakePower => _settings.brakePower;

        public float GetRpmSpeed() => _currentMaxSpeed / _wheelScaleCoefficient * RPM_SPEED_MODIFIER;

        public static int GetMaxAllowedSpeed() => MAX_SPEED;

        public float CalculateDistantView()
        {
            const float startDistance = 1.5f;
            const float zVelocityModifier = 2;
            return startDistance + _zVelocity * zVelocityModifier;
        } 

        public float CalculateCloseView()
        {
            const float startDistance = 0.675f;
            const float zVelocityModifier = 1 / 6f;
            return startDistance + _zVelocity * zVelocityModifier;
        } 
        
        public WheelCollider[] PowerWheels { get; private set; }
    }
}