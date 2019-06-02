using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class DeviceControl
    {
        // Member
        public DeviceList devList;
        public Device[] joyStick;
        public JoyAssgn[] joyAssign;
        public AxAssgn mouseWheelAssign = new AxAssgn();
        public ThrottlePosition throttlePos = new ThrottlePosition();

        /// <summary>
        /// Get Devices.
        /// </summary>
        public DeviceControl()
        { }

        /// <summary>
        /// Get Devices.
        /// </summary>
        public DeviceControl(AppRegInfo appReg)
        {
            // Make Joystick Instances.
            this.devList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            this.joyStick = new Device[devList.Count];
            this.joyAssign = new JoyAssgn[devList.Count];

            System.Xml.Serialization.XmlSerializer serializer;
            System.IO.StreamReader sr;
            string fileName = "";
            string stockFileName = "";
            int i = 0;

            foreach (DeviceInstance dev in devList)
            {
                joyStick[i] = new Device(dev.InstanceGuid);
                joyAssign[i] = new JoyAssgn();

                joyAssign[i].SetDeviceInstance(dev);
                int povnum = joyStick[i].Caps.NumberPointOfViews;
                joyStick.Count();

                fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + joyAssign[i].GetProductName().Replace("/", "-")
                + " {" + joyAssign[i].GetInstanceGUID().ToString().ToUpper() + "}.xml";

                // Load exsisting .xml files.
                if (File.Exists(fileName))
                {
                    serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn));
                    sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                    joyAssign[i] = (JoyAssgn)serializer.Deserialize(sr);
                    sr.Close();
                }
                else
                {
                    stockFileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + joyAssign[i].GetProductName().Replace("/", "-")
                    + " {Stock}.xml";
                    if (File.Exists(stockFileName))
                    {
                        File.Copy(stockFileName, fileName);

                        serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn));
                        sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                        joyAssign[i] = (JoyAssgn)serializer.Deserialize(sr);
                        sr.Close();
                    }
                }
                joyAssign[i].SetDeviceInstance(dev);
                i += 1;
            }

            // Import stock BMS Setup if .xml save file for the joystick does not exist. 
            try
            {
                for (int ii = 0; ii < joyAssign.Count(); ii++)
                {
                    fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + joyAssign[ii].GetProductName().Replace("/", "-")
                       + " {" + joyAssign[ii].GetInstanceGUID().ToString().ToUpper() + "}.xml";
                    if (File.Exists(fileName) == false)
                        joyAssign[ii].ImportStockSetup(appReg, joyStick.Count(), joyStick[ii].Caps.NumberPointOfViews, ii);
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();
            }
            
            // Load MouseWheel .xml file.
            serializer = new System.Xml.Serialization.XmlSerializer(typeof(AxAssgn));
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.Mousewheel.xml";
            if (File.Exists(fileName))
            {
                sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                mouseWheelAssign = (AxAssgn)serializer.Deserialize(sr);
                sr.Close();
            }

            // Load ThrottlePosition .xml file.
            serializer = new System.Xml.Serialization.XmlSerializer(typeof(ThrottlePosition));
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.throttlePosition.xml";
            if (File.Exists(fileName))
            {
                sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));
                throttlePos = (ThrottlePosition)serializer.Deserialize(sr);
                sr.Close();
            }
        }

        public int JoyAxisState(int joyNumber, int joyAxisNumber)
        {
            int input = 0;
            switch (joyAxisNumber)
            {
                case 0:
                    input = this.joyStick[joyNumber].CurrentJoystickState.X;
                    break;
                case 1:
                    input = this.joyStick[joyNumber].CurrentJoystickState.Y;
                    break;
                case 2:
                    input = this.joyStick[joyNumber].CurrentJoystickState.Z;
                    break;
                case 3:
                    input = this.joyStick[joyNumber].CurrentJoystickState.Rx;
                    break;
                case 4:
                    input = this.joyStick[joyNumber].CurrentJoystickState.Ry;
                    break;
                case 5:
                    input = this.joyStick[joyNumber].CurrentJoystickState.Rz;
                    break;
                case 6:
                    input = this.joyStick[joyNumber].CurrentJoystickState.GetSlider()[0];
                    break;
                case 7:
                    input = this.joyStick[joyNumber].CurrentJoystickState.GetSlider()[1];
                    break;
            }
            return input;
        }
    }
}
