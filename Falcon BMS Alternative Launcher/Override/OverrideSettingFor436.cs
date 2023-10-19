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
                + deviceControl.GetJoystickMappingsForButtonsAndHats().Length * CommonConstants.DX_MAX_BUTTONS
                + " " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
        }

        protected override void OverrideButtonsPerDevice(StreamWriter cfg, DeviceControl deviceControl)
        {
            cfg.Write(
                "set g_nButtonsPerDevice "
                + CommonConstants.DX_MAX_BUTTONS
                + " " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
        }
    }
}
