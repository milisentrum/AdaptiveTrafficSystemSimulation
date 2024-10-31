using AdaptiveTrafficSystem.Pedestrians.Modules;
using UnityDevKit.Optimization;

namespace AdaptiveTrafficSystem.Pedestrians
{
    public class Pedestrian : CachedMonoBehaviour
    {
        private IPedestrianModule[] _pedestrianModules;

        protected override void Awake()
        {
            base.Awake();
            _pedestrianModules = GetComponents<IPedestrianModule>();
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            foreach (var module in _pedestrianModules)
            {
                module.Init();
            }
        }
    }
}