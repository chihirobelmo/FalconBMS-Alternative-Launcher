using FalconBMS.Launcher.Windows;
using Leak.Client.Swarm;
using Leak.Common;
using System;
using System.Collections.Generic;
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
            string tracker = "udp://tracker.opentrackr.org:1337/announce";
            FileHash hash = FileHash.Parse("AADE99207C7CAD327381AE2704F95A3AAB07C1C9");

            using (SwarmClient client = new SwarmClient())
            {
                Leak.Client.Notification notification = null;
                SwarmSession session = await client.ConnectAsync(hash, tracker);

                session.Download("c:\\Falcon BMS 4.35 Setup");

                Console.WriteLine("StartPeerConnecting");
                do
                {
                    Console.WriteLine("PeerConnecting");
                    notification = await session.NextAsync();
                }
                while (notification.Type != Leak.Client.NotificationType.PeerConnected);
                Console.WriteLine("PeerConnected");

                Console.WriteLine("StartAsync");
                do
                {
                    Console.WriteLine("Asyncing");
                    notification = await session.NextAsync();
                }
                while (notification.Type != Leak.Client.NotificationType.DataCompleted);
                Console.WriteLine("AsyncComplete");
            }
        }

        public static async Task Download(string trackerSt, string hashSt, string exest, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            string tracker = trackerSt;
            FileHash hash = FileHash.Parse(hashSt);

            using (SwarmClient client = new SwarmClient())
            {
                Leak.Client.Notification notification = null;
                SwarmSession session = await client.ConnectAsync(hash, tracker);

                session.Download(destination);

                Console.WriteLine("StartPeerConnecting");
                do
                {
                    Console.WriteLine("PeerConnecting");
                    notification = await session.NextAsync();
                    if (!status)
                        return;
                }
                while (notification.Type != Leak.Client.NotificationType.PeerConnected);
                Console.WriteLine("PeerConnected");

                do
                {
                    Console.WriteLine("Asyncing :");
                    notification = await session.NextAsync();
                    if (!status)
                        return;
                }
                while (notification.Type != Leak.Client.NotificationType.DataCompleted);

                if (File.Exists(destination + "\\" + hashSt + "\\" + exest))
                {
                    File.Move(destination + "\\" + hashSt + "\\" + exest, destination + "\\" + exest);
                    Directory.Delete(destination + "\\" + hashSt);
                }
            }

            return;
        }

        public static bool CheckMinorUpdate(AppRegInfo appReg)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            return (appReg.getUpdateVersion() == lbs.updateCount);
        }

        public static void DownloadMinorUpdate()
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            System.Diagnostics.Process process;

            if (lbs.release != "true")
                return;

            for (int i = 0; i < lbs.setupCount; i++)
            {
                if (!File.Exists(lbs.destination + "\\" + lbs.updateExe[i].InnerText))
                {
                    status = true;

                    Download(lbs.updateTracker[i].InnerText, lbs.updateHash[i].InnerText, lbs.updateExe[i].InnerText, lbs.destination);
                }
            }
        }

        public static void DoMinorUpdate(MainWindow mainWindow, AppRegInfo appReg)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            System.Diagnostics.Process process;

            for (int i = appReg.getUpdateVersion(); i < lbs.updateCount; i++)  // -3 for debug
            {
                if (File.Exists(lbs.destination + "\\" + lbs.updateExe[i].InnerText))
                {
                    process = System.Diagnostics.Process.Start(lbs.destination + "\\" + lbs.updateExe[i].InnerText);
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    continue;
                }
            }
        }

        public static bool CheckMajorUpdate(ListBox lb, string version)
        {
            return (lb.Items.IndexOf(version) != -1);
        }

        public static void DownloadMajorUpdate()
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            if (lbs.release != "true")
                return;

            string dlpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

            status = true;

            Download(lbs.setupTracker[0].InnerText, lbs.setupHash[0].InnerText, lbs.setupZip[0].InnerText, dlpath);
        }

        public static void DoMajorUpdate(MainWindow mainWindow)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            string dlpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

            if (!Directory.Exists(lbs.destination))
                Directory.CreateDirectory(lbs.destination);
            if (!File.Exists(dlpath + "\\" + lbs.setupZip[0].InnerText))
                return;
            ZipFile.ExtractToDirectory(dlpath + "\\" + lbs.setupZip[0].InnerText, lbs.destination);

            System.Diagnostics.Process process;
            process = System.Diagnostics.Process.Start(lbs.destination + "\\" + lbs.setupExe[0].InnerText);
            mainWindow.minimizeWindowUntilProcessEnds(process);
        }
    }
}
