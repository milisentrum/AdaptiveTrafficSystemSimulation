using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityDevKit.Utils
{
    public class ObjectMouseRotation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject rotatableObject;

        [SerializeField] private float speed = 0.1f;

        private bool rotateState = false;
        private int mousePositionX;

        private void FixedUpdate()
        {
            if (rotateState)
            {
                rotatableObject.transform.Rotate(
                    0,
                    (mousePositionX - (int) Input.mousePosition.x) * speed * Time.deltaTime,
                    0, Space.Self);
            }
        }

        public void OnPointerDown(PointerEventData pointerEventData)
        {
            mousePositionX = (int) Input.mousePosition.x;
            rotateState = true;
        }

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            rotateState = false;
        }
    }
}