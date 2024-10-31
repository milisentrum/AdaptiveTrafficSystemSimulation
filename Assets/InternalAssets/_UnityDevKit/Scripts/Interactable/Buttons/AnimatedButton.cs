using System.Collections;
using MyBox;
using UnityDevKit.Optimization;
using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Interactables.Buttons
{
    public class AnimatedButton : CachedMonoBehaviour
    {
        [Header("Settings")] [SerializeField] [InitializationField]
        private Transform movablePart;

        [SerializeField] [InitializationField] private bool startPressedState = false;

        [SerializeField] [InitializationField] [PositiveValueOnly]
        private float transitionTime = 0.1f;

        [InitializationField] [SerializeField] [PositiveValueOnly] [Range(0.001f, 99.9f)] 
        private float epsilonPercentage = 0.05f;

        [Header("Move delta")] [SerializeField] [InitializationField]
        private Vector3 moveDelta = new Vector3(0f, -0.5f, 0f);

        [Header("Events")] [SerializeField] private UnityEvent onBtnToggle;
        [SerializeField] private UnityEvent onBtnDown;
        [SerializeField] private UnityEvent onBtnUp;

        private bool isPressed;

        private Vector3 startPosition;
        private Vector3 movedPosition;

        private float transitionSpeed;
        private float epsilon;

        private Coroutine currentCoroutine;

        private void Start()
        {
            isPressed = startPressedState;
            startPosition = movablePart.localPosition;
            movedPosition = startPosition + moveDelta;
            var transitionDistance = Vector3.Distance(startPosition, movedPosition);
            transitionSpeed = transitionDistance / transitionTime;

            epsilon = transitionDistance * epsilonPercentage;

            if (isPressed)
            {
                movablePart.localPosition = movedPosition;
            }
        }

        public void Toggle()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            StartCoroutine(ToggleProcess());
        }

        public void ToggleWithReturn()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                isPressed = startPressedState;
            }

            StartCoroutine(FullMoveProcess());
        }

        public void SetDown()
        {
            if (isPressed)
            {
                return;
            }

            Toggle();
        }

        public void SetUp()
        {
            if (!isPressed)
            {
                return;
            }

            Toggle();
        }

        private IEnumerator ToggleProcess()
        {
            if (isPressed)
            {
                movablePart.localPosition = movedPosition; // Force to movement start position 
                currentCoroutine = StartCoroutine(MoveProcess(startPosition));
            }
            else
            {
                movablePart.localPosition = startPosition; // Force to movement start position 
                currentCoroutine = StartCoroutine(MoveProcess(movedPosition));
            }

            isPressed = !isPressed;

            yield return currentCoroutine;
            onBtnToggle.Invoke();
            if (isPressed)
            {
                onBtnDown.Invoke();
            }
            else
            {
                onBtnUp.Invoke();
            }
        }

        private IEnumerator MoveProcess(Vector3 targetPosition)
        {
            {
                while (Vector3.Distance(movablePart.localPosition, targetPosition) > epsilon)
                {
                    movablePart.localPosition = Vector3.MoveTowards(movablePart.localPosition, targetPosition,
                        transitionSpeed * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }

                movablePart.localPosition = targetPosition;
            }
        }

        private IEnumerator FullMoveProcess()
        {
            yield return ToggleProcess();
            yield return ToggleProcess();
        }
    }
}