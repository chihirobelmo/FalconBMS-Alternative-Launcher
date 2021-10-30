using System;
using System.IO;
using System.Text;

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

        #region Fields

        internal static string logData;

        public static readonly string AppDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)}" + "\\FalconBMSAlternativeLauncher";

        public static readonly string LogFilePath = $"{AppDataPath}" + "\\Log.txt";

        #endregion

        #region Methods

        public static void Log(string message)
        {
            string message2 = $"[{DateTime.Now}] [INFO] :: {message}";
            logData = logData + message2 + "/r/n";
            //WriteLogFile(logData);
        }

        public static void Log(string message, LogLevels logLevel)
        {
            string message2 = $"[{DateTime.Now}] [{GetLogLevelText(logLevel)}] :: {message}";
            logData = logData + message2 + "/r/n";
            //WriteLogFile(logData);
        }

        public static void Log(Exception exception)
        {
            string message2 = $"[{DateTime.Now}] [EXCEPTION] {exception.Message}:: \n \n Source: {exception.Source} \n Target Site: {exception.TargetSite} \n Message: {exception.Message} \n Details: {exception.InnerException} \n \n Exception Data: {exception.Data} \n \n Stack Trace: {exception.StackTrace} \n ============ \n";
            logData = logData + message2 + "/r/n";
            //WriteLogFile(logData);
        }

        public static void Log(Exception exception, string message)
        {
            string message2 = $"[{DateTime.Now}] [EXCEPTION] {exception.Message} \n {message} \n Source: {exception.Source} \n Target Site: {exception.TargetSite} \n Message: {exception.Message} \n Details: {exception.InnerException} \n \n Exception Data: {exception.Data} \n \n Stack Trace: {exception.StackTrace} \n ============ \n";
            logData = logData + message2 + "/r/n";
            //WriteLogFile(logData);
        }

        public static void WriteLogFile(string logData)
        {
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(AppDataPath);
            dirInfo.Attributes = dirInfo.Attributes & ~FileAttributes.ReadOnly;

            if (!File.Exists(LogFilePath))
            {
                File.Create(AppDataPath);
                File.SetCreationTimeUtc(LogFilePath, DateTime.UtcNow);
            }

            StreamWriter file = new StreamWriter(LogFilePath, true, Encoding.Default);

            file.Write($"{logData}");
            file.Close();
        }

        public static void WriteLogFile(string logData, bool append)
        {
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }

            if (!File.Exists(LogFilePath))
            {
                File.Create(AppDataPath);
                File.SetCreationTimeUtc(LogFilePath, DateTime.UtcNow);
            }

            StreamWriter file = new StreamWriter(LogFilePath, append, Encoding.Default);

            file.Write($"{logData}");
            file.Close();
        }

        internal static string GetLogLevelText(LogLevels logLevel)
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
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        #endregion
    }
}