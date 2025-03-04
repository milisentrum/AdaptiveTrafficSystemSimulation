using System;
using TMPro;
using UnityEngine;

namespace UnityDevKit.UI_Handlers.TextField
{
    [Serializable]
    public abstract class DisplayValue<T>
    {
        [SerializeField] private string beforeValueText;
        [SerializeField] protected TMP_Text displayText;

        public void SendToTextField(T value)
        {
            var convertedText = ConvertToString(value);
            displayText.text = string.IsNullOrEmpty(beforeValueText)
                ? convertedText
                : $"{beforeValueText}{convertedText}";
        }

        protected abstract string ConvertToString(T value);
    }
}