using System.IO;
using System.Linq;

using Microsoft.DirectX.DirectInput;

namespace FalconBMS.Launcher.Input
{
    public class DeviceControl
    {
        // Member
        public DeviceList devList;
        public Device[] joyStick;
        public JoyAssgn[] joyAssign;
        //public AxAssgn mouseWheelAssign = new AxAssgn();
        public JoyAssgn mouse = new JoyAssgn();

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
            devList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
            joyStick = new Device[devList.Count];
            joyAssign = new JoyAssgn[devList.Count];

            System.Xml.Serialization.XmlSerializer serializer;
            StreamReader sr;
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

                // Load existing .xml files.
                if (File.Exists(fileName))
                {
                    serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn));
                    sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
                    joyAssign[i].Load((JoyAssgn)serializer.Deserialize(sr));
                    sr.Close();
                }
                else
                {
                    stockFileName = Directory.GetCurrentDirectory() + "/Stock/Setup.v100." + joyAssign[i].GetProductName().Replace("/", "-")
                    + " {Stock}.xml";
                    if (File.Exists(stockFileName))
                    {
                        File.Copy(stockFileName, fileName);

                        serializer = new System.Xml.Serialization.XmlSerializer(typeof(JoyAssgn));
                        sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
                        joyAssign[i].Load((JoyAssgn)serializer.Deserialize(sr));
                        sr.Close();
                    }
                }
                joyAssign[i].SetDeviceInstance(dev);
                i += 1;
            }
            
            // Load MouseWheel .xml file.
            serializer = new System.Xml.Serialization.XmlSerializer(typeof(AxAssgn));
            fileName = appReg.GetInstallDir() + "/User/Config/Setup.v100.Mousewheel.xml";
            if (File.Exists(fileName))
            {
                sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false));
                mouse.LoadAx((AxAssgn)serializer.Deserialize(sr));
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
