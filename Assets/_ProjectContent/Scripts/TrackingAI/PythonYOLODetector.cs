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

// --------------------------------------------------------------------------------
// 1) ЛОГГЕР: MyAsyncLogger
// --------------------------------------------------------------------------------
public static class MyAsyncLogger
{
    private static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
    private static Thread loggingThread;
    private static bool isRunning = false;

    private static string logFilePath;
    private static bool isInitialized = false;

    public static void Init()
    {
        if (isInitialized) return;
        logFilePath = Path.Combine(Application.dataPath, "MyAsyncLog.txt");

        isRunning = true;
        loggingThread = new Thread(LoggingThreadMethod);
        loggingThread.Start();
        isInitialized = true;
    }

    public static void Log(string message)
    {
        if (!isInitialized) Init();
        EnqueueMessage($"[{DateTime.Now:HH:mm:ss.fff}] [INFO] {message}");
    }

    public static void LogError(string message)
    {
        if (!isInitialized) Init();
        EnqueueMessage($"[{DateTime.Now:HH:mm:ss.fff}] [ERROR] {message}");
    }

    public static void OnApplicationQuit()
    {
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

    private static void EnqueueMessage(string msg)
    {
        logQueue.Enqueue(msg);
    }

    public static void Shutdown()
    {
        if (!isRunning) return;
        isRunning = false;

        // Ждем завершения, но с таймаутом
        if (loggingThread != null && loggingThread.IsAlive)
        {
            //if (!loggingThread.Join(5000)) // 5 сек на завершение
            //{
            //    loggingThread.Abort(); // Принудительно убиваем (не рекомендуется, но в крайнем случае)
            //}
            loggingThread.Join(2000); // Ждем 2 сек
            loggingThread = null; // Убираем ссылку на поток
        }
    }
}

// --------------------------------------------------------------------------------
// 2) Класс-DTO для парсинга детекций
// --------------------------------------------------------------------------------
[Serializable]
public class DetectionDTO
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

    private void Awake()
    {
        // Инициализация логгера
        if (enableLogging) MyAsyncLogger.Init();

        // Создаём RT для камеры
        captureRT = new RenderTexture(Screen.width, Screen.height, 24);

        // Инициализация ZeroMQ (NetMQ)
        AsyncIO.ForceDotNet.Force();
        requestSocket = new RequestSocket();

        // Подключаемся к тому же адресу, что в Python
        requestSocket.Connect("tcp://127.0.0.1:5555");

        if (enableLogging) Debug.Log("Подключились к tcp://127.0.0.1:5555");
        Debug.Log("[YOLO] Connected to Python YOLO server.");
    }

    private void Start()
    {
        // Запускаем корутину, чтобы каждые N секунд посылать кадр
        StartCoroutine(SendFramesLoop());
    }

    private void OnDisable()
    {
        // Вызывается при выключении объекта (или сцены),
        // но ещё до OnDestroy(). Здесь можно мягко завершить корутины.
        if (!shuttingDown)
        {
            shuttingDown = true;
            StartCoroutine(ShutdownCoroutine());
            StartCoroutine(WaitForShutdown()); // Ожидание перед выгрузкой сцены
        }
    }
    private IEnumerator WaitForShutdown()
    {
        yield return new WaitForSeconds(0.5f);
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
        isRunning = false; // Останавливаем корутину

        yield return new WaitForSeconds(0.1f); // Даем кадру обновиться

        // Останавливаем все корутины, чтобы избежать конфликтов
        StopAllCoroutines();

        // Закрываем и освобождаем сокет запроса
        if (requestSocket != null)
        {
            requestSocket.Dispose();
            requestSocket = null;
        }

        yield return null; // Ждем 1 кадр перед очисткой NetMQ

        // Чистим NetMQ (важно делать это после Dispose)
        NetMQConfig.Cleanup();

        // Освобождаем RenderTexture, если он был создан
        if (captureRT != null)
        {
            captureRT.Release();
            Destroy(captureRT);
            captureRT = null;
        }

        // Закрываем логгер (если используется асинхронный логгер)
        MyAsyncLogger.Shutdown();

        // Дополнительная защита: повторная очистка ZeroMQ (на случай, если остались dangling references)
        if (requestSocket != null)
        {
            requestSocket.Dispose();
            requestSocket = null;
        }

        NetMQConfig.Cleanup();
    }



    private string DetermineWantedString()
    {
        // Пример логики:
        if (detectPeople && !detectTransport)
            return "person";
        else if (!detectPeople && detectTransport)
            return "transport";
        return "";
        //else
        //    // Если оба включены или оба выключены - ищем все
        //    return "all";
    }

    // --------------------------------------------------------------------------------
    // Корутина: каждые 1 сек отправляем кадр, получаем ответ
    // --------------------------------------------------------------------------------
    private IEnumerator SendFramesLoop()
    {
        while (isRunning)
        {
            //Debug.Log("SendFramesLoop: iteration start...");
            Texture2D frame = CaptureCameraFrame();
            if (frame != null)
            {
                try
                {
                    if (mask != null)
                    {
                        ApplyMask(frame, mask);
                    }
                    byte[] jpgBytes = frame.EncodeToJPG();
                    Task<int> detectTask = Task.Run(() => SendToPython(jpgBytes));
                    yield return new WaitUntil(() => detectTask.IsCompleted);
                    int count = detectTask.Result;
                    if (enableLogging)
                        Debug.Log($"Получили детекций: {count}");
                }
                finally
                {
                    Destroy(frame);
                }
            }
            //Debug.Log("Loop iteration 4...");

            yield return new WaitForSeconds(1f); // раз в секунду
        }
    }

    private void DrawBox(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
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

        // 2) Отправляем
        string wanted = DetermineWantedString();
        requestSocket.SendMoreFrame(wanted);
        requestSocket.SendFrame(jpgBytes);

        if (enableLogging) MyAsyncLogger.Log($"Отправлен запрос на детекцию: {wanted}");
        if (enableLogging) MyAsyncLogger.Log($"Размер отправляемого изображения: {jpgBytes.Length} байт");

        // 3) Принимаем
        string reply;
        bool received = requestSocket.TryReceiveFrameString(TimeSpan.FromSeconds(5), out reply);
        if (!received || string.IsNullOrEmpty(reply) || reply.StartsWith("ERROR:"))
        {
            if (enableLogging) MyAsyncLogger.LogError("Python error: " + reply);
            return 0;
        }

        // 4) Парсим JSON
        try
        {
            DetectionDTO result = JsonConvert.DeserializeObject<DetectionDTO>(reply);
            return result.count;
        }
        catch (Exception ex)
        {
            if (enableLogging) Debug.LogError($"JSON parse error: {ex}");
            return 0;
        }
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

        if (enableLogging) SaveDebugImage(source, "original");
        Texture2D maskToUse = maskTex;


        if (maskTex.width != source.width || maskTex.height != source.height)
        {
            maskToUse = ScaleMaskImage(maskTex, source.width, source.height);
            // Если маска размером не совпадает с кадром, можно либо не применять,
            // либо ресайзить маску. Здесь для простоты просто логируем и игнорируем.
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
