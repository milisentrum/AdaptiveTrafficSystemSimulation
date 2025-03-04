using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking
{
    public class TrackingCamera : TrackerBase
    {
        [Foldout("High accuracy tracking", true)] 
        [SerializeField] private bool useRayTracking = false;
        [SerializeField] private Transform rayStartPoint;

        public void ReplaceViewDetector(DetectorBase detector)
        {
           RemoveAllDetectors();
           AddDetector(detector);
        }
        
        protected override bool ConfirmDetection(GameObject detectedObject)
        {
            const float maxCameraRayLength = 100f;
            
            if (!useRayTracking) return true;
            var startPoint = rayStartPoint.position;
            var endPoint = detectedObject.transform.position;
            var direction = endPoint - startPoint;
            if (Physics.Raycast(startPoint, direction, out var hit, maxCameraRayLength))
            {
                if (hit.collider.gameObject == detectedObject)
                {
                    Debug.DrawRay(startPoint, direction * hit.distance, Color.green);
                    return true;
                }

                Debug.DrawRay(startPoint, direction * hit.distance, Color.red);
                return false;
            }

            Debug.DrawRay(startPoint, direction * maxCameraRayLength, Color.red);
            return false;
        }
    }
}