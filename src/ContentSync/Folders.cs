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
        /// Assumes both leftRoot and rightRoot are existing folders.
        /// </summary>
        public static FolderDiffResults DiffFolders(string leftRoot, string rightRoot)
        {
            var leftRelativePaths = GetRelativePathsOfAllFiles(leftRoot);
            var leftOnlyFolders = GetRelativePathsOfAllFolders(leftRoot);
            var rightRelativePaths = GetRelativePathsOfAllFiles(rightRoot);
            var rightOnlyFolders = GetRelativePathsOfAllFolders(rightRoot);

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

        public static HashSet<string> GetRelativePathsOfAllFiles(string rootFolder)
        {
            using (Log.MeasureTime("Scanning files in " + rootFolder))
            {
                var files = Directory.GetFiles(rootFolder, "*", SearchOption.AllDirectories);
                return GetRelativePaths(rootFolder, files);
            }
        }

        public static HashSet<string> GetRelativePathsOfAllFolders(string rootFolder)
        {
            using (Log.MeasureTime("Scanning folders in " + rootFolder))
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