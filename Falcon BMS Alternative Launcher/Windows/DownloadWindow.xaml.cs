using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow
    {
        public bool status;
        public bool which;
        private MainWindow mainWindow;
        private AppRegInfo appReg;
        private ListBox lb;
        public Update.Update up;
        public DownloadWindow(MainWindow mainWindow, AppRegInfo appReg, ListBox lb)
        {
            this.mainWindow = mainWindow;
            this.appReg = appReg;
            this.lb = lb;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //System.Xml.Serialization.XmlSerializer serializer;
            //serializer = new System.Xml.Serialization.XmlSerializer(typeof(Update.Update));
            //
            //string fileName = "UpdateBMS.xml";
            //StreamReader sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
            //up = (Update.Update)serializer.Deserialize(sr);
            //sr.Close();

            LatestBMSStatus lbs = new LatestBMSStatus();

            if (appReg.getUpdateVersion() - 3 == lbs.updateCount)
                Button_Download_Minor.IsEnabled = false;

            if (File.Exists(lbs.destination + "\\" + lbs.setupExe[0].InnerText))
                Button_Download_Major.IsEnabled = false;

            for (int i = appReg.getUpdateVersion(); i < lbs.updateCount; i++)
                if (!File.Exists(lbs.destination + "\\" + lbs.updateExe[i].InnerText))
                    Button_Install_Minor.IsEnabled = false;

            if (!File.Exists(lbs.destination + "\\" + lbs.setupExe[0].InnerText))
                Button_Download_Major.IsEnabled = false;
        }

        public static bool ShowDownloadWindow(MainWindow mainWindow, AppRegInfo ap, ListBox lb)
        {
            DownloadWindow ownWindow = new DownloadWindow(mainWindow, ap, lb);
            ownWindow.ShowDialog();
            return ownWindow.status;
        }

        public void ChangeStatus(string args)
        {
            Label_Status.Content = args;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Torrent.status = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Major_Download_Click(object sender, RoutedEventArgs e)
        {
            DownloadMajorUpdate(this);
        }

        private async void Minor_Download_Click(object sender, RoutedEventArgs e)
        {
            await DownloadMajorUpdate(this);
            await UnzipMajorUpdate();
            await DownloadMinorUpdate(this);
        }

        private void Major_Install_Click(object sender, RoutedEventArgs e)
        {
            DoMajorUpdate(mainWindow);
        }

        private void Minor_Install_Click(object sender, RoutedEventArgs e)
        {
            DoMinorUpdate(mainWindow, appReg);
            DoMajorUpdate(mainWindow);
        }

        public void syncStatus(string args)
        {
            Label_Status.Content = args;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            }
            catch (Exception ex)
            {
                // Do Not Set Diagnostics.Log nor Output here!
            }
        }

        static readonly HashAlgorithm hashProvider = new MD5CryptoServiceProvider();

        public static string ComputeFileHash(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bs = hashProvider.ComputeHash(fs);
                return BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }

        public static bool CheckMinorUpdate(AppRegInfo appReg)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            return (appReg.getUpdateVersion() - 3 == lbs.updateCount);  // -3 for debug
        }

        public static async Task<bool> DownloadMinorUpdate(DownloadWindow dl)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            Torrent.Delete(lbs.destination);

            System.Diagnostics.Process process;

            if (lbs.release != "true")
                return false;

            for (int i = 0; i < lbs.updateCount; i++)
            {
                if (!File.Exists(lbs.destination + "\\" + lbs.updateExe[i].InnerText))
                {
                    Torrent.status = true;

                    Torrent.Download(dl, lbs.updateHash[i].InnerText, lbs.updateExe[i].InnerText, lbs.destination);
                }
            }
            return true;
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

        public static async Task<bool> DownloadMajorUpdate(DownloadWindow dl)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            if (lbs.release != "true")
                return false;

            if (File.Exists(lbs.destination + "\\" + lbs.setupExe[0].InnerText))
                return false;

            string dlpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

            Torrent.Delete(dlpath);

            Torrent.status = true;

            bool task = await Torrent.Download(dl, lbs.setupHash[0].InnerText, lbs.setupZip[0].InnerText, dlpath);

            return task;
        }

        public static void DoMajorUpdate(MainWindow mainWindow)
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            if (!Directory.Exists(lbs.destination))
                return;
            if (!File.Exists(lbs.destination + "\\" + lbs.setupExe[0].InnerText))
                return;

            System.Diagnostics.Process process;
            process = System.Diagnostics.Process.Start(lbs.destination + "\\" + lbs.setupExe[0].InnerText);
            mainWindow.minimizeWindowUntilProcessEnds(process);
        }

        public static async Task<bool> UnzipMajorUpdate()
        {
            LatestBMSStatus lbs = new LatestBMSStatus();

            string dlpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

            if (!Directory.Exists(lbs.destination))
                Directory.CreateDirectory(lbs.destination);
            if (!File.Exists(dlpath + "\\" + lbs.setupZip[0].InnerText))
                return false;
            else
            {
                if (!File.Exists(lbs.destination + "\\" + lbs.setupExe[0].InnerText))
                {
                    FileStream zipStream = File.OpenRead(dlpath + "\\" + lbs.setupZip[0].InnerText);
                    using (ZipArchive archive = new ZipArchive(zipStream))
                        ExtractToDirectory(archive, "C:\\", true);
                }
            }
            return true;
        }
        public static void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                }

                if (file.Name == "")
                {// Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }
    }
}
