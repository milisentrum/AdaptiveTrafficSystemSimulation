using MyBox;
using UnityEngine;

namespace UnityDevKit.Player.Controllers
{
    public class PlayerHolder : Singleton<PlayerHolder>
    {
        [SerializeField] private PlayerController playerController;

        public PlayerController PlayerController => playerController;
    }
}