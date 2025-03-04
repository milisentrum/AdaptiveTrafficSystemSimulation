using AdaptiveTrafficSystem.Tracking.Parameters;
using MyBox;
using TMPro;
using UnityDevKit.Utils.Strings;
using UnityEngine;

namespace AdaptiveTrafficSystem.UI
{
    public class BandwidthParameterDisplay : MonoBehaviour
    {
        [SerializeField] private BandwidthParameter bandwidthParameter;
        [SerializeField] private TMP_Text textHolder;
        [SerializeField] [InitializationField] private float refreshPeriod = 0.5f;

        private void Start()
        {
            InvokeRepeating(nameof(Refresh), refreshPeriod, refreshPeriod);
        }

        private void Refresh()
        {
            textHolder.text = bandwidthParameter.GetValue().ToStringWithAccuracy(1);
        }
    }
}