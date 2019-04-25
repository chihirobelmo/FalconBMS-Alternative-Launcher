using MahApps.Metro.Controls;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FalconBMS_Alternative_Launcher_Cs
{
    /// <summary>
    /// Writer for setting Override
    /// </summary>
    public class OverrideSetting
    {
        protected MainWindow mainWindow;
        protected AppRegInfo appReg;

        /// <summary>
        /// Writer for setting Override
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="appReg"></param>
        public OverrideSetting(MainWindow mainWindow, AppRegInfo appReg)
        {
            this.mainWindow = mainWindow;
            this.appReg = appReg;
        }

        /// <summary>
        /// Execute setting override.
        /// </summary>
        /// <param name="inGameAxis"></param>
        /// <param name="deviceControl"></param>
        /// <param name="keyFile"></param>
        /// <param name="visualAcuity"></param>
        public void Execute(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile)
        {
            if (!System.IO.Directory.Exists(appReg.GetInstallDir() + "/User/Config/Backup/"))
                System.IO.Directory.CreateDirectory(appReg.GetInstallDir() + "/User/Config/Backup/");

            SaveAxisMapping(inGameAxis, deviceControl);
            SaveJoystickCal(inGameAxis, deviceControl);
            SaveDeviceSorting(deviceControl);
            SaveConfigfile(deviceControl);
            SaveKeyMapping(inGameAxis, deviceControl, keyFile);
            SavePop();
            SaveJoyAssignStatus(deviceControl);
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected void SaveJoyAssignStatus(DeviceControl deviceControl)
        {
            //保存先のファイル名
            string fileName = "";

            System.Xml.Serialization.XmlSerializer serializer;
            System.IO.StreamWriter sw;

            for (int i = 0; i < deviceControl.devList.Count; i++)
            {
                fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + deviceControl.joyAssign[i].GetProductName().Replace("/", "-")
                + " {" + deviceControl.joyAssign[i].GetInstanceGUID().ToString().ToUpper() + "}.xml";

                serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn));
                sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));
                serializer.Serialize(sw, deviceControl.joyAssign[i]);

                sw.Close();
            }
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.MouseWheel.xml";

            serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn.AxAssgn));
            sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));
            serializer.Serialize(sw, deviceControl.mouseWheelAssign);

            sw.Close();
            
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.ThrottlePosition.xml";

            serializer = new System.Xml.Serialization.XmlSerializer(typeof(ThrottlePosition));
            sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));
            serializer.Serialize(sw, deviceControl.throttlePos);

            sw.Close();
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected void SaveConfigfile(DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/falcon bms.cfg";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/falcon bms.cfg";
            if (!System.IO.File.Exists(fbackupname) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);

            if (System.IO.File.Exists(filename))
                System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.StreamReader cReader = new System.IO.StreamReader
                (filename, System.Text.Encoding.Default);
            string stResult = "";
            while (cReader.Peek() >= 0)
            {
                string stBuffer = cReader.ReadLine();
                if (stBuffer.Contains("// SETUP OVERRIDE"))
                    continue;
                stResult += stBuffer + "\r\n";
            }
            cReader.Close();
            
            System.IO.StreamWriter cfg = new System.IO.StreamWriter
                (filename, false, System.Text.Encoding.GetEncoding("shift_jis"));
            cfg.Write(stResult);
            cfg.Write("set g_nHotasPinkyShiftMagnitude " + (deviceControl.devList.Count*32).ToString()
                + "          // SETUP OVERRIDE\r\n");
            cfg.Write("set g_bHotasDgftSelfCancel " + Convert.ToInt32(mainWindow.Misc_OverrideSelfCancel.IsChecked)
                + "          // SETUP OVERRIDE\r\n");
            cfg.Write("set g_b3DClickableCursorAnchored " + Convert.ToInt32(mainWindow.Misc_MouseCursorAnchor.IsChecked)
                + "          // SETUP OVERRIDE\r\n");
            cfg.Close();
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected void SaveDeviceSorting(DeviceControl deviceControl)
        {
            string deviceSort = "";
            for (int i = 0; i < deviceControl.devList.Count; i++)
                deviceSort += deviceControl.joyAssign[i].GetDeviceSortingLine();

            // BMS overwrites DeviceSorting.txt if was written in UTF-8.
            string filename = appReg.GetInstallDir() + "/User/Config/DeviceSorting.txt";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/DeviceSorting.txt";
            if ((!System.IO.File.Exists(fbackupname)) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);

            if (System.IO.File.Exists(filename))
                System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.StreamWriter ds = new System.IO.StreamWriter
                (filename, false, System.Text.Encoding.GetEncoding("shift_jis"));
            ds.Write(deviceSort);
            ds.Close();
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected void SaveKeyMapping(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.getKeyFileName();
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/" + appReg.getKeyFileName();
            if (!System.IO.File.Exists(fbackupname) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);

            if (System.IO.File.Exists(filename))
                System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.StreamWriter sw = new System.IO.StreamWriter
                (filename, false, System.Text.Encoding.GetEncoding("utf-8"));
            for (int i = 0; i < keyFile.keyAssign.Length; i++)
                sw.Write(keyFile.keyAssign[i].GetKeyLine());
            for (int i = 0; i < deviceControl.devList.Count; i++)
            {
                sw.Write(deviceControl.joyAssign[i].GetKeyLineDX(i, deviceControl.devList.Count));
                // PRIMARY DEVICE POV
                if (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == i) 
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV());
            }
            sw.Close();
        }

        /// <summary>
        /// Overwrite callsign.pop file.
        /// </summary>
        protected virtual void SavePop()
        {
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected void SaveAxisMapping(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/axismapping.dat";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/axismapping.dat";
            if ((!System.IO.File.Exists(fbackupname)) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);

            if (System.IO.File.Exists(filename))
                System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.FileStream fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            byte[] bs;
            
            if (((InGameAxAssgn)inGameAxis["Pitch"]).GetDeviceNumber() > -1)
            {
                bs = new byte[] 
                {
                    (byte)(((InGameAxAssgn)inGameAxis["Pitch"]).GetDeviceNumber()+2),
                    0x00, 0x00, 0x00
                };
                fs.Write(bs, 0, bs.Length);

                bs = deviceControl.joyAssign[(byte)((InGameAxAssgn)inGameAxis["Pitch"]).GetDeviceNumber()]
                    .GetInstanceGUID().ToByteArray();
                fs.Write(bs, 0, bs.Length);

                bs = new byte[] { (byte)deviceControl.devList.Count, 0x00, 0x00, 0x00 };
                fs.Write(bs, 0, bs.Length);
            }
            else
            {
                bs = new byte[] 
                {
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };
                bs[20] = (byte)deviceControl.devList.Count;
                fs.Write(bs, 0, bs.Length);
            }

            AxisName[] localAxisMappingList = this.getAxisMappingList();
            foreach (AxisName nme in localAxisMappingList)
            {
                if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() == -1)
                {
                    bs = new byte[] 
                    {
                        0xFF, 0xFF, 0xFF, 0xFF,
                        0xFF, 0xFF, 0xFF, 0xFF,
                        0x64, 0x00, 0x00, 0x00,
                        0xFF, 0xFF, 0xFF, 0xFF
                    };
                    fs.Write(bs, 0, bs.Length);
                    continue;
                }
                if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() > -1)
                {
                    bs = new byte[] 
                    {
                        (byte)(((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber()+2),
                        0x00, 0x00, 0x00
                    };
                    fs.Write(bs, 0, bs.Length);
                    bs = new byte[] 
                    {
                        (byte)((InGameAxAssgn)inGameAxis[nme.ToString()]).GetPhysicalNumber(),
                        0x00, 0x00, 0x00
                    };
                    fs.Write(bs, 0, bs.Length);
                }
                if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() == -2)
                {
                    bs = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    fs.Write(bs, 0, bs.Length);
                }
                switch (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeadzone())
                {
                    case AxCurve.None:
                        bs = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                        break;
                    case AxCurve.Small:
                        bs = new byte[] { 0x64, 0x00, 0x00, 0x00 };
                        break;
                    case AxCurve.Medium:
                        bs = new byte[] { 0xF4, 0x01, 0x00, 0x00 };
                        break;
                    case AxCurve.Large:
                        bs = new byte[] { 0xE8, 0x03, 0x00, 0x00 };
                        break;
                }
                fs.Write(bs, 0, bs.Length);
                switch (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetSaturation())
                {
                    case AxCurve.None:
                        bs = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                        break;
                    case AxCurve.Small:
                        bs = new byte[] { 0x1C, 0x25, 0x00, 0x00 };
                        break;
                    case AxCurve.Medium:
                        bs = new byte[] { 0x28, 0x23, 0x00, 0x00 };
                        break;
                    case AxCurve.Large:
                        bs = new byte[] { 0x34, 0x21, 0x00, 0x00 };
                        break;
                }
                fs.Write(bs, 0, bs.Length);
            }
            fs.Close();
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected virtual void SaveJoystickCal(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/joystick.cal";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/joystick.cal";
            if ((!System.IO.File.Exists(fbackupname)) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);

            if (System.IO.File.Exists(filename))
                System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.FileStream fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            byte[] bs;

            AxisName[] localJoystickCalList = appReg.getOverrideWriter().getJoystickCalList();
            foreach (AxisName nme in localJoystickCalList)
            {
                bs = new byte[] 
                {
                    0x00, 0x00, 0x00, 0x00, 0x98, 0x3A, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00
                };
                if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() != -1)
                {
                    bs[12] = 0x01;

                    if (nme == AxisName.Throttle)
                    {
                        double iAB = (double)deviceControl.throttlePos.GetAB();
                        double iIdle = (double)deviceControl.throttlePos.GetIDLE();

                        const double MAXIN = 65536;
                        const double MAXOUT = 14848;

                        iAB = -iAB * (MAXOUT / MAXIN) + MAXOUT;
                        iIdle = -iIdle * (MAXOUT / MAXIN) + MAXOUT;

                        byte[] ab = BitConverter.GetBytes((int)iAB);
                        byte[] idle = BitConverter.GetBytes((int)iIdle);
                        
                        bs[1] = ab[1];
                        bs[5] = idle[1];

                        if (deviceControl.throttlePos.GetAB() > (65536 - 256))
                            bs[1] = 0x00;
                        if (deviceControl.throttlePos.GetIDLE() < 256)
                            bs[5] = 0x3A;
                    }
                }
                if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetInvert())
                {
                    bs[20] = 0x01;
                }
                fs.Write(bs, 0, bs.Length);
            }
            fs.Close();
        }

        public virtual AxisName[] getAxisMappingList() { return this.axisMappingList; }
        public virtual AxisName[] getJoystickCalList() { return this.joystickCalList; }

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
            AxisName.AI_vs_IVC,
            AxisName.FLIR_Brightness
        };
    }

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
            mainWindow.Misc_Platform.IsChecked = false;
            mainWindow.Misc_Platform.Visibility = Visibility.Hidden;
            mainWindow.Label_Platform.Content = "Platform : BMS 4.32 is 32-bit apprecation.";

            mainWindow.Launch_AVC.Visibility = Visibility.Hidden;
            mainWindow.Label_AVC.Visibility = Visibility.Hidden;

            mainWindow.Name_FLIR_Brightness.Visibility = Visibility.Hidden;
            mainWindow.Label_FLIR_Brightness.Visibility = Visibility.Hidden;
            mainWindow.Axis_FLIR_Brightness.Visibility = Visibility.Hidden;
            mainWindow.FLIR_Brightness.Visibility = Visibility.Hidden;

            mainWindow.Name_AI_vs_IVC.Visibility = Visibility.Hidden;
            mainWindow.Label_AI_vs_IVC.Visibility = Visibility.Hidden;
            mainWindow.Axis_AI_vs_IVC.Visibility = Visibility.Hidden;
            mainWindow.AI_vs_IVC.Visibility = Visibility.Hidden;

            mainWindow.Tab_HSI_and_Altimeter.Visibility = Visibility.Collapsed;
        }

        protected override void SavePop()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign().ToString() + ".pop";
            if (!System.IO.File.Exists(filename))
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
                System.IO.FileStream nfs = new System.IO.FileStream
                    (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                nfs.Write(nbs, 0, nbs.Length);
                nfs.Close();
            }
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/" + appReg.GetPilotCallsign().ToString() + ".pop";
            if (!System.IO.File.Exists(fbackupname))
                System.IO.File.Copy(filename, fbackupname, true);

            System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.FileStream fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] bs = new byte[fs.Length];
            fs.Read(bs, 0, bs.Length);
            fs.Close();

            // Set Keyfile selected.
            byte[] keyFileName = System.Text.Encoding.ASCII.GetBytes(appReg.getKeyFileName().Replace(".key", ""));
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

            fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
        }

        public override AxisName[] getAxisMappingList() { return this.axisMappingList; }
        public override AxisName[] getJoystickCalList() { return this.joystickCalList; }

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

    /// <summary>
    /// Writer for BMS4.33 setting Override
    /// </summary>
    public class OverrideSettingFor433 : OverrideSetting
    {
        /// <summary>
        /// Writer for BMS4.33 setting Override
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="appReg"></param>
        public OverrideSettingFor433(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
            mainWindow.Tab_HSI_and_Altimeter.Visibility = Visibility.Collapsed;
        }

        protected override void SavePop()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign().ToString() + ".pop";
            if (!System.IO.File.Exists(filename))
            {
                byte[] nbs = {
                    0x1B, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x42, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x63, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                    0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x1E, 0x1E, 0x7E, 0x3F,
                    0x1E, 0x1E, 0xFE, 0x3F, 0xD3, 0xD2, 0x9E, 0x41, 0x6A, 0x69, 0xB3, 0x42, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x64, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x04, 0xFD, 0xFF, 0xFF, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x6A, 0xF6, 0xFF, 0xFF,
                    0x50, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x6A, 0xF6, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF,
                    0x47, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0xFD, 0xFF, 0xFF, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x6A, 0xF6, 0xFF, 0xFF,
                    0x50, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x6A, 0xF6, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF,
                    0x47, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                    0x42, 0x4D, 0x53, 0x20, 0x2D, 0x20, 0x46, 0x75, 0x6C, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x01, 0x01,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x05, 0x00, 0x00, 0x00,
                    0x28, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x42, 0x4D, 0x53, 0x20, 0x34, 0x2E, 0x33, 0x33, 0x2E, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0xF0, 0xD8, 0xFF, 0xFF
                };
                System.IO.FileStream nfs = new System.IO.FileStream
                    (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                nfs.Write(nbs, 0, nbs.Length);
                nfs.Close();
            }
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/" + appReg.GetPilotCallsign().ToString() + ".pop";
            if (!System.IO.File.Exists(fbackupname))
                System.IO.File.Copy(filename, fbackupname, true);

            System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.FileStream fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] bs = new byte[fs.Length];
            fs.Read(bs, 0, bs.Length);
            fs.Close();

            // Set Keyfile selected.
            byte[] keyFileName = System.Text.Encoding.ASCII.GetBytes(appReg.getKeyFileName().Replace(".key", ""));
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

            // Roll-linked NWS
            bs[362] = 0x00; 
            if (mainWindow.Misc_RollLinkedNWS.IsChecked == true)
                bs[362] = 0x01;

            // Smart Scaling
            bs[12] = 0x01;
            if (mainWindow.Misc_SmartScalingOverride.IsChecked == true)
                bs[12] = 0x05;

            fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
        }
    }

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
            mainWindow.Misc_Platform.IsChecked = true;
            mainWindow.Misc_Platform.Visibility = Visibility.Hidden;
            mainWindow.Label_Platform.Content = "Platform : BMS 4.34 is 64-bit apprecation.";

            mainWindow.CMD_BW.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected override void SaveJoystickCal(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/joystick.cal";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/joystick.cal";
            if ((!System.IO.File.Exists(fbackupname)) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);

            if (System.IO.File.Exists(filename))
                System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.FileStream fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);

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
                        double iAB = (double)deviceControl.throttlePos.GetAB();
                        double iIdle = (double)deviceControl.throttlePos.GetIDLE();

                        const double MAXIN = 65536;
                        const double MAXOUT = 14848;

                        iAB = -iAB * (MAXOUT / MAXIN) + MAXOUT;
                        iIdle = -iIdle * (MAXOUT / MAXIN) + MAXOUT;

                        byte[] ab = BitConverter.GetBytes((int)iAB);
                        byte[] idle = BitConverter.GetBytes((int)iIdle);

                        bs[1] = ab[1];
                        bs[5] = idle[1];

                        if (deviceControl.throttlePos.GetAB() > (65536 - 256))
                            bs[1] = 0x00;
                        if (deviceControl.throttlePos.GetIDLE() < 256)
                            bs[5] = 0x3A;
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
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign().ToString() + ".pop";
            if (!System.IO.File.Exists(filename))
            {
                byte[] nbs = {
                    0x1B, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x42, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x63, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00,
                    0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x40, 0x00, 0x00, 0x80, 0x3F, 0x1E, 0x1E, 0x7E, 0x3F,
                    0x1E, 0x1E, 0xFE, 0x3F, 0xD3, 0xD2, 0x9E, 0x41, 0x6A, 0x69, 0xB3, 0x42, 0x01, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                    0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x64, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x04, 0xFD, 0xFF, 0xFF, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x6A, 0xF6, 0xFF, 0xFF,
                    0x50, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x6A, 0xF6, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF,
                    0x47, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0xFD, 0xFF, 0xFF, 0x04, 0xFD, 0xFF, 0xFF,
                    0x6C, 0xFE, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0xB6, 0xFF, 0xFF, 0xFF, 0x6A, 0xF6, 0xFF, 0xFF,
                    0x50, 0xFB, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x6A, 0xF6, 0xFF, 0xFF, 0xA4, 0xED, 0xFF, 0xFF,
                    0x47, 0xF3, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F,
                    0x42, 0x4D, 0x53, 0x20, 0x2D, 0x20, 0x46, 0x75, 0x6C, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x01, 0x01,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x05, 0x00, 0x00, 0x00,
                    0x28, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x42, 0x4D, 0x53, 0x20, 0x34, 0x2E, 0x33, 0x33, 0x2E, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0xF0, 0xD8, 0xFF, 0xFF
                };
                System.IO.FileStream nfs = new System.IO.FileStream
                    (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                nfs.Write(nbs, 0, nbs.Length);
                nfs.Close();
            }
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/" + appReg.GetPilotCallsign().ToString() + ".pop";
            if (!System.IO.File.Exists(fbackupname))
                System.IO.File.Copy(filename, fbackupname, true);

            System.IO.File.SetAttributes(filename, System.IO.File.GetAttributes(filename) & (~System.IO.FileAttributes.ReadOnly));

            System.IO.FileStream fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] bs = new byte[fs.Length];
            fs.Read(bs, 0, bs.Length);
            fs.Close();

            // Set Keyfile selected.
            byte[] keyFileName = System.Text.Encoding.ASCII.GetBytes(appReg.getKeyFileName().Replace(".key", ""));
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

            // Roll-linked NWS
            bs[362] = 0x00;
            if (mainWindow.Misc_RollLinkedNWS.IsChecked == true)
                bs[362] = 0x01;

            // Smart Scaling
            bs[12] = 0x01;
            if (mainWindow.Misc_SmartScalingOverride.IsChecked == true)
                bs[12] = 0x05;

            fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
        }

        public override AxisName[] getAxisMappingList() { return this.axisMappingList; }
        public override AxisName[] getJoystickCalList() { return this.joystickCalList; }

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
            AxisName.HSI_Course_knob,
            AxisName.HSI_Heading_knob,
            AxisName.Altimeter_knob
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
            AxisName.HSI_Course_knob,
            AxisName.HSI_Heading_knob,
            AxisName.Altimeter_knob
        };
    }

    /// <summary>
    /// Writer for BMS4.35 setting Override
    /// </summary>
    public class OverrideSettingFor435 : OverrideSetting
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
        { }
    }

    /// <summary>
    /// Writer for BMS4.35 setting Override
    /// </summary>
    public class OverrideSettingForUNDEFINED : OverrideSetting
    {
        /// <summary>
        /// Writer for UNDEFINED BMS VERSION setting Override
        /// </summary>
        /// <param name="mainWindow"></param>
        /// <param name="appReg"></param>
        public OverrideSettingForUNDEFINED(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
            mainWindow.Misc_TrackIRZ.Visibility = Visibility.Hidden;
            mainWindow.Misc_ExMouseLook.Visibility = Visibility.Hidden;
            mainWindow.Misc_RollLinkedNWS.Visibility = Visibility.Hidden;
        }

        protected override void SavePop()
        {
        }
    }
}
