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
        // Member
        protected string axisName = "";     // ex:Roll, Pitch, Yaw etc...
        protected System.DateTime assgnDate = DateTime.Parse("12/12/1998 12:00:00");
        protected bool invert = false;
        protected AxCurve saturation = 0;
        protected AxCurve deadzone = 0;

        // Property for XML
        public string AxisName { get { return this.axisName; } set { this.axisName = value; } }
        public DateTime AssgnDate { get { return this.assgnDate; } set { this.assgnDate = value; } }
        public bool Invert { get { return this.invert; } set { this.invert = value; } }
        public AxCurve Saturation { get { return this.saturation; } set { this.saturation = value; } }
        public AxCurve Deadzone { get { return this.deadzone; } set { this.deadzone = value; } }

        // Constructor
        public AxAssgn() { }
        public AxAssgn(String axisName, InGameAxAssgn axisassign)
        {
            this.axisName = axisName;
            this.assgnDate = DateTime.Now;
            this.invert = axisassign.GetInvert();
            this.saturation = axisassign.GetSaturation();
            this.deadzone = axisassign.GetDeadzone();
        }
        public AxAssgn(String axisName, DateTime assgnDate, bool invert, AxCurve saturation, AxCurve deadzone)
        {
            this.axisName = axisName;
            this.assgnDate = assgnDate;
            this.invert = invert;
            this.saturation = saturation;
            this.deadzone = deadzone;
        }

        // Method
        public string GetAxisName() { return this.axisName; }
        public DateTime GetAssignDate() { return this.assgnDate; }
        public bool GetInvert() { return this.invert; }
        public AxCurve GetDeadZone() { return this.deadzone; }
        public AxCurve GetSaturation() { return this.saturation; }

        public AxAssgn Clone()
        {
            return new AxAssgn(this.axisName, this.assgnDate, this.invert, this.saturation, this.deadzone);
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
