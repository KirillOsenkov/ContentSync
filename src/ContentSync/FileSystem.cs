using System.IO;

namespace GuiLabs.FileUtilities
{
    public class FileSystem
    {
        public static void CopyFile(string source, string destination, bool speculative)
        {
            if (speculative)
            {
                Log.WriteLine($"Would copy {source} to {destination}");
            }
            else
            {
                Log.WriteLine($"Copy {source} to {destination}");
                var destinationFolder = Path.GetDirectoryName(destination);
                Directory.CreateDirectory(destinationFolder);
                File.Copy(source, destination, overwrite: true);
            }
        }

        public static void DeleteFile(string deletedFilePath, bool speculative)
        {
            if (speculative)
            {
                Log.WriteLine("Would delete " + deletedFilePath);
            }
            else
            {
                Log.WriteLine("Delete " + deletedFilePath);
                var attributes = File.GetAttributes(deletedFilePath);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(deletedFilePath, attributes & ~FileAttributes.ReadOnly);
                }

                File.Delete(deletedFilePath);
            }
        }

        public static void CreateDirectory(string newFolder, bool speculative)
        {
            if (speculative)
            {
                Log.WriteLine("Would create " + newFolder);
            }
            else
            {
                Log.WriteLine("Create " + newFolder);
                Directory.CreateDirectory(newFolder);
            }
        }

        public static void DeleteDirectory(string deletedFolderPath, bool speculative)
        {
            if (speculative)
            {
                Log.WriteLine("Would delete " + deletedFolderPath);
            }
            else
            {
                Log.WriteLine("Delete " + deletedFolderPath);
                Directory.Delete(deletedFolderPath, recursive: true);
            }
        }
    }
}
