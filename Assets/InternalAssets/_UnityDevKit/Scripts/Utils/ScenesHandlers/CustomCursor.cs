using UnityEngine;

namespace UnityDevKit.Utils.ScenesHandlers
{
    public class CustomCursor : MonoBehaviour
    {
        [SerializeField] private Texture2D cursor;

        private void Start()
        {
            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
        }
    }
}