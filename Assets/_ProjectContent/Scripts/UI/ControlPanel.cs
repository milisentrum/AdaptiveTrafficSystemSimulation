using System.Globalization;
using MyBox;
using UnityEngine;
using TMPro;
using UnityDevKit.Utils.SceneHandlers;
using UnityDevKit.Utils.TimeHandlers;
using UnityEngine.UI;

namespace TrafficModule.UI
{
    public class ControlPanel : MonoBehaviour
    {
        [Separator("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button speedDownButton;
        [SerializeField] private Button speedUpButton;
        
        [Separator("Text")]
        [SerializeField] private TMP_Text speedText;


        private void Start()
        {
            restartButton.onClick.AddListener(Restart);
            exitButton.onClick.AddListener(Exit);
            speedDownButton.onClick.AddListener(SpeedDown);
            speedUpButton.onClick.AddListener(SpeedUp);
            
            TimeManager.Instance.OnTimeModeChanged.AddListener(UpdateText);
        }

        private void Exit()
        {
            SceneLoader.Instance.Quit();
        }

        private void Restart()
        {
            SceneLoader.Instance.Restart();
        }

        private void SpeedDown()
        {
            TimeManager.Instance.SpeedDown();
        }

        private void SpeedUp()
        {
            TimeManager.Instance.SpeedUp();
        }        

        private void UpdateText(float timeScale)
        {
            speedText.text = timeScale.ToString(CultureInfo.InvariantCulture);
        }
    }
}