using MyBox;
using UnityDevKit.Events;
using UnityEngine;

namespace AdaptiveTrafficSystem.Pedestrians.Modules
{
    public class PedestrianStates : MonoBehaviour, IPedestrianModule
    {
        [SerializeField] [ReadOnly] private bool isMoving = true;
        [SerializeField] [ReadOnly] private bool isCrossing = false;
        
        public bool IsMoving => isMoving;
        public bool IsCrossing => isCrossing;
        
        public EventHolderBase OnCommonStateActivated { get; private set; } = new EventHolderBase();
        public EventHolderBase OnWaitingStateActivated { get; private set; } = new EventHolderBase();
        public EventHolderBase OnCrossingStateActivated { get; private set; } = new EventHolderBase();
        public EventHolderBase OnCrossingStateDeactivated { get; private set; } = new EventHolderBase();
        

        public void Init()
        {
        }

        public void SetWaiting()
        {
            isMoving = false;
            OnWaitingStateActivated.Invoke();
        }

        public void SetMoving()
        {
            isMoving = true;
            OnCommonStateActivated.Invoke();
        }
        
        public void SetCrossing()
        {
            isCrossing = true;
            OnCrossingStateActivated.Invoke();
        }

        public void SetNotCrossing()
        {
            isCrossing = false;
            OnCrossingStateDeactivated.Invoke();
        }
    }
}