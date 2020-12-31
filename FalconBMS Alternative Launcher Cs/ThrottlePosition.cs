using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class ThrottlePosition
    {
        // Member
        protected int aB = MainWindow.MAXIN;
        protected int iDLE = 0;

        // Property for XML
        public int AB { get { return aB; } set { aB = value; } }
        public int IDLE { get { return iDLE; } set { iDLE = value; } }

        // Constructor
        public ThrottlePosition(int aB, int iDLE) { this.aB = aB; this.iDLE = iDLE; }
        public ThrottlePosition() { }

        // Method
        public int GetAB() { return aB; }
        public int GetIDLE() { return iDLE; }
    }
}
