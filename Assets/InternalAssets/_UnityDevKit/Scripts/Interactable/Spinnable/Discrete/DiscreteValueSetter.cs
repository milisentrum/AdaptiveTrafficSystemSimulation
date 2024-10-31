using System.Collections.Generic;
using UnityEngine;

namespace UnityDevKit.Interactables.Spinnable.Discrete
{
    [RequireComponent(typeof(IDiscreteSpinnable))]
    public class DiscreteValueSetter : MonoBehaviour
    {
        private IDiscreteSpinnable discreteSpinnable;

        private void Awake()
        {
            discreteSpinnable = GetComponent<IDiscreteSpinnable>();
        }

        public void SetDiscreteValues(List<float> values)
        {
            discreteSpinnable.SetDiscreteValues(values);
        }
    }
}