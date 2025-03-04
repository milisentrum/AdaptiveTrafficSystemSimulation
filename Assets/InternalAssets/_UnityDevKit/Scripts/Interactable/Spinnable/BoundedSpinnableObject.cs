using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Interactables.Spinnable
{
    public class BoundedSpinnableObject : SpinnableObject
    {
        [Header("Bounds")] 
        [SerializeField] protected bool xBounds;

        [SerializeField] [ConditionalField(nameof(xBounds))]
        protected RangedFloat xBoundsRange;

        [SerializeField] protected bool yBounds;

        [SerializeField] [ConditionalField(nameof(yBounds))]
        protected RangedFloat yBoundsRange;

        [SerializeField] protected bool zBounds;

        [SerializeField] [ConditionalField(nameof(zBounds))]
        protected RangedFloat zBoundsRange;

        [SerializeField] protected UnityEvent<Vector3> onSpinEvent;

        private Vector3 _startRot;
        protected Vector3 currentRot;

        protected override void Start()
        {
            base.Start();
            _startRot = localAxis ? CachedTransform.localEulerAngles : CachedTransform.eulerAngles;
            currentRot = Vector3.zero;
        }

        protected override void SpinOnLocalAxis(float spinX, float spinY)
        {
            if (!IsInBounds(spinX, spinY)) return;
            base.SpinOnLocalAxis(spinX, spinY);
            ChangeCurrentRot(spinX, spinY);
        }

        protected override void SpinOnGlobalAxis(float spinX, float spinY)
        {
            if (!IsInBounds(spinX, spinY)) return;
            base.SpinOnGlobalAxis(spinX, spinY);
            ChangeCurrentRot(spinX, spinY);
        }

        protected virtual bool IsInBounds(float spinX, float spinY)
        {
            var futureX = currentRot.x + spinX;
            var futureY = currentRot.y + spinY;
            var futureZ = currentRot.z + spinX;

            var inBounds =
                !(xBounds && (futureX > xBoundsRange.Max || futureX < xBoundsRange.Min)) &&
                !(yBounds && (futureY > yBoundsRange.Max || futureY < yBoundsRange.Min)) &&
                !(zBounds && (futureZ > zBoundsRange.Max || futureZ < zBoundsRange.Min));
            return inBounds;
        }

        private void ChangeCurrentRot(float spinX, float spinY)
        {
            currentRot = new Vector3(
                xBounds ? Mathf.Clamp(currentRot.x + spinX, xBoundsRange.Min, xBoundsRange.Max) : _startRot.x,
                yBounds ? Mathf.Clamp(currentRot.y + spinY, yBoundsRange.Min, yBoundsRange.Max) : _startRot.y,
                zBounds ? Mathf.Clamp(currentRot.z + spinX, zBoundsRange.Min, zBoundsRange.Max) : _startRot.z
            );

            SpinEventInvoke();
        }

        protected virtual void SpinEventInvoke()
        {
            onSpinEvent.Invoke(currentRot);
        }
    }
}