using System;
using UnityDevKit.Utils.TimeHandlers;

namespace UnityDevKit.Scenario.Actions
{
    public abstract class ScenarioAction : IScenarioAction
    {
        private DateTime time;
        private string value;
        
        public ScenarioAction(string value)
        {
            time = DateTime.Now;
            this.value = value;
        }

        public string GetTime() => time.ToDateTimeString();
        
        public abstract string GetName();

        public string GetValue() => value;

        public int CompareTo(IScenarioAction other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(GetName(), other.GetName(), StringComparison.Ordinal);
        }
    }
}