using UnityEngine;

namespace TrafficModule.Vehicle.Data
{
    [CreateAssetMenu(fileName = "VehicleMovementData", menuName = "Vehicles/MovementData", order = 0)]
    public class VehicleMovementData : ScriptableObject
    {
        [SerializeField] private VehicleMovementSettings settings;
        [Range(0, 1f)] public float speedDeltaModifier = 0.1f;
        [Range(0, 1f)] public float powerDeltaModifier = 0.05f;
        [Range(0, 1f)] public float steerAngleDeltaModifier = 0.05f;

        public VehicleMovementSettings GetSettings() => new VehicleMovementSettings
        {
            maxSpeed = (int) (settings.maxSpeed * (1 + Random.Range(-speedDeltaModifier, speedDeltaModifier))),
            motorPower = settings.motorPower * (1 + Random.Range(-powerDeltaModifier, powerDeltaModifier)),
            brakePower = settings.brakePower * (1 + Random.Range(-powerDeltaModifier, powerDeltaModifier)),
            transmissionType = settings.transmissionType,
            maxSteerAngle = (int) (settings.maxSteerAngle * (1 + Random.Range(-steerAngleDeltaModifier, steerAngleDeltaModifier))),
        };
    }
}