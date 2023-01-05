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

        protected override void OverridePovDeviceIDs(StreamWriter cfg, Hashtable inGameAxis)
        {
            InGameAxAssgn rollAxis = (InGameAxAssgn)inGameAxis["Roll"];
            InGameAxAssgn throttleAxis = (InGameAxAssgn)inGameAxis["Throttle"];

            if (rollAxis.GetDeviceNumber() == throttleAxis.GetDeviceNumber())
            {
                return;
            }
            cfg.Write("set g_nNumOfPOVs 2 " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV1DeviceID " + (rollAxis.GetDeviceNumber() + 2) + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV1ID 0 " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV2DeviceID " + (throttleAxis.GetDeviceNumber() + 2) + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_nPOV2ID 0 " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
        }

        protected override void WriteKeyLines(string filename, Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile, int DXnumber)
        {
            StreamWriter sw = new StreamWriter
                (filename, false, Encoding.GetEncoding("utf-8"));
            for (int i = 0; i < keyFile.keyAssign.Length; i++)
                sw.Write(keyFile.keyAssign[i].GetKeyLine());
            for (int i = 0; i < deviceControl.joyAssign.Length; i++)
            {
                InGameAxAssgn rollAxis = (InGameAxAssgn)inGameAxis["Roll"];
                InGameAxAssgn throttleAxis = (InGameAxAssgn)inGameAxis["Throttle"];

                sw.Write(deviceControl.joyAssign[i].GetKeyLineDX(i, deviceControl.joyAssign.Length, DXnumber));
                // PRIMARY DEVICE POV
                if (rollAxis.GetDeviceNumber() == i && rollAxis.GetDeviceNumber() == throttleAxis.GetDeviceNumber())
                {
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV());
                    continue;
                }
                if (rollAxis.GetDeviceNumber() == i)
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV(0));
                if (throttleAxis.GetDeviceNumber() == i)
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV(1));
            }
            sw.Close();
        }
    }

}
