using System.Net;
using AdaptiveTrafficSystem.Api;
using AdaptiveTrafficSystem.Paths;
using AdaptiveTrafficSystem.Tracking.Parameters;
using MyBox;
using UnityDevKit.Events;
using UnityEngine;

namespace AdaptiveTrafficSystem.Net.Websocket
{
    public class WebsocketApiServerWrapper : MonoBehaviour
    {
        [Separator("Connection")] 
        [SerializeField] [InitializationField] private bool connectOnStart;
        [SerializeField] [ConditionalField(nameof(connectOnStart))] private string defaultAddress = "127.0.0.1";
        [SerializeField] [ConditionalField(nameof(connectOnStart))] [PositiveValueOnly] private int defaultPort = 4649; 
        
        [Separator("API Handlers")] 
        [SerializeField] private ControlledPath[] controlledPaths;
        [SerializeField] private ParametersHolder[] parametersHolders;
        [SerializeField] private SpawnSetupController spawnSetupController;

        private readonly EventHolder<bool> _onLaunch = new EventHolder<bool>();
        private readonly EventHolderBase _onStop = new EventHolderBase();
        
        private WebsocketApiServer _apiServer;

        private void Start()
        {
            if (connectOnStart)
            {
                Launch(defaultAddress, defaultPort);
            }
        }

        public void Launch(string ip, int port)
        {
            _apiServer = new WebsocketApiServer(new IPEndPoint(IPAddress.Parse(ip), port));
            _apiServer.InitTrafficLightersApi(controlledPaths);
            _apiServer.InitTrafficParametersApi(parametersHolders);
            _apiServer.InitTrafficSpawnApi(spawnSetupController);
            _apiServer.Start();
            
            _onLaunch.Invoke(_apiServer.IsListening);
        }

        public void Stop()
        {
            _apiServer.Stop();
            _onStop.Invoke();
        }

        public void SubscribeOnServerLaunch(EventHolder<bool>.EventHandler listener)
        {
            _onLaunch.AddListener(listener);
        }
        
        public void SubscribeOnServerStop(EventHolderBase.EventHandler listener)
        {
            _onStop.AddListener(listener);
        }
    }
}