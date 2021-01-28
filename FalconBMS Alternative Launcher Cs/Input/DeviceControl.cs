using System.IO;
using System.Linq;

using FalconBMS.Launcher.Core;

using Microsoft.DirectX.DirectInput;

namespace FalconBMS.Launcher.Input
{
    public class DeviceControl
    {
        // Member
        public DeviceList devList;
        public Device[] joyStick;
        public JoyAssign[] joyAssign;
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
            // TODO: Move try catch loops in this constructor into private async methods.
            // Make Joystick Instances.
            devList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            joyStick = new Device[devList.Count];
            joyAssign = new JoyAssign[devList.Count];

            System.Xml.Serialization.XmlSerializer serializer;
            StreamReader sr;
            string fileName = "";
            string stockFileName = "";
            int i = 0;

            foreach (DeviceInstance dev in devList)
            {
                joyStick[i] = new Device(dev.InstanceGuid);
                joyAssign[i] = new JoyAssign();

                joyAssign[i].SetDeviceInstance(dev);
                int povnum = joyStick[i].Caps.NumberPointOfViews;
                joyStick.Count();

                fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + joyAssign[i].GetProductName().Replace("/", "-")
                + " {" + joyAssign[i].GetInstanceGuid().ToString().ToUpper() + "}.xml";

                // Load exsisting .xml files.
                if (File.Exists(fileName))
                {
                    serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssign));
                    sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
                    joyAssign[i] = (JoyAssign)serializer.Deserialize(sr);
                    sr.Close();
                }
                else
                {
                    stockFileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + joyAssign[i].GetProductName().Replace("/", "-")
                    + " {Stock}.xml";
                    if (File.Exists(stockFileName))
                    {
                        File.Copy(stockFileName, fileName);

                        serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssign));
                        sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
                        joyAssign[i] = (JoyAssign)serializer.Deserialize(sr);
                        sr.Close();
                    }
                }
                joyAssign[i].SetDeviceInstance(dev);
                i += 1;
            }

            // Import stock BMS Setup if .xml save file for the joystick does not exist. 
            try
            {
                for (int ii = 0; ii < joyAssign.Length; ii++)
                {
                    fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100." + joyAssign[ii].GetProductName().Replace("/", "-")
                       + " {" + joyAssign[ii].GetInstanceGuid().ToString().ToUpper() + "}.xml";
                    if (File.Exists(fileName) == false)
                        joyAssign[ii].ImportStockSetup(appReg, joyStick.Length, joyStick[ii].Caps.NumberPointOfViews, ii);
                }
            }
            catch (FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.Message);
                Diagnostics.Log(ex);
                StreamWriter sw = new StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                
                sw.Close();
            }
            
            // Load MouseWheel .xml file.
            serializer = new System.Xml.Serialization.XmlSerializer(typeof(AxAssgn));
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.Mousewheel.xml";
            if (File.Exists(fileName))
            {
                sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
                mouseWheelAssign = (AxAssgn)serializer.Deserialize(sr);
                sr.Close();
            }

            // Load ThrottlePosition .xml file.
            serializer = new System.Xml.Serialization.XmlSerializer(typeof(ThrottlePosition));
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.throttlePosition.xml";
            if (File.Exists(fileName))
            {
                sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
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
                    input = joyStick[joyNumber].CurrentJoystickState.X;
                    break;
                case 1:
                    input = joyStick[joyNumber].CurrentJoystickState.Y;
                    break;
                case 2:
                    input = joyStick[joyNumber].CurrentJoystickState.Z;
                    break;
                case 3:
                    input = joyStick[joyNumber].CurrentJoystickState.Rx;
                    break;
                case 4:
                    input = joyStick[joyNumber].CurrentJoystickState.Ry;
                    break;
                case 5:
                    input = joyStick[joyNumber].CurrentJoystickState.Rz;
                    break;
                case 6:
                    input = joyStick[joyNumber].CurrentJoystickState.GetSlider()[0];
                    break;
                case 7:
                    input = joyStick[joyNumber].CurrentJoystickState.GetSlider()[1];
                    break;
            }
            return input;
        }

    }
}
