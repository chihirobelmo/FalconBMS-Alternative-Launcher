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
            for (int i = 0; i < assign.Length; i++)
                assign[i] = new Assgn();
        }
        public DxAssgn(DxAssgn otherInstance)
        {
            for (int i = 0; i < assign.Length; i++)
                assign[i] = otherInstance.assign[i].Clone();
        }

        // Method
        public void Assign(string callback, Pinky pinky, Behaviour behaviour, Invoke invoke, int soundID)
        {
            assign[(int)pinky + (int)behaviour] = new Assgn(callback, invoke, soundID);
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
        public string Callback { get { return callback; } set { callback = value; } }
        public Invoke Invoke { get { return invoke; } set { invoke = value; } }
        public int SoundID { get { return soundID; } set { soundID = value; } }

        // Constructor
        public Assgn() { }
        public Assgn(Assgn otherInstance)
        {
            callback = otherInstance.callback;
            invoke = otherInstance.invoke;
            soundID = otherInstance.soundID;
        }
        public Assgn(string callback, Invoke invoke, int soundID)
        {
            this.callback = callback;
            this.invoke = invoke;
            this.soundID = soundID;
        }

        // Method
        public string GetCallback() { return callback; }
        public Invoke GetInvoke() { return invoke; }
        public int GetSoundID() { return soundID; }

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
