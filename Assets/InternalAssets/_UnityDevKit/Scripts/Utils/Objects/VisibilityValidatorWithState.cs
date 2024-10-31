using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace UnityDevKit.Utils.Objects
{
    [Serializable]
    public class VisibilityValidatorWithState : IVisibilityValidator
    {
        [SerializeField] private VisibilityValidator validator;
        [SerializeField] [PositiveValueOnly] private float validateInterval = 0.5f;
        [SerializeField] [PositiveValueOnly] private int validateCount = 3;
        
        public bool IsVisible { get; private set; } 
        
        private readonly List<bool> lastVisibleResults = new List<bool>();

        public IEnumerator StartValidating(
            Transform fromPoint, 
            Transform validationObjectTransform)
        {
            while (true)
            {
                yield return ValidateInInterval(
                    fromPoint,
                    validationObjectTransform,
                    validateInterval,
                    validateCount,
                    lastVisibleResults);
                Debug.Log("VALIDATION CHECK");
            }
        }


        public bool Validate(Transform fromPoint, Transform validationObjectTransform)
        {
            IsVisible = validator.Validate(fromPoint, validationObjectTransform);
            return IsVisible;
        }

        public IEnumerator ValidateInInterval(
            Transform fromPoint, 
            Transform validationObjectTransform, 
            float interval,
            int checksCount, 
            List<bool> isVisibleResults)
        {
            yield return validator.ValidateInInterval(
                fromPoint, 
                validationObjectTransform, 
                interval, 
                checksCount,
                isVisibleResults);
            IsVisible = isVisibleResults.Any(result => result);
        }
    }
}