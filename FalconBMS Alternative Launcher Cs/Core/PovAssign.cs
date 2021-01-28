using FalconBMS.Launcher.Input;

namespace FalconBMS.Launcher.Core
{
    /// <summary>
    /// Means each actual POV switches on a joystick.
    /// </summary>
    public class PovAssign
    {
        /// <summary>
        /// One POV switch has 8 directions.
        /// </summary>
        public DirAssign[] direction = new DirAssign[8];

        // Constructor
        public PovAssign()
        {
            for (int i = 0; i < direction.Length; i++)
                direction[i] = new DirAssign();
        }
        public PovAssign(PovAssign otherInstance)
        {
            for (int i = 0; i < direction.Length; i++)
                direction[i] = otherInstance.direction[i].Clone();
        }

        // Method
        public void Assign(int getPointOfView, string callback, Pinky pinky, int soundId)
        {
            if (getPointOfView > 7)
                getPointOfView /= 4500;
            direction[getPointOfView].Assign(callback, pinky, soundId);
        }

        public string GetDirection(int getPointOfView)
        {
            string direction = "";
            if (getPointOfView > 7)
                getPointOfView /= 4500;
            switch (getPointOfView)
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

        public PovAssign Clone()
        {
            return new PovAssign(this);
        }
    }

    /// <summary>
    /// Means each direction on a POV switch,
    /// </summary>
    public class DirAssign
    {
        // Member
        protected string[] callback = { "SimDoNothing", "SimDoNothing" };
        protected int[] soundId = { 0, 0 };
        // [0]=PRESS
        // [1]=PRESS + SHIFT

        // Property for XML
        public string[] Callback { get => callback;
            set => callback = value;
        }
        public int[] SoundId { get => soundId;
            set => soundId = value;
        }

        // Constructor
        public DirAssign() { }
        public DirAssign(DirAssign otherInstance)
        {
            callback[0] = otherInstance.callback[0];
            callback[1] = otherInstance.callback[1];
            soundId[0] = otherInstance.soundId[0];
            soundId[1] = otherInstance.soundId[1];
        }

        // Method
        public string GetCallback(Pinky pinky) { return callback[(int)pinky]; }
        public int GetSoundId(Pinky pinky) { return soundId[(int)pinky]; }

        public void Assign(string callback, Pinky pinky, int soundId)
        {
            this.callback[(int)pinky] = callback;
            this.soundId[(int)pinky] = soundId;
        }

        public void UnAssign(Pinky pinky)
        {
            callback[(int)pinky] = "SimDoNothing";
            soundId[(int)pinky] = 0;
        }

        public DirAssign Clone()
        {
            return new DirAssign(this);
        }
    }
}
