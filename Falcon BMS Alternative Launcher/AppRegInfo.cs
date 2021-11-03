using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher
{
    public class AppRegInfo
    {
        // Member
        private Microsoft.Win32.RegistryKey regkey;
        private string regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.35";
        private Platform platform = Platform.OS_64bit;
        private BMS_Version bms_Version = BMS_Version.BMS435;
        private OverrideSetting overRideSetting;
        private Launcher launcher;

        private string installDir;
        private string currentTheater;
        private string pilotCallsign;

        private MainWindow mainWindow;

        public string keyFileName = "BMS - Full.key";
        public string theaterOwnConfig = "";

        // Method
        public string GetInstallDir() { return installDir; }
        public string GetCurrentTheater() { return currentTheater; }
        public string GetPilotCallsign() { return pilotCallsign; }
        public string getKeyFileName() { return keyFileName; }
        public OverrideSetting getOverrideWriter() { return overRideSetting; }
        public BMS_Version getBMSVersion() { return bms_Version; }
        public Launcher getLauncher() { return launcher; }

        public AppRegInfo(MainWindow mainWindow)
        {
            bool flg = true;
            string version = "DCS World";
            string v = "DCS World";

            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.37 (Internal)", false) != null)
            {
                version = "Falcon BMS 4.37 (Internal)";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.36 (Internal)", false) != null)
            {
                version = "Falcon BMS 4.36 (Internal)";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.37", false) != null)
            {
                version = "Falcon BMS 4.37";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.36", false) != null)
            {
                version = "Falcon BMS 4.36";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.35", false) != null)
            {
                version = "Falcon BMS 4.35";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34", false) != null)
            {
                version = "Falcon BMS 4.34";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33 U1", false) != null)
            {
                version = "Falcon BMS 4.33 U1";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33", false) != null)
            {
                version = "Falcon BMS 4.33";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34", false) != null)
            {
                version = "Falcon BMS 4.32";
                mainWindow.ListBox_BMS.Items.Add(version);
                if (flg)
                {
                    v = version;
                    flg = false;
                }
                if (version == Properties.Settings.Default.BMS_Version)
                {
                    v = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }

            Init(mainWindow, v);
        }

        public void Init(MainWindow mainWindow, string version)
        {
            this.mainWindow = mainWindow;

            switch (version)
            {
                case "Falcon BMS 4.37 (Internal)":
                    bms_Version = BMS_Version.BMS437I;
                    break;
                case "Falcon BMS 4.36 (Internal)":
                    bms_Version = BMS_Version.BMS436I;
                    break;
                case "Falcon BMS 4.37":
                    bms_Version = BMS_Version.BMS437;
                    break;
                case "Falcon BMS 4.36":
                    bms_Version = BMS_Version.BMS436;
                    break;
                case "Falcon BMS 4.35":
                    bms_Version = BMS_Version.BMS435;
                    break;
                case "Falcon BMS 4.34":
                    bms_Version = BMS_Version.BMS434U1;
                    break;
                case "Falcon BMS 4.33 U1":
                    bms_Version = BMS_Version.BMS433U1;
                    break;
                case "Falcon BMS 4.33":
                    bms_Version = BMS_Version.BMS433;
                    break;
                case "Falcon BMS 4.32":
                    bms_Version = BMS_Version.BMS432;
                    break;
                default:
                    bms_Version = BMS_Version.UNDEFINED;
                    break;
            }

            // load command line.
            string[] args = Environment.GetCommandLineArgs();
            Dictionary<string, string> option = new Dictionary<string, string>();
            if (args.Length % 2 == 1)
            {
                for (int index = 1; index < args.Length; index += 2)
                {
                    option.Add(args[index], args[index + 1]);
                }
            }

            // User defined BMS version
            if (option.ContainsKey("/bms"))
            {
                switch (option["/bms"])
                {
                    case "4.32":
                        bms_Version = BMS_Version.BMS432;
                        break;
                    case "4.33":
                        bms_Version = BMS_Version.BMS433;
                        break;
                    case "4.33.1":
                        bms_Version = BMS_Version.BMS433U1;
                        break;
                    case "4.34":
                        bms_Version = BMS_Version.BMS434;
                        break;
                    case "4.34.1":
                        bms_Version = BMS_Version.BMS434U1;
                        break;
                    case "4.35":
                        bms_Version = BMS_Version.BMS435;
                        break;
                    case "4.36":
                        bms_Version = BMS_Version.BMS436I;
                        break;
                    case "4.37":
                        bms_Version = BMS_Version.BMS437I;
                        break;
                    default:
                        bms_Version = BMS_Version.UNDEFINED;
                        break;
                }
            }

            // BMS version
            switch (bms_Version)
            {
                case BMS_Version.BMS432:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.32";
                    keyFileName = "BMS.key";
                    overRideSetting = new OverrideSettingFor432(this.mainWindow, this);
                    launcher = new Launcher432(this, this.mainWindow);
                    //mainWindow.LOGO432.Visibility = Visibility.Visible;
                    break;
                case BMS_Version.BMS433:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    launcher = new Launcher433(this, this.mainWindow);
                    //mainWindow.LOGO433.Visibility = Visibility.Visible;
                    break;
                case BMS_Version.BMS433U1:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33 U1";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    launcher = new Launcher433(this, this.mainWindow);
                    //mainWindow.LOGO433.Visibility = Visibility.Visible;
                    break;
                case BMS_Version.BMS434:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor434(this.mainWindow, this);
                    launcher = new Launcher434(this, this.mainWindow);
                    //mainWindow.LOGO434.Visibility = Visibility.Visible;
                    break;
                case BMS_Version.BMS434U1:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor434U1(this.mainWindow, this);
                    launcher = new Launcher434(this, this.mainWindow);
                    //mainWindow.LOGO434.Visibility = Visibility.Visible;
                    break;
                case BMS_Version.BMS435:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.35";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor435(this.mainWindow, this);
                    launcher = new Launcher435(this, this.mainWindow);
                    //mainWindow.LOGO435.Visibility = Visibility.Visible;
                    break;
                case BMS_Version.BMS436:
                case BMS_Version.BMS436I:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.36 (Internal)";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor436(this.mainWindow, this);
                    launcher = new Launcher436Internal(this, this.mainWindow);
                    //mainWindow.LOGO435.Visibility = Visibility.Visible;
                    break;
                case BMS_Version.BMS437:
                case BMS_Version.BMS437I:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.37 (Internal)";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor437(this.mainWindow, this);
                    launcher = new Launcher437Internal(this, this.mainWindow);
                    //mainWindow.LOGO435.Visibility = Visibility.Visible;
                    break;

                case BMS_Version.UNDEFINED:
                    throw new ArgumentOutOfRangeException(); // Just to be explicit.

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // User defined registry
            if (option.ContainsKey("/reg"))
            {
                regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\" + option["/reg"];
            }

            // User defined key file
            if (option.ContainsKey("/key"))
            {
                keyFileName = option["/key"];
            }

            // Read Registry
            regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, false);
            if (regkey == null)
            {
                regName  = regName.Replace("SOFTWARE\\Wow6432Node\\", "SOFTWARE\\");
                regkey   = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, false);
                platform = Platform.OS_32bit;
                mainWindow.Misc_Platform.IsChecked = false;
                mainWindow.Misc_Platform.IsEnabled = false;
            }
            if (regkey == null)
            {
                MessageBox.Show("Could not find FalconBMS Installed.");
                mainWindow.Close();
                return;
            }

            byte[] bs;
            if (regkey.GetValue("PilotName") == null)
            {
                regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, true);
                bs = new byte[] { 0x4a, 0x6f, 0x65, 0x20, 0x50, 0x69, 0x6c, 0x6f, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                regkey.SetValue("PilotName", bs);
            }
            if (regkey.GetValue("PilotCallsign") == null)
            {
                regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, true);
                bs = new byte[] { 0x56, 0x69, 0x70, 0x65, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                regkey.SetValue("PilotCallsign", bs);
            }
            if (regkey.GetValue("curTheater") == null)
            {
                regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, true);
                regkey.SetValue("curTheater", "Korea KTO");
            }

            installDir     = (string)regkey.GetValue("baseDir");
            currentTheater = (string)regkey.GetValue("curTheater");
            pilotCallsign  = Encoding.UTF8.GetString((byte[])regkey.GetValue("PilotCallsign")).Replace("\0", "");

            regkey.Close();
        }

        public void GetTheater()
        {
            regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, false);
            currentTheater = (string)regkey.GetValue("curTheater");
            regkey.Close();
        }

        /// <summary>
        /// Rewrite Theater setting in the registry.
        /// </summary>
        /// <param name="combobox"></param>
        public void ChangeTheater(ComboBox combobox)
        {
            if (combobox.SelectedIndex == -1)
                return;

            regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, true);
            regkey.SetValue("curTheater", combobox.Items[combobox.SelectedIndex].ToString());
            regkey.Close();

            switch ((string)mainWindow.Dropdown_TheaterList.SelectedItem)
            {
                case "Israel":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On Israel\\Israeli Theater Settings.exe";
                    break;
                case "Ikaros":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On Ikaros\\Ikaros Settings.exe";
                    break;
                default:
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }

    public enum Platform
    {
        OS_32bit,
        OS_64bit
    }

    public enum BMS_Version
    {
        UNDEFINED,
        BMS432,
        BMS433,
        BMS433U1,
        BMS434,
        BMS434U1,
        BMS435,
        BMS436I,
        BMS436,
        BMS437I,
        BMS437
    }
}
