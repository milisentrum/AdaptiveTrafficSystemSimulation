using AdaptiveTrafficSystem.TrafficLighters;
using UnityEngine;

namespace AdaptiveTrafficSystem.TrafficControllers
{
    [RequireComponent(typeof(IControlledEntity))]
    public class TrafficGroupEventsObserver : MonoBehaviour
    {
        [SerializeField] private TL_SyncGroup trafficController;

        private void Start()
        {
            if (trafficController == null)
            {
                Debug.LogError("[TrafficGroupEventsObserver] You have to setup trafficController");
                return;
            }

            var controlledEntity = GetComponent<IControlledEntity>();
            if (controlledEntity == null)
            {
                Debug.LogError(
                    $"[TrafficGroupEventsObserver] There's no IControlledEntity component on {gameObject.name}");
                return;
            }

            trafficController.OnSwitchToOpen.AddListener(controlledEntity.OnTrafficOpen);
            trafficController.OnSwitchToClose.AddListener(controlledEntity.OnTrafficClose);
        }
    }
}