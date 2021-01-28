namespace FalconBMS.Launcher.Input
{
    public class ThrottlePosition
    {
        // Member
        protected int aB = MainWindow.MAXIN;
        protected int iDLE;

        // Property for XML
        public int AB { get => aB;
            set => aB = value;
        }
        public int IDLE { get => iDLE;
            set => iDLE = value;
        }

        // Constructor
        public ThrottlePosition(int aB, int iDLE) { this.aB = aB; this.iDLE = iDLE; }
        public ThrottlePosition() { }

        // Method
        public int GetAB() { return aB; }
        public int GetIDLE() { return iDLE; }
    }
}
