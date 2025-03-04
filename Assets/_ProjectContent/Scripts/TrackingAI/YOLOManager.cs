using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using System.Management.Instrumentation;

public class YOLOManager : MonoBehaviour
{
    [SerializeField] private TextAsset modelAsset;

    private static YOLOManager instance;
    private static InferenceSession session;

    public static InferenceSession Session
    {
        get
        {
            if (session == null)
            {
                InitializeSession();
            }
            return session;
        }
    }

    private void Awake()
    {
        Debug.Log("YOLOManager Awake: " + this.name);
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("YOLOManager assigned instance = " + this.name);
        }
        else if (instance != this)
        {
            Debug.LogWarning("Destroying duplicate YOLOManager on " + this.name);
            Destroy(gameObject);
            return;
        }
    }


    private static void InitializeSession()
    {
        Debug.Log("InitializeSession() called.");
        if (instance == null)
        {
            Debug.LogError("YOLOManager: ��� ���������� � �����. ���������� ���������������� ������!");
            return;
        }

        if (instance.modelAsset == null)
        {
            Debug.LogError("YOLOManager: modelAsset �� �������� � ����������!");
            return;
        }

        try
        {
            var options = new SessionOptions();
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            options.AppendExecutionProvider_CUDA(); // ���� ����� CUDA
            session = new InferenceSession(instance.modelAsset.bytes, options);
            Debug.Log("YOLOManager: ������� ������������� ������ ���������.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"YOLOManager: �� ������� ���������������� ������!\n{ex}");
            session = null;
        }
    }
}