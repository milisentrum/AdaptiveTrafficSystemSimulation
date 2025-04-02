//using UnityEngine;
//using System.Collections;
//using NetMQ;
//using NetMQ.Sockets;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using AdaptiveTrafficSystem.TrafficLighters;

//public class ZMQClientUnity : MonoBehaviour
//{
//    // Укажите в инспекторе все светофоры, которыми хотите управлять индивидуально
//    public List<TrafficLighter> trafficLightersInScene;

//    // Словарь для быстрого поиска светофора по его уникальному ID
//    private Dictionary<string, TrafficLighter> lighterDictionary;

//    private RequestSocket client;
//    private bool isRunning = true;

//    void Start()
//    {
//        // 1) Инициализируем NetMQ
//        AsyncIO.ForceDotNet.Force();
//        client = new RequestSocket();
//        // Поменяйте порт или адрес, если ваш Python-сервер на другом
//        client.Connect("tcp://127.0.0.1:5556");

//        // 2) Формируем словарь "ID -> TrafficLighter"
//        lighterDictionary = new Dictionary<string, TrafficLighter>();
//        foreach (var lighter in trafficLightersInScene)
//        {
//            // Предположим, у каждого TrafficLighter в инспекторе прописано LighterID
//            // и оно совпадает с тем, что приходит из Python
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

//        // 3) Запускаем корутину, которая циклически общается с Python
//        StartCoroutine(LightsRoutine());
//    }

//    // Корутина, аналогичная вашей старой логике, только для индивидуальных светофоров
//    IEnumerator LightsRoutine()
//    {
//        while (isRunning)
//        {
//            // Запрашиваем у Python очередное состояние
//            client.SendFrame("next_state");

//            // Ожидаем JSON-ответ от сервера
//            string response = null;
//            bool msgReceived = false;
//            while (!msgReceived)
//            {
//                msgReceived = client.TryReceiveFrameString(out response);
//                // Чтобы не блокировать Unity, ждём один кадр, пока TryReceiveFrameString не вернёт true
//                yield return null;
//            }

//            Debug.Log($"[ZMQIndividualLightsClient] Received: {response}");

//            // Парсим JSON. Класс LightsState описан в конце
//            var state = JsonConvert.DeserializeObject<LightsState>(response);

//            // Перебираем светофоры, указанные в JSON
//            foreach (var lightData in state.lights)
//            {
//                if (lighterDictionary.TryGetValue(lightData.id, out var lighter))
//                {
//                    // Вызываем SwitchToOpen() или SwitchToClose() для соответствующего TrafficLighter
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
//                    // Если в словаре нет такого ID, Unity не найдёт светофор
//                    Debug.LogWarning($"[ZMQIndividualLightsClient] No TrafficLighter found for ID: {lightData.id}");
//                }
//            }

//            // Ждём, пока длится эта фаза, прежде чем запрашивать у Python следующее состояние
//            yield return new WaitForSeconds(state.duration);

//            Debug.Log("[ZMQIndividualLightsClient] End of cycle. Will request next state again...");
//        }
//    }

//    // При уничтожении объекта (или выходе из Play Mode) корректно закрываем сокет
//    void OnDestroy()
//    {
//        isRunning = false;
//        if (client != null)
//        {
//            client.Close();
//            client.Dispose();
//        }
//        // Завершаем NetMQ
//        NetMQConfig.Cleanup();
//    }

//    // Классы для десериализации JSON
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
    // Список всех светофоров в сцене, которыми управляем
    public List<TrafficLighter> trafficLightersInScene;

    // Для быстрого поиска: "id" -> TrafficLighter
    private Dictionary<string, TrafficLighter> lighterDictionary;

    private RequestSocket client;
    private bool isRunning = true;

    void Start()
    {
        // Инициализация ZeroMQ
        AsyncIO.ForceDotNet.Force();
        client = new RequestSocket();
        client.Connect("tcp://127.0.0.1:5556");
        // Поменяйте порт/адрес, если нужно

        // Формируем словарь "LighterID -> TrafficLighter"
        lighterDictionary = new Dictionary<string, TrafficLighter>();
        foreach (var lighter in trafficLightersInScene)
        {
            // Пусть у TrafficLighter есть поле "public string LighterID"
            // где вы прописали "light_na" и т.д. прямо в Инспекторе
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

        // Запускаем корутину запроса состояний
        StartCoroutine(LightsRoutine());
    }

    IEnumerator LightsRoutine()
    {
        while (isRunning)
        {
            // 1) Отправляем запрос на Python-сервер
            client.SendFrame("next_state");

            // 2) Ждём ответа (JSON)
            string response = null;
            bool msgReceived = false;
            while (!msgReceived)
            {
                msgReceived = client.TryReceiveFrameString(out response);
                // Ждём один кадр, чтобы не блокироваться
                yield return null;
            }

            Debug.Log($"[ZMQClient] Received JSON: {response}");

            // 3) Парсим JSON в класс LightsState
            var state = JsonConvert.DeserializeObject<LightsState>(response);

            // 4) Перебираем, кому "open", кому "close"
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

            // 5) Ждём указанную длительность перед следующим запросом
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

    // Классы для парсинга JSON
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
