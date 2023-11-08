using System;
using System.Collections;
using System.Diagnostics;
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
                + " " + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
        }

        protected override void SaveJoystickCal(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + "joystick.cal";
            string fbackupname = appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER + "joystick.cal";

            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            FileStream fs = File.Create(filename);

            AxisName[] localJoystickCalList = appReg.getOverrideWriter().getJoystickCalList();
            foreach (AxisName nme in localJoystickCalList)
            {
                InGameAxAssgn currentAxis = (InGameAxAssgn)inGameAxis[nme.ToString()];

                byte[] bs = { 
                    0x00, 0x00, 0x00, 0x00, 0x98, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                if (currentAxis.IsAssigned() && !isRollLinkedNWSEnabled(nme))
                {
                    bs[20] = 0x01;
                    bs[21] = (byte)(currentAxis.GetInvert() ? 0x01 : 0x00);

                    if ((nme == AxisName.Throttle || nme == AxisName.Throttle_Right) && currentAxis.IsJoyAssigned())
                    {
                        // Scale: [0,65535] logical (0==fully-idle; 65536==max-burner; regardless of normal vs reverse)
                        double fAB = deviceControl.GetJoystickMappingsForAxes()[currentAxis.GetDeviceNumber()].detentPosition.GetAB();
                        double fIdle = deviceControl.GetJoystickMappingsForAxes()[currentAxis.GetDeviceNumber()].detentPosition.GetIDLE();

                        //BUGFIX: right-throttle needs to mirror the identical idle/ab detents, from primary throttle.
                        if (nme == AxisName.Throttle_Right)
                        {
                            InGameAxAssgn leftThrottleAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Throttle.ToString()];

                            fAB = deviceControl.GetJoystickMappingsForAxes()[leftThrottleAxis.GetDeviceNumber()].detentPosition.GetAB();
                            fIdle = deviceControl.GetJoystickMappingsForAxes()[leftThrottleAxis.GetDeviceNumber()].detentPosition.GetIDLE();
                        }

                        // Adjust logical scale to [0,15000]
                        fAB = fAB * CommonConstants.BINAXISMAX / CommonConstants.AXISMAX;
                        fIdle = fIdle * CommonConstants.BINAXISMAX / CommonConstants.AXISMAX;

                        InGameAxAssgn axis = (InGameAxAssgn)MainWindow.inGameAxis[nme.ToString()];

                        //NB: as of 4.37.3, the detent values in joystick.cal are logical-scale -- they don't vary normal vs reverse.
                        //But, notably, they are still recorded in inverse-scale.. [0,65536] => [15000,0]
                        fAB = CommonConstants.BINAXISMAX - fAB;
                        fIdle = CommonConstants.BINAXISMAX - fIdle;

                        // Ensure detents are firmly gated within [0,15000].
                        int iAB = (int)Math.Round(fAB);
                        int iIdle = (int)Math.Round(fIdle);

                        iAB = Math.Min(Math.Max(0, iAB), 15000);
                        iIdle = Math.Min(Math.Max(0, iIdle), 15000);

                        // Little-endian byte-order encoding: first dword is AB-detent; second dword is Idle-detent.
                        byte iAB0 = (byte)(iAB & 0x00FF);
                        byte iAB1 = (byte)((iAB & 0xFF00) >> 8);

                        byte iIdle0 = (byte)(iIdle & 0x00FF);
                        byte iIdle1 = (byte)((iIdle & 0xFF00) >> 8);

                        bs[0] = iAB0;
                        bs[1] = iAB1;
                        bs[2] = 0;
                        bs[3] = 0;

                        bs[4] = iIdle0;
                        bs[5] = iIdle1;
                        bs[6] = 0;
                        bs[7] = 0;
                    }
                }
                fs.Write(bs, 0, bs.Length);
            }
            fs.Close();
        }

        protected override void SavePop()
        {
            string filename = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + appReg.GetPilotCallsign() + ".pop";
            if (!File.Exists(filename))
            {
                byte[] nbs = { //NB: fresh Viper.pop bits as generated by BMS 4.37.2
                    0x53, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x42, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x64, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                    0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xA0, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x1E, 0x1E, 0x7E, 0x3F,
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
                    0x47, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x12, 0xFD, 0xFF, 0xFF,
                    0x70, 0xFE, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00,
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
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
                    0xFE, 0xF7, 0xFF, 0xFF
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
            byte[] keyFileName = Encoding.ASCII.GetBytes(CommonConstants.BMS_AUTO);
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
            //TODO: consider adding separate checkbox for 'Show Mirrors'
            //TODO: consider AL should get out of the business of duplicating these settings, if BMS 2d UI is not dead code!
            bs[0] &= 0b01110011; //turn off bits 3,4 and 8.. maybe necessary if older pop file is ported forward from older BMS? not sure
            bs[0] |= 0b00010011; //turn on bits 1,2 and 5
            if (mainWindow.Misc_PilotModel.IsChecked == true)
                bs[0] |= 0b00100000; //mask-on bit 6
            else
                bs[0] &= 0b11011111; //mask-off bit 6

            fs = new FileStream
                (filename, FileMode.Create, FileAccess.Write);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
        }

    }
}
