using System;
using System.Globalization;
using MyBox;
using UnityEngine;

namespace UnityDevKit.UI_Handlers.TextField
{
    [Serializable]
    public class DisplayFloatValue : DisplayValue<float>
    {
        [Header("Accuracy")] 
        [SerializeField] protected bool useAccuracy = false;

        [SerializeField] [ConditionalField(nameof(useAccuracy))] [Range(0, 8)] protected int accuracy = 4;
        
        protected override string ConvertToString(float value) =>
            (useAccuracy ? Math.Round(value, accuracy) : value).ToString(CultureInfo.InvariantCulture);
    }
}