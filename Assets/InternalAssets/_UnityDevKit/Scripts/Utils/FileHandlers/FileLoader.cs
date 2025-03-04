using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityDevKit.Utils.FileHandlers
{
    public abstract class FileLoader
    {
        private static string currentPath;
        private static string FormatStreamingAssetsPath(string folder) => $"{Application.streamingAssetsPath}/{folder}";

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
#if UNITY_EDITOR
            currentPath = Application.streamingAssetsPath;
#else
            currentPath = Directory.GetCurrentDirectory();

            var availableExtensions = GetAvailableExtensions();
            foreach (var loadingDirectory in GetLoadingDirectories())
            {
                HandleDirectory(loadingDirectory, availableExtensions);
            }
#endif
        }

        private void HandleDirectory(string loadingDirectory, List<string> availableExtensions)
        {
            var targetFolder = $"{currentPath}/{loadingDirectory}";
            if (Directory.Exists(targetFolder))
            {
                if (FileHandler.GetAllFilesInfo(targetFolder).Any()) return;
            }
            else
            {
                Directory.CreateDirectory(targetFolder);
            }

            Debug.Log($"File copy to {targetFolder}");
            foreach (var fileInfo in FileHandler.GetBuiltInFilesInfo(loadingDirectory).TakeByExtension(availableExtensions))
            {
                try
                {
                    File.Copy(
                        Path.Combine(fileInfo.DirectoryName ?? string.Empty, fileInfo.Name),
                        Path.Combine(targetFolder, fileInfo.Name));
                }

                catch (IOException copyError)
                {
                    Debug.LogError(copyError.Message);
                }
            }
        }

        protected abstract IEnumerable<string> GetLoadingDirectories();
        protected abstract List<string> GetAvailableExtensions();
    }
}