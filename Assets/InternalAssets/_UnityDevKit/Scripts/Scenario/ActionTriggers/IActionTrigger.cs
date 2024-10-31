using UnityDevKit.Scenario.Actions;

namespace UnityDevKit.Scenario.ActionTriggers
{
    public interface IActionTrigger
    {
        void SubscribeToTrigger();
        IScenarioAction CreateAction(string value);
    }
}