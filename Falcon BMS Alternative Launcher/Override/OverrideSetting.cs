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

            MainWindow.deviceControl.SortDevice();

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
        /// As the name implies...
        /// </summary>
        protected void SaveJoyAssignStatus(DeviceControl deviceControl)
        {
            string fileName;
            XmlSerializer serializer;
            StreamWriter sw;

            for (int i = 0; i < deviceControl.joyAssign.Length; i++)
            {
                fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + deviceControl.joyAssign[i].GetProductName().Replace("/", "-")
                + " {" + deviceControl.joyAssign[i].GetInstanceGUID().ToString().ToUpper() + "}.xml";

                try
                {
                    serializer = new XmlSerializer(typeof(JoyAssgn));
                    sw = new StreamWriter(fileName, false, new UTF8Encoding(false));
                    serializer.Serialize(sw, deviceControl.joyAssign[i]);

                    sw.Close();
                }
                catch (Exception ex)
                {
                    Diagnostics.WriteLogFile(ex);

                    continue;
                }

            }
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.MouseWheel.xml";

            serializer = new XmlSerializer(typeof(AxAssgn));
            sw = new StreamWriter(fileName, false, new UTF8Encoding(false));
            serializer.Serialize(sw, deviceControl.mouse.GetMouseAxis());

            sw.Close();
        }

        /// <summary>
        /// As the name implies...
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
            cfg.Write("set g_nHotasPinkyShiftMagnitude " + deviceControl.joyAssign.Length * CommonConstants.DX32
                + "          // SETUP OVERRIDE\r\n");
            cfg.Write("set g_bHotasDgftSelfCancel " + Convert.ToInt32(mainWindow.Misc_OverrideSelfCancel.IsChecked)
                + "          // SETUP OVERRIDE\r\n");
            cfg.Write("set g_b3DClickableCursorAnchored " + Convert.ToInt32(mainWindow.Misc_MouseCursorAnchor.IsChecked)
                + "          // SETUP OVERRIDE\r\n");
            cfg.Close();
        }

        /// <summary>
        /// As the name implies...
        /// </summary>
        protected void SaveDeviceSorting(DeviceControl deviceControl)
        {
            string deviceSort = "";
            for (int i = 0; i < deviceControl.joyAssign.Length; i++)
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
        /// As the name implies...
        /// </summary>
        protected virtual void SaveKeyMapping(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile)
        {
            string filename = appReg.GetInstallDir() + "/User/Config/" + appReg.getAutoKeyFileName();

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamWriter sw = new StreamWriter
                (filename, false, Encoding.GetEncoding("utf-8"));
            for (int i = 0; i < keyFile.keyAssign.Length; i++)
                sw.Write(keyFile.keyAssign[i].GetKeyLine());
            for (int i = 0; i < deviceControl.joyAssign.Length; i++)
            {
                sw.Write(deviceControl.joyAssign[i].GetKeyLineDX(i, deviceControl.joyAssign.Length));
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

                bs = new byte[] { (byte)deviceControl.joyAssign.Length, 0x00, 0x00, 0x00 };
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
                bs[20] = (byte)deviceControl.joyAssign.Length;
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
        /// As the name implies...
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
                        if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() >= 0)
                        {
                            double iAB = deviceControl.joyAssign[((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber()].detentPosition.GetAB();
                            double iIdle = deviceControl.joyAssign[((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber()].detentPosition.GetIDLE();

                            iAB = iAB * 15000 / 65536;
                            iIdle = iIdle * 15000 / 65536;

                            if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetInvert())
                            {
                                iAB = 15000 - iAB;
                                iIdle = 15000 - iIdle;
                            }

                            byte[] ab = BitConverter.GetBytes((int)iAB).Reverse().ToArray();
                            byte[] idle = BitConverter.GetBytes((int)iIdle).Reverse().ToArray();

                            bs[5] = ab[2];
                            bs[1] = idle[2];
                        }
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
    

}
