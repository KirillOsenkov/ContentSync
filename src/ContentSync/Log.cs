using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GuiLabs.FileUtilities
{
    public class Log
    {
        private static readonly object consoleLock = new object();
        private static readonly List<Tuple<string, string>> finalReportEntries = new List<Tuple<string, string>>();

        public static bool Quiet { get; internal set; }

        public static void Write(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            if (Quiet)
            {
                return;
            }

            lock (consoleLock)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Write(message);
                if (color != oldColor)
                {
                    Console.ForegroundColor = oldColor;
                }
            }
        }

        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            if (Quiet)
            {
                return;
            }

            lock (consoleLock)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                if (color != oldColor)
                {
                    Console.ForegroundColor = oldColor;
                }
            }
        }

        public static void AddFinalReportEntry(string title, string elapsedTime)
        {
            finalReportEntries.Add(Tuple.Create(title, elapsedTime));
        }

        public static void PrintFinalReport()
        {
            var leftColumnWidth = finalReportEntries.Max(e => e.Item1.Length) + 2;

            var sb = new StringBuilder();
            foreach (var entry in finalReportEntries)
            {
                var message = (entry.Item1 + ":").PadRight(leftColumnWidth) + entry.Item2;
                sb.AppendLine(message);
            }

            Write(sb.ToString(), ConsoleColor.DarkGray);
        }

        public static IDisposable MeasureTime(string operationTitle)
        {
            return new Disposable(operationTitle);
        }

        private class Disposable : IDisposable
        {
            private Stopwatch stopwatch = Stopwatch.StartNew();
            private string title;

            public Disposable(string operationTitle)
            {
                this.title = operationTitle;
            }

            public void Dispose()
            {
                var elapsedTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
                if (elapsedTime != "00:00:00.000")
                {
                    AddFinalReportEntry(title, elapsedTime);
                }
            }
        }
    }
}
