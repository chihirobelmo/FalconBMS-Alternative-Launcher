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
            if (!Directory.Exists(appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER))
                Directory.CreateDirectory(appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER);

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
            string filename = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + "windowconfig.dat";
            string fbackupname = appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER + "windowconfig.dat";
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
                fileName = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + CommonConstants.SETUPV100 + deviceControl.joyAssign[i].GetProductFileName()
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
            fileName = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + CommonConstants.SETUPV100 + CommonConstants.MOUSEXML;

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
            string filename = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + CommonConstants.USERCFGFILE;
            string fbackupname = appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER + CommonConstants.USERCFGFILE;
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
                if (stBuffer.Contains(CommonConstants.CFGOVERRIDECOMMENT))
                    continue;
                stResult += stBuffer + "\r\n";
            }
            cReader.Close();
            
            StreamWriter cfg = new StreamWriter
                (filename, false, Encoding.GetEncoding("shift_jis"));
            cfg.Write(stResult);

            OverrideButtonsPerDevice(cfg, deviceControl);
            OverrideHotasPinkyShiftMagnitude(cfg, deviceControl);
            OverrideVRHMD(cfg);

            cfg.Write("set g_bHotasDgftSelfCancel " + Convert.ToInt32(mainWindow.Misc_OverrideSelfCancel.IsChecked)
                + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
            cfg.Write("set g_b3DClickableCursorAnchored " + Convert.ToInt32(mainWindow.Misc_MouseCursorAnchor.IsChecked)
                + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");

            OverridePovDeviceIDs(cfg, inGameAxis);

            cfg.Close();
        }

        protected virtual void OverridePovDeviceIDs(StreamWriter cfg, Hashtable inGameAxis) { }

        protected virtual void OverrideHotasPinkyShiftMagnitude(StreamWriter cfg, DeviceControl deviceControl) { }

        protected virtual void OverrideButtonsPerDevice(StreamWriter cfg, DeviceControl deviceControl)
        {
            cfg.Write(
                "set g_nButtonsPerDevice "
                + CommonConstants.DX32
                + CommonConstants.CFGOVERRIDECOMMENT + "\r\n");
        }

        protected virtual void OverrideVRHMD(StreamWriter cfg) { }

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
            string fbackupname = appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER + "DeviceSorting.txt";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            StreamWriter ds = new StreamWriter
                (filename, false, Encoding.GetEncoding("shift_jis"));
            ds.Write(deviceSort);
            ds.Close();
        }

        protected virtual void SaveKeyMapping(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile, int DXnumber)
        {
            string filename = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + appReg.getKeyUserFileName();
            appReg.SetUserKeyFileName(appReg.getKeyUserFileName());
            mainWindow.SetDefaultKeyFile();
            mainWindow.KeyFileSelect_SelectKeyFile();

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            WriteKeyLines(filename, inGameAxis, deviceControl, keyFile, DXnumber);
        }

        protected virtual void WriteKeyLines(string filename, Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile, int DXnumber)
        {
            StreamWriter sw = new StreamWriter
                (filename, false, Encoding.GetEncoding("utf-8"));
            for (int i = 0; i < keyFile.keyAssign.Length; i++)
                sw.Write(keyFile.keyAssign[i].GetKeyLine());
            for (int i = 0; i < deviceControl.joyAssign.Length; i++)
            {
                InGameAxAssgn rollAxis = (InGameAxAssgn)inGameAxis["Roll"];

                sw.Write(deviceControl.joyAssign[i].GetKeyLineDX(i, deviceControl.joyAssign.Length, DXnumber));
                // PRIMARY DEVICE POV
                if (rollAxis.GetDeviceNumber() == i)
                    sw.Write(deviceControl.joyAssign[i].GetKeyLinePOV());
            }
            sw.Close();
        }

        protected virtual void SaveKeyMapping(Hashtable inGameAxis, DeviceControl deviceControl, KeyFile keyFile)
        {
            SaveKeyMapping(inGameAxis, deviceControl, keyFile, CommonConstants.DX32);
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
            string filename = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + "axismapping.dat";
            string fbackupname = appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER + "axismapping.dat";

            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            FileStream fs = new FileStream
                (filename, FileMode.Create, FileAccess.Write);

            byte[] bs;

            InGameAxAssgn pitchAxis = (InGameAxAssgn)inGameAxis["Pitch"];

            if (pitchAxis.GetDeviceNumber() > CommonConstants.JOYNUMUNASSIGNED)
            {
                bs = new byte[] 
                {
                    (byte)(pitchAxis.GetDeviceNumber() + CommonConstants.JOYNUMOFFSET),
                    0x00, 0x00, 0x00
                };
                fs.Write(bs, 0, bs.Length);

                bs = deviceControl.joyAssign[pitchAxis.GetDeviceNumber()].GetInstanceGUID().ToByteArray();
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
                InGameAxAssgn currentAxis = (InGameAxAssgn)inGameAxis[nme.ToString()];

                if (!currentAxis.IsAssigned() || isRollLinkedNWSEnabled(nme))
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
                if (currentAxis.IsJoyAssigned() && 
                    !isRollLinkedNWSEnabled(nme))
                {
                    bs = new byte[] 
                    {
                        (byte)(currentAxis.GetDeviceNumber() + CommonConstants.JOYNUMOFFSET),
                        0x00, 0x00, 0x00
                    };
                    fs.Write(bs, 0, bs.Length);
                    bs = new byte[] 
                    {
                        (byte)currentAxis.GetPhysicalNumber(),
                        0x00, 0x00, 0x00
                    };
                    fs.Write(bs, 0, bs.Length);
                }
                if (currentAxis.IsMoushWheelAssigned())
                {
                    bs = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    fs.Write(bs, 0, bs.Length);
                }

                bs = isRollLinkedNWSEnabled(nme) ?
                    new byte[] { 0x00, 0x00, 0x00, 0x00 } :
                    GetAxDeadZoneByte(currentAxis.GetDeadzone());

                fs.Write(bs, 0, bs.Length);

                bs = isRollLinkedNWSEnabled(nme) ? 
                    new byte[] { 0x00, 0x00, 0x00, 0x00 } :
                    GetAxSaturationByte(currentAxis.GetSaturation());

                fs.Write(bs, 0, bs.Length);
            }
            fs.Close();
        }

        private byte[] GetAxDeadZoneByte(AxCurve axCurve)
        {
            var bs = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            switch (axCurve)
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
            return bs;
        }

        private byte[] GetAxSaturationByte(AxCurve axCurve)
        { 
            var bs = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            switch (axCurve)
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
            return bs;
        }

        /// <summary>
        /// As the name implies...
        /// </summary>
        protected virtual void SaveJoystickCal(Hashtable inGameAxis, DeviceControl deviceControl)
        {
            string filename = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDER + "joystick.cal";
            string fbackupname = appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER + "joystick.cal";

            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, true);

            if (File.Exists(filename))
                File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            FileStream fs = new FileStream
                (filename, FileMode.Create, FileAccess.Write);

            byte[] bs = new byte[] { 0x00 };

            AxisName[] localJoystickCalList = appReg.getOverrideWriter().getJoystickCalList();

            foreach (AxisName nme in localJoystickCalList)
            {
                InGameAxAssgn currentAxis = (InGameAxAssgn)inGameAxis[nme.ToString()];

                SetJoyCalDefaultByte(ref bs);

                if (currentAxis.IsAssigned() && !isRollLinkedNWSEnabled(nme))
                {
                    bs[12] = 0x01;

                    if (nme == AxisName.Throttle && currentAxis.IsJoyAssigned())
                    {
                        double iAB = deviceControl.joyAssign[currentAxis.GetDeviceNumber()].detentPosition.GetAB();
                        double iIdle = deviceControl.joyAssign[currentAxis.GetDeviceNumber()].detentPosition.GetIDLE();

                        iAB = iAB * CommonConstants.BINAXISMAX / CommonConstants.AXISMAX;
                        iIdle = iIdle * CommonConstants.BINAXISMAX / CommonConstants.AXISMAX;

                        if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetInvert() == false)
                        {
                            iAB = CommonConstants.BINAXISMAX - iAB;
                            iIdle = CommonConstants.BINAXISMAX - iIdle;
                        }

                        byte[] ab = BitConverter.GetBytes((int)iAB).Reverse().ToArray();
                        byte[] idle = BitConverter.GetBytes((int)iIdle).Reverse().ToArray();

                        bs[1] = ab[2];
                        bs[5] = idle[2];
                    }
                }
                if (currentAxis.GetInvert())
                {
                    SetJoyCalInvertByte(ref bs);
                }
                fs.Write(bs, 0, bs.Length);
            }
            fs.Close();
        }

        protected virtual void SetJoyCalDefaultByte(ref byte[] bs)
        {
            bs = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x98, 0x3A, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00
            };
        }

        protected virtual void SetJoyCalInvertByte(ref byte[] bs)
        {
            bs[20] = 0x01;
        }

        protected bool isRollLinkedNWSEnabled(AxisName nme)
        {
            return mainWindow.Misc_RollLinkedNWS.IsChecked == true && ( nme == AxisName.Yaw );
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
