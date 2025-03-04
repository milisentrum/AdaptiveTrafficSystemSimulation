using MyBox;
using UnityEngine;
using UnityDevKit.Events;
using UnityDevKit.Utils.TimeHandlers;

namespace UnityDevKit.Interactables
{
    public class InteractableBase : MonoBehaviour
    {
        [Header("Main settings")] [SerializeField]
        private bool setStartActiveState = false;

        [SerializeField] [ConditionalField(nameof(setStartActiveState))]
        private bool activateOnStart = true;

        [SerializeField] private InteractMode interactMode = InteractMode.Instant;

        [Tooltip("Interact time is used for 'HOLDING' mode")] [SerializeField]
        private float interactTime = 1.5f;

        public readonly EventHolder<InteractionBase> OnFocus = new EventHolder<InteractionBase>();
        public readonly EventHolder<InteractionBase> OnDeFocus = new EventHolder<InteractionBase>();
        public readonly EventHolder<InteractionBase> OnInteract = new EventHolder<InteractionBase>();
        public readonly EventHolder<InteractionBase> OnAfterInteract = new EventHolder<InteractionBase>();

        public readonly EventHolder<bool> OnActiveStateChange = new EventHolder<bool>();
        public readonly EventHolder<bool> OnStopStateChange = new EventHolder<bool>();

        protected InteractionBase InteractionSource;

        private bool isActivated = DefaultActiveState;
        private bool isStopped;

        private const bool DefaultActiveState = true;

        private void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
            if (setStartActiveState)
            {
                Activate(activateOnStart);
            }

            EventsSubscriptions();
        }

        // private void OnEnable()
        // {
        //     EventsSubscriptions();
        // }
        //
        // private void OnDisable()
        // {
        //     EventsDescriptions();
        // }

        private void EventsSubscriptions()
        {
            TimeManager.Instance.OnTimeModeChanged.AddListener(CheckTimeScale);
        }

        private void EventsDescriptions()
        {
            TimeManager.Instance.OnTimeModeChanged.RemoveListener(CheckTimeScale);
        }

        public virtual void Activate(bool value)
        {
            isActivated = value;
            OnActiveStateChange.Invoke(isActivated);
        }

        private void CheckTimeScale(float timeScale)
        {
            isStopped = TimeManager.Instance.IsPaused;
            OnStopStateChange.Invoke(isStopped);
        }

        // ----- FOCUS -----
        public virtual void Focus(InteractionBase source)
        {
            if (!IsReady()) return;
            InteractionSource = source;
            OnFocus.Invoke(InteractionSource);
            //Debug.Log("Focusing!");
        }

        public virtual void DeFocus()
        {
            if (!IsReady()) return;
            OnDeFocus.Invoke(InteractionSource);
            InteractionSource = null;
            //Debug.Log("Lost!");
        }

        // ----- INTERACTING -----
        public virtual void StartInteract()
        {
            if (!IsReady()) return;
            switch (interactMode)
            {
                case InteractMode.Instant:
                    Interact();
                    break;
                case InteractMode.Holding:
                    Invoke(nameof(Interact), interactTime); // TODO - change to progress bar
                    // TODO
                    break;
            }

            //Debug.Log("Start interact!");
        }

        protected virtual void Interact()
        {
            if (!IsReady()) return;
            OnInteract.Invoke(InteractionSource);
            //Debug.Log("Interacting!");
        }

        public void ForceInteract(InteractionBase source)
        {
            InteractionSource = source;
            Interact();
        }

        public virtual void AfterInteract()
        {
            if (!IsReady()) return;
            if (interactMode == InteractMode.Holding)
                CancelInvoke();
            OnAfterInteract.Invoke(InteractionSource);
            //Debug.Log("After interact!");
        }

        protected bool IsReady()
        {
            return isActivated && !isStopped;
        }

        private enum InteractMode
        {
            Instant,
            Holding
        }
    }
}