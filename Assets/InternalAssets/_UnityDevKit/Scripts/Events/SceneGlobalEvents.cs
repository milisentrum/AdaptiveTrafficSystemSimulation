using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Events
{
    public class SceneGlobalEvents : MonoBehaviour
    {
        [SerializeField] private UnityEvent onSceneStart;
        [SerializeField] private UnityEvent onSceneChange;

        public static readonly EventHolderBase OnGlobalSceneStart = new EventHolderBase();
        public static readonly EventHolderBase OnGlobalSceneChange = new EventHolderBase();

        private void Start()
        {
            OnGlobalSceneStart.AddListener(onSceneStart.Invoke);
            OnGlobalSceneChange.AddListener(onSceneChange.Invoke);

            OnGlobalSceneStart.Invoke();
        }

        private void OnDestroy()
        {
            OnGlobalSceneStart.RemoveListener(onSceneStart.Invoke);
            OnGlobalSceneChange.RemoveListener(onSceneChange.Invoke);
        }
    }
}