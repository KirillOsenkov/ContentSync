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
                File.Delete(deletedFilePath);
                Console.WriteLine("Delete " + deletedFilePath);
            }

            Console.WriteLine();
            Console.WriteLine($"{diff.LeftOnlyFiles.Count()} new");
            Console.WriteLine($"{diff.ChangedFiles.Count()} changed");
            Console.WriteLine($"{diff.RightOnlyFiles.Count()} deleted");
            Console.WriteLine($"{diff.IdenticalFiles.Count()} identical");
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