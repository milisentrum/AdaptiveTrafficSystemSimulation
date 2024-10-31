using UnityEngine;

namespace UnityDevKit.Player.Controllers
{
    public class PC_PlayerController : PlayerController
    {
        private void Start()
        {
            UnblockPlayer();
        }

        public override void BlockPlayer()
        {
            base.BlockPlayer();
            Cursor.lockState = CursorLockMode.None;
        }

        public override void UnblockPlayer()
        {
            base.UnblockPlayer();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}