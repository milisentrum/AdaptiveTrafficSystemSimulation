using System.Collections;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public class TrafficLighter : TrafficLighterBase
    {
        [Separator("Lighters")]
        [SerializeField] private GameObject greenLighter;
        [SerializeField] private GameObject yellowLighter;
        [SerializeField] private GameObject redLighter;

        private const int GREEN_BLINKING_COUNT = 2;
        private const float GREEN_BLINKING_PERIOD_DURATION = 0.5f;
        private const int YELLOW_PHASE_DURATION = 1;
        private const int RED_YELLOW_PHASE_DURATION = 2;

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
            var greenBlinkingCounter = GREEN_BLINKING_COUNT;
            while (greenBlinkingCounter > 0)
            {
                greenLighter.SetActive(false);
                yield return new WaitForSeconds(GREEN_BLINKING_PERIOD_DURATION);
                greenLighter.SetActive(true);
                yield return new WaitForSeconds(GREEN_BLINKING_PERIOD_DURATION);
                greenBlinkingCounter--;
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

        public float GetSwitchingTimeToRed() => YELLOW_PHASE_DURATION; //GREEN_BLINKING_COUNT * GREEN_BLINKING_PERIOD_DURATION * 2;


        public string GetCurrentLightState()
        {
            bool greenOn = greenLighter.activeSelf;
            bool yellowOn = yellowLighter.activeSelf;
            bool redOn = redLighter.activeSelf;

            if (greenOn && !yellowOn && !redOn)
                return "green";
            else if (!greenOn && yellowOn && !redOn)
                return "yellow";
            else if (!greenOn && !yellowOn && redOn)
                return "red";
            else if (!greenOn && yellowOn && redOn)
                return "red-yellow"; // при переходе на зеленый
            else
                return "unknown";    // мигающий, промежуточный и т.п.
        }

    }
}