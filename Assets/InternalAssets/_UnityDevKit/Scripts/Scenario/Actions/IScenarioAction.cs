using System;

namespace UnityDevKit.Scenario.Actions
{
    public interface IScenarioAction: IComparable<IScenarioAction>
    {
        string GetTime();
        string GetName();
        string GetValue();
    }
}