using UnityDevKit.Interactables;
using UnityDevKit.Interactables.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Interactable.Grabbable
{
    [DisallowMultipleComponent]
    public class GrabbableObject : InteractableExtension
    {
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Rigidbody rb;

        [SerializeField] private UnityEvent onGrab;
        [SerializeField] private UnityEvent onThrow;
        
        private Transform lastBaseHolder;
        private bool rbIsKinematic;
        
        protected override void OnInteractAction(InteractionBase source)
        {
            base.OnInteractAction(source);
            var grabInteraction = source.GetComponentInChildren<GrabbableInteraction>();
            if (grabInteraction == null)
            {
                Debug.LogError("Player tried to grab object without GrabbableInteraction");
                return;
            }
            SaveLastInfo();
            var hasGrabbed = grabInteraction.Grab(this);

            if (hasGrabbed)
            {
                Interactable.Activate(false);
                rb.isKinematic = true;
                onGrab.Invoke();
            }
        }

        private void SaveLastInfo()
        {
            lastBaseHolder = rootTransform.parent;
            rbIsKinematic = rb.isKinematic;
        }
        
        public void ResetState()
        {
            rootTransform.SetParent(lastBaseHolder);
            rb.isKinematic = rbIsKinematic;
            
            Interactable.Activate(true);
            onThrow.Invoke();
        }

        public Transform RootTransform => rootTransform;
    }
}