using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GuiLabs.Common;

namespace GuiLabs.FileUtilities
{
    /// <summary>
    /// For large remote folders it can take hours just to enumerate the contents of the remote folder.
    /// For unreliable connections restarting ContentSync will require re-reading the contents from 
    /// scratch again. To avoid this repeated cost, once the contents of the remote folder have been read
    /// we flush it to disk. If at any time after that ContentSync encounters errors, the cache will persist
    /// until ContentSync is invoked next time. However upon successful completion the cache is cleared.
    /// </summary>
    public class DirectoryContentsCache
    {
        private static readonly string cacheRootFolder = Path.Combine(Path.GetTempPath(), "ContentSync");
        private static readonly HashSet<string> filesWritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static bool TryReadFromCache(string rootFolder, string pattern, HashSet<string> files, HashSet<string> folders)
        {
            if (!Directory.Exists(cacheRootFolder))
            {
                return false;
            }

            string fileListFilePath, folderListFilePath;
            GetCacheFilePaths(rootFolder, pattern, out fileListFilePath, out folderListFilePath);

            if (!File.Exists(fileListFilePath) || !File.Exists(folderListFilePath))
            {
                return false;
            }

            files.AddRange(File.ReadAllLines(fileListFilePath));
            folders.AddRange(File.ReadAllLines(folderListFilePath));

            // pretend we just wrote these files so that they can be deleted on successful completion
            filesWritten.Add(fileListFilePath);
            filesWritten.Add(folderListFilePath);

            return true;
        }

        private static void GetCacheFilePaths(string rootFolder, string pattern, out string fileListFilePath, out string folderListFilePath)
        {
            var hash = Utilities.GetMD5Hash(rootFolder + pattern);
            fileListFilePath = Path.Combine(cacheRootFolder, hash + "_files.txt");
            folderListFilePath = Path.Combine(cacheRootFolder, hash + "_folders.txt");
        }

        public static void SaveToCache(string rootFolder, string pattern, HashSet<string> files, HashSet<string> folders)
        {
            if (files.Count < 10000 && folders.Count < 1000)
            {
                // don't bother caching small amounts of data to disk
                return;
            }

            Directory.CreateDirectory(cacheRootFolder);
            string fileListFilePath, folderListFilePath;
            GetCacheFilePaths(rootFolder, pattern, out fileListFilePath, out folderListFilePath);
            File.WriteAllLines(fileListFilePath, files.OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
            File.WriteAllLines(folderListFilePath, folders.OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
            filesWritten.Add(fileListFilePath);
            filesWritten.Add(folderListFilePath);
        }

        public static void ClearWrittenFilesFromCache()
        {
            foreach (var file in filesWritten)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}