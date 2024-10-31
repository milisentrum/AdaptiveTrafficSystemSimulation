using System;
using MyBox;
using UnityEngine;

namespace UnityDevKit.Utils.Objects
{
    public class VisibilityValidatorExample : MonoBehaviour
    {
        [SerializeField] private VisibilityValidatorWithState visibilityValidator;

        [SerializeField] private Transform observer;
        [SerializeField] private Transform validationObject;

        [ReadOnly] private bool isVisible;

        private Coroutine currentValidatingCoroutine;
        
        private void Start()
        {
            ValidateInInterval();
        }
        
        private void ValidateInInterval()
        {
            if (currentValidatingCoroutine != null)
            {
                StopCoroutine(currentValidatingCoroutine);
            }
            currentValidatingCoroutine = StartCoroutine(
                visibilityValidator.StartValidating(observer, validationObject));
        }

        private void StopValidating()
        {
            if (currentValidatingCoroutine != null)
            {
                StopCoroutine(currentValidatingCoroutine);
            }
        }

        private void Update()
        {
            isVisible = visibilityValidator.IsVisible;

            if (Input.GetKeyUp(KeyCode.V))
            {
                ValidateInInterval();
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                StopValidating();
            }
        }
    }
}