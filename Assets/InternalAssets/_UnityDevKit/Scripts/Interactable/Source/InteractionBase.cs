using UnityEngine;

namespace UnityDevKit.Interactables
{
    public class InteractionBase : MonoBehaviour
    {
        private InteractableBase currentInteractable;
        private bool isFocused;

        private void Awake()
        {
            LoadExtensions();
        }

        protected virtual void Start()
        {
            isFocused = false;
        }

        // ----- FOCUS -----
        public void FocusObject(InteractableBase interactable)
        {
            if (currentInteractable != null && currentInteractable != interactable)
            {
                currentInteractable.DeFocus();
            }

            isFocused = true;
            currentInteractable = interactable;
            currentInteractable.Focus(this);
        }

        public void LoseFocus()
        {
            if (currentInteractable != null)
            {
                currentInteractable.DeFocus();
                currentInteractable = null;
            }

            isFocused = false;
        }

        // ----- INTERACTING -----
        public void Interact()
        {
            if (currentInteractable == null) return;
            currentInteractable.StartInteract();
        }

        public void AfterInteract()
        {
            if (currentInteractable == null) return;
            currentInteractable.AfterInteract();
        }

        public void ForceInteractable(InteractableBase interactableBase)
        {
            currentInteractable = interactableBase;
        }

        public IInteractionExtension[] Extensions { get; private set; }

        private void LoadExtensions()
        {
            Extensions = GetComponentsInChildren<IInteractionExtension>();
        }
    }
}