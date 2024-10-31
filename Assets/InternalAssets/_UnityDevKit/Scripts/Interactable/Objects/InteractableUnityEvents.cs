using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityDevKit.Interactables
{
    public class InteractableUnityEvents : InteractableBase
    {
        [FormerlySerializedAs("OnInteractEvent")] [Header("Events")] public UnityEvent onInteractEvent;
        [FormerlySerializedAs("OnAfterInteractEvent")] public UnityEvent onAfterInteractEvent;
        [FormerlySerializedAs("OnFocusEvent")] public UnityEvent onFocusEvent;
        [FormerlySerializedAs("OnDeFocusEvent")] public UnityEvent onDeFocusEvent;
        [FormerlySerializedAs("OnDeFocusEvent")] public UnityEvent<bool> onActivateStateChangeEvent;
        [FormerlySerializedAs("OnDeFocusEvent")] public UnityEvent onActivateEvent;
        [FormerlySerializedAs("OnDeFocusEvent")] public UnityEvent onDeactivateEvent;

        protected override void Interact()
        {
            base.Interact();
            onInteractEvent.Invoke();
        }

        public override void AfterInteract()
        {
            base.AfterInteract();
            onAfterInteractEvent.Invoke();
        }

        public override void Focus(InteractionBase source)
        {
            base.Focus(source);
            onFocusEvent.Invoke();
        }

        public override void DeFocus()
        {
            base.DeFocus();
            onDeFocusEvent.Invoke();
        }

        public override void Activate(bool value)
        {
            base.Activate(value);
            onActivateStateChangeEvent.Invoke(value);
            if (value)
            {
                onActivateEvent.Invoke();
            }
            else
            {
                onDeactivateEvent.Invoke();
            }
        }
    }
}