using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using FalconBMS.Launcher.Input;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Override
{
    /// <summary>
    /// Writer for BMS4.34 setting Override
    /// </summary>
    public class OverrideSettingFor434 : OverrideSetting
    {
        /// <summary>
        /// Writer for BMS4.34 setting Override
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="appReg"></param>
        public OverrideSettingFor434(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
        }

        /// <summary>
        /// As the name implies...
        /// </summary>
        protected override void SaveJoystickCal(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/joystick.cal";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/joystick.cal";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            FileStream fs = new FileStream
                (filename, FileMode.Create, FileAccess.Write);

            byte[] bs;

            AxisName[] localJoystickCalList = appReg.getOverrideWriter().getJoystickCalList();
            foreach (AxisName nme in localJoystickCalList)
            {
                bs = new byte[]
                {
                    0x00, 0x00, 0x00, 0x00, 0x98, 0x3A, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };
                if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() != -1)
                {
                    bs[12] = 0x01;

                    if (nme == AxisName.Throttle)
                    {
                        if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() >= 0)
                        {
                            double iAB   = deviceControl.joyAssign[((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber()].detentPosition.GetAB();
                            double iIdle = deviceControl.joyAssign[((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber()].detentPosition.GetIDLE();

                            iAB   = iAB   * 15000 / 65536;
                            iIdle = iIdle * 15000 / 65536;

                            byte[] ab   = BitConverter.GetBytes((int)iAB).Reverse().ToArray();
                            byte[] idle = BitConverter.GetBytes((int)iIdle).Reverse().ToArray();

                            bs[5] = ab[2];
                            bs[1] = idle[2];
                        }
                    }
                }
                if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetInvert())
                {
                    bs[20] = 0x01;
                    bs[21] = 0x01;
                }
                fs.Write(bs, 0, bs.Length);
            }
            fs.Close();
        }

        protected override void SavePop()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign() + ".pop";
            if (!File.Exists(filename))
            {
                byte[] nbs = {
                    0x1B, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x42, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x64, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                    0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x80, 0x3F,
                    0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0xA0, 0x41, 0x00, 0x00, 0xB4, 0x42, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x64, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x6A, 0xFF, 0xFF, 0xFF, 0x12, 0xFD, 0xFF, 0xFF,
                    0x70, 0xFE, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF, 0x6E, 0xF6, 0xFF, 0xFF,
                    0x3C, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x6E, 0xF6, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF,
                    0x4E, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x12, 0xFD, 0xFF, 0xFF,
                    0x70, 0xFE, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6A, 0xFF, 0xFF, 0xFF, 0x12, 0xFD, 0xFF, 0xFF,
                    0x70, 0xFE, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF, 0x6E, 0xF6, 0xFF, 0xFF,
                    0x3C, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x6E, 0xF6, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF,
                    0x4E, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3E,
                    0x42, 0x4D, 0x53, 0x20, 0x2D, 0x20, 0x46, 0x75, 0x6C, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x01, 0x01,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x05, 0x00, 0x00, 0x00,
                    0x28, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x42, 0x4D, 0x53, 0x20, 0x34, 0x2E, 0x33, 0x33, 0x2E, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0xFE, 0xF7, 0xFF, 0xFF
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
            byte[] keyFileName = Encoding.ASCII.GetBytes(appReg.getAutoKeyFileName().Replace(".key", ""));
            for (int i = 0; i <= 15; i++)
            {
                if (i >= keyFileName.Length)
                {
                    bs[288 + i] = 0x00;
                    continue;
                }
                bs[288 + i] = keyFileName[i];
            }

            // TrackIR Z-Axis(0:Z-axis 1:FOV)
            bs[336] = 0x00;
            if (mainWindow.Misc_TrackIRZ.IsChecked == true)
                bs[336] = 0x01;

            // External Mouselook
            bs[341] = 0x00;
            if (mainWindow.Misc_ExMouseLook.IsChecked == true)
                bs[341] = 0x01;

            // Natural Head Movement
            bs[342] = 0x00;
            if (mainWindow.Misc_NaturalHeadMovement.IsChecked == true)
                bs[342] = 0x01;

            // Roll-linked NWS
            bs[362] = 0x00;
            if (mainWindow.Misc_RollLinkedNWS.IsChecked == true)
                bs[362] = 0x01;

            // Smart Scaling
            bs[12] = 0x01;
            if (mainWindow.Misc_SmartScalingOverride.IsChecked == true)
                bs[12] = 0x05;

            // Smart Scaling
            bs[0] = 0x13;
            if (mainWindow.Misc_PilotModel.IsChecked == true)
                bs[0] = 0x33;

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
            AxisName.AI_vs_IVC,
            AxisName.HUD_Brightness,
            AxisName.FLIR_Brightness,
            AxisName.HMS_Brightness,
            AxisName.Reticle_Depression,
            AxisName.Camera_Distance,
            AxisName.HSI_Course_Knob,
            AxisName.HSI_Heading_Knob,
            AxisName.Altimeter_Knob
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
            AxisName.AI_vs_IVC,
            AxisName.FLIR_Brightness,
            AxisName.HSI_Course_Knob,
            AxisName.HSI_Heading_Knob,
            AxisName.Altimeter_Knob
        };
    }

}
