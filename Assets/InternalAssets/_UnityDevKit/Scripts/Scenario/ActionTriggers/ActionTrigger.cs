using UnityDevKit.Scenario.Actions;
using UnityEngine;

namespace UnityDevKit.Scenario.ActionTriggers
{
    public abstract class ActionTrigger<T> : MonoBehaviour, IActionTrigger
    where T : MonoBehaviour
    {
        [SerializeField] protected T TriggerEventBase;

        public abstract void SubscribeToTrigger();
        public abstract IScenarioAction CreateAction(string value);
    }
}