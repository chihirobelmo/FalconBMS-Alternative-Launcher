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

        /// <summary>
        /// [0]=X
        /// [1]=Y
        /// [2]=Z
        /// [3]=Rx
        /// [4]=Ry
        /// [5]=Rz
        /// [6]=Slider0
        /// [7]=Slider1
        /// </summary>
        public AxAssgn[] axis = new AxAssgn[8];

        /// <summary>
        /// [0]=POV1
        /// [1]=POV2
        /// [2]=POV3
        /// [3]=POV4
        /// </summary>
        public PovAssgn[] pov = new PovAssgn[4];

        /// <summary>
        /// [N] = DX[N]
        /// </summary>
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
        public DxAssgn[] dx = new DxAssgn[32];

        /// <summary>
        /// Make new instance.
        /// </summary>
        public JoyAssgn() {}
        public JoyAssgn(JoyAssgn otherInstance)
        {
            for (int i = 0; i < this.axis.Length; i++)
                this.axis[i] = otherInstance.axis[i].Clone();
            for (int i = 0; i < this.pov.Length; i++)
                this.pov[i] = otherInstance.pov[i].Clone();
            for (int i = 0; i < this.dx.Length; i++)
                this.dx[i] = otherInstance.dx[i].Clone();
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
                        this.dx[i].assign[ii] = new Assgn();
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
                    if (this.dx[i].assign[ii].GetCallback() == "SimHotasPinkyShift")
                    {
                        if (ii != 0)
                            continue;
                        assign += this.dx[i].assign[ii].GetCallback();
                        assign += " " + (joynum * 32 + i).ToString();
                        assign += " " + ((int)Invoke.Default).ToString();
                        assign += " " + "-2";
                        assign += " " + "0";
                        assign += " " + "0x0";
                        assign += " " + this.dx[i].assign[ii].GetSoundID();
                        assign += "\n";
                        assign += this.dx[i].assign[ii].GetCallback();
                        assign += " " + (numOfDevices * 32 + joynum * 32 + i).ToString();
                        assign += " " + ((int)Invoke.Default).ToString();
                        assign += " " + "-2";
                        assign += " " + "0";
                        assign += " " + "0x0";
                        assign += " " + this.dx[i].assign[ii].GetSoundID();
                        assign += "\n";
                        continue;
                    }
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
                    this.axis[i] = new AxAssgn();
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

        /// <summary>
        /// Make new instance.
        /// </summary>
        public void ImportStockSetup(AppRegInfo appReg, int devcount, int povnum, int joynum)
        {
            Invoke invokeStatus = Invoke.UI;
            Behaviour behaviourStatus = Behaviour.Press;

            string deviceSorting = appReg.GetInstallDir() + "/User/Config/DeviceSorting.txt";
            if (System.IO.Path.GetFileName(deviceSorting) != "DeviceSorting.txt")
                return;
            if (System.IO.File.Exists(deviceSorting) == false)
                return;
            string[] lines = System.IO.File.ReadAllLines(deviceSorting, Encoding.UTF8);
            int currentID = -1;
            for (int i = 0; i < lines.Count(); i++)
            {
                if (lines[i] == this.GetDeviceSortingLine().Replace("\r\n", ""))
                    currentID = i;
            }
            if (currentID == -1)
                return;
            string keyfile = appReg.GetInstallDir() + "/User/Config/" + appReg.getKeyFileName();
            string[] Klines = System.IO.File.ReadAllLines(keyfile, Encoding.UTF8);
            foreach (string stBuffer in Klines)
            {
                string[] stArrayData = stBuffer.Split(' ');
                if (stArrayData.Length < 7)
                    continue;
                if (stBuffer.Substring(0, 1) == "#")
                    continue;
                if (stArrayData[3] == "-2" | stArrayData[3] == "-3")
                {
                    if ((string)stArrayData[2] == "-1")
                        invokeStatus = Invoke.Default;
                    if ((string)stArrayData[2] == "-2")
                        invokeStatus = Invoke.Down;
                    if ((string)stArrayData[2] == "-4")
                        invokeStatus = Invoke.Up;
                    if ((string)stArrayData[2] == "8")
                        invokeStatus = Invoke.UI;
                    if ((string)stArrayData[3] == "0")
                        behaviourStatus = Behaviour.Press;
                    if ((string)stArrayData[3] == "0x42")
                        behaviourStatus = Behaviour.Release;
                }
                // Import DX Setup
                if (stArrayData[3] == "-2")
                {
                    for (int i = 0; i < 32; i++)
                    {
                        if (Int32.Parse(stArrayData[1]) == i + currentID * 32)
                            this.dx[i].Assign((string)stArrayData[0], Pinky.UnShift, behaviourStatus, invokeStatus, 0);
                        if (Int32.Parse(stArrayData[1]) == i + currentID * 32 + devcount * 32) // Okay This has to be the problem. I have to read FalconBMS.cfg for
                            this.dx[i].Assign((string)stArrayData[0], Pinky.Shift, behaviourStatus, invokeStatus, 0);
                    }
                }
                // Import POV Setup
                if (stArrayData[3] == "-3")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (Int32.Parse(stArrayData[1]) != i)
                            continue;
                        if (povnum < 3)
                        {
                            if (i == 0 | i == 1)
                                this.pov[i].direction[Int32.Parse(stArrayData[4])].Assign((string)stArrayData[0], Pinky.UnShift, 0);
                            if (i == 2 | i == 3)
                                this.pov[i - 2].direction[Int32.Parse(stArrayData[4])].Assign((string)stArrayData[0], Pinky.Shift, 0);
                            continue;
                        }
                        this.pov[i].direction[Int32.Parse(stArrayData[4])].Assign((string)stArrayData[0], Pinky.UnShift, 0);
                    }
                }
                // Import Axis Setup
                string filename = appReg.GetInstallDir() + "/User/Config/axismapping.dat";
                if (!System.IO.File.Exists(filename))
                    return;
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                byte[] ad = new byte[fs.Length];
                fs.Read(ad, 0, ad.Length);
                fs.Close();

                filename = appReg.GetInstallDir() + "/User/Config/joystick.cal";
                if (!System.IO.File.Exists(filename))
                    return;
                fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                byte[] jc = new byte[fs.Length];
                fs.Read(jc, 0, jc.Length);
                fs.Close();

                AxisName[] axisMappingList = appReg.getOverrideWriter().getAxisMappingList();
                AxisName[] joystickCalList = appReg.getOverrideWriter().getJoystickCalList();
                for (int i = 0; i < axisMappingList.Count(); i++)
                {
                    for (int ii = 0; ii < devcount; ii++)
                    {
                        if ((int)ad[24 + i * 16] == currentID + 2)
                        {
                            int axisNum = (int)ad[24 + i * 16 + 4];
                            AxCurve deadzone = AxCurve.None;
                            if ((int)ad[24 + i * 16 + 8] == 0x64)
                                deadzone = AxCurve.Small;
                            if ((int)ad[24 + i * 16 + 8] == 0xF4)
                                deadzone = AxCurve.Medium;
                            if ((int)ad[24 + i * 16 + 8] == 0xE8)
                                deadzone = AxCurve.Large;
                            AxCurve saturation = AxCurve.None;
                            if ((int)ad[24 + i * 16 + 12] == 0x1C)
                                saturation = AxCurve.Small;
                            if ((int)ad[24 + i * 16 + 12] == 0x28)
                                saturation = AxCurve.Medium;
                            if ((int)ad[24 + i * 16 + 12] == 0x34)
                                saturation = AxCurve.Large;
                            bool invert = false;

                            for (int iii = 0; iii < joystickCalList.Count(); iii++)
                            {
                                // read joystick.cal
                                if (axisMappingList[i] != joystickCalList[iii])
                                    continue;
                                int invertnum = (int)jc[iii * 28 + 20];
                                if (invertnum == 1)
                                    invert = true;
                            }

                            InGameAxAssgn inGameAxAssgn = new InGameAxAssgn(currentID, axisNum, invert, deadzone, saturation);
                            this.axis[axisNum] = new AxAssgn(axisMappingList[i].ToString(), inGameAxAssgn);
                        }
                    }
                }
            }
        }

        public JoyAssgn Clone()
        {
            return new JoyAssgn(this);
        }
    }
}
