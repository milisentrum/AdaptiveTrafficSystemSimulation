using UnityEngine;

namespace UnityDevKit.Scenario.ActionTriggers
{
    public class ActionsTriggersLauncher : MonoBehaviour
    {
        private IActionTrigger[] _triggers;

        private const float SCENARIO_START_DELAY = 0.5f;
        
        private void Awake()
        {
            _triggers = GetComponentsInChildren<IActionTrigger>();
        }

        private void Start()
        {
            Invoke(nameof(Launch), SCENARIO_START_DELAY);
        }

        public void Launch()
        {
            foreach (var actionTrigger in _triggers)
            {
                actionTrigger.SubscribeToTrigger();
            }
        }
    }
}