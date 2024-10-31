using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Interactables.UsableObjects
{
    public abstract class UsableObject : MonoBehaviour
    {
        [SerializeField] [DefinedValues(0, 1)] private int mainActionMouseInput = 0;
        [SerializeField] [DefinedValues(0, 1)] private int secondaryActionMouseInput = 1;
        
        [SerializeField] private UnityEvent onMainAction;
        [SerializeField] private UnityEvent onSecondaryAction;
        
        protected abstract void MainAction();
        protected abstract void SecondaryAction();

        private void Update()
        {
            if (TriggerInputForMainAction())
            {
                MainAction();
                onMainAction.Invoke();
            }

            if (TriggerInputForSecondaryAction())
            {
                SecondaryAction();
                onSecondaryAction.Invoke();
            }
        }

        protected virtual bool TriggerInputForMainAction()
        {
            return Input.GetMouseButtonDown(mainActionMouseInput);
        }

        protected virtual bool TriggerInputForSecondaryAction()
        {
            return Input.GetMouseButtonDown(secondaryActionMouseInput);
        }
    }
}