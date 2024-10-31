using MyBox;
using UnityEngine;

namespace UnityDevKit.Interactables
{
    public class InteractablesHelpersToggle : MonoBehaviour
    {
        [SerializeField] private InteractableHelper[] helpers;
        [SerializeField] private bool useOnStart;

        [SerializeField] [ConditionalField(nameof(useOnStart))]
        private bool isEnabled = true;

        private void Start()
        {
            if (useOnStart)
            {
                Toggle(isEnabled);
            }
        }

        public void Toggle(bool turnOn)
        {
            foreach (var helper in helpers)
            {
                helper.Activate(turnOn);
            }
        }
    }
}