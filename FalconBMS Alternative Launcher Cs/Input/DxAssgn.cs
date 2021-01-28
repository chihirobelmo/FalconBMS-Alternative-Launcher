namespace FalconBMS.Launcher.Input
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
        public void Assign(string callback, Pinky pinky, Behaviour behaviour, Invoke invoke, int soundId)
        {
            assign[(int)pinky + (int)behaviour] = new Assgn(callback, invoke, soundId);
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
        protected int soundId;

        // Property for XML
        public string Callback { get => callback;
            set => callback = value;
        }
        public Invoke Invoke { get => invoke;
            set => invoke = value;
        }
        public int SoundId { get => soundId;
            set => soundId = value;
        }

        // Constructor
        public Assgn() { }
        public Assgn(Assgn otherInstance)
        {
            callback = otherInstance.callback;
            invoke = otherInstance.invoke;
            soundId = otherInstance.soundId;
        }
        public Assgn(string callback, Invoke invoke, int soundId)
        {
            this.callback = callback;
            this.invoke = invoke;
            this.soundId = soundId;
        }

        // Method
        public string GetCallback() { return callback; }
        public Invoke GetInvoke() { return invoke; }
        public int GetSoundId() { return soundId; }

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
        Ui = 8
    }
}
