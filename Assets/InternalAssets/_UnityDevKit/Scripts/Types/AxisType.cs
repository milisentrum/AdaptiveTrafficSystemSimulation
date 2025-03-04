using UnityEngine;

namespace UnityDevKit.Types
{
    [System.Serializable]
    public class AxisType
    {
        [SerializeField] private bool x;
        [SerializeField] private bool y;
        [SerializeField] private bool z;

        public bool X => x;
        public bool Y => y;
        public bool Z => z;
    }
}