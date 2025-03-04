using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityDevKit.Utils.FileHandlers
{
    public static class FileHandler
    {
        public static string FormatStreamingAssetsPath(string folder) => $"{Application.streamingAssetsPath}/{folder}";

        public static IEnumerable<FileInfo> GetBuiltInFilesInfo(string folder)
        {
            var builtInPath = FormatStreamingAssetsPath(folder);
            return GetAllFilesInfo(builtInPath);
        }

        public static IEnumerable<FileInfo> GetAllFilesInfo(
            string folder,
            SearchOption option = SearchOption.AllDirectories)
        {
            var directoryInfo = new DirectoryInfo(folder);
            return directoryInfo.GetFiles("*.*", option);
        }

        public static IEnumerable<string> SelectNames(this IEnumerable<FileInfo> source) =>
            source.Select(info => info.Name);

        public static IEnumerable<string> SelectFullNames(this IEnumerable<FileInfo> source) =>
            source.Select(info => info.FullName);

        public static IEnumerable<FileInfo> TakeByExtension(this IEnumerable<FileInfo> source,
            List<string> fileExtensions) => source.Where(f => fileExtensions.Contains(f.Extension));

        public static string PreprocessFolderName(string folderName)
        {
            var regexSearch = new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            var resFolderName = r.Replace(folderName, "");

            return resFolderName
                .Replace(' ', '_')
                .Replace('/', '_')
                .Replace('\\', '_');
        }

        public static string HandleIdenticalDirectory(string path)
        {
            var resPath = path;
            var increment = 0;
            while (Directory.Exists(resPath))
            {
                increment++;
                resPath = $"{path}__{increment}";
            }

            Directory.CreateDirectory(resPath);
            return resPath;
        }
    }
}