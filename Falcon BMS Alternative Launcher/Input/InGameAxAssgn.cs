using System;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Input
{
    public class InGameAxAssgn
    {
        protected JoyAssgn joy;      // DeviceNumber(-2=MouseWheel)
        protected int phyAxNum = -1;    // PhysicalAxisNumber
                                        // 0=X 1=Y 2=Z 3=Rx 4=Ry 5=Rz 6=Slider0 7=Slider1
        protected bool invert;
        protected AxCurve saturation = AxCurve.None;
        protected AxCurve deadzone = AxCurve.None;
        protected DateTime assgnDate = DateTime.Parse("12/12/1998 12:00:00");

        public InGameAxAssgn() { }

        public InGameAxAssgn(JoyAssgn joy, int phyAxNum, AxAssgn axis)
        {
            this.joy = joy;
            this.phyAxNum = phyAxNum;
            invert = axis.GetInvert();
            saturation = axis.GetSaturation();
            deadzone = axis.GetDeadZone();
            assgnDate = axis.GetAssignDate();
        }

        public InGameAxAssgn(JoyAssgn joy, int phyAxNum, bool invert, AxCurve deadzone, AxCurve saturation)
        {
            this.joy = joy;
            this.phyAxNum = phyAxNum;
            this.invert = invert;
            this.deadzone = deadzone;
            this.saturation = saturation;
        }

        public int GetDeviceNumber() 
        {
            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
                if (MainWindow.deviceControl.joyAssign[i] == joy)
                    return i;
            if (MainWindow.deviceControl.mouse == joy)
                return -2;
            return -1; 
        }
        public int GetPhysicalNumber() { return phyAxNum; }
        public bool GetInvert() { return invert; }
        public AxCurve GetDeadzone() { return deadzone; }
        public AxCurve GetSaturation() { return saturation; }
        public DateTime getDate() { return assgnDate; }
    }
}
