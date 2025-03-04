using UnityDevKit.Events;
using UnityEngine;

namespace UnityDevKit.Player.Controllers
{
    [DisallowMultipleComponent]
    public abstract class PlayerController: MonoBehaviour
    {
        public readonly EventHolderBase OnPlayerBlock = new EventHolderBase();
        public readonly EventHolderBase OnPlayerUnblock = new EventHolderBase();

        public virtual void BlockPlayer()
        {
            OnPlayerBlock.Invoke();
        }
        
        public virtual void UnblockPlayer()
        {
            OnPlayerUnblock.Invoke();
        }
    }
}