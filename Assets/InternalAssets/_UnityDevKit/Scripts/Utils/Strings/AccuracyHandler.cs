using System;
using System.Globalization;
using MyBox;
using UnityEngine;

namespace UnityDevKit.Utils.Strings
{
    [Serializable]
    public class AccuracyHandler
    {
        [Header("Accuracy")] 
        [SerializeField] protected bool useAccuracy = false;
        [SerializeField] [ConditionalField(nameof(useAccuracy))] [Range(0, 8)] protected int accuracy = 4;
        
        public string ConvertToString(float value) =>
            (useAccuracy ? Math.Round(value, accuracy) : value).ToString(CultureInfo.InvariantCulture); // TODO -- culture setup
    }
}