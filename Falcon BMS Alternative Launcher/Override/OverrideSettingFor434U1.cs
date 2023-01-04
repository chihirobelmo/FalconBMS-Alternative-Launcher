using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using FalconBMS.Launcher.Input;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Override
{
    public class OverrideSettingFor434U1 : OverrideSettingFor434
    {
        public OverrideSettingFor434U1(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
        }

        protected override void SaveConfigfile(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/falcon bms.cfg";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/falcon bms.cfg";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamReader cReader = new StreamReader
                (filename, Encoding.Default);
            string stResult = "";
            while (cReader.Peek() >= 0)
            {
                string stBuffer = cReader.ReadLine();
                if (stBuffer.Contains("// SETUP OVERRIDE"))
                    continue;
                stResult += stBuffer + "\r\n";
            }
            cReader.Close();

            StreamWriter cfg = new StreamWriter
                (filename, false, Encoding.GetEncoding("shift_jis"));
            cfg.Write(stResult);
            cfg.Write("set g_nHotasPinkyShiftMagnitude " + deviceControl.joyAssign.Length * CommonConstants.DX32
                + "          // SETUP OVERRIDE\r\n");
            cfg.Write("set g_bHotasDgftSelfCancel " + Convert.ToInt32(mainWindow.Misc_OverrideSelfCancel.IsChecked)
                + "          // SETUP OVERRIDE\r\n");
            cfg.Write("set g_b3DClickableCursorAnchored " + Convert.ToInt32(mainWindow.Misc_MouseCursorAnchor.IsChecked)
                + "          // SETUP OVERRIDE\r\n");
            if (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == ((InGameAxAssgn)inGameAxis["Throttle"]).GetDeviceNumber())
            {
                cfg.Close();
                return;
            }
            cfg.Write("set g_nNumOfPOVs 2      // SETUP OVERRIDE\r\n");
            cfg.Write("set g_nPOV1DeviceID " + (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() + 2) + "   // SETUP OVERRIDE\r\n");
            cfg.Write("set g_nPOV1ID 0         // SETUP OVERRIDE\r\n");
            cfg.Write("set g_nPOV2DeviceID " + (((InGameAxAssgn)inGameAxis["Throttle"]).GetDeviceNumber() + 2) + "   // SETUP OVERRIDE\r\n");
            cfg.Write("set g_nPOV2ID 0         // SETUP OVERRIDE\r\n");
            cfg.Close();
        }

        protected override void SaveKeyMapping(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile, int DXnumber)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.getKeyFileName();

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamWriter sw = new StreamWriter
                (filename, false, Encoding.GetEncoding("utf-8"));
            for (int i = 0; i < keyFile.keyAssign.Length; i++)
                sw.Write(keyFile.keyAssign[i].GetKeyLine());
            for (int i = 0; i < deviceControl.joyAssign.Length; i++)
            {
                sw.Write(deviceControl.joyAssign[i].GetKeyLineDX(i, deviceControl.joyAssign.Length, DXnumber));
                // PRIMARY DEVICE POV
                if (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == i && ((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == ((InGameAxAssgn)inGameAxis["Throttle"]).GetDeviceNumber())
                {
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV());
                    continue;
                }
                if (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == i)
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV(0));
                if (((InGameAxAssgn)inGameAxis["Throttle"]).GetDeviceNumber() == i)
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV(1));
            }
            sw.Close();
        }
    }

}
