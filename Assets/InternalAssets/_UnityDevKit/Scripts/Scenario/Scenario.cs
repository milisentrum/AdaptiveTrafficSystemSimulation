using System.Collections.Generic;
using System.Linq;
using UnityDevKit.Scenario.Actions;

namespace UnityDevKit.Scenario
{
    public class Scenario
    {
        public List<IScenarioAction> Actions { get; }

        public Scenario()
        {
            Actions = new List<IScenarioAction>();
        }

        public Scenario(List<IScenarioAction> actions)
        {
            Actions = actions;
        }

        public void Add(IScenarioAction action)
        {
            Actions.Add(action);
        }

        public void AddUnique(IScenarioAction action)
        {
            if (Actions.Count > 0 && Actions.Last().CompareTo(action) == 0)
            {
                Actions[Actions.Count - 1] = action;
            }
            else
            {
                Actions.Add(action);
            }
        }

        public void Clear()
        {
            Actions.Clear();
        }
    }
}