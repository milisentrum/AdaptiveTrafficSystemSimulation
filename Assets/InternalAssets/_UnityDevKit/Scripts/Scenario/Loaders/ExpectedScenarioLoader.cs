using UnityDevKit.Scenario.Controllers;
using UnityEngine;

namespace UnityDevKit.Scenario.Loaders
{
    public abstract class ExpectedScenarioLoader : MonoBehaviour
    {
        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            ScenarioController.Instance.LoadExpectedScenario(GetScenario());
        }

        protected abstract Scenario GetScenario();
    }
}