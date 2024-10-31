using UnityDevKit.Optimization;
using UnityEngine;

namespace AdaptiveTrafficSystem.UI
{
    public class CanvasToCameraRotation : CachedMonoBehaviour
    {
        private Transform _cameraTransform;
        
        private void Start()
        {
            if (Camera.main is not null) _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            const int frameFilter = 30;
            
            if (Time.frameCount % frameFilter != 0) return;
            var cam = _cameraTransform.rotation;
            CachedTransform.LookAt(SelfPosition + cam * Vector3.forward, cam * Vector3.up);
        }
    }
}