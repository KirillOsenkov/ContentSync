using System;
using System.IO;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            return;
        }

        string sourceDirectory = args[0];
        string destinationDirectory = args[1];
        string pattern = args[2];

        if (!Directory.Exists(sourceDirectory))
        {
            Console.Error.WriteLine($"Source directory doesn't exist: {sourceDirectory}");
            return;
        }

        if (!Directory.Exists(destinationDirectory))
        {
            Console.Error.WriteLine($"Destination directory doesn't exist: {destinationDirectory}");
            return;
        }

        var files = Directory.GetFiles(sourceDirectory, pattern, SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(file));
            File.Copy(file, destinationFile, overwrite: true);
        }
    }
}