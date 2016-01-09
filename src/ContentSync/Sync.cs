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
            foreach (var leftOnly in diff.LeftOnlyFiles)
            {
                var destinationFilePath = destination + leftOnly;
                var destinationFolder = Path.GetDirectoryName(destinationFilePath);
                Directory.CreateDirectory(destinationFolder);
                File.Copy(source + leftOnly, destinationFilePath);
                Console.WriteLine("Copy " + destinationFilePath);
            }

            foreach (var changed in diff.ChangedFiles)
            {
                var destinationFilePath = destination + changed;
                File.Copy(source + changed, destinationFilePath, overwrite: true);
                Console.WriteLine("Overwrite " + destinationFilePath);
            }

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

            int foldersCreated = 0;
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

            int foldersDeleted = 0;
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

            if (diff.LeftOnlyFiles.Any())
            {
                Console.WriteLine($"{diff.LeftOnlyFiles.Count()} files new");
            }

            if (diff.ChangedFiles.Any())
            {
                Console.WriteLine($"{diff.ChangedFiles.Count()} files changed");
            }

            if (diff.RightOnlyFiles.Any())
            {
                Console.WriteLine($"{diff.RightOnlyFiles.Count()} files deleted");
            }

            if (diff.IdenticalFiles.Any())
            {
                Console.WriteLine($"{diff.IdenticalFiles.Count()} files identical");
            }

            if (foldersCreated > 0)
            {
                Console.WriteLine($"{foldersCreated} folders created");
            }

            if (foldersDeleted > 0)
            {
                Console.WriteLine($"{foldersDeleted} folders deleted");
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