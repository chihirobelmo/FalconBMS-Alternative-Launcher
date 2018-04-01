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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public void SaveJoyAssignStatus()
        {
            //保存先のファイル名
            string fileName = "";

            System.Xml.Serialization.XmlSerializer serializer;
            System.IO.StreamWriter sw;

            for (int i = 0; i < devList.Count; i++)
            {
                fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + joyAssign[i].GetProductName().Replace("/", "-")
                + " {" + joyAssign[i].GetInstanceGUID().ToString().ToUpper() + "}.xml";

                serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn));
                sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));
                serializer.Serialize(sw, joyAssign[i]);

                sw.Close();
            }
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.MouseWheel.xml";

            serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn.AxAssgn));
            sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));
            serializer.Serialize(sw, mouseWheelAssign);

            sw.Close();
            
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.ThrottlePosition.xml";

            serializer = new System.Xml.Serialization.XmlSerializer(typeof(ThrottlePosition));
            sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));
            serializer.Serialize(sw, throttlePos);

            sw.Close();
        }

        public void SaveConfigfile()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/falcon bms.cfg";
            if (!System.IO.File.Exists(filename))
                return;
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/falcon bms.cfg";
            if (!System.IO.File.Exists(fbackupname))
                System.IO.File.Copy(filename, fbackupname, true);

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
            cfg.Write("set g_nHotasPinkyShiftMagnitude " + (devList.Count*32).ToString()
                + "                   // SETUP OVERRIDE\r\n");
            cfg.Write("set g_bHotasDgftSelfCancel " + Convert.ToInt32(this.Misc_OverrideSelfCancel.IsChecked)
                + "                         // SETUP OVERRIDE\r\n");
            cfg.Write("set g_b3DClickableCursorAnchored " + Convert.ToInt32(this.Misc_MouseCursorAnchor.IsChecked)
                + "                   // SETUP OVERRIDE\r\n");
            cfg.Close();
        }

        public void SaveDeviceSorting()
        {
            string deviceSort = "";
            for (int i = 0; i < devList.Count; i++)
                deviceSort += joyAssign[i].GetDeviceSortingLine();

            // BMS overwrites DeviceSorting.txt if was written in UTF-8.
            string filename = appReg.GetInstallDir() + "/User/Config/DeviceSorting.txt";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/DeviceSorting.txt";
            if ((!System.IO.File.Exists(fbackupname)) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);
            System.IO.StreamWriter ds = new System.IO.StreamWriter
                (filename, false, System.Text.Encoding.GetEncoding("shift_jis"));
            ds.Write(deviceSort);
            ds.Close();
        }

        public void SaveKeyMapping()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/BMS - Full.key";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/BMS - Full.key";
            if (!System.IO.File.Exists(fbackupname))
                System.IO.File.Copy(filename, fbackupname, true);
            System.IO.StreamWriter sw = new System.IO.StreamWriter
                (filename, false, System.Text.Encoding.GetEncoding("utf-8"));
            for (int i = 0; i < keyAssign.Length; i++)
                sw.Write(keyAssign[i].GetKeyLine());
            for (int i = 0; i < devList.Count; i++)
            {
                sw.Write(joyAssign[i].GetKeyLineDX(i, devList.Count));
                // PRIMARY DEVICE POV
                if (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == i) 
                    sw.Write(joyAssign[i].GetKeyLinePOV());
            }
            sw.Close();



            System.Windows.Forms.MessageBox.Show(
                keyAssign[1075].GetKeyLine()
                + "\n" + keyAssign[1076].GetKeyLine()
                + "\n" + keyAssign[1077].GetKeyLine()
                + "\n" + keyAssign[1078].GetKeyLine());

            System.Diagnostics.Process.Start(filename);



            filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign().ToString() + ".pop";
            //filename = filename.Replace("\0","");
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
            fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/" + appReg.GetPilotCallsign().ToString() + ".pop";
            if (!System.IO.File.Exists(fbackupname))
                System.IO.File.Copy(filename, fbackupname, true);
            System.IO.FileStream fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] bs = new byte[fs.Length];
            fs.Read(bs, 0, bs.Length);
            fs.Close();
            // 42 4D 53 20 2D 20 46 75 6C 6C 00 00 00 00 00 00 // BMS - FULL
            bs[288 + 0] = 0x42;
            bs[288 + 1] = 0x4D;
            bs[288 + 2] = 0x53;
            bs[288 + 3] = 0x20;
            bs[288 + 4] = 0x2D;
            bs[288 + 5] = 0x20;
            bs[288 + 6] = 0x46;
            bs[288 + 7] = 0x75;
            bs[288 + 8] = 0x6C;
            bs[288 + 9] = 0x6C;
            bs[288 + 10] = 0x00;
            bs[288 + 11] = 0x00;
            bs[288 + 12] = 0x00;
            bs[288 + 13] = 0x00;
            bs[288 + 14] = 0x00;
            bs[288 + 15] = 0x00;

            bs[336] = 0x00; // TrackIR Z-Axis(0:Z-axis 1:FOV)
            if (this.Misc_TrackIRZ.IsChecked == true)
                bs[336] = 0x01;

            bs[341] = 0x00; // External Mouselook
            if (this.Misc_ExMouseLook.IsChecked == true)
                bs[341] = 0x01;

            bs[362] = 0x00; // Roll-linked NWS
            if (this.Misc_RollLinkedNWS.IsChecked == true)
                bs[362] = 0x01;

            fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
        }

        public void SaveAxisMapping()
        {
            string[] axisMappingList = {
                "Pitch",
                "Roll",
                "Yaw",
                "Throttle",
                "Throttle_Right",
                "Toe_Brake",
                "Toe_Brake_Right",
                "FOV",
                "Trim_Pitch",
                "Trim_Yaw",
                "Trim_Roll",
                "Radar_Antenna_Elevation",
                "Range_Knob",
                "Cursor_X",
                "Cursor_Y",
                "COMM_Channel_1",
                "COMM_Channel_2",
                "MSL_Volume",
                "Threat_Volume",
                "Intercom",
                "AI_vs_IVC",
                "HUD_Brightness",
                "FLIR_Brightness",
                "HMS_Brightness",
                "Reticle_Depression",
                "Camera_Distance"
            };
            
            string filename = appReg.GetInstallDir() + "/User/Config/axismapping.dat";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/axismapping.dat";
            if ((!System.IO.File.Exists(fbackupname)) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);
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

                bs = joyAssign[(byte)((InGameAxAssgn)inGameAxis["Pitch"]).GetDeviceNumber()]
                    .GetInstanceGUID().ToByteArray();
                fs.Write(bs, 0, bs.Length);

                bs = new byte[] { (byte)devList.Count, 0x00, 0x00, 0x00 };
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
                bs[20] = (byte)devList.Count;
                fs.Write(bs, 0, bs.Length);
            }

            foreach (string nme in axisMappingList)
            {
                if (((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber() == -1)
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
                if (((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber() > -1)
                {
                    bs = new byte[] 
                    {
                        (byte)(((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber()+2),
                        0x00, 0x00, 0x00
                    };
                    fs.Write(bs, 0, bs.Length);
                    bs = new byte[] 
                    {
                        (byte)((InGameAxAssgn)inGameAxis[nme]).GetPhysicalNumber(),
                        0x00, 0x00, 0x00
                    };
                    fs.Write(bs, 0, bs.Length);
                }
                if (((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber() == -2)
                {
                    bs = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    fs.Write(bs, 0, bs.Length);
                }
                switch (((InGameAxAssgn)inGameAxis[nme]).GetDeadzone())
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
                switch (((InGameAxAssgn)inGameAxis[nme]).GetSaturation())
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

        public void SaveJoystickCal()
        {
            string[] JoystickCalList = {
                "Pitch",
                "Roll",
                "Yaw",
                "Throttle",
                "Throttle_Right",
                "Trim_Pitch",
                "Trim_Yaw",
                "Trim_Roll",
                "Toe_Brake",
                "Toe_Brake_Right",
                "FOV",
                "Radar_Antenna_Elevation",
                "Cursor_X",
                "Cursor_Y",
                "Range_Knob",
                "COMM_Channel_1",
                "COMM_Channel_2",
                "MSL_Volume",
                "Threat_Volume",
                "HUD_Brightness",
                "Reticle_Depression",
                "Camera_Distance",
                "Intercom",
                "HMS_Brightness",
                "AI_vs_IVC",
                "FLIR_Brightness"
            };

            string filename = appReg.GetInstallDir() + "/User/Config/joystick.cal";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/joystick.cal";
            if ((!System.IO.File.Exists(fbackupname)) & (System.IO.File.Exists(filename)))
                System.IO.File.Copy(filename, fbackupname, true);
            System.IO.FileStream fs = new System.IO.FileStream
                (filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            byte[] bs;

            foreach (string nme in JoystickCalList)
            {
                bs = new byte[] 
                {
                    0x00, 0x00, 0x00, 0x00, 0x98, 0x3A, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00
                };
                if (((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber() != -1)
                {
                    bs[12] = 0x01;

                    if (nme == "Throttle")
                    {
                        double iAB = (double)throttlePos.GetAB();
                        double iIdle = (double)throttlePos.GetIDLE();

                        const double MAXIN = 65536;
                        const double MAXOUT = 14848;

                        iAB = -iAB * (MAXOUT / MAXIN) + MAXOUT;
                        iIdle = -iIdle * (MAXOUT / MAXIN) + MAXOUT;

                        byte[] ab = BitConverter.GetBytes((int)iAB);
                        byte[] idle = BitConverter.GetBytes((int)iIdle);
                        
                        bs[1] = ab[1];
                        bs[5] = idle[1];

                        if (throttlePos.GetAB() > (65536 - 256))
                            bs[1] = 0x00;
                        if (throttlePos.GetIDLE() < 256)
                            bs[5] = 0x3A;
                    }
                }
                if (((InGameAxAssgn)inGameAxis[nme]).GetInvert())
                {
                    bs[20] = 0x01;
                }
                fs.Write(bs, 0, bs.Length);
            }
            fs.Close();
        }
    }
}
