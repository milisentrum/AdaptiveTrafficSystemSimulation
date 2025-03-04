using System;
using System.Globalization;
using MyBox;
using TMPro;
using UnityEngine;

namespace UnityDevKit.Parameters
{
    public class ParameterVisualizer : MonoBehaviour
    {
        [SerializeField] private Parameter parameter;
        
        [Header("Text holders")]
        [SerializeField] private TMP_Text nameTextHolder;
        [SerializeField] private TMP_Text valueTextHolder;
        //[SerializeField] private TMP_Text unitsTextHolder;

        [Header("Accuracy")] [SerializeField] private bool useAccuracy = false;

        [SerializeField] [ConditionalField(nameof(useAccuracy))] [Range(0, 8)]
        private int accuracy = 4;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            nameTextHolder.text = parameter.GetName();
            //unitsTextHolder.text = parameter.GetUnits();
            RedrawValue(parameter.GetValue());
            parameter.SubscribeToValueChanged(RedrawValue);
        }

        public void RedrawValue(float value)
        {
            var newValueText =
                (useAccuracy ? Math.Round(value, accuracy) : value).ToString(CultureInfo.InvariantCulture);
            valueTextHolder.text = $"{newValueText} {parameter.GetUnits()}"; // TODO
        }
    }
}