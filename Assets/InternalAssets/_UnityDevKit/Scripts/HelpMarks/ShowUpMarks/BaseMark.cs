using UnityEngine;

namespace UnityDevKit.HelpMarks
{
    public class BaseMark : MonoBehaviour, IHelpMark
    {
        [Header("Main settings")]
        [SerializeField] private GameObject markObject;
        [SerializeField] private bool showOnStart = false;

        private void Start()
        {
            if (showOnStart)
            {
                Show();
            }
        }

        public virtual void Show()
        {
            markObject.SetActive(true);
        }

        public virtual void Close()
        {
            markObject.SetActive(false);
        }
    }
}