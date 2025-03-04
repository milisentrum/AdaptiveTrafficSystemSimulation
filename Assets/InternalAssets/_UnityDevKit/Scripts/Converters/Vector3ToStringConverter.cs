using System.Globalization;
using UnityDevKit.Types;
using UnityEngine;

namespace UnityDevKit.Converters
{
    public class Vector3ToStringConverter : Vector3BaseEventConverter<string>
    {
        [SerializeField] private AxisType axis;

        protected override string Convert(Vector3 vector)
        {
            return (axis.X ? vector.z : axis.Y ? vector.y : vector.z).ToString(CultureInfo.InvariantCulture);
        }
    }
}