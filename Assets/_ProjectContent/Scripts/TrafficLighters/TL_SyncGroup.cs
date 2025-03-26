using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityDevKit.Events;
using UnityDevKit.Triggers;
using UnityEngine;

namespace AdaptiveTrafficSystem.TrafficLighters
{
    public class TL_SyncGroup : MonoBehaviour, ITrafficController
    {
        [SerializeField] private FloatTriggerEvent openPathTime;
        [SerializeField] private List<TrafficLighter> syncLighters;

        public FloatTriggerEvent OpenPathTime => openPathTime;
        public List<TrafficLighter> SyncLighters => syncLighters;

        public EventHolderBase OnSwitchToOpen { get; private set; } = new EventHolderBase();
        public EventHolderBase OnSwitchToClose { get; private set; } = new EventHolderBase();

        public bool IsAllClosed() => syncLighters.All(lighter => lighter.GetMode() == TrafficMode.CLOSE);

        public bool IsAllOpened() => syncLighters.All(lighter => lighter.GetMode() == TrafficMode.OPEN);

        public virtual void SwitchToOpen()
        {
            foreach (var lighter in syncLighters)
            {
                lighter.SwitchToOpen();
            }

            StartCoroutine(TriggerOpenEvents());
        }

        public virtual void SwitchToClose()
        {
            foreach (var lighter in syncLighters)
            {
                lighter.SwitchToClose();
            }

            StartCoroutine(TriggerCloseEvents());
        }

        public TrafficMode GetMode()
        {
            return syncLighters.Count > 0 ? syncLighters[0].GetMode() : TrafficMode.UNKNOWN;
        }

        public IEnumerator WaitUntilSwitchToOpen()
        {
            SwitchToOpen();
            yield return WaitUntilOpened();
        }

        public IEnumerator WaitUntilSwitchToClose()
        {
            SwitchToClose();
            yield return WaitUntilClosed();
        }

        public IEnumerator WaitUntilClosed()
        {
            yield return new WaitUntil(IsAllClosed);
        }

        public IEnumerator WaitUntilOpened()
        {
            yield return new WaitUntil(IsAllOpened);
        }

        private IEnumerator TriggerOpenEvents()
        {
            yield return WaitUntilOpened();
            OnSwitchToOpen.Invoke();
        }
        
        private IEnumerator TriggerCloseEvents()
        {
            yield return WaitUntilClosed();
            OnSwitchToClose.Invoke();
        }
    }
}