using UnityEngine;

namespace UnityDevKit.Utils.ScenesHandlers
{
    public static class CursorHandler
    {
        public static void TurnOnCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public static void TurnOffCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}