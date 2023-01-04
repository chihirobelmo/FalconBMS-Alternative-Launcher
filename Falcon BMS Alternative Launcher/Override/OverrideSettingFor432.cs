using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using FalconBMS.Launcher.Input;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Override
{

    /// <summary>
    /// Writer for BMS4.32 setting Override
    /// </summary>
    public class OverrideSettingFor432 : OverrideSetting
    {
        /// <summary>
        /// Writer for BMS4.32 setting Override
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="appReg"></param>
        public OverrideSettingFor432(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
        }

        protected override void SavePop()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign() + ".pop";
            if (!File.Exists(filename))
            {
                byte[] nbs = {
                     0x1B, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x42, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                     0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x64, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                     0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,
                     0x00, 0x00, 0x00, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
                     0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                     0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
                     0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00,
                     0x12, 0xFD, 0xFF, 0xFF, 0x12, 0xFD, 0xFF, 0xFF, 0x57, 0xFE, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF,
                     0xBA, 0xFF, 0xFF, 0xFF, 0x8C, 0xFB, 0xFF, 0xFF, 0x50, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
                     0x50, 0xFB, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF, 0xDF, 0xF8, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
                     0x01, 0x00, 0x00, 0x00, 0x12, 0xFD, 0xFF, 0xFF, 0x57, 0xFE, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF,
                     0xBA, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                     0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3E, 0x42, 0x4D, 0x53, 0x00, 0x00, 0x00, 0x00, 0x00,
                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01,
                     0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F,
                     0x05, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
                     0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x42, 0x4D, 0x53, 0x20, 0x34, 0x2E, 0x33, 0x32, 0x2E, 0x30,
                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0xF0, 0xD8, 0xFF, 0xFF
                };
                FileStream nfs = new FileStream
                    (filename, FileMode.Create, FileAccess.Write);
                nfs.Write(nbs, 0, nbs.Length);
                nfs.Close();
            }
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/" + appReg.GetPilotCallsign() + ".pop";
            if (!File.Exists(fbackupname))
                File.Copy(filename, fbackupname, true);

            File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            FileStream fs = new FileStream
                (filename, FileMode.Open, FileAccess.Read);
            byte[] bs = new byte[fs.Length];
            fs.Read(bs, 0, bs.Length);
            fs.Close();

            // Set Keyfile selected.
            byte[] keyFileName = Encoding.ASCII.GetBytes(appReg.getKeyUserFileName().Replace(".key", ""));
            for (int i = 0; i <= 15; i++)
            {
                if (i >= keyFileName.Length)
                {
                    bs[232 + i] = 0x00;
                    continue;
                }
                bs[232 + i] = keyFileName[i];
            }

            // TrackIR Z-Axis(0:Z-axis 1:FOV)
            bs[276] = 0x00;
            if (mainWindow.Misc_TrackIRZ.IsChecked == true)
                bs[276] = 0x01;

            // External Mouselook
            bs[281] = 0x00;
            if (mainWindow.Misc_ExMouseLook.IsChecked == true)
                bs[281] = 0x01;

            // Roll-linked NWS
            bs[302] = 0x00;
            if (mainWindow.Misc_RollLinkedNWS.IsChecked == true)
                bs[302] = 0x01;

            // Smart Scaling
            bs[12] = 0x01;
            if (mainWindow.Misc_SmartScalingOverride.IsChecked == true)
                bs[12] = 0x05;

            fs = new FileStream
                (filename, FileMode.Create, FileAccess.Write);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
        }

        public override AxisName[] getAxisMappingList() { return axisMappingList; }
        public override AxisName[] getJoystickCalList() { return joystickCalList; }

        /// <summary>
        /// Axis information order for AxisMapping.dat
        /// </summary>
        private AxisName[] axisMappingList = {
            AxisName.Pitch,
            AxisName.Roll,
            AxisName.Yaw,
            AxisName.Throttle,
            AxisName.Throttle_Right,
            AxisName.Toe_Brake,
            AxisName.Toe_Brake_Right,
            AxisName.FOV,
            AxisName.Trim_Pitch,
            AxisName.Trim_Yaw,
            AxisName.Trim_Roll,
            AxisName.Radar_Antenna_Elevation,
            AxisName.Range_Knob,
            AxisName.Cursor_X,
            AxisName.Cursor_Y,
            AxisName.COMM_Channel_1,
            AxisName.COMM_Channel_2,
            AxisName.MSL_Volume,
            AxisName.Threat_Volume,
            AxisName.Intercom,
            AxisName.HUD_Brightness,
            AxisName.HMS_Brightness,
            AxisName.Reticle_Depression,
            AxisName.Camera_Distance
        };

        /// <summary>
        /// Axis information order for JoyStick.cal
        /// </summary>
        private AxisName[] joystickCalList = {
            AxisName.Pitch,
            AxisName.Roll,
            AxisName.Yaw,
            AxisName.Throttle,
            AxisName.Throttle_Right,
            AxisName.Trim_Pitch,
            AxisName.Trim_Yaw,
            AxisName.Trim_Roll,
            AxisName.Toe_Brake,
            AxisName.Toe_Brake_Right,
            AxisName.FOV,
            AxisName.Radar_Antenna_Elevation,
            AxisName.Cursor_X,
            AxisName.Cursor_Y,
            AxisName.Range_Knob,
            AxisName.COMM_Channel_1,
            AxisName.COMM_Channel_2,
            AxisName.MSL_Volume,
            AxisName.Threat_Volume,
            AxisName.HUD_Brightness,
            AxisName.Reticle_Depression,
            AxisName.Camera_Distance,
            AxisName.Intercom,
            AxisName.HMS_Brightness,
        };
    }

}
