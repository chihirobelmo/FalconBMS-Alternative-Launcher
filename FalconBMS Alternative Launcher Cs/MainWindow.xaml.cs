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
        
        private AppRegInfo appReg;
        public class AppRegInfo
        {
            // Member
            private string installDir;
            private string currentTheater;
            private string pilotCallsign;

            // Method
            public string GetInstallDir() { return this.installDir; }
            public string GetCurrentTheater() { return this.currentTheater; }
            public string GetPilotCallsign() { return this.pilotCallsign; }

            public AppRegInfo(Microsoft.Win32.RegistryKey regkey)
            {
                this.installDir = (string)regkey.GetValue("baseDir");
                this.currentTheater = (string)regkey.GetValue("curTheater");
                this.pilotCallsign = (Encoding.UTF8.GetString((byte[])regkey.GetValue("PilotCallsign"))).Replace("\0", "");
            }

            public void ChangeTheater(ComboBox combobox)
            {
                Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33 U1", true);
                if (regkey == null)
                    regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Benchmark Sims\\Falcon BMS 4.33 U1", true);
                if (regkey == null)
                    return;
                regkey.SetValue("curTheater", combobox.Items[combobox.SelectedIndex].ToString());
                regkey.Close();
            }
        }
        
        public class TheaterList
        {
            /// <summary>
            /// Read theater.lst and apply the list to Combobox.
            /// </summary>
            public TheaterList(AppRegInfo appReg, ComboBox Combo)
            {
                String filename = appReg.GetInstallDir() + "/Data/Terrdata/theaterdefinition/theater.lst";
                if (File.Exists(filename) == false)
                    return;
                string[] definitionfile = File.ReadAllLines(filename, Encoding.UTF8);
                
                var list = new List<string>();
                foreach (string tdf in definitionfile)
                {
                    if (File.Exists(appReg.GetInstallDir() + "\\Data\\" +  tdf) == false)
                        continue;
                    string[] line = File.ReadAllLines(appReg.GetInstallDir() + "\\Data\\" + tdf, Encoding.UTF8);
                    string theatername = "";
                    foreach (string str in line)
                    {
                        if (!str.Contains("name "))
                            continue;
                        theatername = str.Replace("name ", "").Trim();
                        break;
                    }
                    list.Add(theatername);
                }
                for (int ii = 0; ii < list.Count; ii++)
                {
                    Combo.Items.Add(list[ii]);
                    if (list[ii] == appReg.GetCurrentTheater())
                        Combo.SelectedIndex = ii;
                }
            }
        }
        
        public static DeviceList devList;
        public static Device[] joyStick;

        public class GetDevice
        {
            /// <summary>
            /// Get Devices.
            /// </summary>
            public GetDevice(AppRegInfo appReg)
            {
                devList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
                joyStick = new Device[devList.Count];
                joyAssign = new JoyAssgn[devList.Count];
                
                System.Xml.Serialization.XmlSerializer serializer;
                System.IO.StreamReader sr;
                string fileName = "";
                int i = 0;

                foreach (DeviceInstance dev in devList)
                {
                    joyStick[i] = new Device(dev.InstanceGuid);
                    joyAssign[i] = new JoyAssgn();

                    joyAssign[i].SetDeviceInstance(dev);

                    fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + joyAssign[i].GetProductName().Replace("/", "-")
                    + " {" + joyAssign[i].GetInstanceGUID().ToString().ToUpper() + "}.xml";
                    
                    if (File.Exists(fileName))
                    {
                        serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn));
                        sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                        joyAssign[i] = (JoyAssgn)serializer.Deserialize(sr);
                        sr.Close();
                    }
                    joyAssign[i].SetDeviceInstance(dev);
                    i += 1;
                }

                serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn.AxAssgn));
                fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.Mousewheel.xml";
                if (File.Exists(fileName))
                {
                    sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                    mouseWheelAssign = (JoyAssgn.AxAssgn)serializer.Deserialize(sr);
                    sr.Close();
                }

                serializer = new System.Xml.Serialization.XmlSerializer(typeof(ThrottlePosition));
                fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.throttlePosition.xml";
                if (File.Exists(fileName))
                {
                    sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                    throttlePos = (ThrottlePosition)serializer.Deserialize(sr);
                    sr.Close();
                }
            }
        }

        public static int JoyAxisState(int joyNumber, int joyAxisNumber)
        {
            int input = 0;
            switch(joyAxisNumber)
            {
                case 0:
                    input = joyStick[joyNumber].CurrentJoystickState.X;
                    break;
                case 1:
                    input = joyStick[joyNumber].CurrentJoystickState.Y;
                    break;
                case 2:
                    input = joyStick[joyNumber].CurrentJoystickState.Z;
                    break;
                case 3:
                    input = joyStick[joyNumber].CurrentJoystickState.Rx;
                    break;
                case 4:
                    input = joyStick[joyNumber].CurrentJoystickState.Ry;
                    break;
                case 5:
                    input = joyStick[joyNumber].CurrentJoystickState.Rz;
                    break;
                case 6:
                    input = joyStick[joyNumber].CurrentJoystickState.GetSlider()[0];
                    break;
                case 7:
                    input = joyStick[joyNumber].CurrentJoystickState.GetSlider()[1];
                    break;
            }
            return input;
        }

        private DispatcherTimer AxisMovingTimer = new DispatcherTimer();
        private DispatcherTimer KeyMappingTimer = new DispatcherTimer();



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Read Registry
            Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33 U1", false);
            if (regkey == null)
                regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Benchmark Sims\\Falcon BMS 4.33 U1", false);
            if (regkey == null)
            {
                System.Windows.MessageBox.Show("There is no FalconBMS 4.33 U1 Installed.");
                this.Close();
                return;
            }
            appReg = new AppRegInfo(regkey);
            regkey.Close();

            try
            {
                // Read Theater List
                TheaterList theaterlist = new TheaterList(appReg, this.Dropdown_TheaterList);

                // Get Devices
                GetDevice getDevice = new GetDevice(appReg);
                neutralButtons = new NeutralButtons[devList.Count];

                // Aquire joySticks
                AquireAll(true);

                // Reset All Axis Settings
                foreach (String nme in axisNameList)
                    inGameAxis[nme] = new InGameAxAssgn();
                joyAssign_2_inGameAxis();
                ResetAssgnWindow();

                // Read BMS-FULL.key
                string fname = appReg.GetInstallDir() + "\\User\\Config\\BMS - Full.key";
                ReadKeyFile(fname);

                // Write Data Grid
                WriteDataGrid();

                // Set Timer
                AxisMovingTimer.Tick += AxisMovingTimer_Tick;
                AxisMovingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                KeyMappingTimer.Tick += KeyMappingTimer_Tick;
                KeyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);

                // Load Buttons
                this.Misc_Platform.IsChecked = Properties.Settings.Default.Platform;
                this.CMD_ACMI.IsChecked = Properties.Settings.Default.CMD_ACMI;
                this.CMD_WINDOW.IsChecked = Properties.Settings.Default.CMD_WINDOW;
                this.CMD_NOMOVIE.IsChecked = Properties.Settings.Default.CMD_NOMOVIE;
                this.CMD_EF.IsChecked = Properties.Settings.Default.CMD_EF;
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
                CMD_BW.Content = "BANDWIDTH : " + bandWidthDefault.ToString();
                AB_Throttle.Visibility = Visibility.Hidden;
                AB_Throttle_Right.Visibility = Visibility.Hidden;

                //System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();
            }
        }
        
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Platform = (bool)this.Misc_Platform.IsChecked;
            Properties.Settings.Default.CMD_ACMI = (bool)this.CMD_ACMI.IsChecked;
            Properties.Settings.Default.CMD_WINDOW = (bool)this.CMD_WINDOW.IsChecked;
            Properties.Settings.Default.CMD_NOMOVIE = (bool)this.CMD_NOMOVIE.IsChecked;
            Properties.Settings.Default.CMD_EF = (bool)this.CMD_EF.IsChecked;
            Properties.Settings.Default.CMD_BW = this.bandWidthDefault;
            Properties.Settings.Default.NoOverride = (bool)this.ApplicationOverride.IsChecked;
            Properties.Settings.Default.Misc_RLNWS = (bool)this.Misc_RollLinkedNWS.IsChecked;
            Properties.Settings.Default.Misc_MouseCursorAnchor = (bool)this.Misc_MouseCursorAnchor.IsChecked;
            Properties.Settings.Default.Misc_TrackIRZ = (bool)this.Misc_TrackIRZ.IsChecked;
            Properties.Settings.Default.Misc_ExMouseLook = (bool)this.Misc_ExMouseLook.IsChecked;
            Properties.Settings.Default.Misc_OverrideSelfCancel = (bool)this.Misc_OverrideSelfCancel.IsChecked;
            Properties.Settings.Default.Save();

            SaveJoyAssignStatus();
        }





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





        private void Dropdown_TheaterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appReg.ChangeTheater(this.Dropdown_TheaterList);
        }




        
        private int bandWidthDefault = 1024;

        private void CMD_BW_Click(object sender, RoutedEventArgs e)
        {
            bandWidthDefault *= 2;
            if (bandWidthDefault > 10000)
                bandWidthDefault = 512;
            CMD_BW.Content = "BANDWIDTH : " + bandWidthDefault.ToString();
        }

        private void OpenDocs_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Docs");
        }

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
                    strCmdText += "-bw " + bandWidthDefault;

                    if (this.ApplicationOverride.IsChecked == false)
                    {
                        if (!System.IO.Directory.Exists(appReg.GetInstallDir() + "/User/Config/Backup/"))
                            System.IO.Directory.CreateDirectory(appReg.GetInstallDir() + "/User/Config/Backup/");

                        SaveAxisMapping();
                        SaveJoystickCal();
                        SaveDeviceSorting();
                        SaveConfigfile();
                        SaveKeyMapping();

                        SaveJoyAssignStatus();
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

        private void window_Normal(object sender, System.EventArgs e)
        {
            this.Invoke(new System.Action(() => { this.WindowState = WindowState.Normal; }));
        }

        private void Launch_Third(object sender, RoutedEventArgs e)
        {
            string target = "";
            string downloadlink = "";
            string installexe = "";
            switch (((System.Windows.Controls.Button)sender).Name)
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
            }
        }

        private void MouseEnterLauncher(object sender, EventArgs e)
        {
            string nme = ((System.Windows.Controls.Button)sender).Name;

            if (nme.Contains("Launch_"))
            {
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

        private void MouseLeaveLauncher(object sender, EventArgs e)
        {
            string nme = ((System.Windows.Controls.Button)sender).Name;

            if (nme.Contains("Launch_"))
            {
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





        private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
