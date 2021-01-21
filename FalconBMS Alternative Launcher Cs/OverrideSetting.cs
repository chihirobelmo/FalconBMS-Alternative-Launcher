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
            if (!Directory.Exists(appReg.GetInstallDir() + "/User/Config/Backup/"))
                Directory.CreateDirectory(appReg.GetInstallDir() + "/User/Config/Backup/");

            SaveAxisMapping(inGameAxis, deviceControl);
            SaveJoystickCal(inGameAxis, deviceControl);
            SaveDeviceSorting(deviceControl);
            SaveConfigfile(inGameAxis, deviceControl);
            SaveKeyMapping(inGameAxis, deviceControl, keyFile);
            //SavePlcLbk();
            SavePop();
            SaveWindowConfig();
            SaveJoyAssignStatus(deviceControl);
        }

        protected void SaveWindowConfig()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/windowconfig.dat";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/windowconfig.dat";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);
            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            if (!File.Exists(filename))
            {
                byte[] nbs = {
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x70, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x30, 0x02, 0x00, 0x00, 0x30, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xEC, 0x00, 0x00, 0x00, 0xEC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x90, 0x01, 0x00, 0x00, 0x8C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x90, 0x01, 0x00, 0x00, 0x8C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };
                FileStream nfs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                nfs.Write(nbs, 0, nbs.Length);
                nfs.Close();
            }

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

            byte[] bs = new byte[fs.Length];
            fs.Read(bs, 0, bs.Length);

            fs.Close();

            bs[4] = 0x00;

            FileStream nfs2 = new FileStream(filename, FileMode.Create, FileAccess.Write);
            nfs2.Write(bs, 0, bs.Length);
            nfs2.Close();
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected void SaveJoyAssignStatus(DeviceControl deviceControl)
        {
            //保存先のファイル名
            string fileName = "";

            System.Xml.Serialization.XmlSerializer serializer;
            StreamWriter sw;

            for (int i = 0; i < deviceControl.devList.Count; i++)
            {
                fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + deviceControl.joyAssign[i].GetProductName().Replace("/", "-")
                + " {" + deviceControl.joyAssign[i].GetInstanceGUID().ToString().ToUpper() + "}.xml";

                serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn));
                sw = new StreamWriter(fileName, false, new UTF8Encoding(false));
                serializer.Serialize(sw, deviceControl.joyAssign[i]);

                sw.Close();
            }
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.MouseWheel.xml";

            serializer = new System.Xml.Serialization.XmlSerializer(typeof(AxAssgn));
            sw = new StreamWriter(fileName, false, new UTF8Encoding(false));
            serializer.Serialize(sw, deviceControl.mouseWheelAssign);

            sw.Close();
            
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.ThrottlePosition.xml";

            serializer = new System.Xml.Serialization.XmlSerializer(typeof(ThrottlePosition));
            sw = new StreamWriter(fileName, false, new UTF8Encoding(false));
            serializer.Serialize(sw, deviceControl.throttlePos);

            sw.Close();
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected virtual void SaveConfigfile(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/falcon bms.cfg";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/falcon bms.cfg";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamReader cReader = new StreamReader
                (filename, Encoding.Default);
            string stResult = "";
            while (cReader.Peek() >= 0)
            {
                string stBuffer = cReader.ReadLine();
                if (stBuffer.Contains("// SETUP OVERRIDE"))
                    continue;
                stResult += stBuffer + "\r\n";
            }
            cReader.Close();
            
            StreamWriter cfg = new StreamWriter
                (filename, false, Encoding.GetEncoding("shift_jis"));
            cfg.Write(stResult);
            cfg.Write("set g_nHotasPinkyShiftMagnitude " + deviceControl.devList.Count*32
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
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamWriter ds = new StreamWriter
                (filename, false, Encoding.GetEncoding("shift_jis"));
            ds.Write(deviceSort);
            ds.Close();
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected virtual void SaveKeyMapping(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.getKeyFileName();
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/" + appReg.getKeyFileName();
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamWriter sw = new StreamWriter
                (filename, false, Encoding.GetEncoding("utf-8"));
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
        /// Overwrite callsign.plc and callsign.lbk file. (Perhaps this might be only valid for 4.34)
        /// </summary>
        protected void SavePlcLbk()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign() + ".plc";
            if (!File.Exists(filename))
            {
                byte[] nbs = {
                    0x9C, 0x02, 0x91, 0x0D, 0x9D, 0x0C, 0xD3, 0x45, 0xC9, 0x16, 0x90, 0x00, 0x8A, 0x07, 0xD8, 0x6A,
                    0xF4, 0x78, 0xF3, 0x69, 0xE4, 0x5D, 0xC3, 0xAF
                };
                FileStream nfs = new FileStream
                    (filename, FileMode.Create, FileAccess.Write);
                nfs.Write(nbs, 0, nbs.Length);
                nfs.Close();
            }

            filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign() + ".lbk";
            if (!File.Exists(filename))
            {
                byte[] nbs = {
                    0x54, 0x5A, 0x53, 0x10, 0x2F, 0x28, 0x64, 0x62, 0x65, 0x45, 0x3C, 0x53, 0x26, 0x54, 0x74, 0x39,
                    0x58, 0x2B, 0x5F, 0x3A, 0x48, 0x58, 0x50, 0x4C, 0x4A, 0x57, 0x39, 0x19, 0x70, 0x03, 0x23, 0x5A,
                    0x35, 0x40, 0x37, 0x17, 0x5A, 0x3B, 0x48, 0x3C, 0x59, 0x2B, 0x6D, 0x0C, 0x60, 0x33, 0x69, 0x28,
                    0x38, 0x60, 0x3C, 0x2D, 0x6D, 0x02, 0x77, 0x05, 0x25, 0x68, 0x5F, 0x45, 0x41, 0x41, 0x41, 0x07,
                    0x12, 0x7E, 0x1D, 0x72, 0x1C, 0x3C, 0x55, 0x26, 0x06, 0x7F, 0x10, 0x65, 0x17, 0x37, 0xFA, 0xA4,
                    0xD7, 0xA3, 0xC6, 0xB4, 0xF2, 0x93, 0xFF, 0x9C, 0xF3, 0x9D, 0xBD, 0xD4, 0xA7, 0x87, 0xFE, 0x91,
                    0xE4, 0x96, 0xB6, 0xFB, 0x9A, 0xE9, 0x9D, 0xF8, 0x8A, 0xCC, 0xAD, 0xC1, 0xA2, 0xCD, 0xA3, 0x83,
                    0xEA, 0x99, 0xB9, 0xC0, 0xAF, 0xDA, 0xA8, 0x88, 0xC5, 0xA4, 0xD7, 0xA3, 0xC6, 0xB4, 0xF2, 0x93,
                    0xFF, 0x9C, 0xF3, 0x9D, 0xBD, 0xD4, 0xA7, 0x87, 0xFE, 0x91, 0xE4, 0x96, 0xB6, 0xFB, 0x9A, 0xE9,
                    0x9D, 0xF8, 0x8A, 0xCC, 0xCD, 0x4B, 0x28, 0x47, 0x29, 0x09, 0x60, 0x13, 0x33, 0x4A, 0x25, 0x50,
                    0x22, 0x02, 0x4F, 0x2E, 0x5D, 0x29, 0x4C, 0x3E, 0x78, 0x19, 0x75, 0x16, 0x79, 0x17, 0x37, 0x5E,
                    0x2D, 0x0D, 0x74, 0x1B, 0x6E, 0x1C, 0x3C, 0x71, 0x10, 0x63, 0x17, 0x72, 0xA2, 0xF5, 0x95, 0xF9,
                    0x9A, 0xF5, 0x9B, 0xBB, 0xD2, 0xA1, 0x81, 0xF8, 0x97, 0xE2, 0x90, 0xB0, 0xFD, 0x9C, 0xEF, 0x9B,
                    0xFE, 0x8C, 0xCA, 0xAB, 0xC7, 0xA4, 0xCB, 0xA5, 0x85, 0xEC, 0x9F, 0xBF, 0xC6, 0xA9, 0xDC, 0xAE,
                    0x8E, 0xC3, 0xA2, 0xD1, 0xA5, 0xC0, 0xB2, 0xF4, 0x95, 0xF9, 0x9A, 0xF5, 0x9B, 0xBB, 0xD2, 0xA1,
                    0x81, 0xF8, 0x97, 0xE2, 0x90, 0xB0, 0xFD, 0x9C, 0xEF, 0x9B, 0xFE, 0x8C, 0xCA, 0xAB, 0xC7, 0xA4,
                    0xCB, 0xA5, 0x85, 0xEC, 0x9F, 0xBF, 0xC6, 0xA9, 0xDC, 0xAE, 0x8E, 0xC3, 0xA2, 0xD1, 0xA5, 0xC0,
                    0xB2, 0xF4, 0x95, 0xF9, 0x9A, 0xF5, 0x9B, 0xBB, 0xD2, 0xA1, 0x81, 0xF8, 0x97, 0xE2, 0x90, 0xB0,
                    0xFD, 0x9C, 0xEF, 0x9B, 0xFE, 0x8C, 0xCA, 0xAB, 0xC7, 0xA4, 0xCB, 0xA5, 0x85, 0xEC, 0x9F, 0xBF,
                    0xC6, 0xA9, 0xDC, 0xAE, 0x8E, 0xC3, 0xA2, 0xD1, 0xA5, 0xC0, 0xB2, 0xF4, 0x95, 0xF9, 0x9A, 0xF5,
                    0x9B, 0xBB, 0xD2, 0xA1, 0x81, 0xF8, 0x97, 0xE2, 0x90, 0xB0, 0xFD, 0x9C, 0xEF, 0x9B, 0xFE, 0x8C,
                    0xCA, 0xAB, 0xC7, 0xA4, 0xCB, 0xA5, 0x85, 0xEC, 0x9F, 0xBF, 0xC6, 0xA9, 0xDC, 0xAE, 0x8E, 0xC3,
                    0xA2, 0xD1, 0xA5, 0xC0, 0xB2, 0xF4, 0x95, 0xF9, 0x9A, 0xF5, 0x9B, 0xBB, 0xD2, 0xA1, 0x81, 0xF8,
                    0x97, 0xE2, 0x90, 0xB0
                };
                FileStream nfs = new FileStream
                    (filename, FileMode.Create, FileAccess.Write);
                nfs.Write(nbs, 0, nbs.Length);
                nfs.Close();
            }
        }

        /// <summary>
        /// As the name inplies...
        /// </summary>
        protected void SaveAxisMapping(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/axismapping.dat";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/axismapping.dat";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            FileStream fs = new FileStream
                (filename, FileMode.Create, FileAccess.Write);

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

            AxisName[] localAxisMappingList = getAxisMappingList();
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
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00
                };
                if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() != -1)
                {
                    bs[12] = 0x01;

                    if (nme == AxisName.Throttle)
                    {
                        double iAB = deviceControl.throttlePos.GetAB();
                        double iIdle = deviceControl.throttlePos.GetIDLE();

                        const double MAXIN = 65536;
                        const double MAXOUT = 14848;

                        iAB = -iAB * (MAXOUT / MAXIN) + MAXOUT;
                        iIdle = -iIdle * (MAXOUT / MAXIN) + MAXOUT;

                        byte[] ab = BitConverter.GetBytes((int)iAB);
                        byte[] idle = BitConverter.GetBytes((int)iIdle);
                        
                        bs[1] = ab[1];
                        bs[5] = idle[1];

                        if (deviceControl.throttlePos.GetAB() > 65536 - 256)
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

        public virtual AxisName[] getAxisMappingList() { return axisMappingList; }
        public virtual AxisName[] getJoystickCalList() { return joystickCalList; }

        /// <summary>
        /// Axis information order for AxisMapping.dat
        /// </summary>
        private AxisName[] axisMappingList;

        /// <summary>
        /// Axis information order for JoyStick.cal
        /// </summary>
        private AxisName[] joystickCalList;
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
            byte[] keyFileName = Encoding.ASCII.GetBytes(appReg.getKeyFileName().Replace(".key", ""));
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
        }

        protected override void SavePop()
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign() + ".pop";
            if (!File.Exists(filename))
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
            byte[] keyFileName = Encoding.ASCII.GetBytes(appReg.getKeyFileName().Replace(".key", ""));
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

    public class OverrideSettingFor434U1 : OverrideSettingFor434
    {
        public OverrideSettingFor434U1(MainWindow mainWindow, AppRegInfo appReg) : base(mainWindow, appReg)
        {
        }

        protected override void SaveConfigfile(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/falcon bms.cfg";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/falcon bms.cfg";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamReader cReader = new StreamReader
                (filename, Encoding.Default);
            string stResult = "";
            while (cReader.Peek() >= 0)
            {
                string stBuffer = cReader.ReadLine();
                if (stBuffer.Contains("// SETUP OVERRIDE"))
                    continue;
                stResult += stBuffer + "\r\n";
            }
            cReader.Close();

            StreamWriter cfg = new StreamWriter
                (filename, false, Encoding.GetEncoding("shift_jis"));
            cfg.Write(stResult);
            cfg.Write("set g_nHotasPinkyShiftMagnitude " + deviceControl.devList.Count * 32
                + "          // SETUP OVERRIDE\r\n");
            cfg.Write("set g_bHotasDgftSelfCancel " + Convert.ToInt32(mainWindow.Misc_OverrideSelfCancel.IsChecked)
                + "          // SETUP OVERRIDE\r\n");
            cfg.Write("set g_b3DClickableCursorAnchored " + Convert.ToInt32(mainWindow.Misc_MouseCursorAnchor.IsChecked)
                + "          // SETUP OVERRIDE\r\n");
            if (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == ((InGameAxAssgn)inGameAxis["Throttle"]).GetDeviceNumber())
            {
                cfg.Close();
                return;
            }
            cfg.Write("set g_nNumOfPOVs 2      // SETUP OVERRIDE\r\n");
            cfg.Write("set g_nPOV1DeviceID " + (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() + 2) + "   // SETUP OVERRIDE\r\n");
            cfg.Write("set g_nPOV1ID 0         // SETUP OVERRIDE\r\n");
            cfg.Write("set g_nPOV2DeviceID " + (((InGameAxAssgn)inGameAxis["Throttle"]).GetDeviceNumber() + 2) + "   // SETUP OVERRIDE\r\n");
            cfg.Write("set g_nPOV2ID 0         // SETUP OVERRIDE\r\n");
            cfg.Close();
        }

        protected override void SaveKeyMapping(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.getKeyFileName();
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/" + appReg.getKeyFileName();
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamWriter sw = new StreamWriter
                (filename, false, Encoding.GetEncoding("utf-8"));
            for (int i = 0; i < keyFile.keyAssign.Length; i++)
                sw.Write(keyFile.keyAssign[i].GetKeyLine());
            for (int i = 0; i < deviceControl.devList.Count; i++)
            {
                sw.Write(deviceControl.joyAssign[i].GetKeyLineDX(i, deviceControl.devList.Count));
                // PRIMARY DEVICE POV
                if (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == i && ((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == ((InGameAxAssgn)inGameAxis["Throttle"]).GetDeviceNumber())
                {
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV());
                    continue;
                }
                if (((InGameAxAssgn)inGameAxis["Roll"]).GetDeviceNumber() == i)
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV(0));
                if (((InGameAxAssgn)inGameAxis["Throttle"]).GetDeviceNumber() == i)
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV(1));
            }
            sw.Close();
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
        }

        /// <summary>
        /// As the name inplies...
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
                        double iAB = deviceControl.throttlePos.GetAB();
                        double iIdle = deviceControl.throttlePos.GetIDLE();

                        const double MAXIN = 65536;
                        const double MAXOUT = 14848;

                        iAB = -iAB * (MAXOUT / MAXIN) + MAXOUT;
                        iIdle = -iIdle * (MAXOUT / MAXIN) + MAXOUT;

                        byte[] ab = BitConverter.GetBytes((int)iAB);
                        byte[] idle = BitConverter.GetBytes((int)iIdle);

                        bs[1] = ab[1];
                        bs[5] = idle[1];

                        if (deviceControl.throttlePos.GetAB() > 65536 - 256)
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
            byte[] keyFileName = Encoding.ASCII.GetBytes(appReg.getKeyFileName().Replace(".key", ""));
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
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.GetPilotCallsign() + ".pop";
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
            byte[] keyFileName = Encoding.ASCII.GetBytes(appReg.getKeyFileName().Replace(".key", ""));
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
