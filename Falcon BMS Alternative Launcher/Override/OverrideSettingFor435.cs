﻿using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using FalconBMS.Launcher.Input;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Override
{
    /// <summary>
    /// Writer for BMS4.35 setting Override
    /// </summary>
    public class OverrideSettingFor435 : OverrideSettingFor434U1
    {
        /// <summary>
        /// Writer for BMS4.35 setting Override
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="appReg"></param>
        public OverrideSettingFor435(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
        }

        protected override void SavePop()
        {
            string filename = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + appReg.GetPilotCallsign() + ".pop";
            if (!File.Exists(filename))
            {
                byte[] nbs = {
                    0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x42, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x64, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                    0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x1E, 0x1E, 0x7E, 0x3F,
                    0x1E, 0x1E, 0xFE, 0x3F, 0xD3, 0xD2, 0x9E, 0x41, 0x6A, 0x69, 0xB3, 0x42, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x64, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x63, 0xFF, 0xFF, 0xFF, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x6A, 0xF6, 0xFF, 0xFF,
                    0x3B, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x6A, 0xF6, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF,
                    0x47, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x63, 0xFF, 0xFF, 0xFF, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x6A, 0xF6, 0xFF, 0xFF,
                    0x3B, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x6A, 0xF6, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF,
                    0x47, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
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
            string fbackupname = appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER + appReg.GetPilotCallsign() + ".pop";
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
                    bs[336 + i] = 0x00;
                    continue;
                }
                bs[336 + i] = keyFileName[i];
            }

            // TrackIR Z-Axis(0:Z-axis 1:FOV)
            bs[336 + 48] = 0x00;
            if (mainWindow.Misc_TrackIRZ.IsChecked == true)
                bs[336 + 48] = 0x01;

            // External Mouselook
            bs[341 + 48] = 0x00;
            if (mainWindow.Misc_ExMouseLook.IsChecked == true)
                bs[341 + 48] = 0x01;

            // Natural Head Movement
            bs[342 + 48] = 0x00;
            if (mainWindow.Misc_NaturalHeadMovement.IsChecked == true)
                bs[342 + 48] = 0x01;

            // Roll-linked NWS
            bs[362 + 48] = 0x00;
            if (mainWindow.Misc_RollLinkedNWS.IsChecked == true)
                bs[362 + 48] = 0x01;

            // Smart Scaling
            bs[12] = 0x01;
            if (mainWindow.Misc_SmartScalingOverride.IsChecked == true)
                bs[12] = 0x05;

            // Pilot Model -- bit #6; NB: in v4.37 and later, 'Show Mirrors' is bit #7 so don't overwrite that.
            //TODO: consider adding separate checkbox for 'Show Mirrors'?
			      //TODO: consider AL should get out of the business of duplicating these settings, if BMS 2d UI is not dead code?
            bs[0] &= 0b01110011; //turn off bits 3,4 and 8.. maybe necessary if older pop file is ported forward from older BMS? not sure
            bs[0] |= 0b00010011; //turn on bits 1,2 and 5
            if (mainWindow.Misc_PilotModel.IsChecked == true)
                bs[0] |= 0b00100000; //turn on bit 6

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
            AxisName.Altimeter_Knob,
            AxisName.ILS_Volume_Knob
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
            AxisName.Altimeter_Knob,
            AxisName.ILS_Volume_Knob
        };
    }


}
