using UnityEngine;

namespace UnityDevKit.Utils
{
    public class CanvasToCameraRotation : MonoBehaviour
    {
        private Transform cameraTransform;

        private void Start()
        {
            if (!(Camera.main is null)) cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            var rotation = cameraTransform.rotation;
            var cam = new Quaternion(0, rotation.y, 0, rotation.w);
            transform.LookAt(transform.position + cam * Vector3.forward, cam * Vector3.up);
        }
    }
}