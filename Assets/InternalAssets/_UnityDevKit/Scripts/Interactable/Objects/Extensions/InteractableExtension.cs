using UnityEngine;

namespace UnityDevKit.Interactables.Extensions
{
    [RequireComponent(typeof(InteractableBase))]
    public abstract class InteractableExtension : MonoBehaviour
    {
        private void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
            InteractableEventsInit();
        }

        private void InteractableEventsInit()
        {
            Interactable = GetComponent<InteractableBase>();

            Interactable.OnFocus.AddListener(OnFocusAction);
            Interactable.OnDeFocus.AddListener(OnDeFocusAction);
            Interactable.OnInteract.AddListener(OnInteractAction);
            Interactable.OnInteract.AddListener(OnAfterInteractAction);

            Interactable.OnActiveStateChange.AddListener(OnActiveStateChangedAction);
            Interactable.OnStopStateChange.AddListener(OnStopStateChangeAction);
        }

        protected virtual void OnFocusAction(InteractionBase source)
        {
        }

        protected virtual void OnDeFocusAction(InteractionBase source)
        {
        }

        protected virtual void OnInteractAction(InteractionBase source)
        {
        }

        protected virtual void OnAfterInteractAction(InteractionBase source)
        {
        }

        protected virtual void OnActiveStateChangedAction(bool isActive)
        {
        }

        protected virtual void OnStopStateChangeAction(bool isStopped)
        {
        }

        public InteractableBase Interactable { get; private set; }
    }
}