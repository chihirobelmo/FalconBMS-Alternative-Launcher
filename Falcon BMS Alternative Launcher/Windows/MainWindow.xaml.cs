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
using System.Collections.ObjectModel;

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
                // Load UI Properties(Like Button Status).
                appProperties = new AppProperties(this);

                // Read Registry
                appReg = new AppRegInfo(this);

                FillKeyFileList();
                BMSChanged();
                ReloadDevices();

                Diagnostics.Log("Init Devices.");

                UPDATE_AVAILABLE.Visibility = Visibility.Hidden;
                /*
                if (DownloadWindow.CheckMajorUpdate(ListBox_BMS))
                    UPDATE_AVAILABLE.Visibility = Visibility.Hidden;
                else
                {
                    DownloadWindow.ShowDownloadWindow(this, appReg, ListBox_BMS);

                    appReg = new AppRegInfo(this);

                    BMSChanged();
                    ReloadDevices();
                }
                */

                Diagnostics.Log("Update Checked.");
                
                if (appReg.getBMSVersion() == BMS_Version.UNDEFINED)
                {
                    MessageBox.Show("Could Not Find BMS");
                    Diagnostics.WriteLogFile();
                    Close();
                    return;
                }

                Diagnostics.Log("BMS found.");

                /*
                if (DownloadWindow.CheckMinorUpdate(appReg))
                    UPDATE_AVAILABLE.Visibility = Visibility.Hidden;
                else
                {
                    UPDATE_AVAILABLE.Visibility = Visibility.Visible;
                    DownloadWindow.ShowDownloadWindow(this, appReg, ListBox_BMS);
                }
                */

                if ((bool)Misc_VR.IsVisible)
                    if ((bool)Misc_VR.IsChecked)
                        steamVR.Start();

                Diagnostics.Log("Update Visiblity check.");

                // Set Timer
                AxisMovingTimer.Tick += AxisMovingTimer_Tick;
                AxisMovingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                KeyMappingTimer.Tick += KeyMappingTimer_Tick;
                KeyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 32);

                NewDeviceDetectTimer.Tick += NewDeviceDetectTimer_Tick;
                NewDeviceDetectTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);

                NewDeviceDetectTimer.Start();

                Diagnostics.Log("Timer Started.");
            }
            catch (Exception exclose)
            {
                Diagnostics.WriteLogFile(exclose);
                Close();
                return;
            }
        }

        private void NewDeviceDetectTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Microsoft.DirectX.DirectInput.DeviceList devList =
                    Microsoft.DirectX.DirectInput.Manager.GetDevices(
                        Microsoft.DirectX.DirectInput.DeviceClass.GameControl,
                        Microsoft.DirectX.DirectInput.EnumDevicesFlags.AttachedOnly
                        );

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

                // Read Theater List
                TheaterList.PopulateAndSave(appReg, Dropdown_TheaterList);

                // Read BMS-FULL.key
                ReloadKeyFile();

                // Write Data Grid
                WriteDataGrid();
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
                Torrent.status = false;

                if (appReg == null)
                    return;

                // Save UI Properties(Like Button Status).
                appProperties.SaveUISetup();
                if (ApplicationOverride.IsChecked == false)
                    appReg.getOverrideWriter().Execute(inGameAxis, deviceControl, keyFile);
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
                        appReg.getLauncher().execute(sender, true);
                    return;
                }

                appReg.getLauncher().execute(sender);
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
                    string textMessage = "You are going to launch BMS without any setup override from AxisAssign and KeyMapping section.";
                    MessageBox.Show(textMessage, "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);
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
                string target       = "";
                string downloadlink = "";
                string installexe   = "";

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
                    tbButton.BorderBrush = new SolidColorBrush(Colors.LightBlue);
                    tbButton.BorderThickness = new Thickness(1);

                    nme = nme.Replace("Launch_", "");
                    Label tblabel = FindName("Label_" + nme) as Label;
                    if (tblabel == null)
                        return;
                    tblabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 128, 255, 255));
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
                    tblabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
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
    }
}
