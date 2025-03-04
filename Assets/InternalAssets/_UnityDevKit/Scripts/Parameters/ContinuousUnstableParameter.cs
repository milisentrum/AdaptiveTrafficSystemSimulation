using MyBox;
using UnityEngine;

namespace UnityDevKit.Parameters
{
    public class ContinuousUnstableParameter : ContinuousParameter
    {
        [SerializeField] [PositiveValueOnly] private float maxDeltaChange = 5f;
        [SerializeField] [PositiveValueOnly] private float changePeriod = 2f;

        private float _stableValue;

        public void SetStableValue(float value)
        {
            SetValue(value);
            _stableValue = value;
        }
        
        public void StartUnstableChanges()
        {
            InvokeRepeating(nameof(UnstableChange), 0f, changePeriod);
        }

        public void StopUnstableChanges()
        {
            CancelInvoke();
        }

        private void UnstableChange()
        {
            var newUnstableValue = Random.Range(_stableValue - maxDeltaChange, _stableValue + maxDeltaChange);
            SetValue(newUnstableValue);
        }
    }
}