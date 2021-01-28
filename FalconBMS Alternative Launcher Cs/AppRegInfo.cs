using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using FalconBMS.Launcher.Core;
using FalconBMS.Launcher.Windows;

using Microsoft.Win32;

namespace FalconBMS.Launcher
{
    public class AppRegInfo
    {
        // Member
        private RegistryKey regkey;
        private string regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.35";
        private Platform platform = Platform.Os64Bit;
        private BmsVersion bmsVersion = BmsVersion.Bms435;
        private OverrideSetting overRideSetting;
        private Core.Launcher launcher;

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
        public string GetKeyFileName() { return keyFileName; }
        public OverrideSetting GetOverrideWriter() { return overRideSetting; }
        public BmsVersion GetBmsVersion() { return bmsVersion; }
        public Core.Launcher GetLauncher() { return launcher; }

        public AppRegInfo(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // Read Current Directory
            int versionNum = 0;
            int updateNum = 0;
            string exeName = Directory.GetCurrentDirectory() + "/Falcon BMS.exe";
            if (!File.Exists(exeName))
                exeName = Directory.GetCurrentDirectory() + "/../x64/Falcon BMS.exe";
            if (File.Exists(exeName))
            {
                FileVersionInfo exeVersion = FileVersionInfo.GetVersionInfo(exeName);
                versionNum = exeVersion.ProductMinorPart;
                updateNum  = exeVersion.ProductBuildPart;
            }

            switch (versionNum)
            {
                case 32:
                    bmsVersion = BmsVersion.Bms432;
                    break;
                case 33 when updateNum == 0:
                    bmsVersion = BmsVersion.Bms433;
                    break;
                case 33 when updateNum > 0:
                    bmsVersion = BmsVersion.Bms433U1;
                    break;
                case 34 when updateNum == 0:
                    bmsVersion = BmsVersion.Bms434;
                    break;
                case 34 when updateNum > 0:
                    bmsVersion = BmsVersion.Bms434U1;
                    break;
                case 35:
                    bmsVersion = BmsVersion.Bms435;
                    break;
                default:
                    bmsVersion = BmsVersion.Bms435;
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
                        bmsVersion = BmsVersion.Bms432;
                        break;
                    case "4.33":
                        bmsVersion = BmsVersion.Bms433;
                        break;
                    case "4.33.1":
                        bmsVersion = BmsVersion.Bms433U1;
                        break;
                    case "4.34":
                        bmsVersion = BmsVersion.Bms434;
                        break;
                    case "4.34.1":
                        bmsVersion = BmsVersion.Bms434U1;
                        break;
                    case "4.35":
                        bmsVersion = BmsVersion.Bms435;
                        break;
                    default:
                        bmsVersion = BmsVersion.Bms435;
                        break;
                }
            }

            // BMS version
            switch (bmsVersion)
            {
                case BmsVersion.Bms432:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.32";
                    keyFileName = "BMS.key";
                    overRideSetting = new OverrideSettingFor432(this.mainWindow, this);
                    launcher = new Launcher432(this, this.mainWindow);
                    mainWindow.Logo432.Visibility = Visibility.Visible;
                    break;
                case BmsVersion.Bms433:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    launcher = new Launcher433(this, this.mainWindow);
                    mainWindow.Logo433.Visibility = Visibility.Visible;
                    break;
                case BmsVersion.Bms433U1:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33 U1";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    launcher = new Launcher433(this, this.mainWindow);
                    mainWindow.Logo433.Visibility = Visibility.Visible;
                    break;
                case BmsVersion.Bms434:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor434(this.mainWindow, this);
                    launcher = new Launcher434(this, this.mainWindow);
                    mainWindow.Logo434.Visibility = Visibility.Visible;
                    break;
                case BmsVersion.Bms434U1:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor434U1(this.mainWindow, this);
                    launcher = new Launcher434(this, this.mainWindow);
                    mainWindow.Logo434.Visibility = Visibility.Visible;
                    break;
                case BmsVersion.Bms435:
                    regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.35";
                    keyFileName = "BMS - Full.key";
                    overRideSetting = new OverrideSettingFor435(this.mainWindow, this);
                    launcher = new Launcher435(this, this.mainWindow);
                    mainWindow.Logo435.Visibility = Visibility.Visible;
                    break;
                case BmsVersion.Undefined:
                    // TODO: Log Undefined version instance occured here???
                    break;
                default:
                    throw new ArgumentOutOfRangeException(); // TODO: Log Argument out of Range Exception occurence here.
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
            regkey = Registry.LocalMachine.OpenSubKey(regName, false);
            if (regkey == null)
            {
                regName  = regName.Replace("SOFTWARE\\Wow6432Node\\", "SOFTWARE\\");
                regkey   = Registry.LocalMachine.OpenSubKey(regName, false);
                platform = Platform.Os32Bit;
                mainWindow.MiscPlatform.IsChecked = false;
                mainWindow.MiscPlatform.IsEnabled = false;
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
                regkey = Registry.LocalMachine.OpenSubKey(regName, true);
                bs = new byte[] { 0x4a, 0x6f, 0x65, 0x20, 0x50, 0x69, 0x6c, 0x6f, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                regkey?.SetValue("PilotName", bs);
            }
            if (regkey?.GetValue("PilotCallsign") == null)
            {
                regkey = Registry.LocalMachine.OpenSubKey(regName, true);
                bs = new byte[] { 0x56, 0x69, 0x70, 0x65, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                regkey?.SetValue("PilotCallsign", bs);
            }
            if (regkey?.GetValue("curTheater") == null)
            {
                regkey = Registry.LocalMachine.OpenSubKey(regName, true);
                regkey?.SetValue("curTheater", "Korea KTO");
            }

            installDir     = (string)regkey?.GetValue("baseDir");
            currentTheater = (string)regkey?.GetValue("curTheater");
            pilotCallsign  = Encoding.UTF8.GetString((byte[])regkey?.GetValue("PilotCallsign")).Replace("\0", "");

            regkey?.Close();
        }

        /// <summary>
        /// Rewrite Theater setting in the registry.
        /// </summary>
        /// <param name="combobox"></param>
        public void ChangeTheater(ComboBox combobox)
        {
            regkey = Registry.LocalMachine.OpenSubKey(regName, true);
            regkey?.SetValue("curTheater", combobox.Items[combobox.SelectedIndex].ToString());
            regkey?.Close();

            switch ((string)mainWindow.DropdownTheaterList.SelectedItem)
            {
                case "Israel":
                    mainWindow.LaunchTheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On Israel\\Israeli Theater Settings.exe";
                    break;
                case "Ikaros":
                    mainWindow.LaunchTheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On Ikaros\\Ikaros Settings.exe";
                    break;
                default:
                    mainWindow.LaunchTheaterConfig.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }

    public enum Platform
    {
        Os32Bit,
        Os64Bit
    }

    public enum BmsVersion
    {
        Undefined,
        Bms432,
        Bms433,
        Bms433U1,
        Bms434,
        Bms434U1,
        Bms435
    }
}
