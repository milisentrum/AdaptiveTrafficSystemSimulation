using System.Collections;
using UnityEngine;

namespace UnityDevKit.HelpMarks
{
    public class CustomMark : BaseMark
    {
        private bool show;
        public void ShowAfterDuration(float duration)
        {
            StartCoroutine(ShowThreshold(duration));
        }
        
        public void CloseAfterDuration(float duration)
        {
            StartCoroutine(CloseThreshold(duration));
        }
        
        private IEnumerator ShowThreshold(float duration)
        {
            yield return new WaitForSeconds(duration);
            Show();
        }
        
        private IEnumerator CloseThreshold(float duration)
        {
            yield return new WaitForSeconds(duration);
            Close();
        }

        public void ShowOnClick()
        {
            show = !show;
        }

        public override void Close()
        {
            if (!show) base.Close();
        }
    }
}