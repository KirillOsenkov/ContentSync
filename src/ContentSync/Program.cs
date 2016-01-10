using System;
using System.IO;

namespace GuiLabs.FileUtilities
{
    internal class ContentSync
    {
        private static void Main(string[] args)
        {
            string source = null;
            string destination = null;
            if (args.Length == 2)
            {
                source = args[0];
                destination = args[1];
            }
            else
            {
                PrintUsage();
                return;
            }

            if (Directory.Exists(source))
            {
                source = Path.GetFullPath(source);
                if (Directory.Exists(destination))
                {
                    destination = Path.GetFullPath(destination);
                }

                using (Log.MeasureTime("Total time"))
                {
                    Sync.Directories(source, destination);
                }

                Log.PrintFinalReport();

                return;
            }

            if (File.Exists(source) && Directory.Exists(destination))
            {
                Sync.Files(source, Path.Combine(destination, Path.GetFileName(source)));
                return;
            }

            if (File.Exists(source))
            {
                Sync.Files(source, destination);
                return;
            }

            Console.Error.WriteLine($"Cannot sync {source} to {destination}");
        }

        private static void PrintUsage()
        {
            Console.WriteLine(@"Usage: ContentSync.exe <source> <destination>
    Copy/mirror/sync the destination folder to look exactly like source folder.
    Copies missing files, deletes files from destination that are not in source,
    overwrites files in destination that have different contents than in source.
    Doesn't take into account file and date timestamps, works off content only.");
        }
    }
}