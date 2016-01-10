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
        public static void Directories(string source, string destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            source = Paths.TrimSeparator(source);
            destination = Paths.TrimSeparator(destination);

            var diff = Folders.DiffFolders(source, destination);

            using (Log.MeasureTime("Copying new files"))
            {
                foreach (var leftOnly in diff.LeftOnlyFiles)
                {
                    var destinationFilePath = destination + leftOnly;
                    var destinationFolder = Path.GetDirectoryName(destinationFilePath);
                    Directory.CreateDirectory(destinationFolder);
                    File.Copy(source + leftOnly, destinationFilePath);
                    Console.WriteLine("Copy " + destinationFilePath);
                }
            }

            using (Log.MeasureTime("Overwriting changed files"))
            {
                foreach (var changed in diff.ChangedFiles)
                {
                    var destinationFilePath = destination + changed;
                    File.Copy(source + changed, destinationFilePath, overwrite: true);
                    Console.WriteLine("Overwrite " + destinationFilePath);
                }
            }

            using (Log.MeasureTime("Deleting extra files"))
            {
                foreach (var rightOnly in diff.RightOnlyFiles)
                {
                    var deletedFilePath = destination + rightOnly;
                    var attributes = File.GetAttributes(deletedFilePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(deletedFilePath, attributes & ~FileAttributes.ReadOnly);
                    }

                    File.Delete(deletedFilePath);
                    Console.WriteLine("Delete " + deletedFilePath);
                }
            }

            int foldersCreated = 0;
            using (Log.MeasureTime("Creating folders"))
            {
                foreach (var leftOnlyFolder in diff.LeftOnlyFolders)
                {
                    var newFolder = destination + leftOnlyFolder;
                    if (!Directory.Exists(newFolder))
                    {
                        Directory.CreateDirectory(newFolder);
                        Console.WriteLine("Create " + newFolder);
                        foldersCreated++;
                    }
                }
            }

            int foldersDeleted = 0;
            using (Log.MeasureTime("Deleting folders"))
            {
                foreach (var rightOnlyFolder in diff.RightOnlyFolders)
                {
                    var deletedFolderPath = destination + rightOnlyFolder;
                    if (Directory.Exists(deletedFolderPath))
                    {
                        Directory.Delete(deletedFolderPath, recursive: true);
                        Console.WriteLine("Delete " + deletedFolderPath);
                        foldersDeleted++;
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
        public static void Files(string source, string destination)
        {
            if (File.Exists(destination) && FileUtilities.Files.AreContentsIdentical(source, destination))
            {
                return;
            }

            File.Copy(source, destination, overwrite: true);
        }
    }
}