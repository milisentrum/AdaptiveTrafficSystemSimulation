using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Management.Instrumentation;
using UnityEditor.EditorTools;

//#region MyAsyncLogger
//public static class MyAsyncLogger
//{

//    // Очередь для сообщений, которые нужно дописать в файл
//    private static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

//    // Фоновый поток, который будет постоянно писать в файл
//    private static Thread loggingThread;
//    private static bool isRunning = false;

//    private static string logFilePath;
//    private static bool isInitialized = false;

//    public static void Init()
//    {
//        if (isInitialized) return;

//        // Вызываем Application.dataPath ТОЛЬКО в основном потоке (через этот метод)
//        logFilePath = Path.Combine(Application.dataPath, "MyAsyncLog.txt");

//        isRunning = true;
//        loggingThread = new Thread(LoggingThreadMethod);
//        loggingThread.Start();

//        isInitialized = true;
//    }

//    /// <summary> Лог "обычной" информации </summary>
//    public static void Log(string message)
//    {
//        if (!isInitialized) Init();
//        EnqueueMessage($"[{DateTime.Now:HH:mm:ss.fff}] [INFO] {message}");
//    }

//    /// <summary> Лог предупреждения </summary>
//    public static void LogWarning(string message)
//    {
//        if (!isInitialized) Init();
//        EnqueueMessage($"[{DateTime.Now:HH:mm:ss.fff}] [WARN] {message}");
//    }

//    /// <summary> Лог ошибки </summary>
//    public static void LogError(string message)
//    {
//        if (!isInitialized) Init();
//        EnqueueMessage($"[{DateTime.Now:HH:mm:ss.fff}] [ERROR] {message}");
//    }

//    /// <summary>
//    /// Основной поток, который выгружает очередь в файл
//    /// </summary>
//    private static void LoggingThreadMethod()
//    {
//        using (var writer = new StreamWriter(logFilePath, true))
//        {
//            while (isRunning)
//            {
//                while (logQueue.TryDequeue(out string logLine))
//                {
//                    writer.WriteLine(logLine);
//                }
//                writer.Flush();
//                Thread.Sleep(50);
//            }
//        }
//    }

//    private static void EnqueueMessage(string msg)
//    {
//        logQueue.Enqueue(msg);
//    }

//    /// <summary>
//    /// Завершает фоновый поток
//    /// </summary>
//    public static void Shutdown()
//    {
//        if (!isRunning) return;
//        isRunning = false;
//        loggingThread?.Join(); // Ждём, пока поток закончит работу
//    }
//}
//#endregion


//public class YOLODetector : MonoBehaviour
//{
//    [Header("Камера и маска")]
//    [SerializeField] private Camera targetCamera;          // Камера для съёмки сцены
//    [SerializeField] private Texture2D mask;                 // Маска: белые области – оставить, чёрные – закрасить

//    [Header("Параметры модели/инференса")]
//    [SerializeField] private int targetSize = 640;              // Размер входного изображения для модели
//    [SerializeField] private float confidenceThreshold = 0.5f;  // Порог уверенности (повышен для исключения ложных срабатываний)
//    [SerializeField] private float iouThreshold = 0.3f;         // IoU порог для NMS
//    [SerializeField] private bool enableDebugLogging = false;

//    [Header("Фильтр площадей")]
//    [SerializeField] private float minDetectionArea = 50f;    // Минимальная площадь детекции для фильтрации мелких объектов
//    [SerializeField] private float maxDetectionArea = 130f;    // Минимальная площадь детекции для фильтрации мелких объектов

//    // Полный список меток (COCO) – порядок должен совпадать с моделью
//    private readonly string[] labels = new string[] {
//        "person",         // 0
//        "bicycle",        // 1
//        "car",            // 2
//        "motorcycle",     // 3
//        "airplane",       // 4
//        "bus",            // 5
//        "train",          // 6
//        "truck",          // 7
//        "boat",           // 8
//        "traffic light",  // 9
//        "fire hydrant",   // 10
//        "stop sign",      // 11
//        "parking meter",  // 12
//        "bench",          // 13
//        "bird",           // 14
//        "cat",            // 15
//        "dog",            // 16
//        "horse",          // 17
//        "sheep",          // 18
//        "cow",            // 19
//        "elephant",       // 20
//        "bear",           // 21
//        "zebra",          // 22
//        "giraffe",        // 23
//        "backpack",       // 24
//        "umbrella",       // 25
//        "handbag",        // 26
//        "tie",            // 27
//        "suitcase",       // 28
//        "frisbee",        // 29
//        "skis",           // 30
//        "snowboard",      // 31
//        "sports ball",    // 32
//        "kite",           // 33
//        "baseball bat",   // 34
//        "baseball glove", // 35
//        "skateboard",     // 36
//        "surfboard",      // 37
//        "tennis racket",  // 38
//        "bottle",         // 39
//        "wine glass",     // 40
//        "cup",            // 41
//        "fork",           // 42
//        "knife",          // 43
//        "spoon",          // 44
//        "bowl",           // 45
//        "banana",         // 46
//        "apple",          // 47
//        "sandwich",       // 48
//        "orange",         // 49
//        "broccoli",       // 50
//        "carrot",         // 51
//        "hot dog",        // 52
//        "pizza",          // 53
//        "donut",          // 54
//        "cake",           // 55
//        "chair",          // 56
//        "couch",          // 57
//        "potted plant",   // 58
//        "bed",            // 59
//        "dining table",   // 60
//        "toilet",         // 61
//        "tv",             // 62
//        "laptop",         // 63
//        "mouse",          // 64
//        "remote",         // 65
//        "keyboard",       // 66
//        "cell phone",     // 67
//        "microwave",      // 68
//        "oven",           // 69
//        "toaster",        // 70
//        "sink",           // 71
//        "refrigerator",   // 72
//        "book",           // 73
//        "clock",          // 74
//        "vase",           // 75
//        "scissors",       // 76
//        "teddy bear",     // 77
//        "hair drier",     // 78
//        "toothbrush"      // 79
//    };


//    // Целевые классы (индексы), которые нас интересуют: person (0), car (2), bus (5), truck (7)
//    private readonly HashSet<int> targetClassIndices = new HashSet<int> { 0, 2, 3, 5, 6, 7 };

//    //private readonly HashSet<int> targetClassIndices = new HashSet<int> { 0, 1, 3, 5, 30 };

//    private InferenceSession session;
//    // Пул для RenderTexture, чтобы не создавать их каждый раз
//    private RenderTexture captureRT;

//    private void Awake()
//    {
//        MyAsyncLogger.Init();
//        MyAsyncLogger.LogError($"YOLODetector Awake on {this.name}");

//        //session = YOLOManager.Session;
//        MyAsyncLogger.LogError($"YOLODetector got session={session}");

       
//        // Создаём RenderTexture для захвата кадра с камеры (размер экрана)
//        captureRT = new RenderTexture(Screen.width, Screen.height, 24);
//    }

   
//    private void Start()
//    {
//        session = YOLOManager.Session;
//        if (session == null)
//        {
//            Debug.LogError("Session всё ещё null!");
//        }
//        StartCoroutine(ProcessEverySecond());
//    }


//    private void OnDestroy()
//    {
//        // Освобождаем ресурсы
//        if (captureRT != null)
//        {
//            captureRT.Release();
//            Destroy(captureRT);
//        }
//    }

//    /// <summary>
//    /// Основной цикл: каждый раз делаем снимок, выполняем инференс асинхронно и сохраняем результат.
//    /// </summary>
//    private IEnumerator ProcessEverySecond()
//    {
//        while (true)
//        {
//            // Захватываем кадр с камеры
//            Texture2D cameraFrame = CaptureCameraFrame();
//            if (cameraFrame == null)
//            {
//                MyAsyncLogger.LogWarning("[YOLO] Не удалось получить кадр с камеры!");
//                yield return new WaitForSeconds(1f);
//                continue;
//            }

//            // Применяем маску, если она задана
//            if (mask != null)
//            {
//                if (enableDebugLogging)
//                {
//                    MyAsyncLogger.Log("[YOLO] Применяем маску к кадру...");
//                }
//                ApplyMask(cameraFrame, mask);
//            }

//            // Предобрабатываем изображение: масштабирование с сохранением пропорций, заполнение пустых областей
//            Texture2D preprocessed = PreprocessImageTexture(cameraFrame, targetSize, targetSize);
//            // Освобождаем оригинальный кадр
//            Destroy(cameraFrame);

//            // Преобразуем изображение в тензор (float[] в формате channels-first)
//            float[] inputData = ImageToTensorTexture(preprocessed);
//            var tensor = new DenseTensor<float>(inputData, new int[] { 1, 3, targetSize, targetSize });

//            // Выполняем инференс в отдельном потоке, чтобы не блокировать основной поток
//            Task<List<YOLODetection>> inferenceTask = Task.Run(() => RunInference(tensor));
//            yield return new WaitUntil(() => inferenceTask.IsCompleted);
//            List<YOLODetection> rawDetections = inferenceTask.Result;

//            // Применяем NMS и фильтрацию по минимальной площади (для исключения мелких объектов/теней)
//            List<YOLODetection> finalDetections = NonMaximumSuppression(rawDetections, iouThreshold)
//                .Where(det => (det.Box.width < maxDetectionArea) && (det.Box.width > minDetectionArea))
//                //.Where(det => det.Box.width * det.Box.height >= minDetectionArea)
//                .ToList();


//            // Подсчитываем количество пешеходов и ТС
//            //int pedestrians = finalDetections.Count();
//            int pedestrians = finalDetections.Count(d => d.ClassName.Equals("person", StringComparison.OrdinalIgnoreCase));

//            int vehicles = finalDetections.Count(d =>
//                d.ClassName.Equals("car", StringComparison.OrdinalIgnoreCase) ||
//                d.ClassName.Equals("bus", StringComparison.OrdinalIgnoreCase) ||
//                d.ClassName.Equals("truck", StringComparison.OrdinalIgnoreCase) ||
//                d.ClassName.Equals("motorcycle", StringComparison.OrdinalIgnoreCase) ||
//                d.ClassName.Equals("train", StringComparison.OrdinalIgnoreCase)
//            );

//            //MyAsyncLogger.Log($"[YOLO] Обнаружено: ТС = {vehicles}, пешеходов = {pedestrians}");
//            Debug.Log($"[YOLO] Обнаружено: ТС = {vehicles}, пешеходов = {pedestrians}");

//            // Рисуем рамки на изображении
//            Texture2D imageWithBoxes = DrawDetections(preprocessed, finalDetections);
//            // Сохраняем результат в папку DebugImages
//            if (enableDebugLogging)
//            {
//                // Сохраняем результат в папку DebugImages, если включено логирование отладки
//                SaveDebugImage(imageWithBoxes);
//            }
//            // Освобождаем временные текстуры
//            Destroy(preprocessed);
//            Destroy(imageWithBoxes);

//            yield return new WaitForSeconds(1f);
//        }
//    }

//    /// <summary>
//    /// Захватывает кадр с камеры, используя заранее созданный RenderTexture.
//    /// </summary>
//    private Texture2D CaptureCameraFrame()
//    {
//        if (targetCamera == null)
//        {
//            MyAsyncLogger.LogError("Камера не привязана!");
//            return null;
//        }
//        int width = Screen.width;
//        int height = Screen.height;

//        // Используем кэшированный RenderTexture
//        targetCamera.targetTexture = captureRT;
//        targetCamera.Render();
//        RenderTexture.active = captureRT;

//        Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
//        frame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//        frame.Apply();

//        targetCamera.targetTexture = null;
//        RenderTexture.active = null;

//        return frame;
//    }

//    /// <summary>
//    /// Масштабирует маску, если её размеры не совпадают с кадром, и применяет её:
//    /// в областях с темной маской заливаем серым.
//    /// </summary>
//    private void ApplyMask(Texture2D source, Texture2D maskTex)
//    {
//        Texture2D maskToUse = maskTex;
//        if (maskTex.width != source.width || maskTex.height != source.height)
//        {
//            if (enableDebugLogging)
//            {
//                MyAsyncLogger.Log($"Маска ({maskTex.width}x{maskTex.height}) != кадр ({source.width}x{source.height}), ресайз...");
//            }
//            maskToUse = ScaleMaskImage(maskTex, source.width, source.height);
//        }
//        DoMasking(source, maskToUse);
//        // Если использовался ресайз, освобождаем временную маску
//        if (maskToUse != maskTex)
//        {
//            Destroy(maskToUse);
//        }
//    }

//    /// <summary>
//    /// Применяет наложение маски: там, где маска почти чёрная, заливаем серым.
//    /// </summary>
//    private void DoMasking(Texture2D source, Texture2D maskTex)
//    {
//        Color[] sourcePixels = source.GetPixels();
//        Color[] maskPixels = maskTex.GetPixels();

//        for (int i = 0; i < sourcePixels.Length; i++)
//        {
//            // Если маска почти чёрная – считаем, что здесь не нужно оставлять оригинал
//            if (maskPixels[i].r < 0.9f && maskPixels[i].g < 0.9f && maskPixels[i].b < 0.9f)
//            {
//                sourcePixels[i] = new Color(0.5f, 0.5f, 0.5f);
//            }
//        }
//        source.SetPixels(sourcePixels);
//        source.Apply();
//    }

//    /// <summary>
//    /// Масштабирует изображение маски до указанных размеров.
//    /// </summary>
//    private Texture2D ScaleMaskImage(Texture2D source, int targetWidth, int targetHeight)
//    {
//        if (source == null)
//        {
//            MyAsyncLogger.LogError("Нет исходной маски для масштабирования!");
//            return null;
//        }
//        if (!source.isReadable)
//        {
//            MyAsyncLogger.LogError("Маска не имеет Read/Write Enabled.");
//            return null;
//        }
//        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
//        rt.filterMode = FilterMode.Bilinear;
//        Graphics.Blit(source, rt);
//        RenderTexture.active = rt;
//        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
//        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
//        result.Apply();
//        RenderTexture.active = null;
//        RenderTexture.ReleaseTemporary(rt);
//        return result;
//    }

//    /// <summary>
//    /// Масштабирует исходное изображение с сохранением пропорций и заполняет пустые области серым.
//    /// </summary>
//    private Texture2D PreprocessImageTexture(Texture2D image, int targetWidth, int targetHeight)
//    {
//        float scale = Mathf.Min((float)targetWidth / image.width, (float)targetHeight / image.height);
//        int newWidth = Mathf.RoundToInt(image.width * scale);
//        int newHeight = Mathf.RoundToInt(image.height * scale);

//        // Создаём текстуру с заливкой серым
//        Texture2D resized = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
//        Color[] fillPixels = Enumerable.Repeat(new Color(0.5f, 0.5f, 0.5f), targetWidth * targetHeight).ToArray();
//        resized.SetPixels(fillPixels);

//        // Масштабируем исходное изображение
//        Texture2D scaledImage = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
//        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
//        Graphics.Blit(image, rt);
//        RenderTexture.active = rt;
//        scaledImage.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
//        scaledImage.Apply();
//        RenderTexture.active = null;
//        RenderTexture.ReleaseTemporary(rt);

//        // Вставляем масштабированное изображение в центр
//        int left = (targetWidth - newWidth) / 2;
//        int top = (targetHeight - newHeight) / 2;
//        resized.SetPixels(left, top, newWidth, newHeight, scaledImage.GetPixels());
//        resized.Apply();
//        Destroy(scaledImage);
//        return resized;
//    }

//    /// <summary>
//    /// Преобразует Texture2D в массив float в формате channels-first (R, G, B) с нормализацией [0,1].
//    /// </summary>
//    private float[] ImageToTensorTexture(Texture2D image)
//    {
//        int width = image.width;
//        int height = image.height;
//        float[] data = new float[3 * width * height];
//        Color[] pixels = image.GetPixels();

//        // Заполняем канал R
//        for (int i = 0; i < pixels.Length; i++)
//        {
//            data[i] = pixels[i].r;
//        }
//        // Канал G
//        for (int i = 0; i < pixels.Length; i++)
//        {
//            data[i + pixels.Length] = pixels[i].g;
//        }
//        // Канал B
//        for (int i = 0; i < pixels.Length; i++)
//        {
//            data[i + 2 * pixels.Length] = pixels[i].b;
//        }
//        return data;
//    }

//    /// <summary>
//    /// Выполняет инференс модели и собирает "сырые" детекции.
//    /// Запускается в отдельном потоке.
//    /// </summary>
//    private List<YOLODetection> RunInference(DenseTensor<float> tensor)
//    {
//        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", tensor) };
//        List<YOLODetection> detections = new List<YOLODetection>();

//        using (var results = session.Run(inputs))
//        {
//            var output = results.First().AsTensor<float>();
//            int[] dims = output.Dimensions.ToArray();
//            if (dims.Length != 3)
//            {
//                MyAsyncLogger.LogError($"Ожидался тензор [1,84,8400], получен [{string.Join(",", dims)}]");
//                return detections;
//            }
//            int channels = dims[1];
//            int anchors = dims[2];
//            for (int a = 0; a < anchors; a++)
//            {
//                float x = output[0, 0, a];
//                float y = output[0, 1, a];
//                float w = output[0, 2, a];
//                float h = output[0, 3, a];
//                float objConf = output[0, 4, a];

//                int bestClassIndex = -1;
//                float bestClassScore = 0f;
//                // Обходим классы начиная с 5-го канала
//                for (int c = 5; c < channels; c++)
//                {
//                    float clsScore = output[0, c, a];
//                    if (clsScore > bestClassScore)
//                    {
//                        bestClassScore = clsScore;
//                        bestClassIndex = c - 5;
//                    }
//                }


//                float confidence = objConf * bestClassScore;

//                //MyAsyncLogger.Log($"bestClassIndex = {bestClassIndex}, label = {labels[bestClassIndex]}, conf = {confidence}");


//                if (confidence >= confidenceThreshold && targetClassIndices.Contains(bestClassIndex))
//                {
//                    // Преобразуем координаты из центра в координаты верхнего левого угла
//                    float x1 = x - w / 2f;
//                    float y1 = y - h / 2f;
//                    YOLODetection det = new YOLODetection
//                    {
//                        ClassName = labels[bestClassIndex],
//                        Confidence = confidence,
//                        Box = new Rect(x1, y1, w, h)
//                    };
//                    detections.Add(det);
//                }
//            }
//        }
//        return detections;
//    }

//    /// <summary>
//    /// Применяет алгоритм подавления немаксимумов (NMS) для детекций.
//    /// </summary>
//    private List<YOLODetection> NonMaximumSuppression(List<YOLODetection> detections, float iouThreshold)
//    {
//        List<YOLODetection> nmsDetections = new List<YOLODetection>();
//        var grouped = detections.GroupBy(d => d.ClassName);
//        foreach (var group in grouped)
//        {
//            var sorted = group.OrderByDescending(d => d.Confidence).ToList();
//            while (sorted.Count > 0)
//            {
//                YOLODetection best = sorted[0];
//                nmsDetections.Add(best);
//                sorted.RemoveAt(0);
//                sorted = sorted.Where(d => ComputeIoU(best.Box, d.Box) < iouThreshold).ToList();
//            }
//        }
//        return nmsDetections;
//    }

//    /// <summary>
//    /// Вычисляет IoU (коэффициент пересечения) двух прямоугольников.
//    /// </summary>
//    private float ComputeIoU(Rect a, Rect b)
//    {
//        float x1 = Mathf.Max(a.xMin, b.xMin);
//        float y1 = Mathf.Max(a.yMin, b.yMin);
//        float x2 = Mathf.Min(a.xMax, b.xMax);
//        float y2 = Mathf.Min(a.yMax, b.yMax);
//        float intersectionW = Mathf.Max(0, x2 - x1);
//        float intersectionH = Mathf.Max(0, y2 - y1);
//        float intersection = intersectionW * intersectionH;
//        float union = a.width * a.height + b.width * b.height - intersection;
//        return union > 0 ? intersection / union : 0;
//    }

//    /// <summary>
//    /// Рисует рамки детекций на изображении.
//    /// </summary>
//    private Texture2D DrawDetections(Texture2D image, List<YOLODetection> detections)
//    {
//        Texture2D result = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
//        result.SetPixels(image.GetPixels());

//        foreach (var det in detections)
//        {
//            // Логируем название класса
//            MyAsyncLogger.Log($"[YOLO] Detected class: {det.ClassName}, Confidence = {det.Confidence}");

//            // Для пешеходов – зелёная рамка, для ТС – красная
//            Color color = det.ClassName.Equals("person", StringComparison.OrdinalIgnoreCase) ? Color.green : Color.red;
//            DrawRectangle(result, det.Box, color);
//        }

//        result.Apply();
//        return result;
//    }


//    /// <summary>
//    /// Рисует прямоугольник на текстуре.
//    /// </summary>
//    private void DrawRectangle(Texture2D tex, Rect rect, Color color)
//    {
//        int xMin = Mathf.RoundToInt(rect.xMin);
//        int xMax = Mathf.RoundToInt(rect.xMax);
//        int yMin = Mathf.RoundToInt(rect.yMin);
//        int yMax = Mathf.RoundToInt(rect.yMax);

//        // Рисуем верхнюю и нижнюю границы
//        for (int x = xMin; x <= xMax; x++)
//        {
//            tex.SetPixel(x, yMin, color);
//            tex.SetPixel(x, yMax, color);
//        }
//        // Рисуем левую и правую границы
//        for (int y = yMin; y <= yMax; y++)
//        {
//            tex.SetPixel(xMin, y, color);
//            tex.SetPixel(xMax, y, color);
//        }
//    }

//    /// <summary>
//    /// Сохраняет изображение с рамками в папку DebugImages с уникальным именем.
//    /// </summary>
//    private void SaveDebugImage(Texture2D image)
//    {
//        string basePath = Application.dataPath;
//        string debugFolder = Path.Combine(basePath, "DebugImages");
//        if (!Directory.Exists(debugFolder))
//        {
//            Directory.CreateDirectory(debugFolder);
//        }
//        string filename = "detected_result_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png";
//        string path = Path.Combine(debugFolder, filename);
//        File.WriteAllBytes(path, image.EncodeToPNG());
//        //MyAsyncLogger.Log($"[YOLO] Обработанное изображение сохранено: {path}");
//    }
//}

/// <summary>
/// Класс для представления детекции, переименован в YOLODetection для избежания конфликтов.
/// </summary>
//public class YOLODetection
//{
//    public string ClassName; // Название класса (например, "person", "car" и т.д.)
//    public float Confidence; // Уверенность детекции
//    public Rect Box;         // Прямоугольник (координаты в исходном масштабе)
//}
