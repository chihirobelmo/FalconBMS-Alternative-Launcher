using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using FalconBMS.Launcher.Input;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Override
{
    public class OverrideSettingFor437 : OverrideSettingFor436
    {
        public OverrideSettingFor437(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
        }

        protected override void OverrideVRHMD(StreamWriter cfg)
        {
            cfg.Write(
                "set g_nVRHMD "
                + Convert.ToInt32(mainWindow.Misc_VR.IsChecked)
                + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
        }

        protected override void WriteKeyLines(string filename, Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile, int DXnumber)
        {
            StreamWriter sw = new StreamWriter
                (filename, false, Encoding.GetEncoding("utf-8"));
            for (int i = 0; i < keyFile.keyAssign.Length; i++)
                sw.Write(keyFile.keyAssign[i].GetKeyLine());
            for (int i = 0; i < deviceControl.joyAssign.Length; i++)
            {
                sw.Write(deviceControl.joyAssign[i].GetKeyLineDX(i, deviceControl.joyAssign.Length, DXnumber));
                sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV());
            }
            sw.Close();
        }
    }
}
