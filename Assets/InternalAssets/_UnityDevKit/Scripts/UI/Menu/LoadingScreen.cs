using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityDevKit.UI_Handlers.Menu
{
    public class LoadingScreen : MonoBehaviour
    {
        public GameObject loadingPanel;
        public Slider slider;
        public Text sliderText;

        public void LoadLevel(int index)
        {
            StartCoroutine(LoadAsynchronously(index));
        }

        private IEnumerator LoadAsynchronously(int index)
        {
            var operation = SceneManager.LoadSceneAsync(index);

            loadingPanel.SetActive(true);

            while (!operation.isDone)
            {
                var progress = Mathf.Clamp01(operation.progress / .9f);

                slider.value = progress;
                sliderText.text = progress * 100f + "%";
                yield return null;
            }
        }
    }
}