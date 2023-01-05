using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using FalconBMS.Launcher.Input;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Override
{
    public class OverrideSettingFor436 : OverrideSettingFor435
    {
        public OverrideSettingFor436(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
        }

        protected override void OverrideHotasPinkyShiftMagnitude(StreamWriter cfg, DeviceControl deviceControl)
        {
            cfg.Write(
                "set g_nHotasPinkyShiftMagnitude "
                + deviceControl.joyAssign.Length * CommonConstants.DX128
                + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
        }

        protected override void OverrideButtonsPerDevice(StreamWriter cfg, DeviceControl deviceControl)
        {
            cfg.Write(
                "set g_nButtonsPerDevice "
                + CommonConstants.DX128
                + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
        }

        protected override void SaveKeyMapping(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile)
        {
            SaveKeyMapping(inGameAxis, deviceControl, keyFile, CommonConstants.DX128);
        }
    }
}
