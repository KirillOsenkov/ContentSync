using System;
using System.IO;

namespace GuiLabs.FileUtilities
{
    internal class ContentSync
    {
        private static int Main(string[] args)
        {
            var arguments = new Arguments(args);
            if (arguments.Help || args.Length == 0)
            {
                PrintUsage();
                return 0;
            }

            if (!string.IsNullOrEmpty(arguments.Error))
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Invalid arguments:" + Environment.NewLine + arguments.Error + Environment.NewLine);
                Console.ForegroundColor = oldColor;
                PrintUsage();
                return 1;
            }

            string source = arguments.Source;
            string destination = arguments.Destination;

            if (Directory.Exists(source))
            {
                source = Path.GetFullPath(source);
                if (Directory.Exists(destination))
                {
                    destination = Path.GetFullPath(destination);
                }

                using (Log.MeasureTime("Total time"))
                {
                    Sync.Directories(source, destination, arguments);
                }

                Log.PrintFinalReport();

                return 0;
            }

            if (File.Exists(source) && Directory.Exists(destination))
            {
                destination = Path.Combine(destination, Path.GetFileName(source));
                Sync.Files(source, destination, arguments);
                return 0;
            }

            if (File.Exists(source))
            {
                Sync.Files(source, destination, arguments);
                return 0;
            }

            Console.Error.WriteLine($"Cannot find file or directory: {source}");
            return 2;
        }

        private static void PrintUsage()
        {
            Console.WriteLine(@"Usage: ContentSync.exe <source> <destination> [<pattern>] [-c] [-u] [-d] 
                                                          [-dc] [-ds] 
                                                          [-whatif] [-q] 
                                                          [-h]

   Copy/mirror/sync the destination folder to look exactly like source folder.
   Copies missing files, deletes files from destination that are not in source,
   overwrites files in destination that have different contents than in source.
   Doesn't take into account file and date timestamps, works off content only.

   -c       Copy files from source that don't exist in destination (left-only).

   -u       Update files that have changed between source and destination. This
            only overwrites the destination file if the contents are different.

   -d       Delete right-only files (that are in destination but not in 
            source).

   -ds      Delete same files (that exist in source and destination and are 
            same).

   -dc      Delete changed files from destination (can't be used with -u).

   -whatif  Print what would have been done (without changing anything).

   -q       Quiet mode. Do not output anything to the console.

   -h       Display this help and exit.

   Default is: -c -u -d (and if the pattern is not specified, it also syncs
   empty directories (creates empty directories that are in the source and
   deletes empty directories that are in the destination).

   Explicit mode: If any of -c -u -d -ds or -dc are specified explicitly, 
   all the defaults for other arguments are reset to false. For instance
   if you specify -u then -c and -d default to false (and you have to
   specify them explicitly). But if no arguments are specified, -c -u -d
   default to true.

   Project page: https://github.com/KirillOsenkov/ContentSync
");
        }
    }
}