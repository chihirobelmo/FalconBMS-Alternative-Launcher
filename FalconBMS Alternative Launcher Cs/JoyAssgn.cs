using Microsoft.DirectX.DirectInput;
using System;
using System.Linq;
using System.Text;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class JoyAssgn
    {
        // Member
        protected string productName = "";
        protected Guid productGUID;
        protected Guid instanceGUID;

        // Method
        public string GetProductName() { return productName; }
        public Guid GetProductGUID() { return productGUID; }
        public Guid GetInstanceGUID() { return instanceGUID; }

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
        public JoyAssgn()
        {
            for (int i = 0; i < axis.Length; i++)
                axis[i] = new AxAssgn();
            for (int i = 0; i < pov.Length; i++)
                pov[i] = new PovAssgn();
            for (int i = 0; i < dx.Length; i++)
                dx[i] = new DxAssgn();
        }
        public JoyAssgn(JoyAssgn otherInstance)
        {
            productGUID = otherInstance.productGUID;
            productName = otherInstance.productName;
            instanceGUID = otherInstance.instanceGUID;
            for (int i = 0; i < axis.Length; i++)
                axis[i] = otherInstance.axis[i].Clone();
            for (int i = 0; i < pov.Length; i++)
                pov[i] = otherInstance.pov[i].Clone();
            for (int i = 0; i < dx.Length; i++)
                dx[i] = otherInstance.dx[i].Clone();
        }

        /// <summary>
        /// Make new instance.
        /// </summary>
        public void SetDeviceInstance(DeviceInstance deviceInstance)
        {
            productGUID = deviceInstance.ProductGuid;
            productName = deviceInstance.ProductName;
            instanceGUID = deviceInstance.InstanceGuid;
        }

        /// <summary>
        /// UnAssign any DX/POV assignment if the callback name was same.
        /// </summary>
        public void UnassigntargetCallback(string callbackname)
        {
            for (int i = 0; i < dx.Length; i++)
                for (int ii = 0; ii < dx[i].assign.Length; ii++)
                    if (dx[i].assign[ii].GetCallback() == callbackname)
                        dx[i].assign[ii] = new Assgn();
            for (int i = 0; i < pov.Length; i++)
            {
                for (int ii = 0; ii < pov[i].direction.Length; ii++)
                {
                    if (pov[i].direction[ii].GetCallback(Pinky.UnShift) == callbackname)
                        pov[i].direction[ii].UnAssign(Pinky.UnShift);
                    if (pov[i].direction[ii].GetCallback(Pinky.Shift) == callbackname)
                        pov[i].direction[ii].UnAssign(Pinky.Shift);
                }
            }

        }

        /// <summary>
        /// Get ProductGUID and ProductName to write DeviceSorting.txt
        /// </summary>
        public string GetDeviceSortingLine()
        {
            Guid guid = GetProductGUID();
            string str = guid.ToString().ToUpper();
            str = "{" + str + "} \"" + GetProductName() + "\"\r\n";
            return str;
        }

        /// <summary>
        /// Get whole DX button assignment line to write a key file.
        /// </summary>
        public string GetKeyLineDX(int joynum, int numOfDevices)
        {
            string assign = "";
            assign += "\n#======== " + GetProductName() + " ========\n";
            for (int i = 0; i < dx.Length; i++)
            {
                for (int ii = 0; ii < dx[i].assign.Length; ii++)
                {
                    if (dx[i].assign[ii].GetCallback() == "SimDoNothing")
                        continue;
                    if (dx[i].assign[ii].GetCallback() == "SimHotasPinkyShift")
                    {
                        if (ii != 0)
                            continue;
                        assign += dx[i].assign[ii].GetCallback();
                        assign += " " + (joynum * 32 + i);
                        assign += " " + (int)Invoke.Default;
                        assign += " " + "-2";
                        assign += " " + "0";
                        assign += " " + "0x0";
                        assign += " " + dx[i].assign[ii].GetSoundID();
                        assign += "\n";
                        assign += dx[i].assign[ii].GetCallback();
                        assign += " " + (numOfDevices * 32 + joynum * 32 + i);
                        assign += " " + (int)Invoke.Default;
                        assign += " " + "-2";
                        assign += " " + "0";
                        assign += " " + "0x0";
                        assign += " " + dx[i].assign[ii].GetSoundID();
                        assign += "\n";
                        continue;
                    }
                    assign += dx[i].assign[ii].GetCallback();
                    if (ii == 0 | ii == 2)
                        assign += " " + (joynum * 32 + i);
                    if (ii == 1 | ii == 3)
                        assign += " " + (numOfDevices * 32 + joynum * 32 + i);
                    assign += " " + (int)dx[i].assign[ii].GetInvoke();
                    assign += " " + "-2";
                    if (ii == 0 | ii == 1)
                        assign += " " + "0";
                    if (ii == 2 | ii == 3)
                        assign += " " + "0x42";
                    assign += " " + "0x0";
                    assign += " " + dx[i].assign[ii].GetSoundID();
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
            assign += "\n#======== " + GetProductName() + " : POV ========\n";
            for (int i = 0; i < pov.Length; i++)
            {
                for (int ii = 0; ii < pov[i].direction.Length; ii++)
                {
                    for (int iii = 0; iii < 2; iii++)
                    {
                        if (i < 2)
                        {
                            // if (this.pov[i].direction[ii].GetCallback((Pinky)iii) == "SimDoNothing")
                            //    continue;
                            assign += pov[i].direction[ii].GetCallback((Pinky)iii);
                            if ((Pinky)iii == Pinky.UnShift)
                                assign += " " + i;
                            if ((Pinky)iii == Pinky.Shift)
                                assign += " " + (i + 2);
                            assign += " " + "-1";
                            assign += " " + "-3";
                            assign += " " + ii;
                            assign += " " + "0x0";
                            assign += " " + pov[i].direction[ii].GetSoundID((Pinky)iii);
                            assign += "\n";
                        }
                        else
                        {
                            if (pov[i].direction[ii].GetCallback((Pinky)iii) == "SimDoNothing" & pov[i-2].direction[ii].GetCallback((Pinky)iii) != "SimDoNothing")
                                continue;
                            assign += pov[i].direction[ii].GetCallback((Pinky)iii);
                            if ((Pinky)iii == Pinky.UnShift)
                                assign += " " + i;
                            if ((Pinky)iii == Pinky.Shift)
                                assign += " " + (i + 2);
                            assign += " " + "-1";
                            assign += " " + "-3";
                            assign += " " + ii;
                            assign += " " + "0x0";
                            assign += " " + pov[i].direction[ii].GetSoundID((Pinky)iii);
                            assign += "\n";
                        }
                    }
                }
            }
            return assign;
        }

        public string GetKeyLinePOV(int povNum)
        {
            string assign = "";
            assign += "\n#======== " + GetProductName() + " : POV ========\n";
            for (int i = 0; i < 1; i++)
            {
                for (int ii = 0; ii < pov[i].direction.Length; ii++)
                {
                    for (int iii = 0; iii < 2; iii++)
                    {
                        // if (this.pov[i].direction[ii].GetCallback((Pinky)iii) == "SimDoNothing")
                        //    continue;
                        assign += pov[i].direction[ii].GetCallback((Pinky)iii);
                        if ((Pinky)iii == Pinky.UnShift)
                            assign += " " + povNum;
                        if ((Pinky)iii == Pinky.Shift)
                            assign += " " + (povNum + 2);
                        assign += " " + "-1";
                        assign += " " + "-3";
                        assign += " " + ii;
                        assign += " " + "0x0";
                        assign += " " + pov[i].direction[ii].GetSoundID((Pinky)iii);
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
            for (int i = 0; i < axis.Length; i++)
                if (axis[i].GetAxisName() == axisname)
                    axis[i] = new AxAssgn();
        }

        /// <summary>
        /// Write Joy Assignment Status to KeyMappingGridCell.
        /// </summary>
        public string KeyMappingPreviewDX(KeyAssgn keyAssign)
        {
            string result;
            result = "";

            for (int i = 0; i < dx.Length; i++)
            {
                for (int ii = 0; ii < dx[i].assign.Length; ii++)
                {
                    if (dx[i].assign[ii].GetCallback() == "SimDoNothing")
                        continue;
                    if (keyAssign.GetCallback() != dx[i].assign[ii].GetCallback())
                        continue;
                    if (result != "")
                        result += "\n";
                    result += " DX" + (i + 1);
                    if (ii == 1) //PRESS + SHIFT
                        result += " SHIFT";
                    if (ii == 2) //RELEASE
                        result += " RELEASE";
                    if (ii == 3) //RELEASE + SHIFT
                        result += " RELEASE SHIFT";
                    if (dx[i].assign[ii].GetInvoke() == Invoke.Down && ii != 2 && ii !=3 )
                        result += " HOLD";
                }
            }
            return result;
        }

        /// <summary>
        /// Write Joy Assignment Status to KeyMappingGridCell.
        /// </summary>
        public string KeyMappingPreviewPOV(KeyAssgn keyAssign)
        {
            string result = "";
            
            for (int i = 0; i < pov.Length; i++)
            {
                for (int ii = 0; ii < pov[i].direction.Length; ii++)
                {
                    string direction = pov[i].GetDirection(ii);
                    for (int iii = 0; iii < 2; iii++)
                    {
                        if (pov[i].direction[ii].GetCallback((Pinky)iii) == "SimDoNothing")
                            continue;
                        if (keyAssign.GetCallback() != pov[i].direction[ii].GetCallback((Pinky)iii))
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
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == GetDeviceSortingLine().Replace("\r\n", ""))
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
                    if (stArrayData[2] == "-1")
                        invokeStatus = Invoke.Default;
                    if (stArrayData[2] == "-2")
                        invokeStatus = Invoke.Down;
                    if (stArrayData[2] == "-4")
                        invokeStatus = Invoke.Up;
                    if (stArrayData[2] == "8")
                        invokeStatus = Invoke.UI;
                    if (stArrayData[3] == "0")
                        behaviourStatus = Behaviour.Press;
                    if (stArrayData[3] == "0x42")
                        behaviourStatus = Behaviour.Release;
                }
                // Import DX Setup
                if (stArrayData[3] == "-2")
                {
                    for (int i = 0; i < 32; i++)
                    {
                        if (int.Parse(stArrayData[1]) == i + currentID * 32)
                            dx[i].Assign(stArrayData[0], Pinky.UnShift, behaviourStatus, invokeStatus, 0);
                        if (int.Parse(stArrayData[1]) == i + currentID * 32 + devcount * 32) // Okay This has to be the problem. I have to read FalconBMS.cfg for
                            dx[i].Assign(stArrayData[0], Pinky.Shift, behaviourStatus, invokeStatus, 0);
                    }
                }
                // Import POV Setup
                if (stArrayData[3] == "-3")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (int.Parse(stArrayData[1]) != i)
                            continue;
                        if (povnum < 3)
                        {
                            if (i == 0 | i == 1)
                                pov[i].direction[int.Parse(stArrayData[4])].Assign(stArrayData[0], Pinky.UnShift, 0);
                            if (i == 2 | i == 3)
                                pov[i - 2].direction[int.Parse(stArrayData[4])].Assign(stArrayData[0], Pinky.Shift, 0);
                            continue;
                        }
                        pov[i].direction[int.Parse(stArrayData[4])].Assign(stArrayData[0], Pinky.UnShift, 0);
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
                for (int i = 0; i < axisMappingList.Length; i++)
                {
                    for (int ii = 0; ii < devcount; ii++)
                    {
                        if (ad[24 + i * 16] == currentID + 2)
                        {
                            int axisNum = ad[24 + i * 16 + 4];
                            AxCurve deadzone = AxCurve.None;
                            if (ad[24 + i * 16 + 8] == 0x64)
                                deadzone = AxCurve.Small;
                            if (ad[24 + i * 16 + 8] == 0xF4)
                                deadzone = AxCurve.Medium;
                            if (ad[24 + i * 16 + 8] == 0xE8)
                                deadzone = AxCurve.Large;
                            AxCurve saturation = AxCurve.None;
                            if (ad[24 + i * 16 + 12] == 0x1C)
                                saturation = AxCurve.Small;
                            if (ad[24 + i * 16 + 12] == 0x28)
                                saturation = AxCurve.Medium;
                            if (ad[24 + i * 16 + 12] == 0x34)
                                saturation = AxCurve.Large;
                            bool invert = false;

                            for (int iii = 0; iii < joystickCalList.Length; iii++)
                            {
                                // read joystick.cal
                                if (axisMappingList[i] != joystickCalList[iii])
                                    continue;
                                int invertnum = jc[iii * 28 + 20];
                                if (invertnum == 1)
                                    invert = true;
                            }

                            InGameAxAssgn inGameAxAssgn = new InGameAxAssgn(currentID, axisNum, invert, deadzone, saturation);
                            axis[axisNum] = new AxAssgn(axisMappingList[i].ToString(), inGameAxAssgn);
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
