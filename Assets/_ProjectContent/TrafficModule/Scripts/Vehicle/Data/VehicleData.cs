using MyBox;
using UnityEngine;

namespace TrafficModule.Vehicle.Data
{
    [CreateAssetMenu(fileName = "VehicleData", menuName = "Vehicles/VehicleData", order = 0)]
    public class VehicleData : ScriptableObject
    {
        [Separator("Lights")]
        public bool UseLights;
        [Header("Low Beam")] 
        [Range(0f, 2f)] public float LowHeadlightsIntensity;
        [Range(0, 150)] public int LowRange;
        [Range(0f, 179f)] public float LowSpotAngle;
        
        [Header("Main Beam")] 
        [Range(1f, 2f)] public float MainHeadlightsIntensity;
        [Range(80, 150)] public int MainRange;
        [Range(0f, 70f)] public float MainSpotAngle;
        
        [Separator("Movement")]
        [DisplayInspector] public VehicleMovementData MovementData;
    }
}