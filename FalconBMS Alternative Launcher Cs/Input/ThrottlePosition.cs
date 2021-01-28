namespace FalconBMS.Launcher.Input
{
    public class ThrottlePosition
    {
        // Member
        protected int aB = MainWindow.Maxin;
        protected int iDle;

        // Property for XML
        public int Ab { get => aB;
            set => aB = value;
        }
        public int Idle { get => iDle;
            set => iDle = value;
        }

        // Constructor
        public ThrottlePosition(int aB, int iDle) { this.aB = aB; this.iDle = iDle; }
        public ThrottlePosition() { }

        // Method
        public int GetAb() { return aB; }
        public int GetIdle() { return iDle; }
    }
}
