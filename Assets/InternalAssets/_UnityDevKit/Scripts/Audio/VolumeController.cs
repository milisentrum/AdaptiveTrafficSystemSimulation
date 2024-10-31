using UnityEngine;
using UnityEngine.UI;

namespace UnityDevKit.Audio
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [Range(1, 100)] [SerializeField] private int changeVolumeValuePercent;

        private const float MAX_VOLUME = 100f;

        public void ChangeVolume()
        {
            AudioListener.volume = slider.value;
        }

        public void AddValue()
        {
            var value = slider.value;
            value += changeVolumeValuePercent / MAX_VOLUME;
            slider.value = value;
            slider.onValueChanged.Invoke(value);
        }

        public void DecreaseValue()
        {
            var value = slider.value;
            value -= changeVolumeValuePercent / MAX_VOLUME;
            slider.value = value;
            slider.onValueChanged.Invoke(value);
        }
    }
}