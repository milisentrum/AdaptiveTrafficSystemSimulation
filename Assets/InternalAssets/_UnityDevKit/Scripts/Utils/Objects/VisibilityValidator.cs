using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace UnityDevKit.Utils.Objects
{
    [Serializable]
    public class VisibilityValidator : IVisibilityValidator
    {
        [SerializeField] private bool useRayTracking = true;
        [SerializeField] private bool useReverseRayTracking = true;
        [SerializeField] private bool debugRays = true;
        
        private const float MAX_CAMERA_RAY_LENGTH = 100f;
        
        public bool Validate(Transform fromPoint, Transform validationObjectTransform)
        {
            if (!useRayTracking) return true;
            var isVisible = ValidateWithRay(fromPoint, validationObjectTransform);

            if (useReverseRayTracking && !isVisible)
            {
                isVisible = ValidateWithRay(validationObjectTransform, fromPoint);
            }

            return isVisible;
        }

        public IEnumerator ValidateInInterval(
            Transform fromPoint, 
            Transform validationObjectTransform, 
            float interval, 
            int checksCount, List<bool> isVisibleResults)
        {
            isVisibleResults.Clear();
            var checkDelay = interval / checksCount;
            var remainingChecksCount = checksCount;
            while (remainingChecksCount > 0)
            {
                var isVisible = Validate(fromPoint, validationObjectTransform);
                isVisibleResults.Add(isVisible);
                remainingChecksCount--;
                yield return new WaitForSeconds(checkDelay);
            }
        }

        private bool ValidateWithRay(Transform fromPoint, Transform validationObjectTransform)
        {
            var startPoint = fromPoint.position;
            var endPoint = validationObjectTransform.position;
            var direction = endPoint - startPoint;
            if (Physics.Raycast(startPoint, direction, out var hit, MAX_CAMERA_RAY_LENGTH)) // TODO -- layer
            {
                if (hit.collider.transform == validationObjectTransform)
                {
                    if (debugRays)
                    {
                        Debug.DrawRay(startPoint, direction * hit.distance, Color.green);
                        Debug.Log("Did Hit");
                    }
                    return true;
                }

                if (debugRays)
                {
                    Debug.DrawRay(startPoint, direction * hit.distance, Color.red);
                    Debug.Log("Did not Hit");
                }

                return false;
            }

            if (debugRays)
            {
                Debug.DrawRay(startPoint, direction * MAX_CAMERA_RAY_LENGTH, Color.red);
                Debug.Log("Did not Hit");
            }

            return false;
        }
    }
}