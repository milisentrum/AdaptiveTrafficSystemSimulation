using AdaptiveTrafficSystem.Utils;
using MyBox;
using UnityEngine;

namespace AdaptiveTrafficSystem.Paths
{
    public class PathDirection : MonoBehaviour
    {
        [SerializeField] [InitializationField] private string id = IdGenerator.Generate();
        public string Id => id;

        public ControlledPath Path { get; private set; }
        
        public void Init(ControlledPath path)
        {
            Path = path;
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