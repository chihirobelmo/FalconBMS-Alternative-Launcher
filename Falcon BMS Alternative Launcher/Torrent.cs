using FalconBMS.Launcher.Windows;
using Leak.Client;
using Leak.Client.Swarm;
using Leak.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace FalconBMS.Launcher
{
    public class Torrent
    {
        public static bool status;
        public static bool BitSwarm(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(".\\bitswarm.exe", command);

            processStartInfo.CreateNoWindow = false;
            processStartInfo.UseShellExecute = true;

            Process process;

            try
            {
                process = Process.Start(processStartInfo);

                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();
                int exitCode = process.ExitCode;

                process.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Download(DownloadWindow dl, string hashSt, string exest, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            dl.syncStatus("Downloading: " + exest);

            string command = " --fc \"" + destination + "\" --fi \"" + destination + "\" --ft \"" + destination + "\" --fs \"" + destination + "\" " + hashSt + "\"";

            BitSwarm(command);

            dl.syncStatus("Download Complete: " + exest);

            return true;
        }

        public static void Delete(string folderFrom)
        {
            foreach (string pathFrom in System.IO.Directory.EnumerateFiles(folderFrom, "*.bsf"))
                File.Delete(pathFrom);
            foreach (string pathFrom in System.IO.Directory.EnumerateFiles(folderFrom, "*.torrent"))
                File.Delete(pathFrom);
        }
    }
}
