using AdaptiveTrafficSystem.Net.Websocket;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdaptiveTrafficSystem.UI
{
    public class ServerConnectionUi : MonoBehaviour
    {
        [Separator("Server")]
        [SerializeField] private WebsocketApiServerWrapper server;

        [Separator("UI address")] 
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private TMP_InputField portInputField;
        
        [Separator("UI connect buttons")] 
        [SerializeField] private GameObject connectBtnObject;
        [SerializeField] private Button connectBtn;
        [SerializeField] private GameObject disconnectBtnObject;
        [SerializeField] private Button disconnectBtn;

        [Separator("UI status")] 
        [SerializeField] private GameObject statusPanelObject;
        [SerializeField] private GameObject emptyStatusObject;
        [SerializeField] private GameObject okStatusObject;
        [SerializeField] private GameObject errorStatusObject;
        
        [Separator("UI clients")] 
        [SerializeField] private TMP_Text clientsCountText;

        private void Start()
        {
           Init();
           SubscribeOnEvents();
        }

        private void Init()
        {
            TurnOff();
            SetEmptyStatus();
        }

        private void SubscribeOnEvents()
        {
            connectBtn.onClick.AddListener(() => server.Launch(ipInputField.text, int.Parse(portInputField.text)));
            disconnectBtn.onClick.AddListener(() => server.Stop());
            server.SubscribeOnServerLaunch(result =>
            {
                if (result)
                {
                    OnConnect();
                }
                else
                {
                    OnError();
                }
            });
            server.SubscribeOnServerStop(OnDisconnect);
        }

        private void OnConnect()
        {
            TurnOn();
            SetStatus(true);
        }
        
        private void OnDisconnect()
        {
            TurnOff();
            SetEmptyStatus();
        }

        private void OnError()
        {
            TurnOff();
            SetStatus(false);
        }

        private void TurnOn()
        {
            connectBtnObject.SetActive(false);
            disconnectBtnObject.SetActive(true);
            //statusPanelObject.SetActive(true);
        }

        private void TurnOff()
        {
            connectBtnObject.SetActive(true);
            disconnectBtnObject.SetActive(false);
            //statusPanelObject.SetActive(false);
        }

        private void SetStatus(bool isOk)
        {
            okStatusObject.SetActive(isOk);
            errorStatusObject.SetActive(!isOk);
            emptyStatusObject.SetActive(false);
        }
        
        private void SetEmptyStatus()
        {
            okStatusObject.SetActive(false);
            errorStatusObject.SetActive(false);
            emptyStatusObject.SetActive(true);
        }

        private void SetClientsCount(int count)
        {
            clientsCountText.text = count.ToString();
        }
    }
}