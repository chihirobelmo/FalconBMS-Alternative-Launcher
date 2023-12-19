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
            if((bool)mainWindow.VR_SteamVR.IsChecked)
            {
                cfg.Write(
                    "set g_nVRHMD 1"
                    + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            }

            if ((bool)mainWindow.VR_OpenXR.IsChecked)
            {
                cfg.Write(
                    "set g_nVRHMD 2"
                    + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            }
        }
    }
}
