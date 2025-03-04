using UnityEngine;
using UnityEngine.UI;

namespace UnityDevKit.UI_Handlers
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private Text text;

        private void Awake()
        {
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length >= 1) Destroy(gameObject, clipInfo[0].clip.length - 0.1f);

            text = animator.GetComponent<Text>();
        }

        public void SetText(string text)
        {
            animator.GetComponent<Text>().text = text;
        }

        public void SetColor(Color color)
        {
            text.color = color;
        }

        public void ChangeTextScale(float scaleModifier)
        {
            text.fontSize = (int) (text.fontSize * scaleModifier);
        }
    }
}