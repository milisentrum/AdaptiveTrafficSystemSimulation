using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityDevKit.Converters
{
    public abstract class Vector3BaseEventConverter<T> : MonoBehaviour
    {
        [FormerlySerializedAs("OnConvert")] public UnityEvent<T> onConvert;

        public void GetInputVector(Vector3 vector)
        {
            onConvert.Invoke(Convert(vector));
        }

        protected abstract T Convert(Vector3 vector);
    }
}