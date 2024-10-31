using UnityEngine;

namespace UnityDevKit.CameraHandlers
{
    public class CameraUpDown : MonoBehaviour
    {
        [Tooltip("tracking object")] public Transform target;
        [Tooltip("Smooth camera movement")] public float smoothing = 3f;

        [Space] [Header("Zoom")] [Tooltip("Zoom on start")] [Range(10, 70)]
        public int startingZoom = 45;

        [Tooltip("Min zoom border")] [Range(1, 178)]
        public int minZoom = 15;

        [Tooltip("Max zoom border")] [Range(2, 179)]
        public int maxZoom = 75;

        [Tooltip("Zooming speed")] public float zoomSpeed = 10;

        [Space] [Header("Free view")] [Tooltip("Allow free camera mode")]
        public bool freeViewMode = true;

        [Tooltip("Free camera movement speed")]
        public float speed = 10f;

        [Tooltip("Set borders to free camera mode")]
        public bool freeViewBorders = true;

        [Tooltip("Set borders length")] public int bordersLength = 20;

        [Tooltip("Set free camera mode activation key")]
        public KeyCode freeModeKey = KeyCode.LeftAlt;

        private RaycastHit hit;
        private Vector3 offset;
        private Vector3 currentTargetPosition;
        private Camera mainCam;
        private float speedResize;

        private const float SpeedNorm = 0.2f;
        private const float SpeedResizeNorm = 100f;
        private const float ZoomSpeedNorm = 50f;

        private void Start()
        {
            mainCam = GetComponentInChildren<Camera>();
            offset = transform.position - target.position;

            //-----zoom borders check-----
            if (minZoom > maxZoom)
            {
                minZoom += maxZoom;
                maxZoom = minZoom - maxZoom;
                minZoom -= maxZoom;
            }

            if (!(startingZoom >= minZoom && startingZoom <= maxZoom))
                startingZoom = (maxZoom + minZoom) / 2;
            mainCam.fieldOfView = startingZoom;
            //-----------------------------

            currentTargetPosition = target.position + offset;

            // movement parameters normalizing
            speed *= SpeedNorm;
            zoomSpeed *= ZoomSpeedNorm;
            speedResize = speed * SpeedResizeNorm;
        }

        private void Update()
        {
            Zooming(Input.GetAxisRaw("Mouse ScrollWheel"));

            if (freeViewMode && Input.GetKey(freeModeKey))
            {
                FreeCameraMovement();
            }
            else
            {
                TrackingCameraMovement();
            }
        }

        private void Zooming(float scrolling)
        {
            if (scrolling != 0)
            {
                var scroll = Input.GetAxis("Mouse ScrollWheel");
                var fieldOfView = mainCam.fieldOfView;
                fieldOfView -= scroll * zoomSpeed * Time.deltaTime;
                mainCam.fieldOfView = fieldOfView;
                mainCam.fieldOfView = Mathf.Clamp(fieldOfView, minZoom, maxZoom);
                speed -= scrolling * speedResize;
            }
        }

        private void TrackingCameraMovement()
        {
            currentTargetPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, currentTargetPosition, smoothing * Time.deltaTime);
        }

        private void FreeCameraMovement()
        {
            if (!freeViewBorders || Input.mousePosition.x < bordersLength)
            {
                var moveVector = new Vector3(speed, 0, 0);
                transform.position -= moveVector;
                if (transform.position.x < target.position.x - 10.0)
                {
                    transform.position += moveVector;
                }
            }

            if (!freeViewBorders || Input.mousePosition.x > Screen.width - bordersLength)
            {
                var moveVector = new Vector3(speed, 0, 0);
                transform.position += moveVector;
                if (transform.position.x > target.position.x + 10.0)
                {
                    transform.position -= moveVector;
                }
            }

            if (!freeViewBorders || Input.mousePosition.y < bordersLength)
            {
                var moveVector = new Vector3(0, 0, speed);
                transform.position -= moveVector;
                if (transform.position.z < target.position.z - 10.0)
                {
                    transform.position += moveVector;
                }
            }

            if (!freeViewBorders || Input.mousePosition.y > Screen.height - bordersLength)
            {
                var moveVector = new Vector3(0, 0, speed);
                transform.position += moveVector;
                if (transform.position.z > target.position.z + 10.0)
                {
                    transform.position -= moveVector;
                }
            }
        }
    }
}