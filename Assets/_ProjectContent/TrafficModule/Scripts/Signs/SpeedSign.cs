using UnityEngine;

namespace TrafficModule.Signs
{
    public class SpeedSign : MonoBehaviour
    {
        [Range(20, 120)] public int value = 60;

        public SpeedZone zone;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            zone = transform.Find("Zone").GetComponent<SpeedZone>();
            zone.speedRestriction = value;
        }

        public void SetSpeedRestrictionValue(int speed)
        {
            value = speed;
        }
    }
}