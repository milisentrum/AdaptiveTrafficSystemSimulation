using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Interactables.Spinnable.Discrete
{
    public class DiscreteBoundedSpinnableObject : BoundedSpinnableObject, IDiscreteSpinnable
    {
        [Header("Discrete settings")] [SerializeField]
        private bool setStartValues;

        [SerializeField] [ConditionalField(nameof(setStartValues))]
        private List<float> startValues;

        [SerializeField] private UnityEvent onMinAngle;
        [SerializeField] private UnityEvent onMaxAngle;

        protected List<float> discreteValues = new List<float>();
        protected Vector3 currentDiscreteRot = Vector3.zero;

        public virtual void SetDiscreteValues(List<float> values)
        {
            discreteValues = values;
            ChangeMaxMin();
        }

        public virtual void SetNextValue()
        {
            var currentDiscreteValue = GetCurrentDiscreteValue();
            var index = GetIndexOfCurrentDiscreteValue(currentDiscreteValue);
            var nextIndex = index == -1 ? 0 : index - 1;
            if (nextIndex < 0) return;
            var nextValue = discreteValues[nextIndex];
            SetUpdatedValue(currentDiscreteValue, nextValue);
        }

        public virtual void SetPreviousValue()
        {
            var currentDiscreteValue = GetCurrentDiscreteValue();
            var index = GetIndexOfCurrentDiscreteValue(currentDiscreteValue);
            var nextIndex = index == -1 ? 0 : index + 1;
            if (nextIndex >= discreteValues.Count) return;
            var nextValue = discreteValues[nextIndex];
            SetUpdatedValue(currentDiscreteValue, nextValue);
        }

        protected override void Start()
        {
            base.Start();
            if (setStartValues)
            {
                SetDiscreteValues(startValues);
            }
        }

        protected override bool IsInBounds(float spinX, float spinY)
        {
            var axisValue = GetCurrentRotationValue();
            var canRotateUp = axisValue < discreteValues.Min();
            var canRotateDown = axisValue > discreteValues.Max();

            var futureX = currentRot.x + spinX;
            var futureY = currentRot.y + spinY;
            var futureZ = currentRot.z + spinX;
            var (canDownX, canUpX) = CanAxisUpDown(futureX, currentRot.x, canRotateDown, canRotateUp);
            var (canDownY, canUpY) = CanAxisUpDown(futureZ, currentRot.z, canRotateDown, canRotateUp);
            var (canDownZ, canUpZ) = CanAxisUpDown(futureZ, currentRot.z, canRotateDown, canRotateUp);

            var inBounds =
                !(xBounds && (futureX > xBoundsRange.Max && !canDownX || futureX < xBoundsRange.Min && !canUpX)) &&
                !(yBounds && (futureY > yBoundsRange.Max && !canDownY || futureY < yBoundsRange.Min && !canUpY)) &&
                !(zBounds && (futureZ > zBoundsRange.Max && !canDownZ || futureZ < zBoundsRange.Min & !canUpZ));
            return inBounds;
        }

        protected override void SpinEventInvoke()
        {
            var axisValue = GetCurrentRotationValue();
            var closest = discreteValues.Count > 0 ? discreteValues[0] : 0;
            if (discreteValues.Count > 1)
            {
                closest = discreteValues.Aggregate(
                    (x, y) => Math.Abs(x - axisValue) < Math.Abs(y - axisValue) ? x : y);
            }

            UpdateCurrentDiscreteValue(closest);
        }

        private void Trigger(Vector3 newDiscreteRot)
        {
            onSpinEvent.Invoke(newDiscreteRot);
        }

        private void UpdateCurrentDiscreteValue(float closest)
        {
            var newDiscreteRot = new Vector3(
                xBounds ? closest : 0f,
                yBounds ? closest : 0f,
                zBounds ? closest : 0f);

            if (newDiscreteRot != currentDiscreteRot)
            {
                if (closest == discreteValues.Min())
                {
                    onMinAngle.Invoke();
                }
                else if (closest == discreteValues.Max())
                {
                    onMaxAngle.Invoke();
                }

                Trigger(newDiscreteRot);
            }

            currentDiscreteRot = newDiscreteRot;
        }

        private void UpdateCurrentRotation(float deltaRotation)
        {
            ExecuteSpin(deltaRotation, deltaRotation);
        }

        private void ChangeMaxMin()
        {
            var max = discreteValues.Max();
            var min = discreteValues.Min();
            var updatedBounds = new RangedFloat
            {
                Min = min,
                Max = max
            };
            if (xBounds)
            {
                xBoundsRange = updatedBounds;
            }

            if (yBounds)
            {
                yBoundsRange = updatedBounds;
            }

            if (zBounds)
            {
                zBoundsRange = updatedBounds;
            }
        }

        private float GetCurrentDiscreteValue() => GetVectorValueInBounds(currentDiscreteRot);

        private float GetCurrentRotationValue() => GetVectorValueInBounds(currentRot);

        private float GetVectorValueInBounds(Vector3 vector) =>
            xBounds
                ? vector.x
                : yBounds
                    ? vector.y
                    : vector.z;

        private int GetIndexOfCurrentDiscreteValue(float axisDiscreteValue) =>
            discreteValues.IndexOf(axisDiscreteValue);

        private void SetUpdatedValue(float currentDiscreteValue, float updatedValue)
        {
            UpdateCurrentRotation(updatedValue - currentDiscreteValue);
            UpdateCurrentDiscreteValue(updatedValue);
            currentRot = currentDiscreteRot;
        }

        private static (bool, bool) CanAxisUpDown(float futureAxis, float currentRotAxis, bool canRotateDown,
            bool canRotateUp)
        {
            var canAxisDown = futureAxis < currentRotAxis && canRotateDown;
            var canAxisUp = futureAxis > currentRotAxis && canRotateUp;
            return (canAxisDown, canAxisUp);
        }
    }
}