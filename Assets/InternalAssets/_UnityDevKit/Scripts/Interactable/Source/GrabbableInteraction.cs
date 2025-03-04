using System;
using System.Collections;
using System.Linq;
using MyBox;
using UnityDevKit.Interactable.Grabbable;
using UnityEngine;

namespace UnityDevKit.Interactables
{
    public class GrabbableInteraction : MonoBehaviour, IInteractionExtension
    {
        [SerializeField] [InitializationField] private float grabbingTime = 0.2f;
        [SerializeField] private GrabHolder[] grabHolders;

        [Serializable]
        public sealed class GrabHolder
        {
            public Transform HoldTransform;
            public GrabbableObject CurrentObject;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.X))
            {
                Throw();
            }
        }

        public bool Grab(GrabbableObject grabbableObject)
        {
            var firstFreeHolder = grabHolders.FirstOrDefault(holder => holder.CurrentObject == null);
            if (firstFreeHolder == null) return false;
            firstFreeHolder.CurrentObject = grabbableObject;
            TakeObjectToHolder(firstFreeHolder);
            return true;
        }

        private void Throw()
        {
            var firstHolderWithObject = grabHolders.FirstOrDefault(holder => holder.CurrentObject != null);
            if (firstHolderWithObject == null) return;
            firstHolderWithObject.CurrentObject.ResetState();
            firstHolderWithObject.CurrentObject = null;
        }

        private void TakeObjectToHolder(GrabHolder holder)
        {
            StartCoroutine(MoveProcess(holder));
        }

        private IEnumerator MoveProcess(GrabHolder grabHolder)
        {
            const float epsilon = 0.05f;
            const float accelerationModifier = 1.05f;
            var grabbingObjectTransform = grabHolder.CurrentObject.RootTransform;
            var holderTransform = grabHolder.HoldTransform;

            var positionTransitionSpeed =
                Vector3.Distance(grabbingObjectTransform.position, holderTransform.position) / grabbingTime;
            var rotationTransitionSpeed =
                Vector3.Distance(grabbingObjectTransform.eulerAngles, holderTransform.eulerAngles) /
                grabbingTime;

            while (Vector3.Distance(grabbingObjectTransform.position, holderTransform.position) > epsilon ||
                   Vector3.Distance(grabbingObjectTransform.eulerAngles, holderTransform.eulerAngles) > epsilon)
            {
                grabbingObjectTransform.position = Vector3.Lerp(
                    grabbingObjectTransform.position,
                    holderTransform.position,
                    positionTransitionSpeed * Time.deltaTime);
                
                grabbingObjectTransform.eulerAngles = Vector3.Lerp(
                    grabbingObjectTransform.eulerAngles,
                    holderTransform.eulerAngles,
                    rotationTransitionSpeed * Time.deltaTime);
                
                positionTransitionSpeed *= accelerationModifier;
                rotationTransitionSpeed *= accelerationModifier;
                yield return new WaitForEndOfFrame();
            }

            grabbingObjectTransform.SetParent(holderTransform);
            grabbingObjectTransform.localPosition = Vector3.zero;
            grabbingObjectTransform.localEulerAngles = Vector3.zero;
        }
    }
}