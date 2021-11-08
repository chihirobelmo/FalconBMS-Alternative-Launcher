using System;
using System.Collections.Generic;
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
        private AppRegInfo ap;
        private ListBox lb;
        public DownloadWindow(MainWindow mainWindow, AppRegInfo ap, ListBox lb)
        {
            this.mainWindow = mainWindow;
            this.ap = ap;
            this.lb = lb;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void Major_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Minor_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
