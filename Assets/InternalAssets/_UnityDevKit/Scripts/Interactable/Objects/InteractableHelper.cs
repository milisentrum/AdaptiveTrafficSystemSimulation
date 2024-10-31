using UnityEngine;

namespace UnityDevKit.Interactables
{
    public class InteractableHelper : InteractableBase
    {
        [SerializeField] private GameObject helpObject;

        public override void Focus(InteractionBase client)
        {
            base.Focus(client);
            if (!IsReady()) return;
            helpObject.SetActive(true);
        }

        public override void DeFocus()
        {
            base.DeFocus();
            if (!IsReady()) return;
            helpObject.SetActive(false);
        }
    }
}