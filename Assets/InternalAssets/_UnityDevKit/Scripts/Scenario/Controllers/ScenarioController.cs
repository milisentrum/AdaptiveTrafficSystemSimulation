using UnityDevKit.Patterns;
using UnityDevKit.Scenario.Actions;

namespace UnityDevKit.Scenario.Controllers
{
    public class ScenarioController : Singleton<ScenarioController>
    {
        public Scenario ExpectedScenario { get; private set; } = new Scenario();
        public Scenario CurrentScenario { get; private set; } = new Scenario();
        
        public void LoadExpectedScenario(Scenario scenario)
        {
            Reset();
            ExpectedScenario = scenario;
        }

        public void AddToCurrent(IScenarioAction action)
        {
            CurrentScenario.Add(action);
        }
        
        public void AddUniqueToCurrent(IScenarioAction action)
        {
            CurrentScenario.AddUnique(action);
        }
        
        public void Reset()
        {
            ExpectedScenario.Clear();
            CurrentScenario.Clear();
        }
    }
}