using UnityDevKit.Events;
using UnityDevKit.Optimization;
using UnityEngine;

namespace TrafficModule.Utils
{
    public class CameraHolder : CachedMonoBehaviour
    {
        public EventHolder<CameraHolder> RemoveEvent { get; private set; } = new EventHolder<CameraHolder>();

        public void SetCamera(Camera cam)
        {
            cam.transform.SetParent(CachedTransform);
            cam.transform.SetPositionAndRotation(SelfPosition, SelfRotation);
        }
    }
}