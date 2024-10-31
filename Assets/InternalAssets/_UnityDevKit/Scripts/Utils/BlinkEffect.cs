using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityDevKit.Utils
{
    public class BlinkEffect : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        [Range(0, 1)] [SerializeField] private float fadeSpeed = 0.001f;
        [Range(0, 1)] [SerializeField] private float unFadeSpeed = 0.0025f;
        [SerializeField] private UnityEvent onTeleport;
        private Coroutine _blinkingCoroutine;

        public void Blink()
        {
            Blink(fadeSpeed);
        }

        public void Blink(float customFadeSpeed)
        {
            if (_blinkingCoroutine != null)
            {
                StopCoroutine(_blinkingCoroutine);
            }

            _blinkingCoroutine = StartCoroutine(StartBlink(customFadeSpeed));
        }

        public void Fade()
        {
            Fade(fadeSpeed);
        }
        
        
        public void Fade(float customFadeSpeed)
        {
            if (_blinkingCoroutine != null)
            {
                StopCoroutine(_blinkingCoroutine);
            }
            
            _blinkingCoroutine = StartCoroutine(FadeProcess(customFadeSpeed));
        }

        public void UnFade()
        {
            UnFade(unFadeSpeed);
        }

        public void UnFade(float customFadeSpeed)
        {
            if (_blinkingCoroutine != null)
            {
                StopCoroutine(_blinkingCoroutine);
            }
            
            _blinkingCoroutine = StartCoroutine(UnFadeProcess(customFadeSpeed));
        }

        private IEnumerator FadeProcess(float _fadeSpeed)
        {
            var objectColor = fadeImage.color;

            while (fadeImage.color.a < 1)
            {
                var fadeAmount = objectColor.a + (_fadeSpeed + Time.deltaTime);
                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                fadeImage.color = objectColor;
                yield return null;
            }
        }

        private IEnumerator UnFadeProcess(float _fadeSpeed)
        {
            var objectColor = fadeImage.color;
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, 1);
            fadeImage.color = objectColor;

            while (fadeImage.color.a > 0)
            {
                var fadeAmount = objectColor.a - (_fadeSpeed + Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                fadeImage.color = objectColor;
                yield return null;
            }
        }

        private IEnumerator StartBlink(float _fadeSpeed)
        {
            yield return FadeProcess(_fadeSpeed);
            yield return UnFadeProcess(_fadeSpeed);
        }
    }
}