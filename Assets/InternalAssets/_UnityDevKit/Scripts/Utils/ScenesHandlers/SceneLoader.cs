using UnityDevKit.Events;
using UnityDevKit.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityDevKit.Utils.SceneHandlers
{
    public class SceneLoader : Singleton<SceneLoader>
    {     
        private ProjectScenes.SceneInfo _chosenScene;
        private static ProjectScenes.SceneInfo _lastMenuScene = ProjectScenes.SIMULATION;

        private AsyncOperation _loadSceneOperation;
        private bool _blockTransition = false;
        private const float TRANSITION_TIME = 1f;

        
        #region API
        public void SetSceneToLoad(int sceneNumber)
        {
            _chosenScene = ProjectScenes.GetSceneByNumber(sceneNumber);
        }

        public void LoadChosenScene()
        {
            if (_chosenScene.IsMenu)
            {
                _lastMenuScene = _chosenScene;
            }
            SwitchToScene(_chosenScene.Name);
            SceneGlobalEvents.OnGlobalSceneChange.Invoke();
        }

        public void Restart()
        {
            SetSceneToLoad(SceneManager.GetActiveScene().buildIndex);
            LoadChosenScene();
        }

        public void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        #endregion

        private void SwitchToScene(string sceneName)
        {
            if (_blockTransition) return;
            _blockTransition = true;
            _loadSceneOperation = SceneManager.LoadSceneAsync(sceneName);
            _loadSceneOperation.allowSceneActivation = false;
            Invoke(nameof(AllowTransition), TRANSITION_TIME);
        }

        private void AllowTransition()
        {
            _loadSceneOperation.allowSceneActivation = true;
            _blockTransition = false;
        }
    }
}