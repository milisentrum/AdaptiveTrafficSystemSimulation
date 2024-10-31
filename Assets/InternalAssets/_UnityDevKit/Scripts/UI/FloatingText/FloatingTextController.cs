using UnityDevKit.Patterns;
using UnityEngine;

namespace UnityDevKit.UI_Handlers
{
    public class FloatingTextController : Singleton<FloatingTextController>
    {
        private static FloatingText popupText;
        private static GameObject canvas;

        private const float RandomRangeBorder = 0.75f;

        public override void Awake()
        {
            base.Awake();
            Initialize();
        }

        private static void Initialize()
        {
            canvas = GameObject.Find("Canvas");
            if (!popupText)
                popupText = Resources.Load<FloatingText>("Prefabs/Help/PopupTextParent");
        }

        public static void CreateFloatingText(string text, Vector3 location)
        {
            FloatingTextCreation(text, location);
        }

        public static void CreateFloatingText(string text, Vector3 location, Color color)
        {
            var fText = FloatingTextCreation(text, location);
            fText.SetColor(color);
        }

        public static void CreateFloatingText(string text, Vector3 location, float scaleModifier)
        {
            var fText = FloatingTextCreation(text, location);
            fText.ChangeTextScale(scaleModifier);
        }

        public static void CreateFloatingText(string text, Vector3 location, Color color, float scaleModifier)
        {
            var fText = FloatingTextCreation(text, location);
            fText.SetColor(color);
            fText.ChangeTextScale(scaleModifier);
        }

        private static FloatingText FloatingTextCreation(string text, Vector3 location)
        {
            var instance = Instantiate(popupText);
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(location);
            screenPosition.x += Random.Range(-RandomRangeBorder, RandomRangeBorder);
            screenPosition.y += Random.Range(-RandomRangeBorder, RandomRangeBorder);

            Transform fTextTransform;
            (fTextTransform = instance.transform).SetParent(canvas.transform, false);
            fTextTransform.position = screenPosition;
            fTextTransform.rotation = canvas.transform.rotation;
            instance.SetText(text);
            return instance;
        }
    }
}