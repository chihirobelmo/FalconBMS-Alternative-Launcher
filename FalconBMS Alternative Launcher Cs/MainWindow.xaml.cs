using MahApps.Metro.Controls;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FalconBMS_Alternative_Launcher_Cs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            this.MouseWheel += Detect_MouseWheel;
        }
        
        /// <summary>
        /// This is BMS installation information.
        /// </summary>
        private AppRegInfo appReg;

        /// <summary>
        /// This is whole device connected.
        /// </summary>
        public static GetDevice getDevice = new GetDevice();
        /// <summary>
        /// This is BMS - FULL.key file
        /// </summary>
        private KeyFile keyFile;

        private int bandWidthDefault = 1024;
        public string theaterOwnConfig = "";

        private DispatcherTimer AxisMovingTimer = new DispatcherTimer();
        private DispatcherTimer KeyMappingTimer = new DispatcherTimer();
        
        /// <summary>
        /// Execute when launching this app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load UI settings
                SetUIDefault();
                void SetUIDefault()
                {
                    // Load Buttons
                    this.Misc_Platform.IsChecked = Properties.Settings.Default.Platform;
                    this.CMD_ACMI.IsChecked = Properties.Settings.Default.CMD_ACMI;
                    this.CMD_WINDOW.IsChecked = Properties.Settings.Default.CMD_WINDOW;
                    this.CMD_NOMOVIE.IsChecked = Properties.Settings.Default.CMD_NOMOVIE;
                    this.CMD_EF.IsChecked = Properties.Settings.Default.CMD_EF;
                    this.CMD_MONO.IsChecked = Properties.Settings.Default.CMD_MONO;
                    this.bandWidthDefault = Properties.Settings.Default.CMD_BW;
                    this.ApplicationOverride.IsChecked = Properties.Settings.Default.NoOverride;
                    this.Misc_RollLinkedNWS.IsChecked = Properties.Settings.Default.Misc_RLNWS;
                    this.Misc_MouseCursorAnchor.IsChecked = Properties.Settings.Default.Misc_MouseCursorAnchor;
                    this.Misc_TrackIRZ.IsChecked = Properties.Settings.Default.Misc_TrackIRZ;
                    this.Misc_ExMouseLook.IsChecked = Properties.Settings.Default.Misc_ExMouseLook;
                    this.Misc_OverrideSelfCancel.IsChecked = Properties.Settings.Default.Misc_OverrideSelfCancel;

                    // Button Status Default
                    Select_DX_Release.IsChecked = true;
                    Select_PinkyShift.IsChecked = true;
                    CMD_BW.Content = "BW : " + bandWidthDefault.ToString();
                    AB_Throttle.Visibility = Visibility.Hidden;
                    AB_Throttle_Right.Visibility = Visibility.Hidden;
                }

                // Read Registry
                appReg = new AppRegInfo(this);

                // Read Theater List
                TheaterList theaterlist = new TheaterList(appReg, this.Dropdown_TheaterList);

                // Get Devices
                getDevice = new GetDevice(appReg);
                neutralButtons = new NeutralButtons[getDevice.devList.Count];

                // Aquire joySticks
                AquireAll(true);

                // Reset All Axis Settings
                foreach (AxisName nme in axisNameList)
                    inGameAxis[nme.ToString()] = new InGameAxAssgn();
                joyAssign_2_inGameAxis();
                ResetAssgnWindow();

                // Read BMS-FULL.key
                string fname = appReg.GetInstallDir() + "\\User\\Config\\BMS - Full.key";
                keyFile = new KeyFile(fname, appReg);

                // Write Data Grid
                WriteDataGrid();

                // Set Timer
                AxisMovingTimer.Tick += AxisMovingTimer_Tick;
                AxisMovingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                KeyMappingTimer.Tick += KeyMappingTimer_Tick;
                KeyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                //System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();

                this.Close();
            }
        }
        
        /// <summary>
        /// Execute when quiting this app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Platform = (bool)this.Misc_Platform.IsChecked;
            Properties.Settings.Default.CMD_ACMI = (bool)this.CMD_ACMI.IsChecked;
            Properties.Settings.Default.CMD_WINDOW = (bool)this.CMD_WINDOW.IsChecked;
            Properties.Settings.Default.CMD_NOMOVIE = (bool)this.CMD_NOMOVIE.IsChecked;
            Properties.Settings.Default.CMD_EF = (bool)this.CMD_EF.IsChecked;
            Properties.Settings.Default.CMD_MONO = (bool)this.CMD_MONO.IsChecked;
            Properties.Settings.Default.CMD_BW = this.bandWidthDefault;
            Properties.Settings.Default.NoOverride = (bool)this.ApplicationOverride.IsChecked;
            Properties.Settings.Default.Misc_RLNWS = (bool)this.Misc_RollLinkedNWS.IsChecked;
            Properties.Settings.Default.Misc_MouseCursorAnchor = (bool)this.Misc_MouseCursorAnchor.IsChecked;
            Properties.Settings.Default.Misc_TrackIRZ = (bool)this.Misc_TrackIRZ.IsChecked;
            Properties.Settings.Default.Misc_ExMouseLook = (bool)this.Misc_ExMouseLook.IsChecked;
            Properties.Settings.Default.Misc_OverrideSelfCancel = (bool)this.Misc_OverrideSelfCancel.IsChecked;
            Properties.Settings.Default.Save();

            new OverrideSetting(this, appReg, inGameAxis, getDevice, keyFile);
        }
        
        /// <summary>
        /// Execute/Stop timer event when changing top TAB menu (Launcher/AxisAssign/KeyMapping).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.Source.Equals(this.LargeTab))
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
        
        /// <summary>
        /// Rewrite Theater setting in the registry and Show/Hide Theater own config icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dropdown_TheaterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appReg.ChangeTheater(this.Dropdown_TheaterList);

            switch ((String)Dropdown_TheaterList.SelectedItem)
            {
                case "Israel":
                    Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = appReg.GetInstallDir() + "\\Data\\Add-On Israel\\Israeli Theater Settings.exe";
                    break;
                case "Ikaros":
                    Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = appReg.GetInstallDir() + "\\Data\\Add-On Ikaros\\Ikaros Settings.exe";
                    break;
                default:
                    Launch_TheaterConfig.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        
        /// <summary>
        /// Launch Theater own config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_TheaterConfig_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(theaterOwnConfig);
        }
        
        /// <summary>
        /// When clicked BW button, changes BW value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CMD_BW_Click(object sender, RoutedEventArgs e)
        {
            bandWidthDefault *= 2;
            if (bandWidthDefault > 10000)
                bandWidthDefault = 512;
            CMD_BW.Content = "BW : " + bandWidthDefault.ToString();
        }

        /// <summary>
        /// Open BMS Docs and Manuals.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenDocs_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Docs");
        }

        /// <summary>
        ///  Launch BMS utilities.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = "";
                    if (CMD_ACMI.IsChecked == false)
                        strCmdText += "-acmi ";
                    if (CMD_WINDOW.IsChecked == false)
                        strCmdText += "-window ";
                    if (CMD_NOMOVIE.IsChecked == false)
                        strCmdText += "-nomovie ";
                    if (CMD_EF.IsChecked == false)
                        strCmdText += "-ef ";
                    if (CMD_MONO.IsChecked == false)
                        strCmdText += "-mono ";
                    strCmdText += "-bw " + bandWidthDefault;

                    if (this.ApplicationOverride.IsChecked == true)
                    {
                        if (MessageBox.Show("You are going to launch BMS without any setup override from AxisAssign and KeyMapping section. Will you continue?", "WARNING", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel)
                            return;
                    }

                    // OVERRIDE SETTINGS.
                    if (this.ApplicationOverride.IsChecked == false)
                    {
                        new OverrideSetting(this, appReg, inGameAxis, getDevice, keyFile);
                    }


                    appReg.ChangeTheater(this.Dropdown_TheaterList);

                    String appPlatform = appReg.GetInstallDir() + "/Bin/x86/Falcon BMS.exe";
                    if (this.Misc_Platform.IsChecked == true)
                        appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    if (File.Exists(appPlatform) == false)
                    {
                        this.Misc_Platform.IsChecked = false;
                        appPlatform = appReg.GetInstallDir() + "/Bin/x86/Falcon BMS.exe";
                        return;
                    }
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    this.WindowState = WindowState.Minimized;
                    process.Exited += new EventHandler(window_Normal);
                    process.EnableRaisingEvents = true;
                    this.WindowState = WindowState.Minimized;
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    process.Exited += new EventHandler(window_Normal);
                    process.EnableRaisingEvents = true;
                    this.WindowState = WindowState.Minimized;
                    break;
                case "Launch_DISX":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/Display Extraction.exe");
                    break;
                case "Launch_IVCC":
                    System.IO.Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x86/IVC/");
                    System.Diagnostics.Process.Start("IVC Client.exe");
                    break;
                case "Launch_IVCS":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/IVC/IVC Server.exe");
                    break;
                case "Launch_AVC":
                    System.IO.Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x86/");
                    System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/Editor.exe");
                    break;
            }
        }

        /// <summary>
        /// Something I need to launch BMS.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void window_Normal(object sender, System.EventArgs e)
        {
            this.Invoke(new System.Action(() => { this.WindowState = WindowState.Normal; }));
        }

        /// <summary>
        /// Launch third party utilities.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_Third(object sender, RoutedEventArgs e)
        {
            string target = "";
            string downloadlink = "";
            string installexe = "";

            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_WDP":
                    target = @"C:\Weapon Delivery Planner\WeaponDeliveryPlanner.exe";
                    if (File.Exists(target) == true)
                    {
                        System.Diagnostics.Process.Start(target);
                        break;
                    }
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
                    downloadlink = "https://www.bmsforum.org/forum/showthread.php?29203";
                    installexe = Properties.Settings.Default.Third_F4WX + target;
                    if (File.Exists(installexe) == false)
                    {
                        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                        fbd.Description = "Select Install Directory";
                        fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                        fbd.ShowNewFolderButton = false;
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
                case "Launch_F4AWACS":
                    target = "\\F4AWACS.exe";
                    downloadlink = "http://sakgiok.gr/";
                    installexe = Properties.Settings.Default.Third_F4AWACS + target;
                    if (File.Exists(installexe) == false)
                    {
                        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                        fbd.Description = "Select Install Directory";
                        fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                        fbd.ShowNewFolderButton = false;
                        System.Windows.Forms.DialogResult dirResult = fbd.ShowDialog();

                        installexe = fbd.SelectedPath + target;
                        if (File.Exists(installexe))
                            Properties.Settings.Default.Third_F4AWACS = fbd.SelectedPath;
                        else
                        {
                            System.Diagnostics.Process.Start(downloadlink);
                            return;
                        }
                    }
                    System.Diagnostics.Process.Start(installexe);
                    break;
                case "Launch_TIR":
                    target = @"C:\Program Files (x86)\NaturalPoint\TrackIR5\TrackIR5.exe";
                    if (File.Exists(target) == true)
                    {
                        System.Diagnostics.Process.Start(target);
                        break;
                    }
                    target = "\\TrackIR5.exe";
                    downloadlink = "https://www.naturalpoint.com/trackir/";
                    installexe = Properties.Settings.Default.Third_TIR + target;
                    if (File.Exists(installexe) == false)
                    {
                        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                        fbd.Description = "Select Install Directory";
                        fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                        fbd.ShowNewFolderButton = false;
                        System.Windows.Forms.DialogResult dirResult = fbd.ShowDialog();

                        installexe = fbd.SelectedPath + target;
                        if (File.Exists(installexe))
                            Properties.Settings.Default.Third_TIR = fbd.SelectedPath;
                        else
                        {
                            System.Diagnostics.Process.Start(downloadlink);
                            return;
                        }
                    }
                    System.Diagnostics.Process.Start(installexe);
                    break;
                case "Launch_VA":
                    target = @"C:\Program Files (x86)\VoiceAttack\VoiceAttack.exe";
                    if (File.Exists(target) == true)
                    {
                        System.Diagnostics.Process.Start(target);
                        break;
                    }
                    target = @"C:\Program Files (x86)\Steam\steamapps\common\VoiceAttack\VoiceAttack.exe";
                    if (File.Exists(target) == true)
                    {
                        System.Diagnostics.Process.Start(target);
                        break;
                    }
                    target = "\\VoiceAttack.exe";
                    downloadlink = "https://voiceattack.com/";
                    installexe = Properties.Settings.Default.Third_VA + target;
                    if (File.Exists(installexe) == false)
                    {
                        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

                        fbd.Description = "Select Install Directory";
                        fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                        fbd.ShowNewFolderButton = false;
                        System.Windows.Forms.DialogResult dirResult = fbd.ShowDialog();

                        installexe = fbd.SelectedPath + target;
                        if (File.Exists(installexe))
                            Properties.Settings.Default.Third_VA = fbd.SelectedPath;
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

        /// <summary>
        /// Change label color when mouse enters one of launcher icons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseEnterLauncher(object sender, EventArgs e)
        {
            string nme = ((System.Windows.Controls.Button)sender).Name;

            if (nme.Contains("Launch_"))
            {
                if (nme.Contains("Launch_TheaterConfig"))
                    return;
                System.Windows.Controls.Button tbButton = this.FindName(nme) as System.Windows.Controls.Button;
                if (tbButton == null)
                    return;
                tbButton.BorderBrush = new SolidColorBrush(Colors.LightBlue);
                tbButton.BorderThickness = new Thickness(1);

                nme = nme.Replace("Launch_", "");
                Label tblabel = this.FindName("Label_" + nme) as Label;
                if (tblabel == null)
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
            string nme = ((System.Windows.Controls.Button)sender).Name;

            if (nme.Contains("Launch_"))
            {
                if (nme.Contains("Launch_TheaterConfig"))
                    return;
                System.Windows.Controls.Button tbButton = this.FindName(nme) as System.Windows.Controls.Button;
                if (tbButton == null)
                    return;
                tbButton.BorderThickness = new Thickness(0);

                nme = nme.Replace("Launch_", "");
                Label tblabel = this.FindName("Label_" + nme) as Label;
                if (tblabel == null)
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
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
