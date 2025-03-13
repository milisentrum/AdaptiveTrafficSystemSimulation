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
// 1) ������: MyAsyncLogger
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

        // ���� ����������, �� � ���������
        if (loggingThread != null && loggingThread.IsAlive)
        {
            //if (!loggingThread.Join(5000)) // 5 ��� �� ����������
            //{
            //    loggingThread.Abort(); // ������������� ������� (�� �������������, �� � ������� ������)
            //}
            loggingThread.Join(2000); // ���� 2 ���
            loggingThread = null; // ������� ������ �� �����
        }
    }
}

// --------------------------------------------------------------------------------
// 2) �����-DTO ��� �������� ��������
// --------------------------------------------------------------------------------
[Serializable]
public class DetectionDTO
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

    private void Awake()
    {
        // ������������� �������
        if (enableLogging) MyAsyncLogger.Init();

        // ������ RT ��� ������
        captureRT = new RenderTexture(Screen.width, Screen.height, 24);

        // ������������� ZeroMQ (NetMQ)
        AsyncIO.ForceDotNet.Force();
        requestSocket = new RequestSocket();

        // ������������ � ���� �� ������, ��� � Python
        requestSocket.Connect("tcp://127.0.0.1:5555");

        if (enableLogging) Debug.Log("������������ � tcp://127.0.0.1:5555");
        Debug.Log("[YOLO] Connected to Python YOLO server.");
    }

    private void Start()
    {
        // ��������� ��������, ����� ������ N ������ �������� ����
        StartCoroutine(SendFramesLoop());
    }

    private void OnDisable()
    {
        // ���������� ��� ���������� ������� (��� �����),
        // �� ��� �� OnDestroy(). ����� ����� ����� ��������� ��������.
        if (!shuttingDown)
        {
            shuttingDown = true;
            StartCoroutine(ShutdownCoroutine());
            StartCoroutine(WaitForShutdown()); // �������� ����� ��������� �����
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
        isRunning = false; // ������������� ��������

        yield return new WaitForSeconds(0.1f); // ���� ����� ����������

        // ������������� ��� ��������, ����� �������� ����������
        StopAllCoroutines();

        // ��������� � ����������� ����� �������
        if (requestSocket != null)
        {
            requestSocket.Dispose();
            requestSocket = null;
        }

        yield return null; // ���� 1 ���� ����� �������� NetMQ

        // ������ NetMQ (����� ������ ��� ����� Dispose)
        NetMQConfig.Cleanup();

        // ����������� RenderTexture, ���� �� ��� ������
        if (captureRT != null)
        {
            captureRT.Release();
            Destroy(captureRT);
            captureRT = null;
        }

        // ��������� ������ (���� ������������ ����������� ������)
        MyAsyncLogger.Shutdown();

        // �������������� ������: ��������� ������� ZeroMQ (�� ������, ���� �������� dangling references)
        if (requestSocket != null)
        {
            requestSocket.Dispose();
            requestSocket = null;
        }

        NetMQConfig.Cleanup();
    }



    private string DetermineWantedString()
    {
        // ������ ������:
        if (detectPeople && !detectTransport)
            return "person";
        else if (!detectPeople && detectTransport)
            return "transport";
        return "";
        //else
        //    // ���� ��� �������� ��� ��� ��������� - ���� ���
        //    return "all";
    }

    // --------------------------------------------------------------------------------
    // ��������: ������ 1 ��� ���������� ����, �������� �����
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
                        Debug.Log($"�������� ��������: {count}");
                }
                finally
                {
                    Destroy(frame);
                }
            }
            //Debug.Log("Loop iteration 4...");

            yield return new WaitForSeconds(1f); // ��� � �������
        }
    }

    private void DrawBox(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
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

        // 2) ����������
        string wanted = DetermineWantedString();
        requestSocket.SendMoreFrame(wanted);
        requestSocket.SendFrame(jpgBytes);

        if (enableLogging) MyAsyncLogger.Log($"��������� ������ �� ��������: {wanted}");
        if (enableLogging) MyAsyncLogger.Log($"������ ������������� �����������: {jpgBytes.Length} ����");

        // 3) ���������
        string reply;
        bool received = requestSocket.TryReceiveFrameString(TimeSpan.FromSeconds(5), out reply);
        if (!received || string.IsNullOrEmpty(reply) || reply.StartsWith("ERROR:"))
        {
            if (enableLogging) MyAsyncLogger.LogError("Python error: " + reply);
            return 0;
        }

        // 4) ������ JSON
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

        if (enableLogging) SaveDebugImage(source, "original");
        Texture2D maskToUse = maskTex;


        if (maskTex.width != source.width || maskTex.height != source.height)
        {
            maskToUse = ScaleMaskImage(maskTex, source.width, source.height);
            // ���� ����� �������� �� ��������� � ������, ����� ���� �� ���������,
            // ���� ��������� �����. ����� ��� �������� ������ �������� � ����������.
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
