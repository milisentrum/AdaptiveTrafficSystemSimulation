using UnityEngine;

namespace AdaptiveTrafficSystem.CCTV
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "CCTV settings", order = 0)]
    public class CameraSettings : ScriptableObject
    {
        public float fieldOfView = 80;
        public float distance = 30;
    }
}