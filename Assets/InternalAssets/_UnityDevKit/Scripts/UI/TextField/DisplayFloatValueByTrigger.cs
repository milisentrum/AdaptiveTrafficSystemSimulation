using System;
using UnityDevKit.Triggers;
using UnityEngine;

namespace UnityDevKit.UI_Handlers.TextField
{
    [Serializable]
    public class DisplayFloatValueByTrigger : DisplayFloatValue
    {
        [SerializeField] private FloatTriggerEvent triggerEvent;

        public FloatTriggerEvent TriggerEvent => triggerEvent;

        public void Init()
        {
            SendToTextField(triggerEvent.GetValue());
            triggerEvent.SubscribeToValueChanged(SendToTextField);
        }
    }
}