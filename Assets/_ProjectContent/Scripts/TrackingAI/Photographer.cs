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
        // Íàñòðîèì RenderTexture ñ óìåíüøåííûì ðàçðåøåíèåì
        RenderTexture renderTexture = new RenderTexture(outputWidth, outputHeight, 24);
        camera.targetTexture = renderTexture;

        // Ñîçäàåì òåêñòóðó äëÿ ÷òåíèÿ ïèêñåëåé
        Texture2D screenShot = new Texture2D(outputWidth, outputHeight, TextureFormat.RGB24, false);
        camera.Render();

        // Ñ÷èòûâàåì ïèêñåëè ñ RenderTexture
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, outputWidth, outputHeight), 0, 0);
        screenShot.Apply();

        // Îñâîáîæäàåì RenderTexture
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Åñëè âêëþ÷åíà ãàëî÷êà äëÿ ïðåîáðàçîâàíèÿ â ñåðûé, ïðèìåíÿåì ïðåîáðàçîâàíèå
        if (convertToGrayScale)
        {
            screenShot = ConvertToGrayScale(screenShot);
        }

        // Óáåäèìñÿ, ÷òî ïàïêà ñóùåñòâóåò
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Ãåíåðèðóåì èìÿ ôàéëà äëÿ âðåìåííîãî JPEG
        string tempFileName = $"{folderPath}/{trafficLightId}_{timestamp}-photo.jpg";

        // Ñîõðàíÿåì èçîáðàæåíèå â JPEG
        File.WriteAllBytes(tempFileName, screenShot.EncodeToJPG(jpegQuality));

        // Òåïåðü êîíâåðòèðóåì JPEG â WebP ñ ïîìîùüþ cwebp
        string webpFileName = $"{folderPath}/{trafficLightId}_{timestamp}-photo.webp";
        ConvertToWebP(tempFileName, webpFileName);

        // Óäàëÿåì âðåìåííûé JPEG ôàéë
        File.Delete(tempFileName);

        UnityEngine.Debug.Log($"Ñîõðàíåíî èçîáðàæåíèå: {webpFileName}");

        // Äîïîëíèòåëüíî, ñîõðàíÿåì ìåòàäàííûå (ïî æåëàíèþ)
        string metaFileName = $"{folderPath}/{trafficLightId}_{timestamp}-meta.txt";
        File.WriteAllText(metaFileName, "Camera settings or other metadata");
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
