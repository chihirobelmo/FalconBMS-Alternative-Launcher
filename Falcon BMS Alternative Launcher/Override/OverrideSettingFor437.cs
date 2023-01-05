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
    }
}
