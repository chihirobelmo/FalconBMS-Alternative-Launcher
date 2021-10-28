using System;
using System.IO;
using System.Text;

namespace FalconBMS.Launcher
{
    public class Diagnostics
    {
        #region Fields

        public string log;

        public static readonly string AppDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create)}" + "\\FalconBMSAlternativeLauncher";

        public static readonly string LogFilePath = $"{AppDataPath}" + "\\Log.txt";

        #endregion

        #region Methods
        public void Log(string message)
        {
            string message2 = $"[{DateTime.Now}] {message}";
            log = log + message2 + "/r/n";
        }
        public void Log(Exception exception)
        {
            string message2 = $"[{DateTime.Now}] ====== Exception Occured ====== \n \n Source: {exception.Source}  \n {exception.Message} \n Target Site: {exception.TargetSite} \n Message: {exception.Message} \n Details: {exception.InnerException} \n \n Exception Data: {exception.Data} \n \n Stack Trace: {exception.StackTrace} \n ============ \n";
            log = log + message2 + "/r/n";
        }
        public void Log(Exception exception, string message)
        {
            string message2 = $"[{DateTime.Now}] ====== Exception Occured ====== \n \n {message} \n Source: {exception.Source}  \n {exception.Message} \n Target Site: {exception.TargetSite} \n Message: {exception.Message} \n Details: {exception.InnerException} \n \n Exception Data: {exception.Data} \n \n Stack Trace: {exception.StackTrace} \n ============ \n";
            log = log + message2 + "/r/n";
        }

        public void Output()
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

            StreamWriter file = new StreamWriter(Diagnostics.LogFilePath, true, Encoding.Default);

            file.Write($"{log}");
            file.Close();
        }

        public void Output(bool append)
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

            StreamWriter file = new StreamWriter(Diagnostics.LogFilePath, append, Encoding.Default);

            file.Write($"{log}");
            file.Close();
        }

        #endregion
    }
}