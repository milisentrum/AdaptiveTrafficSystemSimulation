using UnityEngine;

namespace AdaptiveTrafficSystem.UI
{
    public class ObjectToggle : MonoBehaviour
    {
        [SerializeField] private GameObject toggleObject;

        public void Toggle()
        {
            toggleObject.SetActive(!toggleObject.activeSelf);
        }
    }
}