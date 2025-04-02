//using UnityEngine;
//using System.Collections;
//using NetMQ;
//using NetMQ.Sockets;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using AdaptiveTrafficSystem.TrafficLighters;

//public class ZMQClientUnity : MonoBehaviour
//{
//    // ������� � ���������� ��� ���������, �������� ������ ��������� �������������
//    public List<TrafficLighter> trafficLightersInScene;

//    // ������� ��� �������� ������ ��������� �� ��� ����������� ID
//    private Dictionary<string, TrafficLighter> lighterDictionary;

//    private RequestSocket client;
//    private bool isRunning = true;

//    void Start()
//    {
//        // 1) �������������� NetMQ
//        AsyncIO.ForceDotNet.Force();
//        client = new RequestSocket();
//        // ��������� ���� ��� �����, ���� ��� Python-������ �� ������
//        client.Connect("tcp://127.0.0.1:5556");

//        // 2) ��������� ������� "ID -> TrafficLighter"
//        lighterDictionary = new Dictionary<string, TrafficLighter>();
//        foreach (var lighter in trafficLightersInScene)
//        {
//            // �����������, � ������� TrafficLighter � ���������� ��������� LighterID
//            // � ��� ��������� � ���, ��� �������� �� Python
//            if (!string.IsNullOrEmpty(lighter.LighterID))
//            {
//                if (!lighterDictionary.ContainsKey(lighter.LighterID))
//                {
//                    lighterDictionary.Add(lighter.LighterID, lighter);
//                }
//                else
//                {
//                    Debug.LogWarning($"[ZMQIndividualLightsClient] Duplicate LighterID found: {lighter.LighterID}");
//                }
//            }
//            else
//            {
//                Debug.LogWarning($"[ZMQIndividualLightsClient] TrafficLighter {lighter.name} has empty LighterID!");
//            }
//        }

//        // 3) ��������� ��������, ������� ���������� �������� � Python
//        StartCoroutine(LightsRoutine());
//    }

//    // ��������, ����������� ����� ������ ������, ������ ��� �������������� ����������
//    IEnumerator LightsRoutine()
//    {
//        while (isRunning)
//        {
//            // ����������� � Python ��������� ���������
//            client.SendFrame("next_state");

//            // ������� JSON-����� �� �������
//            string response = null;
//            bool msgReceived = false;
//            while (!msgReceived)
//            {
//                msgReceived = client.TryReceiveFrameString(out response);
//                // ����� �� ����������� Unity, ��� ���� ����, ���� TryReceiveFrameString �� ����� true
//                yield return null;
//            }

//            Debug.Log($"[ZMQIndividualLightsClient] Received: {response}");

//            // ������ JSON. ����� LightsState ������ � �����
//            var state = JsonConvert.DeserializeObject<LightsState>(response);

//            // ���������� ���������, ��������� � JSON
//            foreach (var lightData in state.lights)
//            {
//                if (lighterDictionary.TryGetValue(lightData.id, out var lighter))
//                {
//                    // �������� SwitchToOpen() ��� SwitchToClose() ��� ���������������� TrafficLighter
//                    if (lightData.state == "open")
//                    {
//                        lighter.SwitchToOpen();
//                    }
//                    else
//                    {
//                        lighter.SwitchToClose();
//                    }
//                }
//                else
//                {
//                    // ���� � ������� ��� ������ ID, Unity �� ����� ��������
//                    Debug.LogWarning($"[ZMQIndividualLightsClient] No TrafficLighter found for ID: {lightData.id}");
//                }
//            }

//            // ���, ���� ������ ��� ����, ������ ��� ����������� � Python ��������� ���������
//            yield return new WaitForSeconds(state.duration);

//            Debug.Log("[ZMQIndividualLightsClient] End of cycle. Will request next state again...");
//        }
//    }

//    // ��� ����������� ������� (��� ������ �� Play Mode) ��������� ��������� �����
//    void OnDestroy()
//    {
//        isRunning = false;
//        if (client != null)
//        {
//            client.Close();
//            client.Dispose();
//        }
//        // ��������� NetMQ
//        NetMQConfig.Cleanup();
//    }

//    // ������ ��� �������������� JSON
//    [System.Serializable]
//    public class LightsState
//    {
//        public List<LightData> lights;
//        public float duration;
//    }

//    [System.Serializable]
//    public class LightData
//    {
//        public string id;
//        public string state;
//    }
//}


using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Collections.Generic;
using AdaptiveTrafficSystem.TrafficLighters;

public class ZMQClientUnity : MonoBehaviour
{
    // ������ ���� ���������� � �����, �������� ���������
    public List<TrafficLighter> trafficLightersInScene;

    // ��� �������� ������: "id" -> TrafficLighter
    private Dictionary<string, TrafficLighter> lighterDictionary;

    private RequestSocket client;
    private bool isRunning = true;

    void Start()
    {
        // ������������� ZeroMQ
        AsyncIO.ForceDotNet.Force();
        client = new RequestSocket();
        client.Connect("tcp://127.0.0.1:5556");
        // ��������� ����/�����, ���� �����

        // ��������� ������� "LighterID -> TrafficLighter"
        lighterDictionary = new Dictionary<string, TrafficLighter>();
        foreach (var lighter in trafficLightersInScene)
        {
            // ����� � TrafficLighter ���� ���� "public string LighterID"
            // ��� �� ��������� "light_na" � �.�. ����� � ����������
            if (!string.IsNullOrEmpty(lighter.LighterID))
            {
                if (!lighterDictionary.ContainsKey(lighter.LighterID))
                {
                    lighterDictionary.Add(lighter.LighterID, lighter);
                }
                else
                {
                    Debug.LogWarning($"Duplicate LighterID: {lighter.LighterID}");
                }
            }
            else
            {
                Debug.LogWarning($"TrafficLighter {lighter.name} has empty LighterID!");
            }
        }

        // ��������� �������� ������� ���������
        StartCoroutine(LightsRoutine());
    }

    IEnumerator LightsRoutine()
    {
        while (isRunning)
        {
            // 1) ���������� ������ �� Python-������
            client.SendFrame("next_state");

            // 2) ��� ������ (JSON)
            string response = null;
            bool msgReceived = false;
            while (!msgReceived)
            {
                msgReceived = client.TryReceiveFrameString(out response);
                // ��� ���� ����, ����� �� �������������
                yield return null;
            }

            Debug.Log($"[ZMQClient] Received JSON: {response}");

            // 3) ������ JSON � ����� LightsState
            var state = JsonConvert.DeserializeObject<LightsState>(response);

            // 4) ����������, ���� "open", ���� "close"
            foreach (var lightData in state.lights)
            {
                if (lighterDictionary.TryGetValue(lightData.id, out var lighter))
                {
                    if (lightData.state == "open")
                        lighter.SwitchToOpen();
                    else
                        lighter.SwitchToClose();
                }
                else
                {
                    Debug.LogWarning($"No TrafficLighter found for ID={lightData.id}");
                }
            }

            // 5) ��� ��������� ������������ ����� ��������� ��������
            yield return new WaitForSeconds(state.duration);
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        if (client != null)
        {
            client.Close();
            client.Dispose();
        }
        NetMQConfig.Cleanup();
    }

    // ������ ��� �������� JSON
    [System.Serializable]
    public class LightsState
    {
        public List<LightData> lights;
        public float duration;
    }

    [System.Serializable]
    public class LightData
    {
        public string id;
        public string state;
    }
}
