using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Diagnostics;

namespace FalconBMS.Launcher
{
    public class SteamVR
    {
        public Process process;
        public string installPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SteamVR\\bin\\win64";
        public SteamVR()
        {
            string regName = "SOFTWARE\\WOW6432Node\\Valve\\Steam";
            RegistryKey regkey = Registry.LocalMachine.OpenSubKey(regName, false);
            if (regkey == null)
            {
                regkey.Close();
                return;
            }
            installPath = (string)regkey.GetValue("InstallPath");
            if (installPath == null)
            {
                regkey.Close();
                return;
            }

            installPath += "\\steamapps\\common\\SteamVR\\bin\\win64\\vrstartup.exe";

            regkey.Close();
        }
        public void Start()
        {
            process = Process.Start(installPath);
        }
        public void Stop()
        {
            process.Close();
        }
    }
}