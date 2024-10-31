using TMPro;
using UnityEngine;

namespace AdaptiveTrafficSystem.Paths
{
    public class PathNameDisplay : MonoBehaviour
    {
        [SerializeField] private ControlledPath path;
        [SerializeField] private TMP_Text nameHolder;

        private void Start()
        {
            nameHolder.text = path.PathName;
        }
    }
}