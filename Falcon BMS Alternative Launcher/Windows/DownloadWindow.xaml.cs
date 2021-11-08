using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public DownloadWindow(MainWindow mainWindow, AppRegInfo appReg, ListBox lb)
        {
            this.mainWindow = mainWindow;
            this.appReg = appReg;
            this.lb = lb;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
            Torrent.DownloadMajorUpdate(this);
        }

        private void Minor_Download_Click(object sender, RoutedEventArgs e)
        {
            //Torrent.Test();
            Torrent.DownloadMinorUpdate(this);
        }

        private void Major_Install_Click(object sender, RoutedEventArgs e)
        {
            Torrent.DoMajorUpdate(mainWindow);
        }

        private void Minor_Install_Click(object sender, RoutedEventArgs e)
        {
            Torrent.DoMinorUpdate(mainWindow, appReg);
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
    }
}
