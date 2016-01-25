using System;
using System.IO;
using System.Linq;

namespace GuiLabs.FileUtilities
{
    public class Sync
    {
        /// <summary>
        /// Assumes source directory exists. destination may or may not exist.
        /// </summary>
        public static void Directories(string source, string destination, Arguments arguments)
        {
            if (!Directory.Exists(destination))
            {
                FileSystem.CreateDirectory(destination, arguments.WhatIf);
            }

            source = Paths.TrimSeparator(source);
            destination = Paths.TrimSeparator(destination);

            var diff = Folders.DiffFolders(source, destination, arguments.Pattern);

            if (arguments.CopyLeftOnlyFiles)
            {
                using (Log.MeasureTime("Copying new files"))
                {
                    foreach (var leftOnly in diff.LeftOnlyFiles)
                    {
                        var destinationFilePath = destination + leftOnly;
                        FileSystem.CopyFile(source + leftOnly, destinationFilePath, arguments.WhatIf);
                    }
                }
            }

            if (arguments.UpdateChangedFiles)
            {
                using (Log.MeasureTime("Updating changed files"))
                {
                    foreach (var changed in diff.ChangedFiles)
                    {
                        var destinationFilePath = destination + changed;
                        FileSystem.CopyFile(source + changed, destinationFilePath, arguments.WhatIf);
                    }
                }
            }
            else if (arguments.DeleteChangedFiles)
            {
                using (Log.MeasureTime("Deleting changed files"))
                {
                    foreach (var changed in diff.ChangedFiles)
                    {
                        var destinationFilePath = destination + changed;
                        FileSystem.DeleteFile(destinationFilePath, arguments.WhatIf);
                    }
                }
            }

            if (arguments.DeleteSameFiles)
            {
                using (Log.MeasureTime("Deleting identical files"))
                {
                    foreach (var same in diff.IdenticalFiles)
                    {
                        var destinationFilePath = destination + same;
                        FileSystem.DeleteFile(destinationFilePath, arguments.WhatIf);
                    }
                }
            }

            if (arguments.DeleteRightOnlyFiles)
            {
                using (Log.MeasureTime("Deleting extra files"))
                {
                    foreach (var rightOnly in diff.RightOnlyFiles)
                    {
                        var deletedFilePath = destination + rightOnly;
                        FileSystem.DeleteFile(deletedFilePath, arguments.WhatIf);
                    }
                }
            }

            int foldersCreated = 0;
            if (arguments.CopyEmptyDirectories)
            {
                using (Log.MeasureTime("Creating folders"))
                {
                    foreach (var leftOnlyFolder in diff.LeftOnlyFolders)
                    {
                        var newFolder = destination + leftOnlyFolder;
                        if (!Directory.Exists(newFolder))
                        {
                            FileSystem.CreateDirectory(newFolder, arguments.WhatIf);
                            foldersCreated++;
                        }
                    }
                }
            }

            int foldersDeleted = 0;
            if (arguments.DeleteRightOnlyDirectories)
            {
                using (Log.MeasureTime("Deleting folders"))
                {
                    foreach (var rightOnlyFolder in diff.RightOnlyFolders)
                    {
                        var deletedFolderPath = destination + rightOnlyFolder;
                        if (Directory.Exists(deletedFolderPath))
                        {
                            FileSystem.DeleteDirectory(deletedFolderPath, arguments.WhatIf);
                            foldersDeleted++;
                        }
                    }
                }
            }

            if (diff.LeftOnlyFiles.Any())
            {
                Log.WriteLine($"{diff.LeftOnlyFiles.Count()} new files", ConsoleColor.Green);
            }

            if (foldersCreated > 0)
            {
                Log.WriteLine($"{foldersCreated} folders created", ConsoleColor.Green);
            }

            if (diff.ChangedFiles.Any())
            {
                Log.WriteLine($"{diff.ChangedFiles.Count()} changed files", ConsoleColor.Yellow);
            }

            if (diff.RightOnlyFiles.Any())
            {
                Log.WriteLine($"{diff.RightOnlyFiles.Count()} deleted files", ConsoleColor.Red);
            }

            if (foldersDeleted > 0)
            {
                Log.WriteLine($"{foldersDeleted} folders deleted", ConsoleColor.Red);
            }

            if (diff.IdenticalFiles.Any())
            {
                Log.WriteLine($"{diff.IdenticalFiles.Count()} identical files", ConsoleColor.White);
            }
        }

        /// <summary>
        /// Assumes source exists, destination may or may not exist.
        /// If it exists and is identical bytes to source, nothing is done.
        /// If it exists and is different, it is overwritten.
        /// If it doesn't exist, source is copied.
        /// </summary>
        public static void Files(string source, string destination, Arguments arguments)
        {
            if (File.Exists(destination) && FileUtilities.Files.AreContentsIdentical(source, destination))
            {
                Log.WriteLine("File contents are identical.", ConsoleColor.White);
                return;
            }

            FileSystem.CopyFile(source, destination, arguments.WhatIf);
        }
    }
}