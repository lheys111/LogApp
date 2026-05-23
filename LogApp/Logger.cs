using System;
using System.IO;

namespace NotesApp
{
    public static class Logger
    {
        private static string logFile = "log.txt";

        public static void Write(string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(logFile, logMessage + Environment.NewLine);
            }
            catch { }
        }

        public static void WriteError(string error)
        {
            Write($"ОШИБКА: {error}");
        }
    }
}