using MyBox;
using UnityEngine;
using UnityEngine.Events;

namespace UnityDevKit.Interactables.Buttons
{
    public class ButtonEventsConnector : MonoBehaviour
    {
        [Foldout("Events from platform logic", true)]
        public UnityEvent onButtonDownFromEvent;
        public UnityEvent onButtonUpFromEvent;
        
        [Foldout("Events to platform logic", true)]
        public UnityEvent onButtonDownToEvent;
        public UnityEvent onButtonUpToEvent;

        #region From platform logic
        public void OnButtonDownFrom()
        {
            onButtonDownFromEvent.Invoke();
        }
        
        public void OnButtonUpFrom()
        {
            onButtonUpFromEvent.Invoke();
        }
        #endregion
        
        #region To platform logic
        public void OnButtonDownTo()
        {
            onButtonDownToEvent.Invoke();
        }
        
        public void OnButtonUpTo()
        {
            onButtonUpToEvent.Invoke();
        }
        #endregion
    }
}