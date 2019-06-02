using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS_Alternative_Launcher_Cs
{

    /// <summary>
    /// Means each actual dx buttons on a joystick.
    /// </summary>
    public class DxAssgn
    {
        /// <summary>
        /// [0]=PRESS
        /// [1]=PRESS + SHIFT
        /// [2]=RELEASE
        /// [3]=RELEASE + SHIFT
        /// </summary>
        public Assgn[] assign = new Assgn[4];

        // Constructor
        public DxAssgn()
        {
            for (int i = 0; i < this.assign.Length; i++)
                this.assign[i] = new Assgn();
        }
        public DxAssgn(DxAssgn otherInstance)
        {
            for (int i = 0; i < this.assign.Length; i++)
                this.assign[i] = otherInstance.assign[i].Clone();
        }

        // Method
        public void Assign(string callback, Pinky pinky, Behaviour behaviour, Invoke invoke, int soundID)
        {
            this.assign[(int)pinky + (int)behaviour] = new Assgn(callback, invoke, soundID);
        }

        public DxAssgn Clone()
        {
            return new DxAssgn(this);
        }
    }

    /// <summary>
    /// MEans each behaviour for a button. 
    /// Press / Pinky+Press / Release / Pinky+Release
    /// </summary>
    public class Assgn
    {
        // Member
        protected string callback = "SimDoNothing";
        protected Invoke invoke = Invoke.Default;
        protected int soundID = 0;

        // Property for XML
        public string Callback { get { return this.callback; } set { this.callback = value; } }
        public Invoke Invoke { get { return this.invoke; } set { this.invoke = value; } }
        public int SoundID { get { return this.soundID; } set { this.soundID = value; } }

        // Constructor
        public Assgn() { }
        public Assgn(Assgn otherInstance)
        {
            this.callback = otherInstance.callback;
            this.invoke = otherInstance.invoke;
            this.soundID = otherInstance.soundID;
        }
        public Assgn(string callback, Invoke invoke, int soundID)
        {
            this.callback = callback;
            this.invoke = invoke;
            this.soundID = soundID;
        }

        // Method
        public string GetCallback() { return this.callback; }
        public Invoke GetInvoke() { return this.invoke; }
        public int GetSoundID() { return this.soundID; }

        public Assgn Clone()
        {
            return new Assgn(this);
        }
    }

    public enum Pinky
    {
        UnShift = 0,
        Shift = 1,
    }

    public enum Behaviour
    {
        Press = 0,
        Release = 2,
    }

    public enum Invoke
    {
        Default = -1,
        Down = -2,
        Up = -4,
        UI = 8
    }
}
