using System.Collections;
using UnityEngine;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public class PedestrianTrafficLighter : TrafficLighterBase
    {
        [SerializeField] private GameObject greenLighter;
        [SerializeField] private GameObject redLighter;

        protected override IEnumerator SwitchToGreen()
        {
            const float redToGreenPhaseDuration = 0.1f;
            
            greenLighter.SetActive(false);
            redLighter.SetActive(true);
            yield return new WaitForSeconds(redToGreenPhaseDuration);
            ActivateGreenLighter();
            _trafficMode = TrafficMode.OPEN;
        }

        protected override IEnumerator SwitchToRed()
        {
            const int greenBlinkingCount = 2;
            const float greenBlinkingPeriodDuration = 0.5f;
            
            redLighter.SetActive(false);
            var greenBlinkingCounter = greenBlinkingCount;
            while (greenBlinkingCounter > 0)
            {
                greenLighter.SetActive(false);
                yield return new WaitForSeconds(greenBlinkingPeriodDuration);
                greenLighter.SetActive(true);
                yield return new WaitForSeconds(greenBlinkingPeriodDuration);
                greenBlinkingCounter--;
            }

            _trafficMode = TrafficMode.CLOSE;
            ActivateRedLighter();
        }

        public void ActivateGreenLighter()
        {
            greenLighter.SetActive(true);
            redLighter.SetActive(false);
        }

        public void ActivateRedLighter()
        {
            greenLighter.SetActive(false);
            redLighter.SetActive(true);
        }
    }
}