﻿using System;

namespace FalconBMS.Launcher.Input
{
    public class InGameAxAssgn
    {
        protected int devNum = -1;      // DeviceNumber(-2=MouseWheel)
        protected int phyAxNum = -1;    // PhysicalAxisNumber
                                        // 0=X 1=Y 2=Z 3=Rx 4=Ry 5=Rz 6=Slider0 7=Slider1
        protected bool invert;
        protected AxCurve saturation = AxCurve.None;
        protected AxCurve deadzone = AxCurve.None;
        protected DateTime assgnDate = DateTime.Parse("12/12/1998 12:00:00");

        public InGameAxAssgn() { }

        public InGameAxAssgn(int devNum, int phyAxNum, AxAssgn axis)
        {
            this.devNum = devNum;
            this.phyAxNum = phyAxNum;
            invert = axis.GetInvert();
            saturation = axis.GetSaturation();
            deadzone = axis.GetDeadZone();
            assgnDate = axis.GetAssignDate();
        }

        public InGameAxAssgn(int devNum, int phyAxNum, bool invert, AxCurve deadzone, AxCurve saturation)
        {
            this.devNum = devNum;
            this.phyAxNum = phyAxNum;
            this.invert = invert;
            this.deadzone = deadzone;
            this.saturation = saturation;
        }

        public int GetDeviceNumber() { return devNum; }
        public int GetPhysicalNumber() { return phyAxNum; }
        public bool GetInvert() { return invert; }
        public AxCurve GetDeadzone() { return deadzone; }
        public AxCurve GetSaturation() { return saturation; }
        public DateTime getDate() { return assgnDate; }
    }
}
