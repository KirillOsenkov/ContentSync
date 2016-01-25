using System;
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

                try
                {
                    Directory.CreateDirectory(destinationFolder);
                    File.Copy(source, destination, overwrite: true);
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Unable to copy {source} to {destination}: {ex.Message}");
                }
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
                try
                {
                    var attributes = File.GetAttributes(deletedFilePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(deletedFilePath, attributes & ~FileAttributes.ReadOnly);
                    }

                    File.Delete(deletedFilePath);
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Unable to delete file {deletedFilePath}: {ex.Message}");
                }
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
                try
                {
                    Directory.CreateDirectory(newFolder);
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Unable to create directory {newFolder}: {ex.Message}");
                }
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
                try
                {
                    Directory.Delete(deletedFolderPath, recursive: true);
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Unable to delete directory {deletedFolderPath}: {ex.Message}");
                }
            }
        }
    }
}
