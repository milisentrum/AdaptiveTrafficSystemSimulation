using UnityEngine;
using UnityEngine.UI;

namespace UnityDevKit.UI_Handlers.Images
{
    [RequireComponent(typeof(Image))]
    public class ImageTransparencyChanger : MonoBehaviour
    {
        [SerializeField] [Range(0, 1f)] private float customTransparency = 0.95f;

        private Image image;
        private Color color;

        private void Awake()
        {
            image = GetComponent<Image>();
            color = image.color;
        }

        public void SetCustom(float transparency)
        {
            image.color = new Color(color.r, color.g, color.b, transparency);
        }

        public void SetCustom()
        {
            SetCustom(customTransparency);
        }

        public void SetNormal()
        {
            SetCustom(color.a);
        }
    }
}