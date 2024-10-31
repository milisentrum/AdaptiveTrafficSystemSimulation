using System.Collections.Generic;
using AdaptiveTrafficSystem.Tracking.Parameters;
using MyBox;
using TMPro;
using UnityEngine;

namespace TrafficModule.UI
{
    public class ParametersDisplay : MonoBehaviour
    {
        [SerializeField] private ParametersHolder parametersHolder;
        [SerializeField] private GameObject textPrefab;
        [SerializeField] private GameObject panel;
        [SerializeField] [PositiveValueOnly] private float refreshPeriod = 1f;

        private ITrackingParameter[] _trackingParameters;

        private readonly Dictionary<ITrackingParameter, TMP_Text> _paramsTextFields = new();

        private void Start()
        {
            const float uiShowDelay = 1f;
            Invoke(nameof(Init), uiShowDelay);
        }

        private void Init()
        {
            _trackingParameters = new ITrackingParameter[]
            {
                parametersHolder.IntensityParameter,
                parametersHolder.AvgSpeedParameter,
                parametersHolder.AvgWaitingTimeParameter,
                parametersHolder.VehicleCountParameter,
                parametersHolder.MaxWaitingTimeParameter
            };
            foreach (var trackingParam in _trackingParameters)
            {
                var newText = Instantiate(textPrefab, panel.transform);
                var textField = newText.GetComponent<TMP_Text>();
                _paramsTextFields.Add(trackingParam, textField);
                textField.text = $"{trackingParam.GetName()}: {trackingParam.GetValue()}";
            }

            InvokeRepeating(nameof(UpdateTexts), 0, refreshPeriod);
        }

        private void UpdateTexts()
        {
            foreach (var (param, textField) in _paramsTextFields)
            {
                textField.text = $"{param.GetName()}: {param.GetValue()}";
            }
        }
    }
}