using System.Collections;
using UnityEngine;
using MyBox;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public class TrafficLighter : TrafficLighterBase
    {
        [SerializeField] private string lighterID;
        public string LighterID => lighterID;
        
        
        [Separator("Lighters")]
        [SerializeField] private GameObject greenLighter;
        [SerializeField] private GameObject yellowLighter;
        [SerializeField] private GameObject redLighter;
        
        const int GREEN_BLINKING_COUNT = 2;
        const float GREEN_BLINKING_PERIOD_DURATION = 0.5f;
        const int YELLOW_PHASE_DURATION = 1;
        const int RED_YELLOW_PHASE_DURATION = 2;

        // -- Флаг для мигающего зелёного --
        private bool _isBlinkingGreen;
        //private bool _isBlinkingYellow;

        protected override void Start()
        {
            base.Start();
            ActivateYellowLighter();
        }

        protected override IEnumerator SwitchToGreen()
        {
            _isBlinkingGreen = false;

            
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
            
            _isBlinkingGreen = true;

            
            var c = GREEN_BLINKING_COUNT;
            while (c > 0)
            {
                greenLighter.SetActive(false);
                yield return new WaitForSeconds(GREEN_BLINKING_PERIOD_DURATION);
                greenLighter.SetActive(true);
                yield return new WaitForSeconds(GREEN_BLINKING_PERIOD_DURATION);
                c--;
            }
            _isBlinkingGreen = false;

            
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

        //public string GetCurrentLightState()
        //{
        //    bool g = greenLighter.activeSelf;
        //    bool y = yellowLighter.activeSelf;
        //    bool r = redLighter.activeSelf;
        //    if (g && !y && !r) return "green";
        //    if (!g && y && !r) return "yellow";
        //    if (!g && !y && r) return "red";
        //    //if (!g && y && r) return "red-yellow";
        //    return "unknown";
        //}

        public string GetCurrentLightState()
        {
            // 1) Если сейчас идёт мигание зелёного, возвращаем "green"
            if (_isBlinkingGreen)
            {
                return "green";
            }

            // 2) Смотрим, какие лампы включены прямо сейчас
            bool g = greenLighter.activeSelf;
            bool y = yellowLighter.activeSelf;
            bool r = redLighter.activeSelf;

            // 3) Учитываем возможные комбинации
            if ((!g && y && r) || (!g && !y && r)) return "red";
            if (g && !y && !r) return "green";
            if (!g && y && !r) return "yellow";

            // Если ни одна из комбинаций не подошла,
            // то, теоретически, мы получили "unknown".
            // Но теперь это будет случаться крайне редко,
            // так как мигание мы закрыли флагом _isBlinkingGreen.
            return "unknown";
        }
    }
}
