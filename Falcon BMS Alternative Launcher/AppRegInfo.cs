using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

using FalconBMS.Launcher.Windows;
using FalconBMS.Launcher.Override;
using FalconBMS.Launcher.Starter;
using Microsoft.Win32;
using FalconBMS.Launcher.Input;

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

        private AbstractStarter starter;

        private int updateVersion;
        private string installDir;
        private string exeDir;
        private string currentTheater;
        private string pilotCallsign;

        private MainWindow mainWindow;

        public string keyFileName     = CommonConstants.DEFAULTKEY + ".key";
        public string keyUserFileName = CommonConstants.USERKEY + ".key";

        public string theaterOwnConfig = "";

        // Method
        public string GetInstallDir() { return installDir; }
        public string GetCurrentTheater() { return currentTheater; }
        public string GetPilotCallsign() { return pilotCallsign; }
        public string getKeyFileName() { return keyFileName; }
        public string getKeyUserFileName() { return keyUserFileName; }

        public void SetUserKeyFileName(string newkey) 
        { 
            keyFileName = newkey + ".key";
        }

        public OverrideSetting getOverrideWriter() { return overRideSetting; }
        public BMS_Version getBMSVersion() { return bms_Version; }
        public AbstractStarter getLauncher() { return starter; }
        public int getUpdateVersion() { return updateVersion; }

        public string[] availableBMSVersions =
        {
            "Falcon BMS 4.38 (Internal)",
            "Falcon BMS 4.37 (Internal)",
            "Falcon BMS 4.37",
            "Falcon BMS 4.36 (Internal)",
            "Falcon BMS 4.36",
            "Falcon BMS 4.35",
            "Falcon BMS 4.34",
            "Falcon BMS 4.33 U1",
            "Falcon BMS 4.33",
            "Falcon BMS 4.32"
        };

        public AppRegInfo(MainWindow mainWindow)
        {
            Diagnostics.Log("Start Reading Registry.");

            bool flg = true;
            string selectedVersion = "Falcon4.0";

            foreach (string version in availableBMSVersions)
            {
                if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Benchmark Sims\\" + version, false) == null)
                    continue;

                if (BMSExists(version)) 
                    mainWindow.ListBox_BMS.Items.Add(version); 
                else 
                    continue; 

                if (flg)
                {
                    selectedVersion = version;
                    flg = false;
                }

                if (version == Properties.Settings.Default.BMS_Version)
                {
                    selectedVersion = version;
                    mainWindow.ListBox_BMS.SelectedIndex = mainWindow.ListBox_BMS.Items.Count - 1;
                }
            }

            Init(mainWindow, selectedVersion);

            Diagnostics.Log("Finished Reading Registry.");
        }

        public bool BMSExists(string version)
        {
            string regName64 = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\" + version;
            string regName32 = "SOFTWARE\\Benchmark Sims\\" + version;

            try
            {
                RegistryKey regkey64 = Registry.LocalMachine.OpenSubKey(regName64, true);
                regName = regName64;
                regkey = regkey64;
            }
            catch (Exception ex1)
            {
                Diagnostics.Log(regName64);
                Diagnostics.Log(ex1);
            }

            if (regkey == null)
            {
                try
                {
                    RegistryKey regkey32 = Registry.LocalMachine.OpenSubKey(regName32, true);

                    if (regkey == null)
                    {
                        Diagnostics.Log("No BMS registries found.");
                        return false;
                    }

                    platform = Platform.OS_32bit;
                    mainWindow.Misc_Platform.IsChecked = false;
                    mainWindow.Misc_Platform.IsEnabled = false;

                    regName = regName32;
                    regkey = regkey32;
                }
                catch (Exception ex2)
                {
                    Diagnostics.Log(regName32);
                    Diagnostics.Log(ex2);
                    return false;
                }

                if (regkey == null)
                {
                    Diagnostics.Log("No BMS registries found.");
                    return false;
                }
            }

            byte[] bs;

            if (regkey.GetValue("PilotName") == null)
            {
                bs = Encoding.ASCII.GetBytes("Joe Pilot\0\0\0\0\0\0\0\0\0\0\0");
                regkey.SetValue("PilotName", bs);
            }
            if (regkey.GetValue("PilotCallsign") == null)
            {
                bs = Encoding.ASCII.GetBytes("Viper\0\0\0\0\0\0\0");
                regkey.SetValue("PilotCallsign", bs);
            }
            if (regkey.GetValue("curTheater") == null)
            {
                regkey.SetValue("curTheater", "Korea KTO");
            }

            installDir = (string)regkey.GetValue("baseDir");
            currentTheater = (string)regkey.GetValue("curTheater");

            pilotCallsign = ReadPilotCallsign((byte[])regkey.GetValue("PilotCallsign"));

            if (platform == Platform.OS_64bit)
                exeDir = installDir + "\\bin\\x64\\Falcon BMS.exe";
            if (platform == Platform.OS_32bit)
                exeDir = installDir + "\\bin\\x86\\Falcon BMS.exe";

            return File.Exists(exeDir);
        }

        public void Init(MainWindow mainWindow, string version)
        {
            this.mainWindow = mainWindow;

            switch (version)
            {
                case "Falcon BMS 4.38 (Internal)":
                    bms_Version     = BMS_Version.BMS438I;
                    keyFileName     = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor438(this.mainWindow, this);
                    starter         = new Starter438Internal(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.37 (Internal)":
                    bms_Version     = BMS_Version.BMS437I;
                    keyFileName     = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor437(this.mainWindow, this);
                    starter         = new Starter437Internal(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.36 (Internal)":
                    bms_Version     = BMS_Version.BMS436I;
                    keyFileName     = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor437(this.mainWindow, this);
                    starter         = new Starter436Internal(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.37":
                    bms_Version     = BMS_Version.BMS437;
                    keyFileName     = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor437(this.mainWindow, this);
                    starter         = new Starter437(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.36":
                    bms_Version     = BMS_Version.BMS436;
                    keyFileName     = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor436(this.mainWindow, this);
                    starter         = new Starter436(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.35":
                    bms_Version     = BMS_Version.BMS435;
                    keyFileName = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor435(this.mainWindow, this);
                    starter         = new Starter435(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.34":
                    bms_Version     = BMS_Version.BMS434U1;
                    keyFileName     = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor434U1(this.mainWindow, this);
                    starter         = new Starter434(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.33 U1":
                    bms_Version     = BMS_Version.BMS433U1;
                    keyFileName     = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    starter         = new Starter433(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.33":
                    bms_Version     = BMS_Version.BMS433;
                    keyFileName     = CommonConstants.DEFAULTKEY + ".key";
                    overRideSetting = new OverrideSettingFor433(this.mainWindow, this);
                    starter         = new Starter433(this, this.mainWindow);
                    break;
                case "Falcon BMS 4.32":
                    bms_Version     = BMS_Version.BMS432;
                    keyFileName     = "BMS.key";
                    overRideSetting = new OverrideSettingFor432(this.mainWindow, this);
                    starter         = new Starter432(this, this.mainWindow);
                    break;
                default:
                    bms_Version = BMS_Version.UNDEFINED;
                    Properties.Settings.Default.BMS_Version = "Falcon4.0";
                    throw new ArgumentOutOfRangeException(); // Just to be explicit.
                    break;
            }

            BMSExists(version);

            updateVersion = CheckUpdateVersion();

            regkey.Close();
        }

        public int CheckUpdateVersion()
        {
            if (!File.Exists(@exeDir))
                return 256;
            FileVersionInfo vi = FileVersionInfo.GetVersionInfo(@exeDir);
            return vi.ProductBuildPart;
        }

        public string ReadPilotCallsign(Byte[] bt)
        {
            Byte[] bts = new byte[0];

            for (int i = 0; i < bt.Length; i++)
            {
                if (bt[i] == 0x00)
                {
                    return Encoding.UTF8.GetString(bts);
                }
                else
                {
                    System.Array.Resize(ref bts, bts.Length + 1);
                    bts[bts.Length - 1] = bt[i];
                }
            }

            return Encoding.UTF8.GetString(bts);
        }

        public void setDPIOverride(string auth)
        {
            string regName = "Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers";
            RegistryKey regkey = Registry.LocalMachine.OpenSubKey(regName, true);
            regkey.SetValue(auth, "~HIGHDPIAWARE");
            regkey.Close();
        }

        public bool isNameDefined()
        {
            regkey = Registry.LocalMachine.OpenSubKey(regName, false);
            if (regkey == null)
            {
                regkey.Close();
                return true;
            }
            if (regkey.GetValue("PilotCallsign") == null)
                return false;
            if (regkey.GetValue("PilotName") == null)
                return false;
            if (ReadPilotCallsign((byte[])regkey.GetValue("PilotCallsign")) == "Viper")
                return false;
            if (ReadPilotCallsign((byte[])regkey.GetValue("PilotName")) == "Joe Pilot")
                return false;
            return true;
        }

        public void ChangeName(string callSign, string pilotName)
        {
            pilotCallsign = callSign;

            regkey = Registry.LocalMachine.OpenSubKey(regName, true);
            if (regkey == null)
            {
                regkey.Close();
                return;
            }

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
            regkey.SetValue("PilotCallsign", bs);

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
            regkey.SetValue("PilotName", bs2);

            regkey.Close();
        }

        public void GetTheater()
        {
            regkey = Registry.LocalMachine.OpenSubKey(regName, false);
            currentTheater = (string)regkey.GetValue("curTheater");
            regkey.Close();
        }

        public void ChangeTheater(ComboBox combobox)
        {
            if (combobox.SelectedIndex == -1)
                return;

            regkey = Registry.LocalMachine.OpenSubKey(regName, true);
            regkey.SetValue("curTheater", combobox.Items[combobox.SelectedIndex].ToString());
            regkey.Close();

            switch ((string)mainWindow.Dropdown_TheaterList.SelectedItem)
            {
                case "Israel":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On Israel\\Israeli Theater Settings.exe";
                    return;
                case "Ikaros":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On Ikaros\\Ikaros Settings.exe";
                    return;
                default:
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Collapsed;
                    break;
            }
            if (mainWindow.Dropdown_TheaterList.SelectedItem.ToString().Contains("Korea Training"))
            {
                mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                theaterOwnConfig = GetInstallDir() + "\\Data\\Add-On " + mainWindow.Dropdown_TheaterList.SelectedItem.ToString() + "\\Korea Training Theater Settings.exe";
            }
            else
            {
                mainWindow.Launch_TheaterConfig.Visibility = Visibility.Collapsed;
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
        BMS437,
        BMS438I,
        BMS438
    }
}
