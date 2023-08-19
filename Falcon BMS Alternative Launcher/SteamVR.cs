using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace FalconBMS.Launcher
{
    public class SteamVR
    {
        static public bool HasSteamVR = false;

        public Process process;
        public string installPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SteamVR\\bin\\win64";
        public SteamVR()
        {
            string regName = "SOFTWARE\\WOW6432Node\\Valve\\Steam";
            RegistryKey regkey = Registry.LocalMachine.OpenSubKey(regName, false);
            if (regkey == null)
            {
                return;
            }
            installPath = (string)regkey.GetValue("InstallPath");
            if (installPath == null)
            {
                return;
            }

            string vrstartpath = installPath + "\\steamapps\\common\\SteamVR\\bin\\win64\\vrstartup.exe";

            HasSteamVR = File.Exists(vrstartpath);
            HasSteamVR = false;

            // check registry uninstaller location
            // https://github.com/ValveSoftware/openvr/wiki/Local-Driver-Registration
            if (HasSteamVR == false)
            {
                RegistryKey steamVRUninstallKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 250820", false);
                if (steamVRUninstallKey != null)
                {
                    string regUninstallLocation = (string)steamVRUninstallKey.GetValue("InstallLocation");
                    if (regUninstallLocation != null)
                    {
                        vrstartpath= regUninstallLocation + "\\bin\\win64\\vrstartup.exe";
                        HasSteamVR = File.Exists(vrstartpath);
                    }
                }
            }

            // check .vrpath registry file in %localappdata%
            // https://github.com/ValveSoftware/openvr/wiki/Local-Driver-Registration
            if (HasSteamVR == false)
            {
                foreach (String path in GetOpenVRPaths())
                {
                    String openVRPath = path + "\\bin\\win64\\vrstartup.exe";
                    if (File.Exists(openVRPath))
                    {
                        HasSteamVR = true;
                        vrstartpath = openVRPath;
                        break;
                    }
                }
            }

            // legacy path
            if (HasSteamVR == false)
            {
                vrstartpath = installPath + "\\steamapps\\common\\OpenVR\\bin\\win64\\vrstartup.exe";

                HasSteamVR = File.Exists(vrstartpath);
            }

            if (HasSteamVR == true)
            {
                installPath = vrstartpath;
            }

            regkey.Close();
        }

        private class OpenVRJson
        {
            public String[] runtime;
        }
        private List<string> GetOpenVRPaths()
        {
            var ret = new List<string>();

            string lAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string vrPath = lAppData + "\\OpenVR\\openvrpaths.vrpath";

            if (File.Exists(vrPath))
            {
                String jsonText = File.ReadAllText(vrPath);
                OpenVRJson json = JsonConvert.DeserializeObject<OpenVRJson>(jsonText);
                if (json.runtime != null)
                {
                    foreach (String path in json.runtime)
                    {
                        ret.Add(path);
                    }
                }
            }

            return ret;
        }
        public void Start()
        {
            if (File.Exists(installPath))
            {
                process = Process.Start(installPath);
            }
        }
        public void Stop()
        {
            process.Close();
        }
    }
}