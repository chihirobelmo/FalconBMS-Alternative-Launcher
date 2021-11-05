using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Input
{
    public class DetentPosition
    {
        // Member
        protected int aB   = CommonConstants.AXISMAX;
        protected int iDLE = CommonConstants.AXISMIN;

        // Property for XML
        public int AB { get => aB;
            set => aB = value;
        }
        public int IDLE { get => iDLE;
            set => iDLE = value;
        }

        // Constructor
        public DetentPosition(int aB, int iDLE) { this.aB = aB; this.iDLE = iDLE; }
        public DetentPosition() { }

        // Method
        public int GetAB() { return aB; }
        public int GetIDLE() { return iDLE; }
    }
}
