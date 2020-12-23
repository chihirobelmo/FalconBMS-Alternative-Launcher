using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class InGameAxAssgn
    {
        protected int devNum = -1;      // DeviceNumber(-2=MouseWheel)
        protected int phyAxNum = -1;    // PhysicalAxisNumber
                                        // 0=X 1=Y 2=Z 3=Rx 4=Ry 5=Rz 6=Slider0 7=Slider1
        protected bool invert = false;
        protected AxCurve saturation = AxCurve.None;
        protected AxCurve deadzone = AxCurve.None;
        protected System.DateTime assgnDate = DateTime.Parse("12/12/1998 12:00:00");

        public InGameAxAssgn() { }

        public InGameAxAssgn(int devNum, int phyAxNum, AxAssgn axis)
        {
            this.devNum = devNum;
            this.phyAxNum = phyAxNum;
            this.invert = axis.Invert;
            this.saturation = axis.Saturation;
            this.deadzone = axis.Deadzone;
            this.assgnDate = axis.AssignDate;
        }

        public InGameAxAssgn(int devNum, int phyAxNum, bool invert, AxCurve deadzone, AxCurve saturation)
        {
            this.devNum = devNum;
            this.phyAxNum = phyAxNum;
            this.invert = invert;
            this.deadzone = deadzone;
            this.saturation = saturation;
        }

        public int GetDeviceNumber() { return this.devNum; }
        public int GetPhysicalNumber() { return this.phyAxNum; }
        public bool GetInvert() { return this.invert; }
        public AxCurve GetDeadzone() { return this.deadzone; }
        public AxCurve GetSaturation() { return this.saturation; }
        public DateTime getDate() { return this.assgnDate; }
    }
}
