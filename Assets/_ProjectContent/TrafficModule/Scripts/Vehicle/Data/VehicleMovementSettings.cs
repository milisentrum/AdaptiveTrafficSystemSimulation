using System;
using MyBox;
using UnityEngine;

namespace TrafficModule.Vehicle.Data
{
    [Serializable]
    public class VehicleMovementSettings
    {
        [Separator("Speed")]
        [Range(0, 150)] [PositiveValueOnly] public int maxSpeed = 100;

        [Separator("Power")]
        [PositiveValueOnly] public float motorPower = 120;
        [PositiveValueOnly] public float brakePower = 300;

        [Separator("Transmission")]
        public TransmissionType transmissionType = TransmissionType.BACKWARD;
        [Range(20, 75)] public int maxSteerAngle = 45;
    }
}