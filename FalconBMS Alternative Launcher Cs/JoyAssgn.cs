using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class JoyAssgn
    {
        // Member
        protected string productName = "";
        protected Guid productGUID;
        protected Guid instanceGUID;

        // Method
        public string GetProductName() { return this.productName; }
        public Guid GetProductGUID() { return this.productGUID; }
        public Guid GetInstanceGUID() { return this.instanceGUID; }

        public AxAssgn[] axis = new AxAssgn[8];
        // [0]=X
        // [1]=Y
        // [2]=Z
        // [3]=Rx
        // [4]=Ry
        // [5]=Rz
        // [6]=Slider0
        // [7]=Slider1
        public PovAssgn[] pov = new PovAssgn[4];
        // [0]=POV1
        // [1]=POV2
        // [2]=POV3
        // [3]=POV4
        public DxAssgn[] dx = new DxAssgn[32];
        // CallBack:      
        // Dx:              The DX button ID (0-31 for 1st Device) Or 
        //                  POV Hat Number (0-1:Unshifted 2-3:Shifted)

        // Invoke:          -1=Default -2=Key Down only -4=Key Up Only 8=UI Or
        //                  -1=If POV

        // Definition:      -2=DX -3=POV

        // Press:           0=PRESS 0x42=RELEASE Or 
        //                  (0-7 CW)=Panning direction for POV

        // None:            Not used: 0x0 ALWAYS
        // SoundID:         Sound ID: -1 Or 0
        // Description:     The description
        public class AxAssgn
        {
            // Member
            protected string axisName = "";     // ex:Roll, Pitch, Yaw etc...
            protected System.DateTime assgnDate = DateTime.Parse("12/12/1998 12:00:00");
            protected bool invert = false;
            protected AxCurve saturation = 0;
            protected AxCurve deadzone = 0;

            // Property for XML
            public string AxisName { get { return this.axisName; } set { this.axisName = value; } }
            public DateTime AssgnDate { get { return this.assgnDate; } set { this.assgnDate = value; } }
            public bool Invert { get { return this.invert; } set { this.invert = value; } }
            public AxCurve Saturation { get { return this.saturation; } set { this.saturation = value; } }
            public AxCurve Deadzone { get { return this.deadzone; } set { this.deadzone = value; } }

            // Constructor
            public AxAssgn(String axisName, MainWindow.InGameAxAssgn axisassign)
            {
                this.axisName = axisName;
                this.assgnDate = DateTime.Now;
                this.invert = axisassign.GetInvert();
                this.saturation = axisassign.GetSaturation();
                this.deadzone = axisassign.GetDeadzone();
            }

            public AxAssgn() { }

            // Method
            public string GetAxisName() { return this.axisName; }
            public DateTime GetAssignDate() { return this.assgnDate; }
            public bool GetInvert() { return this.invert; }
            public AxCurve GetDeadZone() { return this.deadzone; }
            public AxCurve GetSaturation() { return this.saturation; }
        }
        public class DxAssgn
        {
            public Assgn[] assign = new Assgn[4];
            // [0]=PRESS
            // [1]=PRESS + SHIFT
            // [2]=RELEASE
            // [3]=RELEASE + SHIFT
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
                public Assgn(string callback, Invoke invoke, int soundID)
                {
                    this.callback = callback;
                    this.invoke = invoke;
                    this.soundID = soundID;
                }

                public Assgn() { }

                // Method
                public string GetCallback() { return this.callback; }
                public Invoke GetInvoke() { return this.invoke; }
                public int GetSoundID() { return this.soundID; }
            }

            public void Assign(string callback, Pinky pinky, Behaviour behaviour, Invoke invoke, int soundID)
            {
                this.assign[(int)pinky + (int)behaviour] = new Assgn(callback, invoke, soundID);
            }

        }
        public class PovAssgn
        {
            public DirAssgn[] direction = new DirAssgn[8];
            public class DirAssgn
            {
                // Member
                protected string[] callback = new string[2] { "SimDoNothing", "SimDoNothing" };
                protected int[] soundID = new int[2] { 0, 0 };
                // [0]=PRESS
                // [1]=PRESS + SHIFT

                // Property for XML
                public string[] Callback { get { return this.callback; } set { this.callback = value; } }
                public int[] SoundID { get { return this.soundID; } set { this.soundID = value; } }

                // Constructor
                public DirAssgn() { }

                // Method
                public string GetCallback(Pinky pinky) { return this.callback[(int)pinky]; }
                public int GetSoundID(Pinky pinky) { return this.soundID[(int)pinky]; }

                public void Assign(string callback, Pinky pinky, int soundID)
                {
                    this.callback[(int)pinky] = callback;
                    this.soundID[(int)pinky] = soundID;
                }

                public void UnAssign(Pinky pinky)
                {
                    this.callback[(int)pinky] = "SimDoNothing";
                    this.soundID[(int)pinky] = 0;
                }
            }

            public void Assign(int GetPointofView, string callback, Pinky pinky, int soundID)
            {
                if (GetPointofView > 7)
                    GetPointofView = GetPointofView / 4500;
                this.direction[GetPointofView].Assign(callback, pinky, soundID);
            }

            public string GetDirection(int GetPointOfView)
            {
                string direction = "";
                if (GetPointOfView > 7)
                    GetPointOfView = GetPointOfView / 4500;
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
        }

        /// <summary>
        /// Make new instance.
        /// </summary>
        public JoyAssgn()
        {
            for (int i = 0; i < this.axis.Length; i++)
                this.axis[i] = new JoyAssgn.AxAssgn();
            for (int i = 0; i < this.pov.Length; i++)
            {
                this.pov[i] = new JoyAssgn.PovAssgn();
                for (int ii = 0; ii < this.pov[i].direction.Length; ii++)
                    this.pov[i].direction[ii] = new JoyAssgn.PovAssgn.DirAssgn();
            }
            for (int i = 0; i < this.dx.Length; i++)
            {
                this.dx[i] = new JoyAssgn.DxAssgn();
                for (int ii = 0; ii < this.dx[i].assign.Length; ii++)
                    this.dx[i].assign[ii] = new JoyAssgn.DxAssgn.Assgn();
            }
        }

        /// <summary>
        /// Make new instance.
        /// </summary>
        public void SetDeviceInstance(DeviceInstance deviceInstance)
        {
            this.productGUID = deviceInstance.ProductGuid;
            this.productName = deviceInstance.ProductName;
            this.instanceGUID = deviceInstance.InstanceGuid;
        }

        /// <summary>
        /// UnAssign any DX/POV assignment if the callback name was same.
        /// </summary>
        public void UnassigntargetCallback(string callbackname)
        {
            for (int i = 0; i < this.dx.Length; i++)
                for (int ii = 0; ii < this.dx[i].assign.Length; ii++)
                    if (this.dx[i].assign[ii].GetCallback() == callbackname)
                        this.dx[i].assign[ii] = new DxAssgn.Assgn();
            for (int i = 0; i < this.pov.Length; i++)
            {
                for (int ii = 0; ii < this.pov[i].direction.Length; ii++)
                {
                    if (this.pov[i].direction[ii].GetCallback(Pinky.UnShift) == callbackname)
                        this.pov[i].direction[ii].UnAssign(Pinky.UnShift);
                    if (this.pov[i].direction[ii].GetCallback(Pinky.Shift) == callbackname)
                        this.pov[i].direction[ii].UnAssign(Pinky.Shift);
                }
            }

        }

        /// <summary>
        /// Get ProductGUID and ProductName to write DeviceSorting.txt
        /// </summary>
        public string GetDeviceSortingLine()
        {
            Guid guid = this.GetProductGUID();
            string str = guid.ToString().ToUpper();
            str = "{" + str + "} \"" + this.GetProductName() + "\"\r\n";
            return str;
        }

        /// <summary>
        /// Get whole DX button assignment line to write a key file.
        /// </summary>
        public string GetKeyLineDX(int joynum, int numOfDevices)
        {
            string assign = "";
            assign += "\n#======== " + this.GetProductName() + " ========\n";
            for (int i = 0; i < this.dx.Length; i++)
            {
                for (int ii = 0; ii < this.dx[i].assign.Length; ii++)
                {
                    if (this.dx[i].assign[ii].GetCallback() == "SimDoNothing")
                        continue;
                    assign += this.dx[i].assign[ii].GetCallback();
                    if (ii == 0 | ii == 2)
                        assign += " " + (joynum * 32 + i).ToString();
                    if (ii == 1 | ii == 3)
                        assign += " " + (numOfDevices * 32 + joynum * 32 + i).ToString();
                    assign += " " + ((int)this.dx[i].assign[ii].GetInvoke()).ToString();
                    assign += " " + "-2";
                    if (ii == 0 | ii == 1)
                        assign += " " + "0";
                    if (ii == 2 | ii == 3)
                        assign += " " + "0x42";
                    assign += " " + "0x0";
                    assign += " " + this.dx[i].assign[ii].GetSoundID();
                    assign += "\n";
                }
            }
            return assign;
        }

        /// <summary>
        /// Get each POV hat assignment line to write a key file.
        /// </summary>
        public string GetKeyLinePOV()
        {
            string assign = "";
            assign += "\n#======== " + this.GetProductName() + " : POV ========\n";
            for (int i = 0; i < this.pov.Length; i++)
            {
                for (int ii = 0; ii < this.pov[i].direction.Length; ii++)
                {
                    for (int iii = 0; iii < 2; iii++)
                    {
                        if (this.pov[i].direction[ii].GetCallback((Pinky)iii) == "SimDoNothing")
                            continue;
                        assign += this.pov[i].direction[ii].GetCallback((Pinky)iii);
                        if ((Pinky)iii == Pinky.UnShift)
                            assign += " " + i.ToString();
                        if ((Pinky)iii == Pinky.Shift)
                            assign += " " + (i + 2).ToString();
                        assign += " " + "-1";
                        assign += " " + "-3";
                        assign += " " + ii.ToString();
                        assign += " " + "0x0";
                        assign += " " + this.pov[i].direction[ii].GetSoundID((Pinky)iii);
                        assign += "\n";
                    }
                }
            }
            return assign;
        }

        /// <summary>
        /// Reset Physical axis which has assigned to "sender.name"
        /// </summary>
        public void ResetPreviousAxis(string axisname)
        {
            for (int i = 0; i < this.axis.Length; i++)
                if (this.axis[i].GetAxisName() == axisname)
                    this.axis[i] = new JoyAssgn.AxAssgn();
        }

        /// <summary>
        /// Write Joy Assignment Status to KeyMappingGridCell.
        /// </summary>
        public string KeyMappingPreviewDX(KeyAssgn keyAssign)
        {
            string result;
            result = "";

            for (int i = 0; i < this.dx.Length; i++)
            {
                for (int ii = 0; ii < this.dx[i].assign.Length; ii++)
                {
                    if (this.dx[i].assign[ii].GetCallback() == "SimDoNothing")
                        continue;
                    if (keyAssign.GetCallback() != this.dx[i].assign[ii].GetCallback())
                        continue;
                    if (result != "")
                        result += "\n";
                    result += " DX" + (i + 1);
                    if (ii == 1) //PRESS + SHIFT
                        result += " SHFT";
                    if (ii == 2) //RELEASE
                        result += " REL";
                    if (ii == 3) //RELEASE + SHIFT
                        result += " REL SHFT";
                    if (this.dx[i].assign[ii].GetInvoke() == Invoke.Down)
                        result += " INV: DN";
                    if (this.dx[i].assign[ii].GetInvoke() == Invoke.Up)
                        result += " INV: UP";
                }
            }
            return result;
        }


        /// <summary>
        /// Write Joy Assignment Status to KeyMappingGridCell.
        /// </summary>
        public string KeyMappingPreviewPOV(KeyAssgn keyAssign)
        {
            string result;
            result = "";
            
            for (int i = 0; i < this.pov.Length; i++)
            {
                for (int ii = 0; ii < this.pov[i].direction.Length; ii++)
                {
                    string direction = this.pov[i].GetDirection(ii);
                    for (int iii = 0; iii < 2; iii++)
                    {
                        if (this.pov[i].direction[ii].GetCallback((Pinky)iii) == "SimDoNothing")
                            continue;
                        if (keyAssign.GetCallback() != this.pov[i].direction[ii].GetCallback((Pinky)iii))
                            continue;
                        if (result != "")
                            result += "\n";
                        result += " POV" + (i + 1) + "." + direction;
                        if (iii == 1) //PRESS + SHIFT
                            result += " SHFT";
                    }
                }
            }
            return result;
        }
    }

    public enum AxCurve
    {
        None,
        Small,
        Medium,
        Large
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
