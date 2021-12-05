using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

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

        public static readonly string AppDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create)}" + "\\Benchmark_Sims";

        public static readonly string LogFilePath = $"{AppDataPath}" + "\\Launcher_Log.txt";

        #endregion

        #region Methods

        public static void Log(string message)
        {
            string message2 = $"[{DateTime.Now}] [INFO] :: {message}";
            logData = logData + message2 + "\r\n";
            //WriteLogFile(logData);
        }

        public static void Log(string message, LogLevels logLevel)
        {
            string message2 = $"[{DateTime.Now}] [{GetLogLevelText(logLevel)}] :: {message}";
            logData = logData + message2 + "\r\n";
            //WriteLogFile(logData);
        }

        public static void Log(Exception exception)
        {
            string message2 = $"[{DateTime.Now}] [EXCEPTION] {exception.Message}:: \r\n \r\n Source: {exception.Source} \r\n Target Site: {exception.TargetSite} \r\n Message: {exception.Message} \r\n Details: {exception.InnerException} \r\n \r\n Exception Data: {exception.Data} \r\n \r\n Stack Trace: {exception.StackTrace} \r\n ============ \r\n";
            logData = logData + message2 + "\r\n";
            //WriteLogFile(logData);
        }

        public static void Log(Exception exception, string message)
        {
            string message2 = $"[{DateTime.Now}] [EXCEPTION] {exception.Message} \r\n {message} \r\n Source: {exception.Source} \r\n Target Site: {exception.TargetSite} \r\n Message: {exception.Message} \r\n Details: {exception.InnerException} \r\n \r\n Exception Data: {exception.Data} \r\n \r\n Stack Trace: {exception.StackTrace} \r\n ============ \r\n";
            logData = logData + message2 + "\r\n";
            //WriteLogFile(logData);
        }

        public static void WriteLogFile()
        {
            WriteLogFile(false, logData);
        }

        public static void WriteLogFile(Exception e)
        {
            WriteLogFile(false, logData + $"{e}");
            MessageBox.Show("Error Log Saved to " + LogFilePath);
        }

        public static void WriteLogFile(bool append, Exception e)
        {
            WriteLogFile(append, $"{e}");
            MessageBox.Show("Error Log Saved to " + LogFilePath);
        }

        public static void WriteLogFile(bool append, string args)
        {
            try
            {
                if (!Directory.Exists(AppDataPath))
                {
                    Directory.CreateDirectory(AppDataPath);
                }

                DirectoryInfo dirInfo = new DirectoryInfo(AppDataPath);
                dirInfo.Attributes = dirInfo.Attributes & ~FileAttributes.ReadOnly;

                if (!File.Exists(LogFilePath))
                {
                    FileStream fs = File.Create(LogFilePath);
                    fs.Close();
                    File.SetCreationTimeUtc(LogFilePath, DateTime.UtcNow);
                }

                StreamWriter file = new StreamWriter(LogFilePath, append, Encoding.Default);

                file.Write(args);
                file.Close(); 
            }
            catch
            {
                MessageBox.Show(args);
            }
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