namespace FalconBMS.Launcher.Input
{
    /// <summary>
    /// Means each actual POV switches on a joystick.
    /// </summary>
    public class PovAssgn
    {
        /// <summary>
        /// One POV switch has 8 directions.
        /// </summary>
        public DirAssgn[] direction = new DirAssgn[8];

        // Constructor
        public PovAssgn()
        {
            for (int i = 0; i < direction.Length; i++)
                direction[i] = new DirAssgn();
        }
        public PovAssgn(PovAssgn otherInstance)
        {
            for (int i = 0; i < direction.Length; i++)
                direction[i] = otherInstance.direction[i].Clone();
        }

        // Method
        public void Assign(int GetPointofView, string callback, Pinky pinky, int soundID)
        {
            if (GetPointofView > 7)
                GetPointofView /= 4500;
            direction[GetPointofView].Assign(callback, pinky, soundID);
        }

        public string GetDirection(int GetPointOfView)
        {
            string direction = "";
            if (GetPointOfView > 7)
                GetPointOfView /= 4500;
            switch (GetPointOfView)
            {
                case 0:
                    direction = "UP";
                    break;
                case 1:
                    direction = "UPRIGHT";
                    break;
                case 2:
                    direction = "RIGHT";
                    break;
                case 3:
                    direction = "DOWNRIGHT";
                    break;
                case 4:
                    direction = "DOWN";
                    break;
                case 5:
                    direction = "DOWNLEFT";
                    break;
                case 6:
                    direction = "LEFT";
                    break;
                case 7:
                    direction = "UPLEFT";
                    break;
            }
            return direction;
        }

        public PovAssgn Clone()
        {
            return new PovAssgn(this);
        }
    }

    /// <summary>
    /// Means each direction on a POV switch,
    /// </summary>
    public class DirAssgn
    {
        // Member
        protected string[] callback = { CommonConstants.SIMDONOTHING, CommonConstants.SIMDONOTHING };
        protected int[] soundID = { 0, 0 };
        // [0]=PRESS
        // [1]=PRESS + SHIFT

        // Property for XML
        public string[] Callback { get => callback;
            set => callback = value;
        }
        public int[] SoundID { get => soundID;
            set => soundID = value;
        }

        // Constructor
        public DirAssgn() { }
        public DirAssgn(DirAssgn otherInstance)
        {
            callback[0] = otherInstance.callback[0];
            callback[1] = otherInstance.callback[1];
            soundID[0] = otherInstance.soundID[0];
            soundID[1] = otherInstance.soundID[1];
        }

        // Method
        public string GetCallback(Pinky pinky) { return callback[(int)pinky]; }
        public int GetSoundID(Pinky pinky) { return soundID[(int)pinky]; }

        public void Assign(string callback, Pinky pinky, int soundID)
        {
            this.callback[(int)pinky] = callback;
            this.soundID[(int)pinky] = soundID;
        }

        public void UnAssign(Pinky pinky)
        {
            callback[(int)pinky] = CommonConstants.SIMDONOTHING;
            soundID[(int)pinky] = 0;
        }

        public DirAssgn Clone()
        {
            return new DirAssgn(this);
        }
    }
}
