using UnityEngine;

namespace UnityDevKit.Optimization
{
    public class MathData : MonoBehaviour
    {
        public static Vector3 VectorOne { get; private set; }
        public static Vector3 VectorZero { get; private set; }
        public static Vector3 VectorForward { get; private set; }

        // TODO - complete others

        private void Awake()
        {
            VectorOne = Vector3.one;
            VectorZero = Vector3.zero;
            VectorForward = Vector3.forward;
        }

        public static float Floor(float _value)
        {
            return _value >= 0f ? (int) _value : (int) _value - 1;
        }

        public static int FloorToInt(float _value)
        {
            return _value >= 0f ? (int) _value : (int) _value - 1;
        }
    }
}