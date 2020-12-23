using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS_Alternative_Launcher_Cs
{
    /// <summary>
    /// Means each physical axis on a joystick.
    /// </summary>
    public class AxAssgn
    {
        // Property for XML
        public AxisName? AxisName { get; set; }
        public DateTime AssignDate { get; set; }
        public bool Invert { get; set; }
        public AxCurve Saturation { get; set; }
        public AxCurve Deadzone { get; set; }

        // Constructor
        public AxAssgn() { }
        public AxAssgn(AxisName axisName, InGameAxAssgn axisassign) :
            this(axisName, DateTime.Parse("12/12/1998 12:00:00"), axisassign.GetInvert(), axisassign.GetSaturation(), axisassign.GetDeadzone())
        { }

        public AxAssgn(AxisName? axisName, DateTime assgnDate, bool invert, AxCurve saturation, AxCurve deadzone)
        {
            this.AxisName = axisName;
            this.AssignDate = assgnDate;
            this.Invert = invert;
            this.Saturation = saturation;
            this.Deadzone = deadzone;
        }

        public AxAssgn Clone()
        {
            return new AxAssgn(this.AxisName, this.AssignDate, this.Invert, this.Saturation, this.Deadzone);
        }
    }

    public enum AxCurve
    {
        None,
        Small,
        Medium,
        Large
    }
}
