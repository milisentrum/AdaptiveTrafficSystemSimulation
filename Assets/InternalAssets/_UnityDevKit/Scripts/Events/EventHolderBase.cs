namespace UnityDevKit.Events
{
    public class EventHolderBase
    {
        public delegate void EventHandler();

        public event EventHandler OnEventTriggered;

        public void Invoke()
        {
            OnEventTriggered?.Invoke();
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