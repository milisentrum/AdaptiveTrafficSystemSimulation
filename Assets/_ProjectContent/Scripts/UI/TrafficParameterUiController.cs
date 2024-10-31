using AdaptiveTrafficSystem.Paths;
using MyBox;
using TMPro;
using UnityDevKit.Triggers;
using UnityDevKit.UI_Handlers.TextField;
using UnityEngine;
using UnityEngine.UI;

namespace AdaptiveTrafficSystem.UI
{
    public class TrafficParameterUiController : MonoBehaviour
    {
        [Separator("Data")] 
        [SerializeField] private ControlledPath path;
        [SerializeField] private FloatTriggerEvent parameter;
        
        [Separator("UI")]
        [SerializeField] private DisplayFloatValue displayFloatValue;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text pathNameText;

        private void Start()
        {
            parameter.SubscribeToValueChanged(displayFloatValue.SendToTextField);
            parameter.SubscribeToValueChanged(SetSliderValue);

            var parameterValue = parameter.GetValue();
            displayFloatValue.SendToTextField(parameterValue);
            slider.value = parameterValue;
            pathNameText.text = path.PathName;
            slider.onValueChanged.AddListener(parameter.SetValue);
        }

        private void SetSliderValue(float value)
        {
            slider.value = value;
        }
    }
}