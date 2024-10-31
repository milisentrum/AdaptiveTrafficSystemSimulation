using UnityEngine;
using UnityDevKit.Effects;

namespace UnityDevKit.Interactables.Extensions
{
    [RequireComponent(typeof(HighlightEffect))]
    public class InteractableHighlighter : InteractableExtension
    {
        private HighlightEffect highlightEffect;


        // TODO - blinking mode;

        protected override void Init()
        {
            base.Init();
            OutlineSetup();
            ChildEffectSetup();
        }

        private void ChildEffectSetup()
        {
            var properties = highlightEffect.Properties;
        }

        protected override void OnActiveStateChangedAction(bool isActive)
        {
            base.OnActiveStateChangedAction(isActive);
            ToggleOutline(isActive);
        }

        // ----- OUTLINE -----
        private void OutlineSetup()
        {
            highlightEffect = GetComponent<HighlightEffect>();
        }

        private void ToggleOutline(bool enabled)
        {
            if (enabled)
            {
                highlightEffect.Apply();
            }
            highlightEffect.Remove();
        }

        private void IncreaseOutline()
        {
            highlightEffect.IncreaseEffectPower();
        }

        private void DecreaseOutline()
        {
            highlightEffect.DecreaseEffectPower();
        }

        // ----- INTERACT -----
        protected override void OnFocusAction(InteractionBase source)
        {
            base.OnFocusAction(source);
            //ToggleOutline(true);
            highlightEffect.Apply();
        }

        protected override void OnDeFocusAction(InteractionBase source)
        {
            base.OnDeFocusAction(source);
            //ToggleOutline(false);
            highlightEffect.Remove();
        }

        protected override void OnInteractAction(InteractionBase source)
        {
            base.OnInteractAction(source);
            IncreaseOutline();
        }

        protected override void OnAfterInteractAction(InteractionBase source)
        {
            base.OnAfterInteractAction(source);
            DecreaseOutline();
        }
    }
}