using UnityDevKit.Types;
using UnityEngine;

namespace UnityDevKit.Converters
{
    public class Vector3ToFloatConverter : Vector3BaseEventConverter<float>
    {
        [SerializeField] private AxisType axis;

        protected override float Convert(Vector3 vector)
        {
            return axis.X ? vector.z : axis.Y ? vector.y : vector.z;
        }
    }
}