using TMPro;
using UnityDevKit.Triggers;
using UnityDevKit.Utils.Strings;
using UnityEngine;

namespace AdaptiveTrafficSystem.UI
{
    public class TrafficLightersPhaseDisplay : MonoBehaviour
    {
        [SerializeField] private FloatTriggerEvent phaseHolder;
        [SerializeField] private TMP_Text phaseText;

        private float _lastValue;
        
        private void Update()
        {
            const int frameFilter = 60;
            var currentValue = phaseHolder.GetValue();
            if (Time.frameCount % frameFilter != 0 || _lastValue == currentValue) return;
            _lastValue = currentValue;
            phaseText.text = _lastValue.ToStringWithAccuracy(0);
        }
    }
}