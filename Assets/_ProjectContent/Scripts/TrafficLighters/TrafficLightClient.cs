using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Collections.Generic;
using AdaptiveTrafficSystem.Paths;

// Пример класса, который будет общаться с Python-сервером
public class TrafficLightClient : MonoBehaviour
{
    // Ссылки на ControlledPath (где у вас Path_1 / Path_2)
    public ControlledPath Path_1;
    public ControlledPath Path_2;

    private RequestSocket client;
    private bool isRunning = true;

    void Start()
    {
        // Инициализация NetMQ
        AsyncIO.ForceDotNet.Force();
        client = new RequestSocket();
        // Подключаемся к тому же порту, что и сервер
        client.Connect("tcp://127.0.0.1:5555");

        // Запускаем корутину, которая регулярно будет спрашивать у сервера, что включать
        StartCoroutine(RequestStateCoroutine());
    }

    // Основная корутина: циклически запрашивает "следующее состояние"
    // и включает/выключает нужные светофоры с указанной задержкой
    IEnumerator RequestStateCoroutine()
    {
        while (isRunning)
        {
            // Отправляем запрос на сервер
            client.SendFrame("next_state");

            // Ждём ответа (JSON)
            string response = null;
            bool gotMessage = false;

            // Ждём, пока сервер пришлёт ответ. Можно сделать timeout
            while (!gotMessage)
            {
                gotMessage = client.TryReceiveFrameString(out response);
                yield return null; // подождать кадр
            }

            Debug.Log("Received from Python: " + response);

            // Парсим JSON. Для этого есть класс StateData (см. ниже)
            var state = JsonConvert.DeserializeObject<StateData>(response);

            // 1) Открыть тот путь, который сказали
            if (state.open_path == "Path_1")
            {
                Path_1.TrafficGroup.SwitchToOpen();
            }
            else if (state.open_path == "Path_2")
            {
                Path_2.TrafficGroup.SwitchToOpen();
            }
            // (можно добавить больше условий, если путей больше)

            // 2) Закрыть те пути, что перечислены в close_paths
            foreach (var closeName in state.close_paths)
            {
                if (closeName == "Path_1")
                {
                    Path_1.TrafficGroup.SwitchToClose();
                }
                else if (closeName == "Path_2")
                {
                    Path_2.TrafficGroup.SwitchToClose();
                }
            }

            // 3) Ждём, пока "длится" данная фаза
            //    То есть на duration секунд (или можно дождаться SwitchToOpen и анимации)
            yield return new WaitForSeconds(state.duration);
        }
    }

    // Останавливаем сокет при выгрузке
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

    // Класс для удобной десериализации JSON
    [System.Serializable]
    public class StateData
    {
        public string open_path;
        public List<string> close_paths;
        public float duration;
    }
}
