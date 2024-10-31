using System;
using UnityDevKit.Optimization;
using UnityDevKit.Player.Controllers;

namespace UnityDevKit.Player.Extensions
{
    public abstract class PlayerExtension : CachedMonoBehaviour
    {
        public PlayerController PlayerController { get; protected set; }

        protected bool isBlocked;
        
        protected override void Awake()
        {
            base.Awake();
            PlayerController = GetComponentInParent<PlayerController>();
            if (PlayerController == null)
            {
                throw new NullReferenceException("Player extension can't get reference to player controller");
            }
        }

        protected virtual void Start()
        {
            PlayerController.OnPlayerBlock.AddListener(Block);
            PlayerController.OnPlayerUnblock.AddListener(Unblock);
        }

        protected virtual void Update()
        {
            if (isBlocked) return;
        }

        protected void Block()
        {
            isBlocked = true;
        }
        
        protected void Unblock()
        {
            isBlocked = false;
        }
    }
}