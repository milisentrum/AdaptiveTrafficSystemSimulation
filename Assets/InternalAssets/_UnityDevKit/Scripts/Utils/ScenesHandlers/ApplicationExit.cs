using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Utils.ScenesHandlers
{
    public class ApplicationExit : MonoBehaviour
    {
        [SerializeField] private UnityEvent onExit;

        public void Quit()
        {
            onExit.Invoke();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}