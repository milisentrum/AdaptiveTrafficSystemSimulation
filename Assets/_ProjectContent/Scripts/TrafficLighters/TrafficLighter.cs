using System.Collections;
using UnityEngine;
using MyBox;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public class TrafficLighter : TrafficLighterBase
    {
        [Separator("Lighters")]
        [SerializeField] private GameObject greenLighter;
        [SerializeField] private GameObject yellowLighter;
        [SerializeField] private GameObject redLighter;

        const int GREEN_BLINKING_COUNT = 2;
        const float GREEN_BLINKING_PERIOD_DURATION = 0.5f;
        const int YELLOW_PHASE_DURATION = 1;
        const int RED_YELLOW_PHASE_DURATION = 2;

        protected override void Start()
        {
            base.Start();
            ActivateYellowLighter();
        }

        protected override IEnumerator SwitchToGreen()
        {
            greenLighter.SetActive(false);
            yellowLighter.SetActive(true);
            redLighter.SetActive(true);
            yield return new WaitForSeconds(RED_YELLOW_PHASE_DURATION);
            ActivateGreenLighter();
            _trafficMode = TrafficMode.OPEN;
        }

        protected override IEnumerator SwitchToRed()
        {
            yellowLighter.SetActive(false);
            redLighter.SetActive(false);
            var c = GREEN_BLINKING_COUNT;
            while (c > 0)
            {
                greenLighter.SetActive(false);
                yield return new WaitForSeconds(GREEN_BLINKING_PERIOD_DURATION);
                greenLighter.SetActive(true);
                yield return new WaitForSeconds(GREEN_BLINKING_PERIOD_DURATION);
                c--;
            }
            greenLighter.SetActive(false);
            yellowLighter.SetActive(true);
            redLighter.SetActive(false);
            _trafficMode = TrafficMode.CLOSE;
            yield return new WaitForSeconds(YELLOW_PHASE_DURATION);
            ActivateRedLighter();
        }

        public void ActivateGreenLighter()
        {
            greenLighter.SetActive(true);
            yellowLighter.SetActive(false);
            redLighter.SetActive(false);
        }

        public void ActivateRedLighter()
        {
            greenLighter.SetActive(false);
            yellowLighter.SetActive(false);
            redLighter.SetActive(true);
        }

        public void ActivateYellowLighter()
        {
            greenLighter.SetActive(false);
            yellowLighter.SetActive(true);
            redLighter.SetActive(false);
        }

        public float GetSwitchingTimeToGreen() => RED_YELLOW_PHASE_DURATION;
        public float GetSwitchingTimeToRed() => YELLOW_PHASE_DURATION;

        public string GetCurrentLightState()
        {
            bool g = greenLighter.activeSelf;
            bool y = yellowLighter.activeSelf;
            bool r = redLighter.activeSelf;
            if (g && !y && !r) return "green";
            if (!g && y && !r) return "yellow";
            if (!g && !y && r) return "red";
            //if (!g && y && r) return "red-yellow";
            return "unknown";
        }
    }
}
