using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class AppRegInfo
    {
        // Member
        private Microsoft.Win32.RegistryKey regkey;
        private string regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34";
        private Platform platform = Platform.OS_64bit;
        private BMS_Version bms_Version = BMS_Version.BMS434;
        private OverrideSetting overRideSetting;

        private string installDir;
        private string currentTheater;
        private string pilotCallsign;

        private MainWindow mainWindow;

        public string keyFileName = "BMS - Full.key";
        public string theaterOwnConfig = "";

        // Method
        public string GetInstallDir() { return this.installDir; }
        public string GetCurrentTheater() { return this.currentTheater; }
        public string GetPilotCallsign() { return this.pilotCallsign; }
        public string getKeyFileName() { return this.keyFileName; }
        public OverrideSetting getOverrideWriter() { return this.overRideSetting; }
        public BMS_Version getBMSVersion() { return this.bms_Version; }

        public AppRegInfo(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // Read Current Directry
            int versionNum = 0;
            int updateNum = 0;
            string exeName = System.IO.Directory.GetCurrentDirectory() + "/Falcon BMS.exe";
            if (!System.IO.File.Exists(exeName))
                exeName = System.IO.Directory.GetCurrentDirectory() + "/../x64/Falcon BMS.exe";
            if (System.IO.File.Exists(exeName))
            {
                System.Diagnostics.FileVersionInfo exeVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(exeName);
                versionNum = exeVersion.ProductMinorPart;
                updateNum  = exeVersion.ProductBuildPart;
            }

            if (versionNum == 32)
            {
                this.bms_Version = BMS_Version.BMS432;
            }
            else if (versionNum == 33 && updateNum == 0)
            {
                this.bms_Version = BMS_Version.BMS433;
            }
            else if (versionNum == 33 && updateNum > 0)
            {
                this.bms_Version = BMS_Version.BMS433U1;
            }
            else if (versionNum == 34)
            {
                this.bms_Version = BMS_Version.BMS434;
            }
            else if (versionNum == 35)
            {
                this.bms_Version = BMS_Version.BMS435;
            }
            else
            {
                this.bms_Version = BMS_Version.BMS434;
            }

            // load command line.
            string[] args = Environment.GetCommandLineArgs();
            var option = new Dictionary<string, string>();
            for (int index = 1; index < args.Length; index += 2)
            {
                option.Add(args[index], args[index + 1]);
            }

            // User defined BMS version
            if (option.ContainsKey("/bms") == true)
            {
                switch (option["/bms"])
                {
                    case "4.32":
                        this.bms_Version = BMS_Version.BMS432;
                        break;
                    case "4.33":
                        this.bms_Version = BMS_Version.BMS433;
                        break;
                    case "4.33.1":
                        this.bms_Version = BMS_Version.BMS433U1;
                        break;
                    case "4.34":
                        this.bms_Version = BMS_Version.BMS434;
                        break;
                    case "4.35":
                        this.bms_Version = BMS_Version.BMS435;
                        break;
                    default:
                        this.bms_Version = BMS_Version.BMS434;
                        break;
                }
            }

            // BMS version
            switch (this.bms_Version)
            {
                case BMS_Version.BMS432:
                    this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.32";
                    this.keyFileName = "BMS.key";
                    this.overRideSetting = new OverrideSettingFor432(this.mainWindow, this);
                    mainWindow.LOGO432.Visibility = System.Windows.Visibility.Visible;
                    break;
                case BMS_Version.BMS433:
                    this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33";
                    this.keyFileName = "BMS - Full.key";
                    this.overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    mainWindow.LOGO433.Visibility = System.Windows.Visibility.Visible;
                    break;
                case BMS_Version.BMS433U1:
                    this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33 U1";
                    this.keyFileName = "BMS - Full.key";
                    this.overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    mainWindow.LOGO433.Visibility = System.Windows.Visibility.Visible;
                    break;
                case BMS_Version.BMS434:
                    this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34";
                    this.keyFileName = "BMS - Full.key";
                    this.overRideSetting = new OverrideSettingFor434(this.mainWindow, this);
                    mainWindow.LOGO434.Visibility = System.Windows.Visibility.Visible;
                    break;
                case BMS_Version.BMS435:
                    this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.35";
                    this.keyFileName = "BMS - Full.key";
                    this.overRideSetting = new OverrideSettingFor435(this.mainWindow, this);
                    mainWindow.LOGO435.Visibility = System.Windows.Visibility.Visible;
                    break;
                case BMS_Version.UNDEFINED:
                    this.overRideSetting = new OverrideSettingForUNDEFINED(this.mainWindow, this);
                    mainWindow.LOGO000.Visibility = System.Windows.Visibility.Visible;
                    break;
                default:
                    break;
            }

            // User defined registry
            if (option.ContainsKey("/reg") == true)
            {
                this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\" + option["/reg"];
            }

            // User defined key file
            if (option.ContainsKey("/key") == true)
            {
                this.keyFileName = option["/key"];
            }

            // Read Registry
            this.regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(this.regName, false);
            if (regkey == null)
            {
                this.regName  = this.regName.Replace("SOFTWARE\\Wow6432Node\\", "SOFTWARE\\");
                this.regkey   = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, false);
                this.platform = Platform.OS_32bit;
                mainWindow.Misc_Platform.IsChecked = false;
                mainWindow.Misc_Platform.IsEnabled = false;
            }
            if (regkey == null)
            {
                System.Windows.MessageBox.Show("Could not find FalconBMS Installed.");
                mainWindow.Close();
                return;
            }

            byte[] bs;
            if (regkey.GetValue("PilotName") == null)
            {
                this.regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, true);
                bs = new byte[] { 0x4a, 0x6f, 0x65, 0x20, 0x50, 0x69, 0x6c, 0x6f, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                regkey.SetValue("PilotName", bs);
            }
            if (regkey.GetValue("PilotCallsign") == null)
            {
                this.regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, true);
                bs = new byte[] { 0x56, 0x69, 0x70, 0x65, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                regkey.SetValue("PilotCallsign", bs);
            }
            if (regkey.GetValue("curTheater") == null)
            {
                this.regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, true);
                regkey.SetValue("curTheater", "Korea KTO");
            }

            this.installDir     = (string)regkey.GetValue("baseDir");
            this.currentTheater = (string)regkey.GetValue("curTheater");
            this.pilotCallsign  = (Encoding.UTF8.GetString((byte[])regkey.GetValue("PilotCallsign"))).Replace("\0", "");

            this.regkey.Close();
        }

        /// <summary>
        /// Rewrite Theater setting in the registry.
        /// </summary>
        /// <param name="combobox"></param>
        public void ChangeTheater(ComboBox combobox)
        {
            this.regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(this.regName, true);
            this.regkey.SetValue("curTheater", combobox.Items[combobox.SelectedIndex].ToString());
            this.regkey.Close();

            switch ((String)mainWindow.Dropdown_TheaterList.SelectedItem)
            {
                case "Israel":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    this.theaterOwnConfig = this.GetInstallDir() + "\\Data\\Add-On Israel\\Israeli Theater Settings.exe";
                    break;
                case "Ikaros":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    this.theaterOwnConfig = this.GetInstallDir() + "\\Data\\Add-On Ikaros\\Ikaros Settings.exe";
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
        BMS435
    }
}
