using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UnityDevKit.UI_Handlers
{
    public class TextSameSizeSetter : MonoBehaviour
    {
        [SerializeField] private List<TMP_Text> tmpTexts;

        private void Start()
        {
            Resize();
        }

        public void Resize()
        {
            foreach (var text in tmpTexts)
            {
                text.enableAutoSizing = true;
            }

            var smallestSize = tmpTexts.Select(text => text.fontSize).Append(float.MaxValue).Min();

            foreach (var text in tmpTexts)
            {
                text.enableAutoSizing = false;
                text.fontSize = smallestSize;
            }
        }
    }
}