using System.Collections;
using MyBox;
using UnityEngine;

namespace UnityDevKit.Parameters
{
    public class ContinuousParameter : Parameter
    {
        [Header("Continuous value change")] 
        [SerializeField] [PositiveValueOnly] private float transitionTime = 1.5f;
        [SerializeField] [PositiveValueOnly] [Range(1, 60)] private int updateRate = 15;

        private Coroutine _currentCoroutine;

        public override void SetValue(float newValue)
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            _currentCoroutine = StartCoroutine(SmoothValueChange(newValue));
        }

        private IEnumerator SmoothValueChange(float targetValue)
        {
            var valueChangeDelta = (targetValue - Value) / updateRate;

            var timeStep = transitionTime / updateRate;

            for (var i = 0; i < updateRate; i++)
            {
                Value += valueChangeDelta;
                ValueChanged(Value);
                yield return new WaitForSeconds(timeStep);
            }

            Value = targetValue;
            ValueChanged(Value);
        }
    }
}