using UnityEngine;
using System.Collections;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using AdaptiveTrafficSystem.TrafficLighters;

#region LOGGING
public static class MyAsyncLogger
{
    private static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
    private static Thread loggingThread;
    private static bool isRunning = false;
    private static string logFilePath;
    private static bool isInitialized = false;
    private static bool isApplicationQuitting = false;

    public static void Init()
    {
        if (isInitialized || isApplicationQuitting) return;
        logFilePath = Path.Combine(Application.dataPath, "MyAsyncLog.txt");

        isRunning = true;
        loggingThread = new Thread(LoggingThreadMethod);
        loggingThread.IsBackground = true;
        loggingThread.Start();

        isInitialized = true;
    }

    public static void Log(string message)
    {
        if (isApplicationQuitting) return;
        if (!isInitialized) Init();
        logQueue.Enqueue($"[{DateTime.Now:HH:mm:ss.fff}] [INFO] {message}");
    }

    public static void LogError(string message)
    {
        if (!isInitialized) Init();
        logQueue.Enqueue($"[{DateTime.Now:HH:mm:ss.fff}] [ERROR] {message}");
    }

    public static void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        Shutdown();
    }

    private static void LoggingThreadMethod()
    {
        using (var writer = new StreamWriter(logFilePath, true))
        {
            while (isRunning)
            {
                while (logQueue.TryDequeue(out string msg))
                {
                    writer.WriteLine(msg);
                }
                writer.Flush();
                Thread.Sleep(50);
            }
        }
    }

    public static void Shutdown()
    {
        if (!isRunning) return;
        isRunning = false;

        if (loggingThread != null && loggingThread.IsAlive)
        {
            loggingThread.Join(2000);
            loggingThread = null;
        }
    }
}
#endregion

#region LOGGINGCSV

public static class MyCsvLogger
{
    private static string csvFilePath;
    private static bool isInitialized = false;
    private static bool isRunning = false;
    private static bool isApplicationQuitting = false;

    private static Thread loggingThread;
    private static ConcurrentQueue<string> csvQueue = new ConcurrentQueue<string>();

    public static void Init()
    {
        if (isInitialized || isApplicationQuitting) return;

        csvFilePath = Path.Combine(Application.dataPath, "detections_02_04.csv");
        
        // Если файл не существует – пишем заголовок. 
        // Можно поправить поля, если нужно другое количество столбцов:
        if (!File.Exists(csvFilePath))
        {
            File.WriteAllText(csvFilePath, "CameraName;DetectType;TrafficLightState;Count;Timestamp;Duration\n");
        }

        isRunning = true;
        loggingThread = new Thread(LoggingThreadMethod);
        loggingThread.IsBackground = true; 
        loggingThread.Start();

        isInitialized = true;
    }

    /// <summary>
    /// Основной метод фонового потока, где идёт запись в CSV.
    /// </summary>
    private static void LoggingThreadMethod()
    {
        // Открываем файл в режиме "append" (дописывать в конец).
        using (var writer = new StreamWriter(csvFilePath, true))
        {
            // Пока не остановлены – вычитываем очередь и пишем в файл.
            while (isRunning)
            {
                while (csvQueue.TryDequeue(out string line))
                {
                    writer.Write(line); 
                    // line уже содержит символ новой строки \n
                }
                writer.Flush();
                Thread.Sleep(50);
            }
        }
    }

    /// <summary>
    /// Записывает строку CSV в очередь (будет сохранена в фоновом потоке).
    /// </summary>
    public static void WriteDetection(
        string cameraName,
        string detectType,
        string trafficLightState,
        int count,
        string timestamp,
        float duration)
    {
        if (!isInitialized) Init();

        // Формируем строку CSV (с разделителем ";", для Excel в некоторых регионах)
        string line = $"{cameraName};{detectType};{trafficLightState};{count};{timestamp};{duration}\n";
        csvQueue.Enqueue(line);
    }

    /// <summary>
    /// Гладко останавливает фоновый поток.
    /// </summary>
    public static void Shutdown()
    {
        if (!isRunning) return;
        isRunning = false;

        if (loggingThread != null && loggingThread.IsAlive)
        {
            loggingThread.Join(2000); 
            loggingThread = null;
        }
    }

    /// <summary>
    /// Вызывается при выходе из приложения (можно вызвать из OnApplicationQuit).
    /// </summary>
    public static void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        Shutdown();
    }
}

#endregion

// --------------------------------------------------------------------------------
// 2) Класс-DTO для парсинга детекций
// --------------------------------------------------------------------------------
[Serializable]
public class DetectionDTO
{
    public string trafficLightState; // Состояние светофора (например, "красный", "зеленый")
    public int count;
    public string timestamp; // Временная метка
    public float duration;
}

// Вспомогательный класс, чтобы распарсить {"count": N} из Python
[Serializable]
public class PythonResponseDTO
{
    public int count;
}


// --------------------------------------------------------------------------------
// 3) Основной скрипт PythonYOLOMinimal
// --------------------------------------------------------------------------------
public class PythonYOLODetector : MonoBehaviour
{
    [Header("Камера и маска")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Texture2D mask; // маска (0,0,0) - заливаем серым

    [Header("Что ищем?")]
    [SerializeField] private bool detectPeople = false;     // искать людей?
    [SerializeField] private bool detectTransport = false; // искать транспорт?

    [Header("Параметры детекции (локальный фильтр)")]
    [SerializeField] private bool enableLogging = true;         // Логгировать ли в файл

    private bool drawDetections = true; // Рисовать рамки на экране?
    private List<DetectionDTO> lastDetections = new List<DetectionDTO>(); // Список последних детекций

    // ZeroMQ
    private RequestSocket requestSocket;
    private RenderTexture captureRT;
    private bool isRunning = true; // Флаг для управления корутиной
    private bool shuttingDown = false;
    
    //[SerializeField] private TrafficLighter trafficLightCars;
    //[SerializeField] private TrafficLighter trafficLightPedestrians;
    [SerializeField] private TrafficLighter trafficLight; // для этой камеры – нужный светофор

    private string lastLightState = "";
    private float lastLightChangeTime = 0f;

    private void Awake()
    {
        if (enableLogging) MyAsyncLogger.Init();

        // Создаём RT для камеры
        captureRT = new RenderTexture(Screen.width, Screen.height, 24);

        // Инициализация ZeroMQ (NetMQ)
        AsyncIO.ForceDotNet.Force();
        requestSocket = new RequestSocket();

        // Подключаемся к тому же адресу, что в Python
        requestSocket.Connect("tcp://127.0.0.1:5555");

        if (enableLogging) Debug.Log("Подключились к tcp://127.0.0.1:5555");
        //Debug.Log("[YOLO] Connected to Python YOLO server.");
    }

    private void Start()
    {
        Application.targetFrameRate = 15; // или 10
        // Запускаем корутину, чтобы каждые N секунд посылать кадр
        StartCoroutine(SendFramesLoop());
    }

    private void OnDisable()
    {
        if (captureRT != null)
        {
            captureRT.Release();
            Destroy(captureRT);
            captureRT = null;
        }
    }
   
    private void OnApplicationQuit()
    {
        if (!shuttingDown)
        {
            shuttingDown = true;
            StartCoroutine(ShutdownCoroutine());
        }
    }

    private IEnumerator ShutdownCoroutine()
    {
        // Останавливаем корутину отправки кадров
        isRunning = false;

        // Даем время на завершение текущего кадра
        yield return new WaitForSeconds(0.1f);

        // Останавливаем все корутины, чтобы избежать возможных конфликтов
        StopAllCoroutines();

        // Освобождаем сокет запроса, если он создан
        if (requestSocket != null)
        {
            requestSocket.Dispose();
            requestSocket = null;
        }

        // Ждем один кадр перед дальнейшей очисткой
        yield return null;

        // Освобождаем RenderTexture, если он создан
        if (captureRT != null)
        {
            captureRT.Release();
            Destroy(captureRT);
            captureRT = null;
        }

        // Завершаем асинхронный логгер
        MyAsyncLogger.Shutdown();

        // Очищаем ресурсы NetMQ (вызывается один раз)
        NetMQConfig.Cleanup();
    }

    private string DetermineWantedString()
    {
        if (detectPeople && !detectTransport) return "person";
        if (!detectPeople && detectTransport) return "transport";
        return ""; // или "all"
    }

    // --------------------------------------------------------------------------------
    // Корутина: каждые 1 сек отправляем кадр, получаем ответ
    // --------------------------------------------------------------------------------
    private IEnumerator SendFramesLoop()
    {
        while (isRunning)
        {
            Texture2D frame = CaptureCameraFrame();
            if (frame != null)
            {
                try
                {
                    if (mask != null) ApplyMask(frame, mask);
                    byte[] jpgBytes = frame.EncodeToJPG();
                    int count = SendToPython(jpgBytes);
                    Debug.Log($"Camera: {targetCamera.name}, Count: {count}");
                }
                finally
                {
                    Destroy(frame);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // --------------------------------------------------------------------------------
    // Отправка изображения в Python, получение JSON
    // --------------------------------------------------------------------------------
    private int SendToPython(byte[] jpgBytes)
    {
        if (requestSocket == null)
        {
            if (enableLogging) Debug.LogError("requestSocket == null");
            return 0;
        }

        // Узнаём, что именно детектируем: "person" или "transport"
        string wanted = DetermineWantedString();
        requestSocket.SendMoreFrame(wanted);
        requestSocket.SendFrame(jpgBytes);

        if (enableLogging)
            MyAsyncLogger.Log($"Request: {wanted}, image size: {jpgBytes.Length}");

        bool received = requestSocket.TryReceiveFrameString(TimeSpan.FromSeconds(5), out string reply);
        if (!received || string.IsNullOrEmpty(reply) || reply.StartsWith("ERROR:"))
        {
            if (enableLogging) MyAsyncLogger.LogError("Python error: " + reply);
            return 0;
        }

        PythonResponseDTO pyResp;
        try
        {
            pyResp = JsonConvert.DeserializeObject<PythonResponseDTO>(reply);
        }
        catch (Exception ex)
        {
            if (enableLogging) Debug.LogError($"JSON parse error: {ex}");
            return 0;
        }

        // Получаем состояние светофора
        string trafficLightState = GetTrafficLightState();
        // Рассчитываем, сколько секунд цвет горит (если есть UpdateAndGetDuration)
        float dur = UpdateAndGetDuration(trafficLightState);

        // Текущая дата/время
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // Собираем DTO, если всё же нужно
        DetectionDTO result = new DetectionDTO
        {
            trafficLightState = trafficLightState,
            count = pyResp.count,
            timestamp = timeStamp,
            duration = dur
        };

        // Дополнительно определяем название камеры и "тип" (wanted), чтобы записать в CSV
        string cameraName = (targetCamera != null) ? targetCamera.name : "NoCamera";
        string detectType = wanted; // "person" или "transport"

        // Пишем в CSV
        MyCsvLogger.WriteDetection(
            cameraName,          // Название камеры
            detectType,          // "person"/"transport"
            result.trafficLightState,
            result.count,
            result.timestamp,
            result.duration
        );

        // Если нужно – можно логировать JSON в файл .txt или в консоль
        // string logJson = JsonConvert.SerializeObject(result);
        // MyAsyncLogger.Log($"Detections + Light: {logJson}");

        return pyResp.count;
    }

    public string GetTrafficLightState()
    {
        // Пример: если просто один светофор
        if (trafficLight == null) return "no-trafficlight-linked";
        return trafficLight.GetCurrentLightState();
    }

    private float UpdateAndGetDuration(string newState)
    {
        // Если состояние светофора (red/green/etc.) только что изменилось,
        // сбрасываем таймер
        if (newState != lastLightState)
        {
            lastLightState = newState;
            lastLightChangeTime = Time.time;
        }

        // Возвращаем, сколько секунд уже горит текущий цвет
        return Time.time - lastLightChangeTime;
    }


    // --------------------------------------------------------------------------------
    // Захват кадра с камеры
    // --------------------------------------------------------------------------------
    private Texture2D CaptureCameraFrame()
    {
        if (!targetCamera)
        {
            if (enableLogging) Debug.LogError("Не назначена targetCamera");
            return null;
        }

        targetCamera.targetTexture = captureRT;
        targetCamera.Render();
        RenderTexture.active = captureRT;

        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();

        targetCamera.targetTexture = null;
        RenderTexture.active = null;

        return tex;
    }

    private Texture2D ScaleMaskImage(Texture2D source, int targetWidth, int targetHeight)
    {
        if (source == null) return null;
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    private void SaveDebugImage(Texture2D image, string prefix)
    {
        string basePath = Application.dataPath;
        string debugFolder = Path.Combine(basePath, "DebugImages");
        if (!Directory.Exists(debugFolder))
        {
            Directory.CreateDirectory(debugFolder);
        }
        string filename = prefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png";
        string path = Path.Combine(debugFolder, filename);
        File.WriteAllBytes(path, image.EncodeToPNG());
        //Debug.Log($"[SaveDebugImage] Сохранено: {path}");
    }


    // --------------------------------------------------------------------------------
    // Маска: чёрные участки -> серый цвет
    // --------------------------------------------------------------------------------
    private void ApplyMask(Texture2D source, Texture2D maskTex)
    {
        Texture2D maskToUse = maskTex;


        if (maskTex.width != source.width || maskTex.height != source.height)
        {
            maskToUse = ScaleMaskImage(maskTex, source.width, source.height);
            if (enableLogging)
                Debug.Log("[Mask] Размер маски != размер кадра.");
        }
        if (maskToUse.width != source.width || maskToUse.height != source.height)
        {
            Debug.LogError($"[Mask] Ошибка! Размер маски ({maskToUse.width}x{maskToUse.height}) не совпадает с размером кадра ({source.width}x{source.height})!");
            return;
        }

        Color[] src = source.GetPixels();
        Color[] msk = maskToUse.GetPixels();
        for (int i = 0; i < src.Length; i++)
        {
            // Если маска "чёрная" (примерно <0.1)
            if (msk[i].r < 0.1f && msk[i].g < 0.1f && msk[i].b < 0.1f)
            {
                // Закрашиваем серым
                src[i] = new Color(0.5f, 0.5f, 0.5f);
            }
        }
        source.SetPixels(src);
        source.Apply();
        if (enableLogging) SaveDebugImage(source, "masked");

        if (maskToUse != maskTex)
        {
            Destroy(maskToUse);
        }
    }
}
