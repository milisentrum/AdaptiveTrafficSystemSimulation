using UnityEngine;

namespace AdaptiveTrafficSystem.Tracking.Filters
{
    public class DirectionFilter : MonoBehaviour, IFilter
    {
        [SerializeField] private Transform anchorObject;
        [SerializeField] protected float minAngle = 0;
        [SerializeField] protected float maxAngle = 100;

        public GameObject Filter(GameObject sourceObject)
        {
            var rb = sourceObject.GetComponentInChildren<Rigidbody>();
            var rbVelocity = rb.velocity;

            var targetDir = anchorObject.position - rb.position;
            var angle = Vector3.Angle(rbVelocity, targetDir);

            var filteredObject = angle >= minAngle && angle < maxAngle ? sourceObject : null;
            return filteredObject;
        }
    }
}