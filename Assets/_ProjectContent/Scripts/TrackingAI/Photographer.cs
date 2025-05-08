using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics; // Äëÿ çàïóñêà âíåøíåãî ïðîöåññà
using UnityEngine;

public class Photographer : MonoBehaviour
{
    public Camera camera; // Êàìåðà, ñ êîòîðîé ñíèìàåòñÿ èçîáðàæåíèå
    public string folderPath = "TrafficLightImages"; // Ïàïêà äëÿ ñîõðàíåíèÿ èçîáðàæåíèé
    public int outputWidth = 1280;  // Øèðèíà èçîáðàæåíèÿ
    public int outputHeight = 720; // Âûñîòà èçîáðàæåíèÿ
    public int jpegQuality = 75;   // Êà÷åñòâî JPEG (1-100)
    public bool convertToGrayScale = true;
    public List<GameObject> cars = new List<GameObject>();     // classId = 0
    public List<GameObject> people = new List<GameObject>();   // classId = 1

    // Ïóòü ê cwebp.exe (óñòàíîâè ïóòü â ñîîòâåòñòâèè ñ ðàñïîëîæåíèåì òâîåãî ôàéëà)
    public string cwebpPath = @"C:\traffic_light\AdaptiveTrafficSystemSimulation\Assets\Plugins\libwebp\cwebp.exe";

    // Ôóíêöèÿ äëÿ ïðåîáðàçîâàíèÿ èçîáðàæåíèÿ â îòòåíêè ñåðîãî
    private Texture2D ConvertToGrayScale(Texture2D original)
    {
        Texture2D grayTexture = new Texture2D(original.width, original.height);

        for (int y = 0; y < original.height; y++)
        {
            for (int x = 0; x < original.width; x++)
            {
                Color pixelColor = original.GetPixel(x, y);
                float grayValue = pixelColor.r * 0.3f + pixelColor.g * 0.59f + pixelColor.b * 0.11f;
                grayTexture.SetPixel(x, y, new Color(grayValue, grayValue, grayValue));
            }
        }
        grayTexture.Apply();
        return grayTexture;
    }

    public void CaptureImage(string trafficLightId)
    {
        RenderTexture renderTexture = new RenderTexture(outputWidth, outputHeight, 24);
        camera.targetTexture = renderTexture;

        Texture2D screenShot = new Texture2D(outputWidth, outputHeight, TextureFormat.RGB24, false);
        camera.Render();

        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, outputWidth, outputHeight), 0, 0);
        screenShot.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        if (convertToGrayScale)
        {
            screenShot = ConvertToGrayScale(screenShot);
        }

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string tempFileName = $"{folderPath}/{trafficLightId}_{timestamp}-photo.jpg";
        File.WriteAllBytes(tempFileName, screenShot.EncodeToJPG(jpegQuality));

        string webpFileName = $"{folderPath}/{trafficLightId}_{timestamp}-photo.webp";
        ConvertToWebP(tempFileName, webpFileName);

        // === Объединяем людей и машины ===
        List<(GameObject obj, int classId)> targets = new List<(GameObject, int)>();
        foreach (var car in cars) if (car != null) targets.Add((car, 0));
        foreach (var person in people) if (person != null) targets.Add((person, 1));

        string labelFileName = $"{folderPath}/{trafficLightId}_{timestamp}-photo.txt";
        List<string> yoloLabels = new List<string>();

        foreach (var (obj, classId) in targets)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) continue;

            Bounds bounds = renderer.bounds;
            Vector3[] corners = new Vector3[8];

            corners[0] = bounds.min;
            corners[1] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            corners[3] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            corners[4] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            corners[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            corners[7] = bounds.max;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var corner in corners)
            {
                Vector3 screenPoint = camera.WorldToScreenPoint(corner);
                screenPoint.y = outputHeight - screenPoint.y;

                minX = Mathf.Min(minX, screenPoint.x);
                minY = Mathf.Min(minY, screenPoint.y);
                maxX = Mathf.Max(maxX, screenPoint.x);
                maxY = Mathf.Max(maxY, screenPoint.y);
            }

            minX = Mathf.Clamp(minX, 0, outputWidth);
            maxX = Mathf.Clamp(maxX, 0, outputWidth);
            minY = Mathf.Clamp(minY, 0, outputHeight);
            maxY = Mathf.Clamp(maxY, 0, outputHeight);

            float bboxWidth = maxX - minX;
            float bboxHeight = maxY - minY;
            float x_center = (minX + bboxWidth / 2f) / outputWidth;
            float y_center = (minY + bboxHeight / 2f) / outputHeight;

            string label = $"{classId} {x_center:F6} {y_center:F6} {bboxWidth / outputWidth:F6} {bboxHeight / outputHeight:F6}";
            yoloLabels.Add(label);
        }

        File.WriteAllLines(labelFileName, yoloLabels.ToArray());

        File.Delete(tempFileName);
        UnityEngine.Debug.Log($"✅ Сохранено изображение: {webpFileName}");
    }

    // Ìåòîä äëÿ êîíâåðòàöèè èçîáðàæåíèÿ â WebP ÷åðåç cwebp
    private void ConvertToWebP(string inputFilePath, string outputFilePath)
    {
        // Ñîçäàåì ïðîöåññ äëÿ âûïîëíåíèÿ êîìàíäû cwebp
        Process process = new Process();
        process.StartInfo.FileName = cwebpPath; // Ïóòü ê cwebp.exe
        process.StartInfo.Arguments = $"-q {jpegQuality} \"{inputFilePath}\" -o \"{outputFilePath}\""; // Êîìàíäà äëÿ êîíâåðòàöèè
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        // Çàïóñêàåì ïðîöåññ
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        // Âûâîäèì îøèáêè è ðåçóëüòàò â êîíñîëü (äëÿ îòëàäêè)
        if (!string.IsNullOrEmpty(errors))
        {
            UnityEngine.Debug.LogError(errors);
        }
        else
        {
            UnityEngine.Debug.Log("WebP conversion successful!");
        }
    }

    void Start()
    {
        StartCoroutine(CaptureImagesPeriodically());
    }

    IEnumerator CaptureImagesPeriodically()
    {
        while (true)
        {
            CaptureImage("TrafficLight_01");
            yield return new WaitForSeconds(3f); // Èíòåðâàë â ñåêóíäàõ
        }
    }
}
