using System;
using System.Diagnostics;
using System.Text;

namespace GuiLabs.FileUtilities
{
    public class Log
    {
        private static readonly object consoleLock = new object();
        private static readonly StringBuilder finalReport = new StringBuilder();

        public static void Write(string message, ConsoleColor color = ConsoleColor.Gray)
        {
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

        public static void AddFinalReportEntry(string message)
        {
            finalReport.AppendLine(message);
        }

        public static void PrintFinalReport()
        {
            Write(finalReport.ToString(), ConsoleColor.DarkGray);
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
                    AddFinalReportEntry(title + ": " + elapsedTime);
                }
            }
        }
    }
}
