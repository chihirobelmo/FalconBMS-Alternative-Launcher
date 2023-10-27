using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace FalconBMS.Launcher
{
    public static class Diagnostics
    {
        public enum LogLevels
        {
            Info,
            Warning,
            Error,
            Exception,
        }

        static StreamWriter logWriter;

        static Diagnostics()
        {
            string appDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create)}";
            string logFileDirectory = Path.Combine(appDataPath, @"Benchmark_Sims");

            if (false == Directory.Exists(logFileDirectory))
                Directory.CreateDirectory(logFileDirectory);

            string logFilePath = Path.Combine(logFileDirectory, @"Launcher_Log.txt");

            logWriter = Utils.CreateUtf8TextWihoutBom(logFilePath, append:false);
            logWriter.AutoFlush = true;
        }

        public static void Log(string message)
        {
            Log(message, LogLevels.Info);
        }

        public static void Log(string message, LogLevels logLevel)
        {
            string level = GetLogLevelText(logLevel);

            Debug.WriteLine($"LOG_{level}: " + message);

            string message2 = $"[{DateTime.Now}] [{level}] :: {message}";
            logWriter.WriteLine(message2);
        }

        public static void Log(Exception exception)
        {
            Debug.WriteLine("LOG_EXCEPTION: " + exception.Message);

            string message2 = $"[{DateTime.Now}] [EXCEPTION] {exception.Message}:: \r\n \r\n Source: {exception.Source} \r\n Target Site: {exception.TargetSite} \r\n Message: {exception.Message} \r\n Details: {exception.InnerException} \r\n \r\n Exception Data: {exception.Data} \r\n \r\n Stack Trace: {exception.StackTrace} \r\n ============ \r\n";
            logWriter.WriteLine(message2);
        }

        public static void ShowErrorMsgbox(Exception ex)
        {
            ShowErrorMsgbox(
                "An unknown problem occurred, check error log for details:\r\n\r\n"+
                "%LocalAppData%\\Benchmark_Sims\\Launcher_Log.txt", ex);
        }

        public static void ShowErrorMsgbox(string message, Exception ex = null)
        {
            if (ex != null)
                message += "\r\n\r\n" + ex.GetType() + "\r\n" + ex.Message;

            MessageBox.Show(message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void FinalizeLogfile()
        {
            logWriter.Close();
            logWriter = null;
        }

        private static string GetLogLevelText(LogLevels logLevel)
        {
            switch (logLevel)
            {
                case LogLevels.Info:
                    return "INFO";
                case LogLevels.Warning:
                    return "WARNING";
                case LogLevels.Error:
                    return "ERROR";
                case LogLevels.Exception:
                    return "EXCEPTION";
                default:
                    throw new InvalidProgramException("GetLogLevelText");
            }
        }

    }
}