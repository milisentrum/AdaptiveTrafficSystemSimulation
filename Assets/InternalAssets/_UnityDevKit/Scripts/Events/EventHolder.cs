namespace UnityDevKit.Events
{
    public class EventHolder<T>
    {
        public delegate void EventHandler(T value);

        public event EventHandler OnEventTriggered;

        public void Invoke(T value)
        {
            OnEventTriggered?.Invoke(value);
        }

        public void AddListener(EventHandler listener)
        {
            OnEventTriggered += listener;
        }

        public void RemoveListener(EventHandler listener)
        {
            OnEventTriggered -= listener;
        }

        public void RemoveAllListeners()
        {
            if (OnEventTriggered == null) return;
            for (var index = OnEventTriggered.GetInvocationList().Length - 1; index >= 0; index--)
            {
                OnEventTriggered -= OnEventTriggered?.GetInvocationList()[index] as EventHandler;
            }
        }
    }
}