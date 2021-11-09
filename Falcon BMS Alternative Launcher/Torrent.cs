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

        public static async Task Test()
        {
            string[] tracker = {"udp://tracker.opentrackr.org:1337/announce",
                                "udp://tracker.openbittorrent.com:80/announce",
                                "udp://tracker.leechers-paradise.org:6969/announce",
                                "udp://tracker.coppersurfer.tk:6969/announce",
                                "udp://thetracker.org:80/announce",
                                "udp://tracker.torrent.eu.org:451/announce"};

            string hash = "AADE99207C7CAD327381AE2704F95A3AAB07C1C9";

            return;
        }

        public static async Task Download(DownloadWindow dl, string hashSt, string exest, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            dl.syncStatus("Downloading : " + exest);

            string command = " --fc \"" + destination + "\" --fi \"" + destination + "\" --ft \"" + destination + "\" --fs \"" + destination + "\" " + hashSt + "\"";
            string check = ".\\bitswarm.exe" + command;
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
            }
            catch
            { }

            return;
        }

        public static bool CheckMinorUpdate(AppRegInfo appReg)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();
             
            return (appReg.getUpdateVersion() - 3 == lbs.updateCount);  // -3 for debug
        }

        public static void DownloadMinorUpdate(DownloadWindow dl)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            System.Diagnostics.Process process;

            if (lbs.release != "true")
                return;

            for (int i = 0; i < lbs.updateCount; i++)
            {
                if (!File.Exists(lbs.destination + "\\" + lbs.updateExe[i].InnerText))
                {
                    status = true;

                    Download(dl, lbs.updateHash[i].InnerText, lbs.updateExe[i].InnerText, lbs.destination);
                }
            }
        }

        public static void DoMinorUpdate(MainWindow mainWindow, AppRegInfo appReg)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            System.Diagnostics.Process process;

            for (int i = appReg.getUpdateVersion() - 3; i < lbs.updateCount; i++)  // -3 for debug
            {
                if (File.Exists(lbs.destination + "\\" + lbs.updateExe[i].InnerText))
                {
                    process = System.Diagnostics.Process.Start(lbs.destination + "\\" + lbs.updateExe[i].InnerText);
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    continue;
                }
            }
        }

        public static bool CheckMajorUpdate(ListBox lb)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            string version = lbs.registory;

            return (lb.Items.IndexOf(version) != -1);
        }

        public static void DownloadMajorUpdate(DownloadWindow dl)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            if (lbs.release != "true")
                return;

            if (!File.Exists(lbs.destination + "\\" + lbs.setupExe[0].InnerText))
                return;

            string dlpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

            status = true;

            Download(dl, lbs.setupHash[0].InnerText, lbs.setupZip[0].InnerText, dlpath);
        }

        public static void DoMajorUpdate(MainWindow mainWindow)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            string dlpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

            if (!Directory.Exists(lbs.destination))
                Directory.CreateDirectory(lbs.destination);
            if (!File.Exists(dlpath + "\\" + lbs.setupZip[0].InnerText))
                return;
            else
                if (!File.Exists(lbs.destination + "\\" + lbs.setupExe[0].InnerText))
                    ZipFile.ExtractToDirectory(dlpath + "\\" + lbs.setupZip[0].InnerText, "C:\\");

            System.Diagnostics.Process process;
            process = System.Diagnostics.Process.Start(lbs.destination + "\\" + lbs.setupExe[0].InnerText);
            mainWindow.minimizeWindowUntilProcessEnds(process);
        }
    }
}
