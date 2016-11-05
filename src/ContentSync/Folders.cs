using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GuiLabs.FileUtilities
{
    public class Folders
    {
        /// <summary>
        /// Assumes leftRoot is an existing folder. rightRoot may not exist if operating in speculative mode.
        /// </summary>
        public static FolderDiffResults DiffFolders(
            string leftRoot,
            string rightRoot,
            string pattern,
            bool recursive = true,
            bool compareContents = true)
        {
            HashSet<string> leftRelativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> leftOnlyFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (Log.MeasureTime("Scanning source directory"))
            {
                GetRelativePathsOfAllFiles(leftRoot, pattern, recursive, leftRelativePaths, leftOnlyFolders);
            }

            HashSet<string> rightRelativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> rightOnlyFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (Directory.Exists(rightRoot))
            {
                using (Log.MeasureTime("Scanning destination directory"))
                {
                    GetRelativePathsOfAllFiles(rightRoot, pattern, recursive, rightRelativePaths, rightOnlyFolders);
                }
            }

            var leftOnlyFiles = new List<string>();
            var identicalFiles = new List<string>();
            var changedFiles = new List<string>();
            var rightOnlyFiles = new HashSet<string>(rightRelativePaths, StringComparer.OrdinalIgnoreCase);

            var commonFolders = leftOnlyFolders.Intersect(rightOnlyFolders, StringComparer.OrdinalIgnoreCase).ToArray();
            leftOnlyFolders.ExceptWith(commonFolders);
            rightOnlyFolders.ExceptWith(commonFolders);

            int current = 0;
            int total = leftRelativePaths.Count;

            using (Log.MeasureTime("Comparing"))
            {
                Parallel.ForEach(
                    leftRelativePaths,
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                    left =>
                    {
                        var leftFullPath = leftRoot + left;
                        var rightFullPath = rightRoot + left;

                        bool rightContains = rightRelativePaths.Contains(left);
                        if (rightContains)
                        {
                            bool areSame = !compareContents || Files.AreContentsIdentical(leftFullPath, rightFullPath);
                            if (areSame)
                            {
                                lock (identicalFiles)
                                {
                                    identicalFiles.Add(left);
                                }
                            }
                            else
                            {
                                lock (changedFiles)
                                {
                                    changedFiles.Add(left);
                                }
                            }
                        }
                        else
                        {
                            lock (leftOnlyFiles)
                            {
                                leftOnlyFiles.Add(left);
                            }
                        }

                        lock (rightOnlyFiles)
                        {
                            rightOnlyFiles.Remove(left);
                        }

                        Interlocked.Increment(ref current);
                    });
            }

            using (Log.MeasureTime("Sorting"))
            {
                leftOnlyFiles.Sort();
                identicalFiles.Sort();
                changedFiles.Sort();

                return new FolderDiffResults(
                    leftOnlyFiles,
                    identicalFiles,
                    changedFiles,
                    rightOnlyFiles.OrderBy(s => s).ToArray(),
                    leftOnlyFolders.OrderBy(s => s).ToArray(),
                    rightOnlyFolders.OrderBy(s => s).ToArray());
            }
        }

        private static readonly FieldInfo pathField = typeof(FileSystemInfo).GetField("FullPath", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void GetRelativePathsOfAllFiles(string rootFolder, string pattern, bool recursive, HashSet<string> files, HashSet<string> folders)
        {
            // don't go through the cache for non-recursive case
            if (recursive && DirectoryContentsCache.TryReadFromCache(rootFolder, pattern, files, folders))
            {
                return;
            }

            var rootDirectoryInfo = new DirectoryInfo(rootFolder);
            var prefixLength = rootFolder.Length;
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var fileSystemInfos = rootDirectoryInfo.EnumerateFileSystemInfos(pattern, searchOption);
            foreach (var fileSystemInfo in fileSystemInfos)
            {
                string relativePath = (string)pathField.GetValue(fileSystemInfo);
                relativePath = relativePath.Substring(prefixLength);
                if (fileSystemInfo is FileInfo)
                {
                    files.Add(relativePath);
                }
                else
                {
                    folders.Add(relativePath);
                }
            }

            if (recursive)
            {
                DirectoryContentsCache.SaveToCache(rootFolder, pattern, files, folders);
            }
        }
    }
}