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
        public static Update.Update update;
        public DownloadWindow(MainWindow mainWindow, AppRegInfo appReg, ListBox lb)
        {
            this.mainWindow = mainWindow;
            this.appReg = appReg;
            this.lb = lb;

            InitializeComponent();
        }

        public static void CheckUpdateInformation()
        {
            if (update != null)
                return;
            try
            {
                System.Xml.Serialization.XmlSerializer serializer;
                serializer = new System.Xml.Serialization.XmlSerializer(typeof(Update.Update));

                string fileName = "UpdateBMS.xml";
                StreamReader sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
                update = (Update.Update)serializer.Deserialize(sr);
                sr.Close();
            }
            catch 
            { 
                // WIP
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            status = true;
            CheckUpdateInformation();
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
            status = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            status = false;
            Close();
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            DownloadMajorUpdate();
            UnzipMajorUpdate();
            DownloadMinorUpdate();
        }

        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            DoMinorUpdate();
            DoMajorUpdate();
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
        public static bool CheckMinorUpdate(AppRegInfo appReg)
        {
            if (update == null)
                return true;

            CheckUpdateInformation();

            return appReg.getUpdateVersion() == update.bms.inclementalUpdate.Length;  
        }

        public static bool CheckMajorUpdate(ListBox lb)
        {
            if (update == null)
                return true;

            CheckUpdateInformation();

            return lb.Items.IndexOf(update.bms.registry.name) != -1;
        }

        public bool DownloadMinorUpdate()
        {
            if (update == null)
                return true;

            if (!update.bms.release)
                return false;
            if (!update.bms.installer.DestinationExist())
                Directory.CreateDirectory(update.bms.installer.exe.destination);

            for (int i = 0; i < update.bms.inclementalUpdate.Length; i++)
            {
                update.bms.inclementalUpdate[i].DeleteBsf();
                if (!update.bms.inclementalUpdate[i].Exist())
                {
                    if (!status)
                        return false;
                    update.bms.inclementalUpdate[i].Download(this);
                }
            }
            return true;
        }

        public bool DownloadMajorUpdate()
        {
            if (update == null)
                return true;

            update.bms.installer.DeleteBsf();

            if (!update.bms.release)
                return false;
            if (!update.bms.installer.exe.DestinationExist())
                Directory.CreateDirectory(update.bms.installer.exe.destination);
            if (update.bms.installer.exe.Exist())
                return false;
            if (update.bms.installer.Exist())
                return false;

            Torrent.status = true;

            if (!status)
                return false;

            update.bms.installer.Download(this);

            return true;
        }

        public static void DoMinorUpdate()
        {
            if (update == null)
                return;

            if (!update.bms.installer.exe.DestinationExist())
                Directory.CreateDirectory(update.bms.installer.exe.destination);

            System.Diagnostics.Process process;

            for (int i = 0; i < update.bms.inclementalUpdate.Length; i++)  // -3 for debug
                if (update.bms.inclementalUpdate[i].Exist())
                    update.bms.inclementalUpdate[i].Execute();
        }

        public void DoMajorUpdate()
        {
            if (update == null)
                return;

            if (update.bms.installer.exe.Exist())
                update.bms.installer.exe.Execute();
        }

        public bool UnzipMajorUpdate()
        {
            if (update == null)
                return true;

            if (!Directory.Exists(update.bms.installer.exe.destination))
                Directory.CreateDirectory(update.bms.installer.exe.destination);
            if (update.bms.installer.exe.Exist())
                return false;
            else
                if (!update.bms.installer.exe.Exist())
                    update.bms.installer.ExtractToDirectory();
            return true;
        }
    }
}
