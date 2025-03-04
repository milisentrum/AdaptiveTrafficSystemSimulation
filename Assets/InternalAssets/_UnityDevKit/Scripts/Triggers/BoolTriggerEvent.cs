namespace UnityDevKit.Triggers
{
    /// <summary>
    ///  Class BoolTriggerEvent.
    ///  Implements basic logic of bool trigger with subscription.
    ///  Inherits from abstract class TriggerEventBase.
    ///  <inheritdoc/>
    /// </summary>
    public class BoolTriggerEvent : TriggerEventBase<bool>
    {
        // delegate/event of true and false value set events
        public delegate void TriggerFixedValueHandler();

        public event TriggerFixedValueHandler OnTrueValueSet;
        public event TriggerFixedValueHandler OnFalseValueSet;

        /// <summary>
        /// <inheritdoc/>
        /// Invokes true/false value set event method.  
        /// </summary>
        protected override void AdditionalEvents(bool newValue)
        {
            if (newValue)
                TrueValueSet();
            else
                FalseValueSet();
        }

        /// <summary>
        /// Invokes true value set event.  
        /// </summary>
        protected virtual void TrueValueSet()
        {
            OnTrueValueSet?.Invoke();
        }

        /// <summary>
        /// Invokes true value set event.  
        /// </summary>
        protected virtual void FalseValueSet()
        {
            OnFalseValueSet?.Invoke();
        }

        /// <summary>
        /// Operator 'true' overload.
        /// Based on GetValue method.
        /// </summary>
        /// <returns>Wrapped value.</returns>
        public static bool operator true(BoolTriggerEvent trigger)
        {
            return trigger.GetValue();
        }

        /// <summary>
        /// Operator 'false' overload.
        /// Based on GetValue method.
        /// </summary>
        /// <returns>Inverted wrapped value.</returns>
        public static bool operator false(BoolTriggerEvent trigger)
        {
            return !trigger.GetValue();
        }

        #region API

        /// <summary>
        ///  Method SubscribeToTrueValueSet.
        ///  Implements subscription on true value set event.
        ///  <param name="listener">Subscriber - function with no arguments.</param>
        /// </summary>
        public void SubscribeToTrueValueSet(TriggerFixedValueHandler listener)
        {
            OnTrueValueSet += listener;
        }

        /// <summary>
        ///  Method UnSubscribeFromTrueValueSet.
        ///  Implements unsubscription from true value set event.
        ///  <param name="listener">Subscriber - function with no arguments.</param>
        /// </summary>
        public void UnSubscribeFromTrueValueSet(TriggerFixedValueHandler listener)
        {
            OnTrueValueSet -= listener;
        }

        /// <summary>
        ///  Method SubscribeToFalseValueSet.
        ///  Implements subscription on false value set event.
        ///  <param name="listener">Subscriber - function with no arguments.</param>
        /// </summary>
        public void SubscribeToFalseValueSet(TriggerFixedValueHandler listener)
        {
            OnFalseValueSet += listener;
        }

        /// <summary>
        ///  Method UnSubscribeFromFalseValueSet.
        ///  Implements unsubscription from false value set event.
        ///  <param name="listener">Subscriber - function with no arguments.</param>
        /// </summary>
        public void UnSubscribeFromFalseValueSet(TriggerFixedValueHandler listener)
        {
            OnFalseValueSet -= listener;
        }

        /// <summary>
        /// Method SwitchValue.
        /// Implements wrapped value invert
        /// </summary>
        public void SwitchValue()
        {
            SetValue(!Value);
        }

        #endregion
    }
}