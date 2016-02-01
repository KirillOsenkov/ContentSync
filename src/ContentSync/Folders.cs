using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuiLabs.FileUtilities
{
    public class Folders
    {
        /// <summary>
        /// Assumes leftRoot is an existing folder. rightRoot may not exist if operating in speculative mode.
        /// </summary>
        public static FolderDiffResults DiffFolders(string leftRoot, string rightRoot, string pattern)
        {
            var leftRelativePaths = GetRelativePathsOfAllFiles(leftRoot, pattern);
            var leftOnlyFolders = GetRelativePathsOfAllFolders(leftRoot);

            HashSet<string> rightRelativePaths;
            HashSet<string> rightOnlyFolders;
            if (Directory.Exists(rightRoot))
            {
                rightRelativePaths = GetRelativePathsOfAllFiles(rightRoot, pattern);
                rightOnlyFolders = GetRelativePathsOfAllFolders(rightRoot);
            }
            else
            {
                rightRelativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                rightOnlyFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                            bool areSame = Files.AreContentsIdentical(leftFullPath, rightFullPath);
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

        public static HashSet<string> GetRelativePathsOfAllFiles(string rootFolder, string pattern)
        {
            using (Log.MeasureTime("Scanning files"))
            {
                var files = Directory.GetFiles(rootFolder, pattern, SearchOption.AllDirectories);
                return GetRelativePaths(rootFolder, files);
            }
        }

        public static HashSet<string> GetRelativePathsOfAllFolders(string rootFolder)
        {
            using (Log.MeasureTime("Scanning folders"))
            {
                var folders = Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories);
                return GetRelativePaths(rootFolder, folders);
            }
        }

        private static HashSet<string> GetRelativePaths(string rootFolder, string[] files)
        {
            var prefixLength = rootFolder.Length;
            var relative = files.Select(f => f.Substring(prefixLength));
            return new HashSet<string>(relative, StringComparer.OrdinalIgnoreCase);
        }
    }
}