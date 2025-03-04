using MyBox;
using UnityEngine;

namespace UnityDevKit.Interactables.Spinnable
{
    public class SpinSound : MonoBehaviour
    {
        [Header("Main settings")] [SerializeField] private Transform spinObjectTransform;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AxisT axis;

        [Header("Tracking settings")] 
        [SerializeField] [PositiveValueOnly] private float startSpinDelta = 3;
        [SerializeField] [PositiveValueOnly] private float checkPeriod = 0.1f;
        [SerializeField] [PositiveValueOnly] private float maxSpinDelta = 3;
        [SerializeField] [PositiveValueOnly] private float maxCounterDelta = 3;
        [SerializeField] [PositiveValueOnly] private int lowDeltaMaxCount = 10;
        
        [SerializeField] private AnimationCurve curve;

        private float spinDelta = 0;
        private float previousAngle = 0;
        private float counter = 0;

        private bool isSpinning = false;

        private float baseVolume;

        public enum AxisT
        {
            XAxis,
            YAxis,
            ZAxis
        };

        private void Start()
        {
            baseVolume = audioSource.volume;

            previousAngle = GetCurrentAngle();
        }

        public void StartSound()
        {
            if (isSpinning) return;
            var currentAngle = GetCurrentAngle();
            spinDelta = GetCurrentAngleDelta(currentAngle);
            if (spinDelta > startSpinDelta)
            {
                audioSource.Play();
                previousAngle = currentAngle;
                isSpinning = true;
                counter = 0;
                InvokeRepeating(nameof(CheckSpin), 0, checkPeriod);
            }
        }

        public void StopSound()
        {
            isSpinning = false;
            audioSource.Stop();
            CancelInvoke();
        }

        private void CheckSpin()
        {
            var currentAngle = GetCurrentAngle();
            spinDelta = GetCurrentAngleDelta(currentAngle);
            previousAngle = currentAngle;
            
            CalculateCounter(spinDelta);
            TweakSound();

            if (counter >= lowDeltaMaxCount)
            {
                StopSound();
            }
        }

        private void CalculateCounter(float spinDelta)
        {

            var normalizeSpinDelta = Mathf.Min(spinDelta, maxSpinDelta) / maxSpinDelta;
            var normalizedCounterDelta = curve.Evaluate(normalizeSpinDelta);

            var counterDelta = (normalizedCounterDelta - 0.5f) * maxCounterDelta * 2;
            counter = Mathf.Max(counter - counterDelta, 0);
        }

        private void TweakSound()
        {
            var ratio = 1 - Mathf.Clamp(counter / lowDeltaMaxCount, 0, 1);
            audioSource.volume = baseVolume * ratio;
        }

        private float GetCurrentAngle() => axis == AxisT.XAxis ? spinObjectTransform.localEulerAngles.x :
            axis == AxisT.YAxis ? spinObjectTransform.localEulerAngles.y : spinObjectTransform.localEulerAngles.z;

        private float GetCurrentAngleDelta(float currentAngle) => Mathf.Abs(currentAngle - previousAngle);
    }
}