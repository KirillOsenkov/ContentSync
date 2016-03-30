using System;
using System.IO;

namespace GuiLabs.FileUtilities
{
    public class FileSystem
    {
        public static bool CopyFile(string source, string destination, bool speculative)
        {
            if (speculative)
            {
                Log.WriteLine($"Would copy {source} to {destination}");
            }
            else
            {
                var destinationFolder = Path.GetDirectoryName(destination);

                try
                {
                    if (!string.IsNullOrEmpty(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }

                    File.Copy(source, destination, overwrite: true);
                    Log.WriteLine($"Copy {source} to {destination}");
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Unable to copy {source} to {destination}: {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        public static bool DeleteFile(string deletedFilePath, bool speculative)
        {
            if (speculative)
            {
                Log.WriteLine("Would delete " + deletedFilePath);
            }
            else
            {
                try
                {
                    // this can happen if the directory contents cache is out-of-date
                    if (!File.Exists(deletedFilePath))
                    {
                        return true;
                    }

                    var attributes = File.GetAttributes(deletedFilePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(deletedFilePath, attributes & ~FileAttributes.ReadOnly);
                    }

                    File.Delete(deletedFilePath);
                    Log.WriteLine("Delete " + deletedFilePath);
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Unable to delete file {deletedFilePath}: {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        public static bool CreateDirectory(string newFolder, bool speculative)
        {
            if (speculative)
            {
                Log.WriteLine("Would create " + newFolder);
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(newFolder);
                    Log.WriteLine("Create " + newFolder);
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Unable to create directory {newFolder}: {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        public static bool DeleteDirectory(string deletedFolderPath, bool speculative)
        {
            if (speculative)
            {
                Log.WriteLine("Would delete " + deletedFolderPath);
            }
            else
            {
                try
                {
                    Directory.Delete(deletedFolderPath, recursive: true);
                    Log.WriteLine("Delete " + deletedFolderPath);
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Unable to delete directory {deletedFolderPath}: {ex.Message}");
                    return false;
                }
            }

            return true;
        }
    }
}
