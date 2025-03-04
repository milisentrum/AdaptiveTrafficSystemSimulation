using System.Collections.Generic;

namespace UnityDevKit.Interactables.Spinnable.Discrete
{
    public interface IDiscreteSpinnable
    {
        void SetDiscreteValues(List<float> values);
        void SetNextValue();
        void SetPreviousValue();
    }
}