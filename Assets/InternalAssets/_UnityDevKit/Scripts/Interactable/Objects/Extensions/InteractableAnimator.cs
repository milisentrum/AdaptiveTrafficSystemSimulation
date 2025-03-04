using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityDevKit.Interactables.Extensions
{
    [RequireComponent(typeof(Animator))]
    public class InteractableAnimator : MonoBehaviour
    {
        [FormerlySerializedAs("OnAnimationPlay")] public UnityEvent onAnimationPlay;
        [FormerlySerializedAs("OnAnimationOpen")] public UnityEvent onAnimationOpen;
        [FormerlySerializedAs("OnAnimationClose")] public UnityEvent onAnimationClose;
        [FormerlySerializedAs("AfterCloseAnim")] public UnityEvent afterCloseAnim;
        [FormerlySerializedAs("AfterOpenAnim")] public UnityEvent afterOpenAnim;

        private Animator animator;
        private static readonly int IsOpen = Animator.StringToHash("Interacted");

        private bool isOpen;

        private void Start()
        {
            animator = GetComponent<Animator>();

            isOpen = animator.GetBool(IsOpen);
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            animator.SetBool(IsOpen, isOpen);
            StartCoroutine(ToggleCoroutine());
        }

        private IEnumerator ToggleCoroutine()
        {
            while (animator.GetCurrentAnimatorClipInfo(0).Length == 0)
            {
                yield return null;
            }

            onAnimationPlay.Invoke();

            if (isOpen)
            {
                onAnimationOpen.Invoke();
                var clipLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
                Invoke(nameof(InvokeAfterOpenAnim), clipLength);
            }
            else
            {
                var clipLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
                Invoke(nameof(InvokeAfterCloseAnim), clipLength);
                onAnimationClose.Invoke();
            }
        }

        private void InvokeAfterOpenAnim()
        {
            afterOpenAnim.Invoke();
        }

        private void InvokeAfterCloseAnim()
        {
            afterCloseAnim.Invoke();
        }
    }
}