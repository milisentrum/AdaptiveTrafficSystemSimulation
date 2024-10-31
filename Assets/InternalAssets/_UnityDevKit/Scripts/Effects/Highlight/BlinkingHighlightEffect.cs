using System.Collections;
using UnityEngine;

namespace UnityDevKit.Effects
{
    public class BlinkingHighlightEffect : HighlightEffect
    {
        [SerializeField] private float minIntensity = 50f;
        [SerializeField] private float maxIntensity = 100f;
        [SerializeField] private float blinkPeriod = 0.05f;
        [SerializeField] private float blinkIntensityChange = 0.5f;

        private Coroutine _currentCoroutine;

        public override void Apply()
        {
            base.Apply();

            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            _currentCoroutine = StartCoroutine(Blinking());
        }

        private IEnumerator Blinking()
        {
            var currentPower = GetEffectPower();
            while (true)
            {
                while (currentPower < maxIntensity)
                {
                    currentPower += blinkIntensityChange;
                    SetEffectPower(currentPower);
                    yield return new WaitForSeconds(blinkPeriod);
                }

                currentPower = maxIntensity;
                SetEffectPower(currentPower);

                while (currentPower > minIntensity)
                {
                    currentPower -= blinkIntensityChange;
                    SetEffectPower(currentPower);
                    yield return new WaitForSeconds(blinkPeriod);
                }

                currentPower = minIntensity;
                SetEffectPower(currentPower);
            }
        }
    }
}