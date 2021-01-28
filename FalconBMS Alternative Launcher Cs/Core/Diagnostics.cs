using System;
using System.IO;
using System.Text;

namespace FalconBMS.Launcher
{
    public static class Diagnostics
    {
        #region Fields

        public static readonly string AppDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)}/FalconBMSAlternativeLauncher";

        public static readonly string LogFilePath = $"{AppDataPath}/Log.txt";

        #endregion

        #region Methods

        public static void Log(string message)
        {
            // Check for log file, if none exists create new one.
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }

            if (!File.Exists(LogFilePath))
            {
                File.Create(AppDataPath);
                File.SetCreationTimeUtc(LogFilePath, DateTime.UtcNow);
            }

            StreamWriter file = new StreamWriter(LogFilePath, true, Encoding.GetEncoding("shift_jis"));

            file.WriteAsync($"[{DateTime.Now}] {message}");

            file.Close();
        }

        public static void Log(Exception exception)
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

            StreamWriter file = new StreamWriter(LogFilePath, true, Encoding.GetEncoding("shift_jis"));

            file.Write(
                $"[{DateTime.Now}] ====== Exception Occured ====== \n \n Source: {exception.Source}  \n {exception.Message} \n Target Site: {exception.TargetSite} \n Message: {exception.Message} \n Details: {exception.InnerException} \n \n Exception Data: {exception.Data} \n \n Stack Trace: {exception.StackTrace} \n ============ \n");

            file.Close();
        }

        public static void Log(Exception exception, bool append)
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

            StreamWriter file = new StreamWriter(LogFilePath, append, Encoding.GetEncoding("shift_jis"));

            file.Write(
                $"[{DateTime.Now}] ====== Exception Occured ====== \n \n Source: {exception.Source}  \n {exception.Message} \n Target Site: {exception.TargetSite} \n Message: {exception.Message} \n Details: {exception.InnerException} \n \n Exception Data: {exception.Data} \n \n Stack Trace: {exception.StackTrace} \n ============ \n");
            file.Close();
        }

        public static void Log(Exception exception, bool append, string message)
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

            StreamWriter file = new StreamWriter(LogFilePath, append, Encoding.GetEncoding("shift_jis"));

            file.Write(
                $"[{DateTime.Now}] ====== Exception Occured ====== \n \n {message} \n Source: {exception.Source}  \n {exception.Message} \n Target Site: {exception.TargetSite} \n Message: {exception.Message} \n Details: {exception.InnerException} \n \n Exception Data: {exception.Data} \n \n Stack Trace: {exception.StackTrace} \n ============ \n");
            file.Close();
        }

        #endregion
    }
}