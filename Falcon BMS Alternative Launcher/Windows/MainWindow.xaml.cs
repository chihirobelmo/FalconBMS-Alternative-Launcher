using System;
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
using System.Linq;

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
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
            }
        }

        public static DeviceControl deviceControl;

        public AppRegInfo appReg;

        private AppProperties appProperties;

        internal static bool bmsHasBeenLaunched = false;

        private DispatcherTimer AxisMovingTimer = new DispatcherTimer();
        private DispatcherTimer KeyMappingTimer = new DispatcherTimer();
        private DispatcherTimer NewDeviceDetectTimer = new DispatcherTimer();

        protected override void OnInitialized(EventArgs e)
        {
            // Ensure base window object is fully initialized, before proceeding.
            base.OnInitialized(e);

            // Site ourselves as the app's main window.. this is a bit of a codesmell but necessary for more determinstic use of MessageBox and other dialogs.
            Program.mainWin = this;

            Diagnostics.Log("Post_OnInitialized.");

            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                System.Version ver = asm.GetName().Version;

                // NB: the AutoUpdaterDotNET library will create its own background (UI) thread, to do network I/O and display UI if necessary.
                AutoUpdater.Mandatory = true;
                AutoUpdater.Synchronous = false;
                AutoUpdater.Start("https://raw.githubusercontent.com/chihirobelmo/FalconBMS-Alternative-Launcher/master/Falcon%20BMS%20Alternative%20Launcher/AutoUpdate.xml", asm);
                
                Diagnostics.Log("AutoUpdate-check initiated.");

                string BMS_Launcher_version = "FalconBMS Launcher v" + ver.Major + "." + ver.Minor + "." + ver.Build + "." + ver.Revision;
                AL_Version_Number.Content = BMS_Launcher_version;

                Diagnostics.Log(BMS_Launcher_version);

                System.Threading.ThreadPool.QueueUserWorkItem(_ThreadPool_UpdateRss, this.Dispatcher);
            }
            catch (Exception expass)
            {
                Diagnostics.Log(expass);
            }

            try
            {
                appProperties = new AppProperties(this);
                appReg = new AppRegInfo(this);
                InitDevices();

                if (appReg.getBMSVersion() == BMS_Version.UNDEFINED)
                {
                    Diagnostics.Log("Failed to find BMS installation.");
                    Diagnostics.ShowErrorMsgbox("Could Not Find BMS!");
                    Close();
                    return;
                }

                StartVR();
                StartTimers();
            }
            catch (Exception exclose)
            {
                Diagnostics.Log(exclose);
                Diagnostics.ShowErrorMsgbox(exclose);
                Close();
                return;
            }
            Diagnostics.Log("Post_OnInitialized complete.");
        }

        private void _ThreadPool_UpdateRss(object state)
        {
            //NB: We are on a background threadpool thread -- no interaction with UI elements allowed!
            try
            {
                Dispatcher thisDispatcher = (Dispatcher)state;

                RSSReader.Read("https://www.falcon-bms.com/news/feed/", "https://www.falcon-bms.com");
                RSSReader.Read("https://www.falcon-lounge.com/news/feed/", "https://www.falcon-lounge.com");

                Diagnostics.Log("Completed RSS fetch on background-thread.");

                // Schedule remaining work via PostMessage, back on the UI-thread's message queue.
                thisDispatcher.BeginInvoke((Action)delegate { _Post_UpdateRss(); });
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                return;
            }
        }

        private void _Post_UpdateRss()
        {
            RSSReader.Write(News);
            Diagnostics.Log("RSS update finished.");
        }

        private void StartTimers()
        {
            Diagnostics.Log("Start Timers.");

            // Set Timer
            AxisMovingTimer.Tick += AxisMovingTimer_Tick;
            AxisMovingTimer.Interval = TimeSpan.FromMilliseconds(30);

            KeyMappingTimer.Tick += KeyMappingTimer_Tick;
            KeyMappingTimer.Interval = TimeSpan.FromMilliseconds(30);

            NewDeviceDetectTimer.Tick += NewDeviceDetectTimer_Tick;
            NewDeviceDetectTimer.Interval = TimeSpan.FromSeconds(3);

            NewDeviceDetectTimer.Start();

            Diagnostics.Log("Timers Started.");
        }

        private void StartVR()
        {
            Diagnostics.Log("Start VR Check.");

            if ((bool)Misc_VR.IsVisible)
                if ((bool)Misc_VR.IsChecked)
                    steamVR.Start();

            Diagnostics.Log("Finished VR Check.");
        }

        private void InitDevices()
        {
            Diagnostics.Log("Start Init Devices.");

            ReloadDevicesAndXmlMappings();
            ReloadKeyfilesTheatersAndUpdateUI();

            Diagnostics.Log("Finished Init Devices.");
        }

        private void NewDeviceDetectTimer_Tick(object sender, EventArgs e)
        {
            Microsoft.DirectX.DirectInput.DeviceList devList = null;
            try
            {
                devList = Microsoft.DirectX.DirectInput.Manager.GetDevices(
                        Microsoft.DirectX.DirectInput.DeviceClass.GameControl,
                        Microsoft.DirectX.DirectInput.EnumDevicesFlags.AttachedOnly
                        );
            }
            catch (Exception ex)
            {
                // need this as sometimes error might happen when detected a new device.
                Diagnostics.Log(ex);
                return;
            }

            try
            {
                if (deviceControl.GetHwDeviceList().Length != devList.Count)
                {
                    AxisMovingTimer.Stop();
                    KeyMappingTimer.Stop();

                    ReloadDevicesAndXmlMappings();

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
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
                Close();
            }

            return;
        }

        private void ReloadKeyfilesTheatersAndUpdateUI()
        {
            try
            {
                statusAssign = Status.GetNeutralPos;

                LargeTab.SelectedIndex = 0;

                // Read Theater List
                TheaterList.PopulateAndSave(appReg, Dropdown_TheaterList);

                appReg.ChangeCfgPath();

                // Read BMS-FULL.key file(s)
                deviceControl.LoadKeyBindingsFromUserOrStockKeyfiles(appReg);

                // Write Data Grid
                WriteDataGrid();

                // Update category headers.
                UpdateCategoryHeaders();
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
                Close();
            }
        }

        public void ReloadDevicesAndXmlMappings()
        {
            try
            {
                // Get Devices //REVIEW: doesn't this throw away unsaved changes / risk data loss?
                deviceControl = DeviceControl.EnumerateAttachedDevicesAndLoadXml(appReg);

                neutralButtons = new NeutralButtons[deviceControl.GetJoystickMappingsForButtonsAndHats().Length];

                // Aquire joySticks
                AquireAll();

                ResortDevices();
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                appProperties.SaveUISetup();

                // Save axes, buttons and hats, and key mapping.
                if (deviceControl == null)
                    return;

                deviceControl.SaveXml();

                //HACK: This is necessary to avoid the double-write race condition (viz. overwriting the keyfile twice 
                //in quick succession, while BMS process is starting up and reading it).  We don't have a single
                //"Document" class to encapsulate a conventional "dirty" flag, to know when we need to save user's
                //work.  So for now, this ugly hackery.
                if (bmsHasBeenLaunched == false)
                    appReg.getOverrideWriter().SaveKeyMapping(inGameAxis, deviceControl);
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                return;
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
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                if (appReg.IsUniqueNameDefined() == false)
                {
                    CallsignWindow.ShowCallsignWindow(appReg);

                    if (appReg.IsUniqueNameDefined() == false)
                        return;
                }

                appReg.getLauncher().execute(sender);
            }
            catch (FileNotFoundException ex)
            {
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                        string textMessage = "You are about to launch BMS without applying setup-overrides from AxisAssign and KeyMapping section.";
                        MessageBoxResult mbr = MessageBox.Show(Program.mainWin, textMessage, "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (mbr != MessageBoxResult.Yes) return;

                        Properties.Settings.Default.FirstTimeNonOverride = false;
                    }
                }
                else
                {
                    appReg.getOverrideWriter().Execute(inGameAxis, deviceControl);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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
                MessageBox.Show(Program.mainWin, "Falcon BMS crashes when using Alt+Tab in FullScreen Mode. Recommend Enabling Window Mode.\n(WINDOW button turning on light)", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);
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

                if (this.ListBox_BMS.SelectedIndex < 0)
                    return;

                string newVersion = this.ListBox_BMS.SelectedItem.ToString();
                Properties.Settings.Default.BMS_Version = newVersion;

                // Don't lose user's recent changes!
                if (deviceControl != null)
                {
                    deviceControl.SaveXml();
                    appReg.getOverrideWriter().SaveKeyMapping(inGameAxis, deviceControl);
                }

                appReg.UpdateSelectedBMSVersion(newVersion);

                ReloadDevicesAndXmlMappings();
                ReloadKeyfilesTheatersAndUpdateUI();
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
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

        private void ImportKeyfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mbr = MessageBox.Show(this, 
                "WARNING -- selecting a new key file will erase and replace all keyboard " +
                "bindings, in the currently selected profile.\r\n\r\nProceed with caution!", 
                "Import Key File - WARNING", 
                MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (mbr != MessageBoxResult.OK) return;

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.InitialDirectory = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDERBACKSLASH;
            ofd.Filter = "Key files (*.key)|*.key|All files (*.*)|*.*";

            bool? ans2 = ofd.ShowDialog(this);
            if (ans2 != true) return;

            string newKeyfilePath = ofd.FileName;

            if (false == File.Exists(newKeyfilePath))
            {
                MessageBox.Show(this, "File not found: "+newKeyfilePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (false == KeyFile.ValidateKeyfileLines(newKeyfilePath))
            {
                MessageBox.Show(this,
                    "Key file contains one or more incorrectly formed lines -- please see error log at \n\n" +
                    "\"%LocalAppData%\\Benchmark_Sims\\Launcher_Log.txt\" \n\n" +
                    "for a complete list.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            deviceControl.ImportKeyfileIntoCurrentProfile(newKeyfilePath);
            UpdateCategoryHeaders();
            WriteDataGrid();
            return;
        }

    }
}
