using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

using FalconBMS.Launcher.Core;
using FalconBMS.Launcher.Input;

using MahApps.Metro.Controls;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            InitializeComponent();

            MouseWheel += Detect_MouseWheel;
        }

        public static DeviceControl deviceControl;

        private AppRegInfo appReg;
        private KeyFile keyFile;

        private AppProperties appProperties;

        private DispatcherTimer axisMovingTimer = new DispatcherTimer();
        private DispatcherTimer keyMappingTimer = new DispatcherTimer();
        
        public static bool flgYame64;

        /// <summary>
        /// Execute when launching this app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // load command line.
                string[] args = Environment.GetCommandLineArgs();

                if (args.Length % 2 == 1)
                {
                    Dictionary<string, string> option = new Dictionary<string, string>();
                    for (int index = 1; index < args.Length; index += 2)
                    {
                        option.Add(args[index], args[index + 1]);
                    }
                    if (option.ContainsKey("/yame"))
                        if (option["/yame"] == "true")
                            flgYame64 = true;

                    if (flgYame64)
                    {
                        LargeTab.SelectedIndex = 1;
                        TabLauncher.Visibility = Visibility.Collapsed;

                        Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                        BackGroundBox1.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                        BackGroundBox2.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                        BackGroundBox3.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                        BackGroundBox4.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                        BackGroundImage.Opacity = 0;

                        ButtonApplyYame64.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ButtonApplyYame64.Visibility = Visibility.Hidden;
                    }
                }
            }

            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                StreamWriter file = new StreamWriter(Diagnostics.LogFilePath, append: true, Encoding.GetEncoding("shift_jis"));
                

                Diagnostics.Log(ex);
                //StreamWriter sw = new StreamWriter("C:\\FBMSAltLauncherErrorLog.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                //sw.Write(ex.Message);
                //sw.Close();

                MessageBox.Show($"Error Log Saved To {Diagnostics.AppDataPath}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                Close();
            }

            // Load UI Properties(Like Button Status).
            appProperties = new AppProperties(this);

            // Read Registry
            appReg = new AppRegInfo(this);

            if (appReg.GetBmsVersion() == BmsVersion.Undefined)
            {
                Close();
                return;
            }

            try
            {
                // Read Theater List
                TheaterList.Populate(appReg, DropdownTheaterList);

                // Get Devices
                deviceControl = new DeviceControl(appReg);
                neutralButtons = new Input.MainWindow.NeutralButtons[deviceControl.devList.Count];

                // Aquire joySticks
                Input.MainWindow.AquireAll(true);

                // Reset All Axis Settings
                foreach (AxisName nme in Input.MainWindow.axisNameList)
                    Input.MainWindow.inGameAxis[nme.ToString()] = new InGameAxAssign();
                joyAssign_2_inGameAxis();
                ResetAssgnWindow();

                // Read BMS-FULL.key
                string fname = appReg.GetInstallDir() + "\\User\\Config\\" + appReg.GetKeyFileName();
                keyFile = new KeyFile(fname, appReg);

                // Write Data Grid
                WriteDataGrid();

                // Set Timer
                axisMovingTimer.Tick += AxisMovingTimer_Tick;
                axisMovingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                keyMappingTimer.Tick += KeyMappingTimer_Tick;
                keyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                //System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            }

            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Diagnostics.Log(ex);
                //StreamWriter sw = new StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                //sw.Write(ex.Message);
                //sw.Close();

                MessageBox.Show($"Error Log Saved To  {Diagnostics.AppDataPath}", "Warning", MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
            }
        }
        
        /// <summary>
        /// Execute when quiting this app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (appReg == null)
                    return;

                // Save UI Properties(Like Button Status).
                appProperties.SaveUiSetup();
                if (ApplicationOverride.IsChecked == false)
                    appReg.GetOverrideWriter().Execute(Input.MainWindow.inGameAxis, deviceControl, keyFile);
            }

            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                Diagnostics.Log(ex);
                //StreamWriter sw = new StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                //sw.Write(ex.Message);
                //sw.Close();

                MessageBox.Show($"Error Log Saved To {Diagnostics.AppDataPath}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                Close();
            }
        }
        
        /// <summary>
        /// Execute/Stop timer event when changing top TAB menu (Launcher/AxisAssign/KeyMapping).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.Source.Equals(LargeTab))
                return;

            int value = LargeTab.SelectedIndex;

            if (value == 1)
                axisMovingTimer.Start();
            else
                axisMovingTimer.Stop();

            if (value == 2)
            {
                keyMappingTimer.Start();
                KeyMappingGrid.Items.Refresh();
            }
            else
                keyMappingTimer.Stop();
        }
        
        /// <summary>
        /// Rewrite Theater setting in the registry and Show/Hide Theater own config icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dropdown_TheaterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appReg.ChangeTheater(DropdownTheaterList);
        }
        
        /// <summary>
        /// Launch Theater own config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_TheaterConfig_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(appReg.theaterOwnConfig);
        }
        
        /// <summary>
        /// When clicked BW button, changes BW value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CMD_BW_Click(object sender, RoutedEventArgs e)
        {
            appProperties.CMD_BW_Click();
        }

        /// <summary>
        /// Open BMS Docs and Manuals.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenDocs_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(appReg.GetInstallDir() + "/Docs");
        }

        /// <summary>
        ///  Launch BMS utilities.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                appReg.GetLauncher().Execute(sender);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                Diagnostics.Log(ex);

                //StreamWriter sw = new StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                //sw.Write(ex.Message);
                //sw.Close();

                MessageBox.Show($"Error log saved to {Diagnostics.AppDataPath}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                Close();
            }
        }

        /// <summary>
        /// As the name implies.
        /// </summary>
        /// <returns></returns>
        public int GetBwValue()
        {
            return appProperties.bandWidthDefault;
        }

        /// <summary>
        /// OverrideSettings.
        /// </summary>
        public void ExecuteOverride()
        {
            try
            {
                // throw new Exception("An exception occurs.");
                if (ApplicationOverride.IsChecked == true)
                {
                    string textMessage = "You are going to launch BMS without any setup override from AxisAssign and KeyMapping section.";
                    MessageBox.Show(textMessage, "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    appReg.GetOverrideWriter().Execute(Input.MainWindow.inGameAxis, deviceControl, keyFile);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                Diagnostics.Log(ex);
                //StreamWriter sw = new StreamWriter("C:\\FBMSAltLauncherErrorLog.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                //sw.Write(ex.Message);
                //sw.Close();

                Close();
            }
        }

        /// <summary>
        /// As the name implies.
        /// </summary>
        /// <param name="process"></param>
        public void MinimizeWindowUntilProcessEnds(Process process)
        {
            process.Exited += window_Normal;
            process.EnableRaisingEvents = true;
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Something I need to launch BMS.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void window_Normal(object sender, EventArgs e)
        {
            this.Invoke(() => { WindowState = WindowState.Normal; });
        }

        /// <summary>
        /// Launch third party utilities.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_Third(object sender, RoutedEventArgs e)
        {
            try
            {

                string target = "";
                string downloadlink = "";
                string installexe = "";

                switch (((Button)sender).Name)
                {
                    case "Launch_WDP":
                        target = "\\WeaponDeliveryPlanner.exe";
                        downloadlink = "http://www.weapondeliveryplanner.nl/";
                        installexe = Properties.Settings.Default.Third_WDP + target;
                        if (File.Exists(installexe) == false)
                        {
                            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                            fbd.Description = "Select Install Directory";
                            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                            fbd.ShowNewFolderButton = false;
                            System.Windows.Forms.DialogResult dirResult = fbd.ShowDialog();

                            installexe = fbd.SelectedPath + target;
                            if (File.Exists(installexe))
                                Properties.Settings.Default.Third_WDP = fbd.SelectedPath;
                            else
                            {
                                Process.Start(downloadlink);
                                return;
                            }
                        }
                        Process.Start(installexe);
                        break;
                    case "Launch_MC":
                        target = "\\Mission Commander.exe";
                        downloadlink = "http://www.weapondeliveryplanner.nl/";
                        installexe = Properties.Settings.Default.Third_MC + target;
                        if (File.Exists(installexe) == false)
                        {
                            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                            fbd.Description = "Select Install Directory";
                            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                            fbd.ShowNewFolderButton = false;
                            System.Windows.Forms.DialogResult dirResult = fbd.ShowDialog();

                            installexe = fbd.SelectedPath + target;
                            if (File.Exists(installexe))
                                Properties.Settings.Default.Third_MC = fbd.SelectedPath;
                            else
                            {
                                Process.Start(downloadlink);
                                return;
                            }
                        }
                        Process.Start(installexe);
                        break;
                    case "Launch_WC":
                        target = "\\Weather Commander.exe";
                        downloadlink = "http://www.weapondeliveryplanner.nl/";
                        installexe = Properties.Settings.Default.Third_WC + target;
                        if (File.Exists(installexe) == false)
                        {
                            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                            fbd.Description = "Select Install Directory";
                            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                            fbd.ShowNewFolderButton = false;
                            System.Windows.Forms.DialogResult dirResult = fbd.ShowDialog();

                            installexe = fbd.SelectedPath + target;
                            if (File.Exists(installexe))
                                Properties.Settings.Default.Third_WC = fbd.SelectedPath;
                            else
                            {
                                Process.Start(downloadlink);
                                return;
                            }
                        }
                        Process.Start(installexe);
                        break;
                    case "Launch_F4WX":
                        target = "\\F4Wx.exe";
                        downloadlink = "https://www.benchmarksims.org/forum/showthread.php?29203";
                        installexe = Properties.Settings.Default.Third_F4WX + target;
                        if (File.Exists(installexe) == false)
                        {
                            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog
                            {
                                Description = "Select Install Directory",
                                RootFolder = Environment.SpecialFolder.MyComputer,
                                ShowNewFolderButton = false
                            };

                            System.Windows.Forms.DialogResult dirResult = fbd.ShowDialog();

                            installexe = fbd.SelectedPath + target;
                            if (File.Exists(installexe))
                                Properties.Settings.Default.Third_F4WX = fbd.SelectedPath;
                            else
                            {
                                Process.Start(downloadlink);
                                return;
                            }
                        }
                        Process.Start(installexe);
                        break;
                }
            }

            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                Diagnostics.Log(ex);
                //StreamWriter sw = new StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, Encoding.GetEncoding("shift_jis"));
                //sw.Write(ex.Message);
                //sw.Close();

                MessageBox.Show($"Error Log Saved to {Diagnostics.AppDataPath}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                Close();
            }
        }

        /// <summary>
        /// Change label color when mouse enters one of launcher icons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseEnterLauncher(object sender, EventArgs e)
        {
            string nme = ((Button)sender).Name;

            if (nme.Contains("Launch_"))
            {
                if (nme.Contains("Launch_TheaterConfig"))
                    return;

                if (!(FindName(nme) is Button tbButton))
                    return;
                tbButton.BorderBrush = new SolidColorBrush(Colors.LightBlue);
                tbButton.BorderThickness = new Thickness(1);

                nme = nme.Replace("Launch_", "");

                if (!(FindName("Label_" + nme) is Label tblabel))
                    return;
                tblabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 128, 255, 255));
            }
        }

        /// <summary>
        /// Reset label color when mouse leaves a launcher icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseLeaveLauncher(object sender, EventArgs e)
        {
            string nme = ((Button)sender).Name;

            if (nme.Contains("Launch_"))
            {
                if (nme.Contains("Launch_TheaterConfig"))
                    return;

                if (!(FindName(nme) is Button tbButton))
                    return;
                tbButton.BorderThickness = new Thickness(0);

                nme = nme.Replace("Launch_", "");

                if (!(FindName("Label_" + nme) is Label tblabel))
                    return;
                tblabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
            }
        }
        
        /// <summary>
        /// Allow user to drag the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            }
            catch
            {

            }
        }

        private void Apply_YAME64(object sender, RoutedEventArgs e)
        {
            appReg.GetOverrideWriter().Execute(Input.MainWindow.inGameAxis, deviceControl, keyFile);
            Close();
        }

        private void WDP_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("http://www.weapondeliveryplanner.nl/");
        }

        private void Serfoss2003_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("https://apps.dtic.mil/docs/citations/ADA414893");
        }

        private void CMD_WINDOW_Click(object sender, RoutedEventArgs e)
        {
            if (CmdWindow.IsChecked == true)
                MessageBox.Show("FalconBMS crashes when Alt+TAB in FullScreen Mode. Recommend Enabling Window Mode.\n(WINDOW button turning on light)", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool pressedByHand;

        private void Select_PinkyShift_Click(object sender, RoutedEventArgs e)
        {
            if (SelectPinkyShift.IsChecked == false)
                pressedByHand = true;
            else
                pressedByHand = false;
        }
    }
}
