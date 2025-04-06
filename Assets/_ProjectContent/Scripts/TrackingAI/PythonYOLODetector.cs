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
        
        // ���� ���� �� ���������� � ����� ���������. 
        // ����� ��������� ����, ���� ����� ������ ���������� ��������:
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
    /// �������� ����� �������� ������, ��� ��� ������ � CSV.
    /// </summary>
    private static void LoggingThreadMethod()
    {
        // ��������� ���� � ������ "append" (���������� � �����).
        using (var writer = new StreamWriter(csvFilePath, true))
        {
            // ���� �� ����������� � ���������� ������� � ����� � ����.
            while (isRunning)
            {
                while (csvQueue.TryDequeue(out string line))
                {
                    writer.Write(line); 
                    // line ��� �������� ������ ����� ������ \n
                }
                writer.Flush();
                Thread.Sleep(50);
            }
        }
    }

    /// <summary>
    /// ���������� ������ CSV � ������� (����� ��������� � ������� ������).
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

        // ��������� ������ CSV (� ������������ ";", ��� Excel � ��������� ��������)
        string line = $"{cameraName};{detectType};{trafficLightState};{count};{timestamp};{duration}\n";
        csvQueue.Enqueue(line);
    }

    /// <summary>
    /// ������ ������������� ������� �����.
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
    /// ���������� ��� ������ �� ���������� (����� ������� �� OnApplicationQuit).
    /// </summary>
    public static void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        Shutdown();
    }
}

#endregion

// --------------------------------------------------------------------------------
// 2) �����-DTO ��� �������� ��������
// --------------------------------------------------------------------------------
[Serializable]
public class DetectionDTO
{
    public string trafficLightState; // ��������� ��������� (��������, "�������", "�������")
    public int count;
    public string timestamp; // ��������� �����
    public float duration;
}

// ��������������� �����, ����� ���������� {"count": N} �� Python
[Serializable]
public class PythonResponseDTO
{
    public int count;
}


// --------------------------------------------------------------------------------
// 3) �������� ������ PythonYOLOMinimal
// --------------------------------------------------------------------------------
public class PythonYOLODetector : MonoBehaviour
{
    [Header("������ � �����")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Texture2D mask; // ����� (0,0,0) - �������� �����

    [Header("��� ����?")]
    [SerializeField] private bool detectPeople = false;     // ������ �����?
    [SerializeField] private bool detectTransport = false; // ������ ���������?

    [Header("��������� �������� (��������� ������)")]
    [SerializeField] private bool enableLogging = true;         // ����������� �� � ����

    private bool drawDetections = true; // �������� ����� �� ������?
    private List<DetectionDTO> lastDetections = new List<DetectionDTO>(); // ������ ��������� ��������

    // ZeroMQ
    private RequestSocket requestSocket;
    private RenderTexture captureRT;
    private bool isRunning = true; // ���� ��� ���������� ���������
    private bool shuttingDown = false;
    
    //[SerializeField] private TrafficLighter trafficLightCars;
    //[SerializeField] private TrafficLighter trafficLightPedestrians;
    [SerializeField] private TrafficLighter trafficLight; // ��� ���� ������ � ������ ��������

    private string lastLightState = "";
    private float lastLightChangeTime = 0f;

    private void Awake()
    {
        if (enableLogging) MyAsyncLogger.Init();

        // ������ RT ��� ������
        captureRT = new RenderTexture(Screen.width, Screen.height, 24);

        // ������������� ZeroMQ (NetMQ)
        AsyncIO.ForceDotNet.Force();
        requestSocket = new RequestSocket();

        // ������������ � ���� �� ������, ��� � Python
        requestSocket.Connect("tcp://127.0.0.1:5555");

        if (enableLogging) Debug.Log("������������ � tcp://127.0.0.1:5555");
        //Debug.Log("[YOLO] Connected to Python YOLO server.");
    }

    private void Start()
    {
        Application.targetFrameRate = 15; // ��� 10
        // ��������� ��������, ����� ������ N ������ �������� ����
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
        // ������������� �������� �������� ������
        isRunning = false;

        // ���� ����� �� ���������� �������� �����
        yield return new WaitForSeconds(0.1f);

        // ������������� ��� ��������, ����� �������� ��������� ����������
        StopAllCoroutines();

        // ����������� ����� �������, ���� �� ������
        if (requestSocket != null)
        {
            requestSocket.Dispose();
            requestSocket = null;
        }

        // ���� ���� ���� ����� ���������� ��������
        yield return null;

        // ����������� RenderTexture, ���� �� ������
        if (captureRT != null)
        {
            captureRT.Release();
            Destroy(captureRT);
            captureRT = null;
        }

        // ��������� ����������� ������
        MyAsyncLogger.Shutdown();

        // ������� ������� NetMQ (���������� ���� ���)
        NetMQConfig.Cleanup();
    }

    private string DetermineWantedString()
    {
        if (detectPeople && !detectTransport) return "person";
        if (!detectPeople && detectTransport) return "transport";
        return ""; // ��� "all"
    }

    // --------------------------------------------------------------------------------
    // ��������: ������ 1 ��� ���������� ����, �������� �����
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
    // �������� ����������� � Python, ��������� JSON
    // --------------------------------------------------------------------------------
    private int SendToPython(byte[] jpgBytes)
    {
        if (requestSocket == null)
        {
            if (enableLogging) Debug.LogError("requestSocket == null");
            return 0;
        }

        // �����, ��� ������ �����������: "person" ��� "transport"
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

        // �������� ��������� ���������
        string trafficLightState = GetTrafficLightState();
        // ������������, ������� ������ ���� ����� (���� ���� UpdateAndGetDuration)
        float dur = UpdateAndGetDuration(trafficLightState);

        // ������� ����/�����
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // �������� DTO, ���� �� �� �����
        DetectionDTO result = new DetectionDTO
        {
            trafficLightState = trafficLightState,
            count = pyResp.count,
            timestamp = timeStamp,
            duration = dur
        };

        // ������������� ���������� �������� ������ � "���" (wanted), ����� �������� � CSV
        string cameraName = (targetCamera != null) ? targetCamera.name : "NoCamera";
        string detectType = wanted; // "person" ��� "transport"

        // ����� � CSV
        MyCsvLogger.WriteDetection(
            cameraName,          // �������� ������
            detectType,          // "person"/"transport"
            result.trafficLightState,
            result.count,
            result.timestamp,
            result.duration
        );

        // ���� ����� � ����� ���������� JSON � ���� .txt ��� � �������
        // string logJson = JsonConvert.SerializeObject(result);
        // MyAsyncLogger.Log($"Detections + Light: {logJson}");

        return pyResp.count;
    }

    public string GetTrafficLightState()
    {
        // ������: ���� ������ ���� ��������
        if (trafficLight == null) return "no-trafficlight-linked";
        return trafficLight.GetCurrentLightState();
    }

    private float UpdateAndGetDuration(string newState)
    {
        // ���� ��������� ��������� (red/green/etc.) ������ ��� ����������,
        // ���������� ������
        if (newState != lastLightState)
        {
            lastLightState = newState;
            lastLightChangeTime = Time.time;
        }

        // ����������, ������� ������ ��� ����� ������� ����
        return Time.time - lastLightChangeTime;
    }


    // --------------------------------------------------------------------------------
    // ������ ����� � ������
    // --------------------------------------------------------------------------------
    private Texture2D CaptureCameraFrame()
    {
        if (!targetCamera)
        {
            if (enableLogging) Debug.LogError("�� ��������� targetCamera");
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
        //Debug.Log($"[SaveDebugImage] ���������: {path}");
    }


    // --------------------------------------------------------------------------------
    // �����: ������ ������� -> ����� ����
    // --------------------------------------------------------------------------------
    private void ApplyMask(Texture2D source, Texture2D maskTex)
    {
        Texture2D maskToUse = maskTex;


        if (maskTex.width != source.width || maskTex.height != source.height)
        {
            maskToUse = ScaleMaskImage(maskTex, source.width, source.height);
            if (enableLogging)
                Debug.Log("[Mask] ������ ����� != ������ �����.");
        }
        if (maskToUse.width != source.width || maskToUse.height != source.height)
        {
            Debug.LogError($"[Mask] ������! ������ ����� ({maskToUse.width}x{maskToUse.height}) �� ��������� � �������� ����� ({source.width}x{source.height})!");
            return;
        }

        Color[] src = source.GetPixels();
        Color[] msk = maskToUse.GetPixels();
        for (int i = 0; i < src.Length; i++)
        {
            // ���� ����� "������" (�������� <0.1)
            if (msk[i].r < 0.1f && msk[i].g < 0.1f && msk[i].b < 0.1f)
            {
                // ����������� �����
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
