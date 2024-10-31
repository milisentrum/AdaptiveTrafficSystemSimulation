using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Events
{
    public class InvokeEvents : MonoBehaviour
    {
        [Header("Invoke event")]
        [SerializeField] private bool useInvoke = true;
        [SerializeField] private UnityEvent onInvokeEvent;
        [SerializeField] private float invokeTime;

        [Header("InvokeRepeating event")]
        [SerializeField] private bool useInvokeRepeating = false;
        [SerializeField] private UnityEvent onInvokeRepeatingEvent;
        [SerializeField] private float repeatingStartTime;
        [SerializeField] private float repeatingTime;

        private void Start()
        {
            if (useInvoke)
            {
                Invoke(nameof(TriggerInvokeEvent), invokeTime);
            }

            if (useInvokeRepeating)
            {
                InvokeRepeating(nameof(TriggerInvokeRepeatingEvent), repeatingStartTime, repeatingTime);
            }
        }

        private void TriggerInvokeEvent()
        {
            onInvokeEvent.Invoke();
        }

        private void TriggerInvokeRepeatingEvent()
        {
            onInvokeRepeatingEvent.Invoke();
        }
    }
}