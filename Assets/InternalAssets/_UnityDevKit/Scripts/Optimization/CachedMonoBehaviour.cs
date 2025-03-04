using UnityEngine;

namespace UnityDevKit.Optimization
{
    public class CachedMonoBehaviour : MonoBehaviour
    {
        public Transform CachedTransform { get; private set; }

        public Vector3 SelfPosition => CachedTransform.position;

        public Quaternion SelfRotation => CachedTransform.rotation;

        protected virtual void Awake()
        {
            CachedTransform = transform;
        }
    }
}