using UnityEngine;

namespace UnityDevKit.Utils
{
    public class TransformFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform follow;

        [SerializeField] private bool controlPosition;
        [SerializeField] private bool controlRotation;
        [SerializeField] private bool controlScale;

        private Vector3 previousPosition;
        private Vector3 previousRotation;
        private Vector3 previousScale;

        private void Start()
        {
            previousPosition = target.localPosition;
            previousRotation = target.localEulerAngles;
            previousScale = target.localScale;
        }

        private void Update()
        {
            if (controlPosition)
            {
                var localPosition = target.localPosition;
                follow.localPosition += localPosition - previousPosition;
                previousPosition = localPosition;
            }

            if (controlRotation)
            {
                var localEulerAngles = target.localEulerAngles;
                follow.localEulerAngles += localEulerAngles - previousRotation;
                previousRotation = localEulerAngles;
            }

            if (controlScale)
            {
                var localScale = target.localScale;
                follow.localScale += localScale - previousScale;
                previousScale = localScale;
            }
        }
    }
}