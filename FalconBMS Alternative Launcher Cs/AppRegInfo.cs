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
        private string regName;
        private Platform platform;
        private BMS_Version bms_Version;

        private string installDir;
        private string currentTheater;
        private string pilotCallsign;

        private MainWindow mainWindow;
        public string theaterOwnConfig = "";

        // Method
        public string GetInstallDir() { return this.installDir; }
        public string GetCurrentTheater() { return this.currentTheater; }
        public string GetPilotCallsign() { return this.pilotCallsign; }

        public AppRegInfo(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // Read Current Directry
            string stCurrentDir = System.IO.Directory.GetCurrentDirectory();
            if (stCurrentDir.Contains("Falcon BMS 4.32"))
            {
                this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.32";
                this.bms_Version = BMS_Version.BMS432;
                this.platform = Platform.OS_32bit;
                mainWindow.Misc_Platform.IsChecked = false;
                mainWindow.Misc_Platform.IsEnabled = false;
                mainWindow.LOGO432.Visibility = System.Windows.Visibility.Visible;
            }
            else if (stCurrentDir.Contains("Falcon BMS 4.33") && !stCurrentDir.Contains("Falcon BMS 4.33 U1"))
            {
                this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33";
                this.bms_Version = BMS_Version.BMS433;
                this.platform = Platform.OS_64bit;
                mainWindow.LOGO433.Visibility = System.Windows.Visibility.Visible;
            }
            else if (stCurrentDir.Contains("Falcon BMS 4.33 U1"))
            {
                this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33 U1";
                this.bms_Version = BMS_Version.BMS433U1;
                this.platform = Platform.OS_64bit;
                mainWindow.LOGO433.Visibility = System.Windows.Visibility.Visible;
            }
            else if (stCurrentDir.Contains("Falcon BMS 4.34"))
            {
                this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.34";
                this.bms_Version = BMS_Version.BMS434;
                this.platform = Platform.OS_64bit;
                mainWindow.LOGO434.Visibility = System.Windows.Visibility.Visible;
            }
            else if (stCurrentDir.Contains("Falcon BMS 4.35"))
            {
                this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.35";
                this.bms_Version = BMS_Version.BMS435;
                this.platform = Platform.OS_64bit;
                mainWindow.LOGO435.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.regName = "SOFTWARE\\Wow6432Node\\Benchmark Sims\\Falcon BMS 4.33 U1";
                this.bms_Version = BMS_Version.BMS433U1;
                this.platform = Platform.OS_64bit;
                mainWindow.LOGO433.Visibility = System.Windows.Visibility.Visible;
            }

            // Read Registry
            this.regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, false);

            if (regkey == null)
            {
                this.regName = this.regName.Replace("SOFTWARE\\Wow6432Node\\", "SOFTWARE\\");
                this.regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regName, false);
                this.platform = Platform.OS_32bit;
                mainWindow.Misc_Platform.IsChecked = false;
                mainWindow.Misc_Platform.IsEnabled = false;
            }
            if (regkey == null)
            {
                System.Windows.MessageBox.Show("There is no FalconBMS 4.33 U1 Installed.");
                mainWindow.Close();
                return;
            }

            this.installDir = (string)regkey.GetValue("baseDir");
            this.currentTheater = (string)regkey.GetValue("curTheater");
            this.pilotCallsign = (Encoding.UTF8.GetString((byte[])regkey.GetValue("PilotCallsign"))).Replace("\0", "");

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
                    theaterOwnConfig = this.GetInstallDir() + "\\Data\\Add-On Israel\\Israeli Theater Settings.exe";
                    break;
                case "Ikaros":
                    mainWindow.Launch_TheaterConfig.Visibility = Visibility.Visible;
                    theaterOwnConfig = this.GetInstallDir() + "\\Data\\Add-On Ikaros\\Ikaros Settings.exe";
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
        BMS432,
        BMS433,
        BMS433U1,
        BMS434,
        BMS435
    }
}
