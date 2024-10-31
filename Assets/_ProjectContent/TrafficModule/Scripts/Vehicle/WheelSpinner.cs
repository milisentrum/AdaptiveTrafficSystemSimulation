using UnityEngine;

namespace TrafficModule.Vehicle
{
    public class WheelSpinner : MonoBehaviour
    {
        [SerializeField] private WheelCollider wheelCollider;
        
        private Quaternion _wheelRotation;

        private void Update()
        {
            const float speedThreshold = 5;
            const int stepsBelowThreshold = 12;
            const int stepsAboveThreshold = 15;
            
            wheelCollider.GetWorldPose(out _, out _wheelRotation);
            transform.rotation = _wheelRotation;
            wheelCollider.ConfigureVehicleSubsteps(speedThreshold, stepsBelowThreshold, stepsAboveThreshold);
        }
    }
}