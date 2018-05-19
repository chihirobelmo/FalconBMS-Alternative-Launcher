using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FalconBMS_Alternative_Launcher_Cs
{
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
}
