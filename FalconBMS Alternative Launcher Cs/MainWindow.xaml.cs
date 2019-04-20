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
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            InitializeComponent();

            this.MouseWheel += Detect_MouseWheel;
        }

        public static DeviceControl deviceControl;

        private AppRegInfo appReg;
        private KeyFile keyFile;
        private VisualAcuity visualAcuity;

        private AppProperties appProperties;

        private DispatcherTimer AxisMovingTimer = new DispatcherTimer();
        private DispatcherTimer KeyMappingTimer = new DispatcherTimer();
        
        public static bool FLG_YAME64 = false;

        /// <summary>
        /// Execute when launching this app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                // load command line.
                string[] args = Environment.GetCommandLineArgs();
                var option = new Dictionary<string, string>();
                for (int index = 1; index < args.Length; index += 2)
                {
                    option.Add(args[index], args[index + 1]);
                }
                if (option.ContainsKey("/yame") == true)
                    if (option["/yame"] == "true")
                        FLG_YAME64 = true;
                if (option.ContainsKey("/visibility") == true)
                {
                    if (option["/visibility"] == "true")
                    {
                        Tab_VisualAcuity.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Tab_VisualAcuity.Visibility = Visibility.Collapsed;
                        Misc_SmartScalingOverride.IsChecked = false;
                    }
                }
                else
                {
                    Tab_VisualAcuity.Visibility = Visibility.Collapsed;
                    Misc_SmartScalingOverride.IsChecked = false;
                }

                if (FLG_YAME64)
                {
                    LargeTab.SelectedIndex = 1;
                    Tab_Launcher.Visibility = Visibility.Collapsed;

                    this.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                    BackGroundBox1.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                    BackGroundBox2.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                    BackGroundImage.Opacity = 0;

                    Button_Apply_YAME64.Visibility = Visibility.Visible;
                }
                else
                {
                    Button_Apply_YAME64.Visibility = Visibility.Hidden;
                }

                // Load UI Properties(Like Button Status).
                this.appProperties = new AppProperties(this);

                // Read Registry
                this.appReg = new AppRegInfo(this);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter("C:\\FBMSAltLauncherErrorLog.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();

                MessageBox.Show("Error Log Saved To C:\\FBMSAltLauncherErrorLog.txt", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }

            try
            {
                // Read Theater List
                TheaterList theaterlist = new TheaterList(appReg, this.Dropdown_TheaterList);

                // Get Devices
                deviceControl = new DeviceControl(appReg);
                this.neutralButtons = new NeutralButtons[deviceControl.devList.Count];

                // Aquire joySticks
                AquireAll(true);

                // Reset All Axis Settings
                foreach (AxisName nme in axisNameList)
                    inGameAxis[nme.ToString()] = new InGameAxAssgn();
                joyAssign_2_inGameAxis();
                ResetAssgnWindow();

                // Read BMS-FULL.key
                string fname = appReg.GetInstallDir() + "\\User\\Config\\" + appReg.getKeyFileName();
                this.keyFile = new KeyFile(fname, appReg);

                // Write Data Grid
                WriteDataGrid();

                // Set Timer
                this.AxisMovingTimer.Tick += this.AxisMovingTimer_Tick;
                this.AxisMovingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                this.KeyMappingTimer.Tick += this.KeyMappingTimer_Tick;
                this.KeyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                // Set VisualAcuity page graph and results.
                this.visualAcuity = new VisualAcuity(this);

                //System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();

                MessageBox.Show("Error Log Saved To " + appReg.GetInstallDir() + "\\Error.txt", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);

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
            try
            {
                // Save UI Properties(Like Button Status).
                this.appProperties.SaveUISetup();
                this.appReg.getOverrideWriter().Execute(inGameAxis, deviceControl, keyFile, visualAcuity);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();

                MessageBox.Show("Error Log Saved To " + appReg.GetInstallDir() + "\\Error.txt", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
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
                this.AxisMovingTimer.Start();
            else
                this.AxisMovingTimer.Stop();

            if (value == 2)
            {
                this.KeyMappingTimer.Start();
                this.KeyMappingGrid.Items.Refresh();
            }
            else
                this.KeyMappingTimer.Stop();
        }
        
        /// <summary>
        /// Rewrite Theater setting in the registry and Show/Hide Theater own config icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dropdown_TheaterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.appReg.ChangeTheater(this.Dropdown_TheaterList);
        }
        
        /// <summary>
        /// Launch Theater own config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_TheaterConfig_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(appReg.theaterOwnConfig);
        }
        
        /// <summary>
        /// When clicked BW button, changes BW value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CMD_BW_Click(object sender, RoutedEventArgs e)
        {
            this.appProperties.CMD_BW_Click();
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
            try
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
                        strCmdText += "-bw " + appProperties.bandWidthDefault;

                        // OVERRIDE SETTINGS.
                        if (this.ApplicationOverride.IsChecked == true)
                        {
                            string textMessage = "You are going to launch BMS without any setup override from AxisAssign and KeyMapping section. Will you continue?";
                            if (MessageBox.Show(textMessage, "WARNING", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel)
                                return;
                        }
                        else
                        {
                            appReg.getOverrideWriter().Execute(inGameAxis, deviceControl, keyFile, visualAcuity);
                        }

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
            catch (System.IO.FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();

                MessageBox.Show("Error Log Saved To " + appReg.GetInstallDir() + "\\Error.txt", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
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
            try
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
            catch (System.IO.FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();

                MessageBox.Show("Error Log Saved To " + appReg.GetInstallDir() + "\\Error.txt", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
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

        /// <summary>
        /// Hey Don't input any charcters on me.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_hFOV_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new System.Text.RegularExpressions.Regex("[0-9]").IsMatch(e.Text);
        }

        /// <summary>
        /// Hey Don't Copy+Paste on me.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_hFOV_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
                e.Handled = true;
        }
        
        private void TextBox_hFOV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
                this.visualAcuity.SeeArbitraryFOVResult();
        }

        private void Apply_YAME64(object sender, RoutedEventArgs e)
        {
            this.appReg.getOverrideWriter().Execute(inGameAxis, deviceControl, keyFile, visualAcuity);
            this.Close();
        }
    }
}
