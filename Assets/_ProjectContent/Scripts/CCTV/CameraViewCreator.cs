using AdaptiveTrafficSystem.Tracking;
using UnityEngine;
using MyBox;
#if UNITY_EDITOR
using UnityEditor.ProBuilder;
#endif
using UnityEngine.ProBuilder;

namespace AdaptiveTrafficSystem.CCTV
{
    [RequireComponent(typeof(TrackingCamera))]
    public class CameraViewCreator : MonoBehaviour
    {
        [Header("Main Settings")] 
        [SerializeField] private Transform viewHolder;
        [SerializeField] private GameObject currentView;

        [SerializeField] [DisplayInspector] private CameraSettings cameraSettings;

        private TrackingCamera _trackingCamera;

        private void Start()
        {
            _trackingCamera = GetComponent<TrackingCamera>();
        }

        private void CreateView()
        {
            const float startAngle = 45;
            const int coneSubdivideAxis = 4;
            var mesh = ShapeGenerator.GenerateCone(
                PivotLocation.Center,
                AngleToConeRadius(cameraSettings),
                cameraSettings.distance,
                coneSubdivideAxis);

            mesh.ToMesh();
            mesh.Refresh();
#if UNITY_EDITOR
            mesh.Optimize();
#endif
            var meshTransform = mesh.transform;
            meshTransform.SetParent(viewHolder);

            meshTransform.localPosition = new Vector3(
                0,
                -cameraSettings.distance / 2f * meshTransform.localScale.y,
                0);

            meshTransform.localEulerAngles = new Vector3(0, startAngle, 0);

            var meshGameObject = mesh.gameObject;

            var meshCollider = meshGameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.isTrigger = true;

            meshGameObject.GetComponent<MeshRenderer>().enabled = false;

            var colliderDetector = meshGameObject.AddComponent<ColliderDetector>();

            if (currentView != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(currentView);
#else
            Destroy(currentView);
#endif
            }

            currentView = meshGameObject;

#if UNITY_EDITOR
            _trackingCamera = GetComponent<TrackingCamera>();
#endif
            _trackingCamera.ReplaceViewDetector(colliderDetector);
        }

        private static float AngleToConeRadius(CameraSettings cameraSettings)
        {
            var radius =
                Mathf.Abs(Mathf.Tan((cameraSettings.fieldOfView / 2) * Mathf.Deg2Rad) * cameraSettings.distance);
            return radius;
        }

#if UNITY_EDITOR
        [ButtonMethod]
        public string CreateNewView()
        {
            CreateView();
            return "New camera view has been created";
        }
#endif
    }
}