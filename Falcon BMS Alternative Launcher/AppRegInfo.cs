using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

using Microsoft.Win32;

using FalconBMS.Launcher.Windows;
using FalconBMS.Launcher.Override;
using FalconBMS.Launcher.Starter;

using FalconBMS.Launcher.Input;

namespace FalconBMS.Launcher
{
    public class AppRegInfo
    {
        // Member
        private string regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.37";

        private BMS_Version bms_Version = BMS_Version.BMS435;

        private OverrideSetting overRideSetting;

        private AbstractStarter starter;

        private string installDir;
        private string exeDir;
        private string currentTheater;
        private string pilotCallsign;

        private MainWindow mainWindow;

        public string theaterOwnConfig = "";

        // Method
        public string GetInstallDir() { return installDir; }
        public string GetCurrentTheater() { return currentTheater; }
        public string GetPilotCallsign() { return pilotCallsign; }

        public OverrideSetting getOverrideWriter() { return overRideSetting; }
        public BMS_Version getBMSVersion() { return bms_Version; }
        public AbstractStarter getLauncher() { return starter; }

        // This will list teh available BMS versions to the launcher list.
        public string[] availableBMSVersions =
        {
            "Falcon BMS 4.38 (Internal)",
            "Falcon BMS 4.37 (Internal)",
            "Falcon BMS 4.37",
            "Falcon BMS 4.36",
            "Falcon BMS 4.35",
            "Falcon BMS 4.34",
            "Falcon BMS 4.33",
            "Falcon BMS 4.32"
        };

        public AppRegInfo(MainWindow window)
        {
            this.mainWindow = window;

            Diagnostics.Log("Start Reading Registry.");

            // Enumerate the available versions, and populate the listbox in a more deterministic, reliable sort-order.
            var foundVersions = new List<string>(10);
            foreach (string version in availableBMSVersions)
            {
                if (Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\" + version, writable:false) == null)
                    continue;

                if (BMSExists(version))
                    foundVersions.Add(version);
            }
            if (foundVersions.Count == 0)
            {
                MessageBox.Show(window, "Unable to locate any BMS installation(s)!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sort order: most recent releases on top.
            foundVersions.Sort((a, b) => { return -1 * String.CompareOrdinal(a, b); });

            // Data-bind the list to the UI control.
            mainWindow.ListBox_BMS.ItemsSource = foundVersions;

            string selectedVersion = null;

            // If we have a saved pref, and it's available, select it.
            if (!string.IsNullOrEmpty(Properties.Settings.Default.BMS_Version))
            {
                int idx = foundVersions.IndexOf(Properties.Settings.Default.BMS_Version);
                if (idx >= 0)
                {
                    selectedVersion = Properties.Settings.Default.BMS_Version;
                    mainWindow.ListBox_BMS.SelectedIndex = idx;
                }
            }

            // If no previously saved pref is found, select the latest and greatest.
            if (string.IsNullOrEmpty(selectedVersion))
            {
                selectedVersion = foundVersions[0];
                mainWindow.ListBox_BMS.SelectedIndex = 0;
            }

            UpdateSelectedBMSVersion(selectedVersion);

            Diagnostics.Log("Finished Reading Registry.");
            return;
        }

        public void UpdateSelectedBMSVersion(string version)
        {
            InitOverriderAndStarterFor(version);

            //TODO: refactor this innocent-looking boolean getter, which has side-effects to init many member fields for
            // the currently selected version.. in the meantime, we must call it again now that we have selectedVersion.
            BMSExists(version);
            return;
        }

        public bool BMSExists(string version)
        {
            string regName64 = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\" + version;

            RegistryKey rk = Registry.LocalMachine.OpenSubKey(regName64, writable:false);
            if (rk == null) return false;

            this.installDir = (string)rk.GetValue("baseDir");
            if (String.IsNullOrEmpty(installDir)) return false;

            this.currentTheater = ReadCurrentTheater();
            this.pilotCallsign = ReadPilotCallsign();

            //if (platform == Platform.OS_64bit)
            exeDir = installDir + "\\bin\\x64\\Falcon BMS.exe";
            return File.Exists(exeDir);
        }

        public void ChangeCfgPath()
        {
            try
            {
                RegistryKey regkeyCFG = Registry.CurrentUser.OpenSubKey("SOFTWARE\\F4Patch\\Settings", writable:true);
                regkeyCFG.SetValue("F4Exe", installDir + "\\Launcher.exe");
                regkeyCFG.Close();
            }
            catch (Exception exCFG)
            {
                Diagnostics.Log(exCFG);
                return;
            }
        }

        public void InitOverriderAndStarterFor(string version)
        {
            switch (version)
            {
                case "Falcon BMS 4.38 (Internal)":
                    bms_Version     = BMS_Version.BMS438I;
                    overRideSetting = new OverrideSettingFor438(this.mainWindow, this);
                    starter         = new Starter438Internal(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.37 (Internal)":
                    bms_Version     = BMS_Version.BMS437I;
                    overRideSetting = new OverrideSettingFor437(this.mainWindow, this);
                    starter         = new Starter437Internal(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.37":
                    bms_Version     = BMS_Version.BMS437;
                    overRideSetting = new OverrideSettingFor437(this.mainWindow, this);
                    starter         = new Starter437(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.36":
                    bms_Version     = BMS_Version.BMS436;
                    overRideSetting = new OverrideSettingFor436(this.mainWindow, this);
                    starter         = new Starter436(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.35":
                    bms_Version     = BMS_Version.BMS435;
                    overRideSetting = new OverrideSettingFor435(this.mainWindow, this);
                    starter         = new Starter435(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.34":
                    bms_Version     = BMS_Version.BMS434U1;
                    overRideSetting = new OverrideSettingFor434U1(this.mainWindow, this);
                    starter         = new Starter434(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.33":
                    bms_Version     = BMS_Version.BMS433;
                    overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    starter         = new Starter433(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.32":
                    bms_Version     = BMS_Version.BMS432;
                    overRideSetting = new OverrideSettingFor432(this.mainWindow, this);
                    starter         = new Starter432(this, this.mainWindow);
                    break;
                default:
                    bms_Version = BMS_Version.UNDEFINED;
                    Properties.Settings.Default.BMS_Version = null;
                    throw new ArgumentOutOfRangeException(); // Just to be explicit.
                    break;
            }
        }

        public string ReadPilotCallsign()
        {
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(regName, writable: false))
            {
                byte[] bits = (byte[])(rk.GetValue("PilotCallsign"));
                if (bits == null) return "Viper";

                return Encoding.ASCII.GetString(bits).TrimEnd('\0');
            }
        }

        public string ReadPilotName()
        {
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(regName, writable: false))
            {
                byte[] bits = (byte[])(rk.GetValue("PilotName"));
                if (bits == null) return "Joe Pilot";

                return Encoding.ASCII.GetString(bits).TrimEnd('\0');
            }
        }

        public bool IsUniqueNameDefined()
        {
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(regName, writable:false))
            {
                if (rk == null) return false;

                if (rk.GetValue("PilotCallsign") == null)
                    return false;
                if (rk.GetValue("PilotName") == null)
                    return false;
                if (ReadPilotCallsign() == "Viper")
                    return false;
                if (ReadPilotName() == "Joe Pilot")
                    return false;

                return true;
            }
        }

        public void ChangeName(string callSign, string pilotName)
        {
            pilotCallsign = callSign;

            RegistryKey rk = Registry.LocalMachine.OpenSubKey(regName, writable:true);
            if (rk == null)
                return;

            byte[] bs = new byte[12];
            byte[] bCallsign = Encoding.ASCII.GetBytes(callSign);
            for (int i = 0; i < bs.Length; i++)
            {
                if (i >= bCallsign.Length)
                {
                    bs[i] = 0x00;
                    continue;
                }
                bs[i] = bCallsign[i];
            }
            rk.SetValue("PilotCallsign", bs);

            byte[] bs2 = new byte[20];
            byte[] bPilotName = Encoding.ASCII.GetBytes(pilotName);
            for (int i = 0; i < bs2.Length; i++)
            {
                if (i >= bPilotName.Length)
                {
                    bs2[i] = 0x00;
                    continue;
                }
                bs2[i] = bPilotName[i];
            }
            rk.SetValue("PilotName", bs2);

            rk.Close();
        }

        public string ReadCurrentTheater()
        {
            using (RegistryKey regkey = Registry.LocalMachine.OpenSubKey(regName, writable: false))
            {
                string s = (string)(regkey.GetValue("curTheater"));
                if (String.IsNullOrEmpty(s)) return "Korea KTO";

                return s;
            }
        }

        public void ChangeTheater(ComboBox combobox)
        {
            if (combobox.SelectedIndex == -1)
                return;

            RegistryKey rk = Registry.LocalMachine.OpenSubKey(regName, writable:true);
            if (rk == null)
                return;

            rk.SetValue("curTheater", combobox.Items[combobox.SelectedIndex].ToString());
            rk.Close();

            switch ((string)mainWindow.Dropdown_TheaterList.SelectedItem)
            {
                case "Israel":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On Israel\\Israel Theater Settings.exe";
                    return;
                case "Ikaros":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On Ikaros\\Ikaros Settings.exe";
                    return;
                default:
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Collapsed;
                    break;
            }
        }
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
        BMS437,
        BMS438I,
        BMS438
    }
}
