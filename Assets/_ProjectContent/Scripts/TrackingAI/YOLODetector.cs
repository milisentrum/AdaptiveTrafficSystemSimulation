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

//    // ������� ��� ���������, ������� ����� �������� � ����
//    private static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

//    // ������� �����, ������� ����� ��������� ������ � ����
//    private static Thread loggingThread;
//    private static bool isRunning = false;

//    private static string logFilePath;
//    private static bool isInitialized = false;

//    public static void Init()
//    {
//        if (isInitialized) return;

//        // �������� Application.dataPath ������ � �������� ������ (����� ���� �����)
//        logFilePath = Path.Combine(Application.dataPath, "MyAsyncLog.txt");

//        isRunning = true;
//        loggingThread = new Thread(LoggingThreadMethod);
//        loggingThread.Start();

//        isInitialized = true;
//    }

//    /// <summary> ��� "�������" ���������� </summary>
//    public static void Log(string message)
//    {
//        if (!isInitialized) Init();
//        EnqueueMessage($"[{DateTime.Now:HH:mm:ss.fff}] [INFO] {message}");
//    }

//    /// <summary> ��� �������������� </summary>
//    public static void LogWarning(string message)
//    {
//        if (!isInitialized) Init();
//        EnqueueMessage($"[{DateTime.Now:HH:mm:ss.fff}] [WARN] {message}");
//    }

//    /// <summary> ��� ������ </summary>
//    public static void LogError(string message)
//    {
//        if (!isInitialized) Init();
//        EnqueueMessage($"[{DateTime.Now:HH:mm:ss.fff}] [ERROR] {message}");
//    }

//    /// <summary>
//    /// �������� �����, ������� ��������� ������� � ����
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
//    /// ��������� ������� �����
//    /// </summary>
//    public static void Shutdown()
//    {
//        if (!isRunning) return;
//        isRunning = false;
//        loggingThread?.Join(); // ���, ���� ����� �������� ������
//    }
//}
//#endregion


//public class YOLODetector : MonoBehaviour
//{
//    [Header("������ � �����")]
//    [SerializeField] private Camera targetCamera;          // ������ ��� ������ �����
//    [SerializeField] private Texture2D mask;                 // �����: ����� ������� � ��������, ������ � ���������

//    [Header("��������� ������/���������")]
//    [SerializeField] private int targetSize = 640;              // ������ �������� ����������� ��� ������
//    [SerializeField] private float confidenceThreshold = 0.5f;  // ����� ����������� (������� ��� ���������� ������ ������������)
//    [SerializeField] private float iouThreshold = 0.3f;         // IoU ����� ��� NMS
//    [SerializeField] private bool enableDebugLogging = false;

//    [Header("������ ��������")]
//    [SerializeField] private float minDetectionArea = 50f;    // ����������� ������� �������� ��� ���������� ������ ��������
//    [SerializeField] private float maxDetectionArea = 130f;    // ����������� ������� �������� ��� ���������� ������ ��������

//    // ������ ������ ����� (COCO) � ������� ������ ��������� � �������
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


//    // ������� ������ (�������), ������� ��� ����������: person (0), car (2), bus (5), truck (7)
//    private readonly HashSet<int> targetClassIndices = new HashSet<int> { 0, 2, 3, 5, 6, 7 };

//    //private readonly HashSet<int> targetClassIndices = new HashSet<int> { 0, 1, 3, 5, 30 };

//    private InferenceSession session;
//    // ��� ��� RenderTexture, ����� �� ��������� �� ������ ���
//    private RenderTexture captureRT;

//    private void Awake()
//    {
//        MyAsyncLogger.Init();
//        MyAsyncLogger.LogError($"YOLODetector Awake on {this.name}");

//        //session = YOLOManager.Session;
//        MyAsyncLogger.LogError($"YOLODetector got session={session}");

       
//        // ������ RenderTexture ��� ������� ����� � ������ (������ ������)
//        captureRT = new RenderTexture(Screen.width, Screen.height, 24);
//    }

   
//    private void Start()
//    {
//        session = YOLOManager.Session;
//        if (session == null)
//        {
//            Debug.LogError("Session �� ��� null!");
//        }
//        StartCoroutine(ProcessEverySecond());
//    }


//    private void OnDestroy()
//    {
//        // ����������� �������
//        if (captureRT != null)
//        {
//            captureRT.Release();
//            Destroy(captureRT);
//        }
//    }

//    /// <summary>
//    /// �������� ����: ������ ��� ������ ������, ��������� �������� ���������� � ��������� ���������.
//    /// </summary>
//    private IEnumerator ProcessEverySecond()
//    {
//        while (true)
//        {
//            // ����������� ���� � ������
//            Texture2D cameraFrame = CaptureCameraFrame();
//            if (cameraFrame == null)
//            {
//                MyAsyncLogger.LogWarning("[YOLO] �� ������� �������� ���� � ������!");
//                yield return new WaitForSeconds(1f);
//                continue;
//            }

//            // ��������� �����, ���� ��� ������
//            if (mask != null)
//            {
//                if (enableDebugLogging)
//                {
//                    MyAsyncLogger.Log("[YOLO] ��������� ����� � �����...");
//                }
//                ApplyMask(cameraFrame, mask);
//            }

//            // ���������������� �����������: ��������������� � ����������� ���������, ���������� ������ ��������
//            Texture2D preprocessed = PreprocessImageTexture(cameraFrame, targetSize, targetSize);
//            // ����������� ������������ ����
//            Destroy(cameraFrame);

//            // ����������� ����������� � ������ (float[] � ������� channels-first)
//            float[] inputData = ImageToTensorTexture(preprocessed);
//            var tensor = new DenseTensor<float>(inputData, new int[] { 1, 3, targetSize, targetSize });

//            // ��������� �������� � ��������� ������, ����� �� ����������� �������� �����
//            Task<List<YOLODetection>> inferenceTask = Task.Run(() => RunInference(tensor));
//            yield return new WaitUntil(() => inferenceTask.IsCompleted);
//            List<YOLODetection> rawDetections = inferenceTask.Result;

//            // ��������� NMS � ���������� �� ����������� ������� (��� ���������� ������ ��������/�����)
//            List<YOLODetection> finalDetections = NonMaximumSuppression(rawDetections, iouThreshold)
//                .Where(det => (det.Box.width < maxDetectionArea) && (det.Box.width > minDetectionArea))
//                //.Where(det => det.Box.width * det.Box.height >= minDetectionArea)
//                .ToList();


//            // ������������ ���������� ��������� � ��
//            //int pedestrians = finalDetections.Count();
//            int pedestrians = finalDetections.Count(d => d.ClassName.Equals("person", StringComparison.OrdinalIgnoreCase));

//            int vehicles = finalDetections.Count(d =>
//                d.ClassName.Equals("car", StringComparison.OrdinalIgnoreCase) ||
//                d.ClassName.Equals("bus", StringComparison.OrdinalIgnoreCase) ||
//                d.ClassName.Equals("truck", StringComparison.OrdinalIgnoreCase) ||
//                d.ClassName.Equals("motorcycle", StringComparison.OrdinalIgnoreCase) ||
//                d.ClassName.Equals("train", StringComparison.OrdinalIgnoreCase)
//            );

//            //MyAsyncLogger.Log($"[YOLO] ����������: �� = {vehicles}, ��������� = {pedestrians}");
//            Debug.Log($"[YOLO] ����������: �� = {vehicles}, ��������� = {pedestrians}");

//            // ������ ����� �� �����������
//            Texture2D imageWithBoxes = DrawDetections(preprocessed, finalDetections);
//            // ��������� ��������� � ����� DebugImages
//            if (enableDebugLogging)
//            {
//                // ��������� ��������� � ����� DebugImages, ���� �������� ����������� �������
//                SaveDebugImage(imageWithBoxes);
//            }
//            // ����������� ��������� ��������
//            Destroy(preprocessed);
//            Destroy(imageWithBoxes);

//            yield return new WaitForSeconds(1f);
//        }
//    }

//    /// <summary>
//    /// ����������� ���� � ������, ��������� ������� ��������� RenderTexture.
//    /// </summary>
//    private Texture2D CaptureCameraFrame()
//    {
//        if (targetCamera == null)
//        {
//            MyAsyncLogger.LogError("������ �� ���������!");
//            return null;
//        }
//        int width = Screen.width;
//        int height = Screen.height;

//        // ���������� ������������ RenderTexture
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
//    /// ������������ �����, ���� � ������� �� ��������� � ������, � ��������� �:
//    /// � �������� � ������ ������ �������� �����.
//    /// </summary>
//    private void ApplyMask(Texture2D source, Texture2D maskTex)
//    {
//        Texture2D maskToUse = maskTex;
//        if (maskTex.width != source.width || maskTex.height != source.height)
//        {
//            if (enableDebugLogging)
//            {
//                MyAsyncLogger.Log($"����� ({maskTex.width}x{maskTex.height}) != ���� ({source.width}x{source.height}), ������...");
//            }
//            maskToUse = ScaleMaskImage(maskTex, source.width, source.height);
//        }
//        DoMasking(source, maskToUse);
//        // ���� ������������� ������, ����������� ��������� �����
//        if (maskToUse != maskTex)
//        {
//            Destroy(maskToUse);
//        }
//    }

//    /// <summary>
//    /// ��������� ��������� �����: ���, ��� ����� ����� ������, �������� �����.
//    /// </summary>
//    private void DoMasking(Texture2D source, Texture2D maskTex)
//    {
//        Color[] sourcePixels = source.GetPixels();
//        Color[] maskPixels = maskTex.GetPixels();

//        for (int i = 0; i < sourcePixels.Length; i++)
//        {
//            // ���� ����� ����� ������ � �������, ��� ����� �� ����� ��������� ��������
//            if (maskPixels[i].r < 0.9f && maskPixels[i].g < 0.9f && maskPixels[i].b < 0.9f)
//            {
//                sourcePixels[i] = new Color(0.5f, 0.5f, 0.5f);
//            }
//        }
//        source.SetPixels(sourcePixels);
//        source.Apply();
//    }

//    /// <summary>
//    /// ������������ ����������� ����� �� ��������� ��������.
//    /// </summary>
//    private Texture2D ScaleMaskImage(Texture2D source, int targetWidth, int targetHeight)
//    {
//        if (source == null)
//        {
//            MyAsyncLogger.LogError("��� �������� ����� ��� ���������������!");
//            return null;
//        }
//        if (!source.isReadable)
//        {
//            MyAsyncLogger.LogError("����� �� ����� Read/Write Enabled.");
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
//    /// ������������ �������� ����������� � ����������� ��������� � ��������� ������ ������� �����.
//    /// </summary>
//    private Texture2D PreprocessImageTexture(Texture2D image, int targetWidth, int targetHeight)
//    {
//        float scale = Mathf.Min((float)targetWidth / image.width, (float)targetHeight / image.height);
//        int newWidth = Mathf.RoundToInt(image.width * scale);
//        int newHeight = Mathf.RoundToInt(image.height * scale);

//        // ������ �������� � �������� �����
//        Texture2D resized = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
//        Color[] fillPixels = Enumerable.Repeat(new Color(0.5f, 0.5f, 0.5f), targetWidth * targetHeight).ToArray();
//        resized.SetPixels(fillPixels);

//        // ������������ �������� �����������
//        Texture2D scaledImage = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
//        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
//        Graphics.Blit(image, rt);
//        RenderTexture.active = rt;
//        scaledImage.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
//        scaledImage.Apply();
//        RenderTexture.active = null;
//        RenderTexture.ReleaseTemporary(rt);

//        // ��������� ���������������� ����������� � �����
//        int left = (targetWidth - newWidth) / 2;
//        int top = (targetHeight - newHeight) / 2;
//        resized.SetPixels(left, top, newWidth, newHeight, scaledImage.GetPixels());
//        resized.Apply();
//        Destroy(scaledImage);
//        return resized;
//    }

//    /// <summary>
//    /// ����������� Texture2D � ������ float � ������� channels-first (R, G, B) � ������������� [0,1].
//    /// </summary>
//    private float[] ImageToTensorTexture(Texture2D image)
//    {
//        int width = image.width;
//        int height = image.height;
//        float[] data = new float[3 * width * height];
//        Color[] pixels = image.GetPixels();

//        // ��������� ����� R
//        for (int i = 0; i < pixels.Length; i++)
//        {
//            data[i] = pixels[i].r;
//        }
//        // ����� G
//        for (int i = 0; i < pixels.Length; i++)
//        {
//            data[i + pixels.Length] = pixels[i].g;
//        }
//        // ����� B
//        for (int i = 0; i < pixels.Length; i++)
//        {
//            data[i + 2 * pixels.Length] = pixels[i].b;
//        }
//        return data;
//    }

//    /// <summary>
//    /// ��������� �������� ������ � �������� "�����" ��������.
//    /// ����������� � ��������� ������.
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
//                MyAsyncLogger.LogError($"�������� ������ [1,84,8400], ������� [{string.Join(",", dims)}]");
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
//                // ������� ������ ������� � 5-�� ������
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
//                    // ����������� ���������� �� ������ � ���������� �������� ������ ����
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
//    /// ��������� �������� ���������� ������������ (NMS) ��� ��������.
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
//    /// ��������� IoU (����������� �����������) ���� ���������������.
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
//    /// ������ ����� �������� �� �����������.
//    /// </summary>
//    private Texture2D DrawDetections(Texture2D image, List<YOLODetection> detections)
//    {
//        Texture2D result = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
//        result.SetPixels(image.GetPixels());

//        foreach (var det in detections)
//        {
//            // �������� �������� ������
//            MyAsyncLogger.Log($"[YOLO] Detected class: {det.ClassName}, Confidence = {det.Confidence}");

//            // ��� ��������� � ������ �����, ��� �� � �������
//            Color color = det.ClassName.Equals("person", StringComparison.OrdinalIgnoreCase) ? Color.green : Color.red;
//            DrawRectangle(result, det.Box, color);
//        }

//        result.Apply();
//        return result;
//    }


//    /// <summary>
//    /// ������ ������������� �� ��������.
//    /// </summary>
//    private void DrawRectangle(Texture2D tex, Rect rect, Color color)
//    {
//        int xMin = Mathf.RoundToInt(rect.xMin);
//        int xMax = Mathf.RoundToInt(rect.xMax);
//        int yMin = Mathf.RoundToInt(rect.yMin);
//        int yMax = Mathf.RoundToInt(rect.yMax);

//        // ������ ������� � ������ �������
//        for (int x = xMin; x <= xMax; x++)
//        {
//            tex.SetPixel(x, yMin, color);
//            tex.SetPixel(x, yMax, color);
//        }
//        // ������ ����� � ������ �������
//        for (int y = yMin; y <= yMax; y++)
//        {
//            tex.SetPixel(xMin, y, color);
//            tex.SetPixel(xMax, y, color);
//        }
//    }

//    /// <summary>
//    /// ��������� ����������� � ������� � ����� DebugImages � ���������� ������.
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
//        //MyAsyncLogger.Log($"[YOLO] ������������ ����������� ���������: {path}");
//    }
//}

/// <summary>
/// ����� ��� ������������� ��������, ������������ � YOLODetection ��� ��������� ����������.
/// </summary>
//public class YOLODetection
//{
//    public string ClassName; // �������� ������ (��������, "person", "car" � �.�.)
//    public float Confidence; // ����������� ��������
//    public Rect Box;         // ������������� (���������� � �������� ��������)
//}
