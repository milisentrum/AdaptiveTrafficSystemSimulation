using UnityEngine;
using MyBox;
using UnityDevKit.Triggers;

namespace UnityDevKit.Parameters
{
    public class Parameter : FloatTriggerEvent, IParameter
    {
        [Header("Parameter")]
        [SerializeField] private string paramName;
        [SerializeField] private string paramUnits;

        [Header("Random start value")] 
        [SerializeField] private bool useRandomStartValue;

        [SerializeField] 
        [ConditionalField(nameof(useRandomStartValue))] private RangedFloat minMaxRange;
        
        public string GetName() => paramName;
        
        public string GetUnits() => paramUnits;

        public void Reset()
        {
            SetValue(startValue);
        }

        public void SetValueInRange(float min, float max)
        {
            SetValue(Random.Range(min, max));
        }
        
        public void SetValueInRange(RangedFloat bounds)
        {
            SetValueInRange(bounds.Min, bounds.Max);
        }
        
        protected override void StartValueInit()
        {
            if (useRandomStartValue)
            {
                GenerateRandomStartValue();
            }
            else
            {
                base.StartValueInit();
            }
        }

        private void GenerateRandomStartValue()
        {
            SetValueInRange(minMaxRange);
        }
    }
}