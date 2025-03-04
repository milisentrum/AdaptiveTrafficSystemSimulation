using UnityDevKit.Utils.TimeHandlers;
using UnityEngine;

namespace UnityDevKit.UI_Handlers
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private KeyCode closeKey = KeyCode.Escape;

        private bool isPaused = false;

        private void Update()
        {
            if (!Input.GetKeyDown(closeKey)) return;
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        private void Resume()
        {
            pauseMenuUI.SetActive(false);
            isPaused = false;
            TimeManager.Instance.Resume();
        }

        private void Pause()
        {
            pauseMenuUI.SetActive(true);
            isPaused = true;
            TimeManager.Instance.Pause();
        }
    }
}