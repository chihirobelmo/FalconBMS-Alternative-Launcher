using System;

namespace FalconBMS.Launcher.Input
{
    /// <summary>
    /// Means each physical axis on a joystick.
    /// </summary>
    public class AxAssgn : ICloneable
    {
        // Member
        protected string axisName = "";     // ex:Roll, Pitch, Yaw etc...
        protected DateTime assgnDate = new DateTime(1998, 12, 12, 12, 0, 0);
        protected bool invert;
        protected AxCurve saturation = 0;
        protected AxCurve deadzone = 0;

        // Property for XML
        public string AxisName { get => axisName;
            set => axisName = value;
        }
        public DateTime AssgnDate { get => assgnDate;
            set => assgnDate = value;
        }
        public bool Invert { get => invert;
            set => invert = value;
        }
        public AxCurve Saturation { get => saturation;
            set => saturation = value;
        }
        public AxCurve Deadzone { get => deadzone;
            set => deadzone = value;
        }

        // Constructor
        public AxAssgn() { }
        public AxAssgn(string axisName, InGameAxAssgn axisassign)
        {
            this.axisName = axisName;
            assgnDate = DateTime.Now;
            invert = axisassign.GetInvert();
            saturation = axisassign.GetSaturation();
            deadzone = axisassign.GetDeadzone();
        }
        public AxAssgn(string axisName, DateTime assgnDate, bool invert, AxCurve saturation, AxCurve deadzone)
        {
            this.axisName = axisName;
            this.assgnDate = assgnDate;
            this.invert = invert;
            this.saturation = saturation;
            this.deadzone = deadzone;
        }

        // Method
        public string GetAxisName() { return axisName; }
        public DateTime GetAssignDate() { return assgnDate; }
        public bool GetInvert() { return invert; }
        public AxCurve GetDeadZone() { return deadzone; }
        public AxCurve GetSaturation() { return saturation; }

        object ICloneable.Clone() => Clone();

        public AxAssgn Clone()
        {
            return new AxAssgn(axisName, assgnDate, invert, saturation, deadzone);
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
