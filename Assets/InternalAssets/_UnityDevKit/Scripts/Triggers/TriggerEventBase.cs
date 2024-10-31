using UnityEngine;

namespace UnityDevKit.Triggers
{
    /// <summary>
    ///  Abstract generic class TriggerEventBase.
    ///  Implements basic logic of trigger with subscription.
    ///  Based on delegate/event.
    /// <typeparam name="T">Wrapped value type.</typeparam>
    /// </summary>
    public abstract class TriggerEventBase<T> : MonoBehaviour
    {
        // Serialized fields of main settings
        [SerializeField] protected T startValue;
        [SerializeField] protected bool useStartValue;
        [SerializeField] protected bool isSingleEvent;
        
        // Wrapped value
        protected T Value;

        // State field
        private bool _wasInvoked;

        // delegate/event of value changed event
        public delegate void TriggerValueHandler(T value);
        public event TriggerValueHandler OnValueChanged;

        /// <summary>
        ///  UnityEngine method.
        ///  Implement start value setup option.
        /// </summary>
        private void Awake()
        {
            if (useStartValue) StartValueInit();
        }

        /// <summary>
        ///  Virtual Method StartValueInit.
        ///  Implement start value setup.
        /// </summary>
        protected virtual void StartValueInit()
        {
            SetValue(startValue);
        }
        

        /// <summary>
        ///  Method ValueChanged.
        ///  Implements invocation of all subscribers.
        /// <param name="value">Changed wrapped value.</param>
        /// <typeparam name="T">Wrapped value type.</typeparam>
        /// </summary>
        protected void ValueChanged(T value)
        {
            // Checks if value can be changed only for one time and was it already invoked 
            if (isSingleEvent && _wasInvoked) return;
            OnValueChanged?.Invoke(value);
            AdditionalEvents(value);
            _wasInvoked = true;
        }

        /// <summary>
        ///  Method AdditionalEvents.
        ///  Addition events on value changed action.
        ///  <param name="value">Changed wrapped value of type T.</param>
        /// </summary>
        protected virtual void AdditionalEvents(T value)
        {
        }

        #region API

        /// <summary>
        ///  Method SubscribeToValueChanged.
        ///  Implements subscription on value change event.
        ///  <param name="listener">Subscriber - function with 1 argument of type T.</param>
        /// </summary>
        public void SubscribeToValueChanged(TriggerValueHandler listener)
        {
            OnValueChanged += listener;
        }

        /// <summary>
        ///  Method UnSubscribeFromValueChanged.
        ///  Implements unsubscription from value change event.
        ///  <param name="listener">Subscriber - function with 1 argument of type T.</param>
        /// </summary>
        public void UnSubscribeFromValueChanged(TriggerValueHandler listener)
        {
            OnValueChanged -= listener;
        }

        /// <summary>
        ///  Method UnSubscribeAll.
        ///  Implements unsubscription of all subscribers from value change event.
        /// </summary>
        public void UnSubscribeAll()
        {
            if (OnValueChanged == null) return;
            
            // Iteration through all subscribers
            for (var index = OnValueChanged.GetInvocationList().Length - 1; index >= 0; index--)
            {
                OnValueChanged -= OnValueChanged?.GetInvocationList()[index] as TriggerValueHandler;
            }
        }

        /// <summary>
        ///  Method GetValue.
        ///  Implements getter of wrapped value.
        /// <returns>Wrapped value.</returns>
        /// <typeparam name="T">Wrapped value type.</typeparam>
        /// </summary>
        public T GetValue()
        {
            return Value;
        }

        /// <summary>
        ///  Virtual method SetValue.
        ///  Implements setter of wrapped value and invokes OnValueChanged event.
        ///  <param name="newValue">New wrapped value.</param>
        /// <typeparam name="T">Wrapped value type.</typeparam>
        /// </summary>
        public virtual void SetValue(T newValue)
        {
            Value = newValue;
            ValueChanged(Value);
        }

        #endregion
    }
}