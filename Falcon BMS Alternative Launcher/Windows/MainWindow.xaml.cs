﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

using FalconBMS.Launcher.Input;

using MahApps.Metro.Controls;

using AutoUpdaterDotNET;
using System.Reflection;
using System.Xml;
using System.Threading.Tasks;
using FalconBMS.Launcher.Servers;
using System.Security.Policy;
using System.Linq;
using ControlzEx.Standard;
using System.Collections;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        static SteamVR steamVR = new SteamVR();
        public MainWindow()
        {
            try
            {
                RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
                InitializeComponent();

                MouseWheel += Detect_MouseWheel;
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
            }
        }

        public static DeviceControl deviceControl;

        public AppRegInfo appReg;
        private KeyFile keyFile;

        private AppProperties appProperties;

        private DispatcherTimer AxisMovingTimer = new DispatcherTimer();
        private DispatcherTimer KeyMappingTimer = new DispatcherTimer();
        private DispatcherTimer NewDeviceDetectTimer = new DispatcherTimer();

        private PhoneBookParser phoneBookParser;

        public static bool FLG_YAME64;

        /// <summary>
        /// Execute when launching this app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                System.Version ver = asm.GetName().Version;

                AutoUpdater.Mandatory = true;
                AutoUpdater.Start("https://raw.githubusercontent.com/chihirobelmo/FalconBMS-Alternative-Launcher/master/Falcon%20BMS%20Alternative%20Launcher/AutoUpdate.xml", asm);

                Diagnostics.Log("Launcher Update Checked");

                string BMS_Launcher_version = "FalconBMS Launcher v" + ver.Major + "." + ver.Minor + "." + ver.Build;
                AL_Version_Number.Content = BMS_Launcher_version;

                Diagnostics.Log(BMS_Launcher_version);

                RSSReader.Read("https://www.falcon-bms.com/news/feed/", "https://www.falcon-bms.com");
                RSSReader.Read("https://www.falcon-lounge.com/news/feed/", "https://www.falcon-lounge.com");
                RSSReader.Write(News);

                Diagnostics.Log("RSS Read and Write Finished");
            }
            catch (Exception expass)
            {
                Diagnostics.WriteLogFile(expass);
            }

            try
            {
                appProperties = new AppProperties(this);
                appReg = new AppRegInfo(this);
                InitDeveices();

                if (appReg.getBMSVersion() == BMS_Version.UNDEFINED)
                {
                    MessageBox.Show("Could Not Find BMS");
                    Diagnostics.WriteLogFile();
                    Close();
                    return;
                }

                StartVR();
                StartTimers();
            }
            catch (Exception exclose)
            {
                Diagnostics.WriteLogFile(exclose);
                Close();
                return;
            }
        }

        private void StartTimers()
        {
            Diagnostics.Log("Start Timers.");

            // Set Timer
            AxisMovingTimer.Tick += AxisMovingTimer_Tick;
            AxisMovingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

            KeyMappingTimer.Tick += KeyMappingTimer_Tick;
            KeyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 32);

            NewDeviceDetectTimer.Tick += NewDeviceDetectTimer_Tick;
            NewDeviceDetectTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);

            NewDeviceDetectTimer.Start();

            Diagnostics.Log("Timers Started.");

            // first arg is always the program name
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                BringToForeground(Environment.GetCommandLineArgs().Skip(1).ToArray());
            }
        }

        private void StartVR()
        {
            Diagnostics.Log("Start VR Check.");

            if ((bool)Misc_VR.IsVisible)
                if ((bool)Misc_VR.IsChecked)
                    steamVR.Start();

            Diagnostics.Log("Finished VR Check.");
        }

        private void InitDeveices()
        {
            Diagnostics.Log("Start Init Devices.");

            FillKeyFileList();
            BMSChanged();
            ReloadDevices();

            Diagnostics.Log("Finished Init Devices.");
        }

        private async void NewDeviceDetectTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Microsoft.DirectX.DirectInput.DeviceList devList = await Task.Run(() =>
                    Microsoft.DirectX.DirectInput.Manager.GetDevices(
                        Microsoft.DirectX.DirectInput.DeviceClass.GameControl,
                        Microsoft.DirectX.DirectInput.EnumDevicesFlags.AttachedOnly
                        ));

                try
                {
                    if (deviceControl.devList.Count != devList.Count)
                    {
                        AxisMovingTimer.Stop();
                        KeyMappingTimer.Stop();

                        ReloadDevices();

                        int value = LargeTab.SelectedIndex;
                        if (value == 1)
                            AxisMovingTimer.Start();
                        if (value == 2)
                        {
                            KeyMappingTimer.Start();
                            KeyMappingGrid.Items.Refresh();
                        }
                    }
                }
                catch (Exception ex001)
                {
                    Diagnostics.WriteLogFile(ex001);
                    Close();
                }
            }
            catch (Exception ex)
            {
                // need this as some error might happen when detected a new device.
                Diagnostics.Log(ex);
            }
        }

        private void BMSChanged()
        {
            try
            {
                statusAssign = Status.GetNeutralPos;

                LargeTab.SelectedIndex = 0;

                // disable the phonebook first so the theater update does not result in phonebook updates
                ServerGrid.ItemsSource = null;
                ServerGrid.DataContext = null;

                // Read Theater List
                TheaterList.PopulateAndSave(appReg, Dropdown_TheaterList);

                appReg.ChangeCfgPath();

                // Read BMS-FULL.key
                ReloadKeyFile();

                // Write Data Grid
                WriteDataGrid();

                // Refresh servers
     
                phoneBookParser = new PhoneBookParser(appReg);
                ServerGrid.ItemsSource = phoneBookParser.ServerConnections;
                ServerGrid.DataContext = phoneBookParser;
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        private void ReloadKeyFile()
        {
            string fname = appReg.GetInstallDir() + "\\User\\Config\\" + appReg.getKeyFileName();
            keyFile = new KeyFile(fname, appReg);
        }

        public void ReloadDevices()
        {
            try
            {
                // Get Devices
                deviceControl = new DeviceControl(appReg);

                neutralButtons = new NeutralButtons[deviceControl.joyAssign.Length];

                // Aquire joySticks
                AquireAll(true);

                ResortDevices();
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        public void ResortDevices()
        {
            try
            {
                // Reset All Axis Settings
                foreach (AxisName nme in axisNameList)
                    inGameAxis[nme.ToString()] = new InGameAxAssgn();

                joyAssign_2_inGameAxis();
                ResetAssgnWindow();
                ResetJoystickColumn();
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        public void RefreshDevices()
        {
            try
            {
                // Reset All Axis Settings
                foreach (AxisName nme in axisNameList)
                    inGameAxis[nme.ToString()] = new InGameAxAssgn();

                joyAssign_2_inGameAxis();
                ResetAssgnWindow();
                RefreshJoystickColumn();
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
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
            Diagnostics.WriteLogFile();

            try
            {
                if (appReg == null)
                    return;

                // Save UI Properties(Like Button Status).
                appProperties.SaveUISetup();
                appReg.getOverrideWriter().SaveKeyMapping(inGameAxis, deviceControl, keyFile);
                appReg.getOverrideWriter().SaveJoyAssignStatus(deviceControl);
            }
            catch (Exception ex)
            {
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
            try
            {
                if (!e.Source.Equals(LargeTab))
                    return;

                int value = LargeTab.SelectedIndex;

                if (value == 1)
                    AxisMovingTimer.Start();
                else
                    AxisMovingTimer.Stop();

                if (value == 2)
                {
                    KeyMappingTimer.Start();
                    KeyMappingGrid.Items.Refresh();
                }
                else
                    KeyMappingTimer.Stop();
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        /// <summary>
        /// Rewrite Theater setting in the registry and Show/Hide Theater own config icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dropdown_TheaterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                appReg.ChangeTheater(Dropdown_TheaterList);
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        /// <summary>
        /// Launch Theater own config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_TheaterConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(appReg.theaterOwnConfig);
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        /// <summary>
        /// When clicked BW button, changes BW value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CMD_BW_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                appProperties.CMD_BW_Click();
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        /// <summary>
        /// Open BMS Docs and Manuals.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Docs");
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
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
                if (!appReg.isNameDefined())
                {
                    if (!CallsignWindow.ShowCallsignWindow(appReg))
                    {
                        appReg.getLauncher().execute(sender, true);
                    }
                }
                else
                {
                    appReg.getLauncher().execute(sender);
                }
            }
            catch (FileNotFoundException ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        /// <summary>
        /// As the name implies.
        /// </summary>
        /// <returns></returns>
        public int getBWValue()
        {
            return appProperties.bandWidthDefault;
        }

        /// <summary>
        /// OverrideSettings.
        /// </summary>
        public void executeOverride()
        {
            try
            {
                // throw new Exception("An exception occurs.");
                if (ApplicationOverride.IsChecked == true)
                {
                    if (Properties.Settings.Default.FirstTimeNonOverride)
                    {
                        string textMessage = "You are going to launch BMS without any setup override from AxisAssign and KeyMapping section.";
                        MessageBox.Show(textMessage, "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);
                        Properties.Settings.Default.FirstTimeNonOverride = false;
                    }
                }
                else
                {
                    appReg.getOverrideWriter().Execute(inGameAxis, deviceControl, keyFile);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }
        public void minimizeWindowUntilProcessEnds(System.Diagnostics.Process process)
        {
            process.Exited += window_Normal;
            process.EnableRaisingEvents = true;
            WindowState = WindowState.Minimized;
        }

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
                                System.Diagnostics.Process.Start(downloadlink);
                                return;
                            }
                        }
                        System.Diagnostics.Process.Start(installexe);
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
                                System.Diagnostics.Process.Start(downloadlink);
                                return;
                            }
                        }
                        System.Diagnostics.Process.Start(installexe);
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
                                System.Diagnostics.Process.Start(downloadlink);
                                return;
                            }
                        }
                        System.Diagnostics.Process.Start(installexe);
                        break;
                    case "Launch_F4WX":
                        target = "\\F4Wx.exe";
                        downloadlink = "https://forum.falcon-bms.com/topic/8267/f4wx-real-weather-converter";
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
                                System.Diagnostics.Process.Start(downloadlink);
                                return;
                            }
                        }
                        System.Diagnostics.Process.Start(installexe);
                        break;
                    case "Launch_F4RADAR":
                        downloadlink = "https://forum.falcon-bms.com/topic/18356/f4radar-lightweight-standalone-radar-application";
                        installexe = Properties.Settings.Default.Third_F4RADAR;
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
                                System.Diagnostics.Process.Start(downloadlink);
                                return;
                            }
                        }
                        System.Diagnostics.Process.Start(installexe);
                        break;
                }
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
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
            try
            {
                string nme = ((Button)sender).Name;

                if (nme.Contains("Launch_"))
                {
                    if (nme.Contains("Launch_TheaterConfig"))
                        return;
                    Button tbButton = FindName(nme) as Button;
                    if (tbButton == null)
                        return;
                    tbButton.BorderBrush = CommonConstants.LIGHTBLUE;
                    tbButton.BorderThickness = new Thickness(1);

                    nme = nme.Replace("Launch_", "");
                    Label tblabel = FindName("Label_" + nme) as Label;
                    if (tblabel == null)
                        return;
                    tblabel.Foreground = CommonConstants.BLUEILUM;
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
            }
        }

        /// <summary>
        /// Reset label color when mouse leaves a launcher icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseLeaveLauncher(object sender, EventArgs e)
        {
            try
            {
                string nme = ((Button)sender).Name;

                if (nme.Contains("Launch_"))
                {
                    if (nme.Contains("Launch_TheaterConfig"))
                        return;
                    Button tbButton = FindName(nme) as Button;
                    if (tbButton == null)
                        return;
                    tbButton.BorderThickness = new Thickness(0);

                    nme = nme.Replace("Launch_", "");
                    Label tblabel = FindName("Label_" + nme) as Label;
                    if (tblabel == null)
                        return;
                    tblabel.Foreground = CommonConstants.WHITEILUM;
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
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
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
            }
        }

        private void Apply_YAME64(object sender, RoutedEventArgs e)
        {
            //appReg.getOverrideWriter().Execute(inGameAxis, deviceControl, keyFile);
            Close();
        }

        private void WDP_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.weapondeliveryplanner.nl/");
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
            }
        }

        private void Serfoss2003_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://apps.dtic.mil/docs/citations/ADA414893");
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
            }
        }

        private void CMD_WINDOW_Click(object sender, RoutedEventArgs e)
        {
            return;
            if (CMD_WINDOW.IsChecked == true)
                MessageBox.Show("FalconBMS crashes when Alt+TAB in FullScreen Mode. Recommend Enabling Window Mode.\n(WINDOW button turning on light)", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool pressedByHand;

        private void Select_PinkyShift_Click(object sender, RoutedEventArgs e)
        {
            if (Select_PinkyShift.IsChecked == false)
                pressedByHand = true;
            else
                pressedByHand = false;
        }

        private void ListBox_BMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!e.Source.Equals(ListBox_BMS))
                    return;
                if (appReg == null)
                    return;

                Properties.Settings.Default.BMS_Version = this.ListBox_BMS.SelectedItem.ToString();
                appReg.Init(this, this.ListBox_BMS.SelectedItem.ToString());

                BMSChanged();
                ReloadDevices();
            }
            catch (Exception ex)
            {
                Diagnostics.WriteLogFile(ex);
                Close();
            }
        }

        private void Misc_VR_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Misc_VR.IsChecked)
                steamVR.Start();
            else
                steamVR.Stop();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Button_Refresh_Servers.IsEnabled = false;
            await phoneBookParser.RefreshAllOnline();
            Button_Refresh_Servers.IsEnabled = true ;
        }


        private void Launch_Multiplayer_Click(object sender, RoutedEventArgs e)
        {
            ServerConnection serverConnection = (ServerConnection)ServerGrid.SelectedItem;
            if (serverConnection != null)
            {
                // check the theater
                string theaterName = serverConnection.TheaterName;

                if (theaterName != "" && !Dropdown_TheaterList.Items.Contains(theaterName))
                {
                    MessageBox.Show(String.Format("The Theater '{0}' could not be found. Please install the Theater and try again.", theaterName), "Theater not found",
                 MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (theaterName != "")
                    {
                        Dropdown_TheaterList.SelectedItem = theaterName;
                    }

                    if (!string.IsNullOrEmpty(serverConnection.IniFileUrl))
                    {
                        DownloadIniFile(serverConnection);
                    }

                    else
                    {
                        Launch_BMS_Large.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                }
            }

        }
        private void ServerGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerGrid.SelectedItem != null)
            {
                Launch_BMS_Multiplayer.IsEnabled = true;
            }
            else
            {
                Launch_BMS_Multiplayer.IsEnabled = false;
            }

        }

        private void ServerGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // VERY UGLY, however inline creating a new row in datagrid does not seem to trigger PropertyUpdated
                phoneBookParser.ForceUpdateConnectionInFile((ServerConnection)e.Row.Item, e.Row.GetIndex());
            }

        }

        internal async Task BringToForeground(string[] args)
        {
            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }

            // According to some sources these steps gurantee that an app will be brought to foreground.
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();


            if (args.Length == 1)
            {
                try
                {
                    Uri uri = new Uri(args[0]);
                    LargeTab.SelectedIndex = 1;
                    phoneBookParser.AddEntry(ServerConnection.From(uri));

                }
                catch (UriFormatException ex)
                {
                    Diagnostics.Log("called with invalid bms URI: " + Environment.GetCommandLineArgs()[1]);
                }
            }
        }

        private async Task DownloadIniFile(ServerConnection serverConnection)
        {
            // filename != ini (security issue)
            if (!serverConnection.IniFileName.ToLower().EndsWith(".ini"))
            {
                MessageBox.Show("The Launcher can only download ini files", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // theater(dir) not found
            string campaignDir = "";
            if (!TheaterList.AvailableTheatersCampaignDirs.TryGetValue(serverConnection.TheaterName, out campaignDir))
            {
                MessageBox.Show("Theater {0} not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // file exists
            string fileName = appReg.GetInstallDir() + "/Data/" + campaignDir + "/" + serverConnection.IniFileName;
            fileName = fileName.Replace("/", "\\");

            if (File.Exists(fileName))
            {
                if (MessageBox.Show(string.Format("The INI file {0} already exists.\nOverwrite it?", fileName), "File exists", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
            }         

            Download_Label.Visibility = Visibility.Visible;
            Download_Label.Content = string.Format("Downloading {0} ...", serverConnection.IniFileName);

            await F4ServerInfoService.DownloadIniFile(serverConnection, fileName);

            Download_Label.Visibility = Visibility.Hidden;
            Launch_BMS_Multiplayer.IsEnabled = true;
            ServerGrid.IsEnabled = true;
            Launch_BMS_Large.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void Server_Grid_Server_Info_Click(object sender, RoutedEventArgs e)
        {
            ServerConnection serverConnection = ((FrameworkElement)sender).DataContext as ServerConnection;

            Uri link = new Uri(serverConnection.ServerInfoUrl);
            Process.Start(link.AbsoluteUri);
        }

    }
}

