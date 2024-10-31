using UnityEngine;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public class TL_PedestrianSyncGroup : TL_SyncGroup
    {
        [SerializeField] private TL_SyncGroup connectedGroup;

        private void Start()
        {
            connectedGroup.OnSwitchToOpen.AddListener(SwitchToClose);
            connectedGroup.OnSwitchToClose.AddListener(SwitchToOpen);
        }
    }
}