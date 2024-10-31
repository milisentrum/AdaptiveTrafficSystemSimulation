using TrafficModule.Vehicle;
using UnityEngine;

namespace TrafficModule.Signs
{
    public class SpeedZone : MonoBehaviour
    {
        public int speedRestriction;

        private void OnTriggerEnter(Collider other)
        {
            var transport = other.gameObject.GetComponentInParent<VehicleController>();
            if (transport != null)
            {
                //transport.SetMaxSpeed(speedRestriction);
            }
        }
    }
}