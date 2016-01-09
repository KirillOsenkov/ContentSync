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
            var rightRelativePaths = GetRelativePathsOfAllFiles(rightRoot);

            var leftOnlyFiles = new List<string>();
            var identicalFiles = new List<string>();
            var changedFiles = new List<string>();
            var rightOnlyFiles = new HashSet<string>(rightRelativePaths, StringComparer.OrdinalIgnoreCase);

            int current = 0;
            int total = leftRelativePaths.Count;

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

            leftOnlyFiles.Sort();
            identicalFiles.Sort();
            changedFiles.Sort();

            return new FolderDiffResults(
                leftOnlyFiles,
                identicalFiles,
                changedFiles,
                rightOnlyFiles.OrderBy(s => s).ToArray());
        }

        public static HashSet<string> GetRelativePathsOfAllFiles(string rootFolder)
        {
            var files = Directory.GetFiles(rootFolder, "*", SearchOption.AllDirectories);
            var prefixLength = rootFolder.Length;
            var relative = files.Select(f => f.Substring(prefixLength));
            return new HashSet<string>(relative, StringComparer.OrdinalIgnoreCase);
        }
    }
}