using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityDevKit.UI_Handlers.Buttons
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleElementEvents : MonoBehaviour
    {
       [SerializeField] private UnityEvent onTrueChange;
       [SerializeField] private UnityEvent onFalseChange;

       private Toggle toggle;

       private void Awake()
       {
           toggle = GetComponent<Toggle>();
       }

       private void Start()
       {
           toggle.onValueChanged.AddListener(OnChangeHandle);
       }

       private void OnChangeHandle(bool value)
       {
           if (value)
           {
               onTrueChange.Invoke();
           }
           else
           {
               onFalseChange.Invoke();
           }
       }
    }
}