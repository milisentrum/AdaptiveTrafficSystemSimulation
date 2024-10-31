using AdaptiveTrafficSystem.TrafficLighters;
using AdaptiveTrafficSystem.Utils;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Paths
{
    [RequireComponent(typeof(TL_SyncGroup))]
    public class ControlledPath : MonoBehaviour
    {
        [SerializeField] private string pathName = "Улица";

        [SerializeField] [InitializationField] private string id = IdGenerator.Generate();
        public string Id => id;
        
        public string PathName => pathName;

        [SerializeField] private PathDirection[] pathDirections;

        public PathDirection[] PathDirections => pathDirections;
        
        public TL_SyncGroup TrafficGroup { get; private set; }

        private void Awake()
        {
            TrafficGroup = GetComponent<TL_SyncGroup>();
            
            Init();
        }

        private void Init()
        {
            foreach (var direction in pathDirections)
            {
                direction.Init(this);
            }
        }
        
#if UNITY_EDITOR
        [ButtonMethod]
        public string GenerateId()
        {
            id = IdGenerator.Generate();
            return $"ID {id} was generated for {name} gameObject";
        }
#endif
    }
}
