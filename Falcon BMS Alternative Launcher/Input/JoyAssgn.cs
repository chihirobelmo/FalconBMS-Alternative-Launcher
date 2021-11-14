using System;
using System.IO;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

using Microsoft.DirectX.DirectInput;

namespace FalconBMS.Launcher.Input
{
    public class JoyAssgn : ICloneable
    {
        public void Load(JoyAssgn j)
        {
            detentPosition = j.detentPosition;

            axis = j.axis;
            pov  = j.pov;

            for (int i = 0; i < j.dx.Length; i++)
            {
                if (i >= dx.Length)
                    return;
                dx[i] = j.dx[i];
            }
        }
        public void LoadAx(AxAssgn a)
        {
            axis[0] = a;
        }
        public AxAssgn GetMouseAxis()
        {
            return axis[0];
        }
        public int GetAssignedNumber()
        {
            return dx.Sum(d => d.GetAssignedNumber());
        }

        // Member
        protected string productName = "";
        protected Guid productGUID;
        protected Guid instanceGUID;

        // Method
        public string GetProductName() { return productName; }
        public Guid GetProductGUID() { return productGUID; }
        public Guid GetInstanceGUID() { return instanceGUID; }

        /// <summary>
        /// Detent Position
        /// </summary>
        public DetentPosition detentPosition = new DetentPosition();

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
        public DxAssgn[] dx = new DxAssgn[CommonConstants.DX128];

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

            detentPosition = otherInstance.detentPosition;

            productName = Regex.Replace(productName, "[^A-Z|a-z|0-9|~|`|\\[|\\]|\\{|\\}|\\-|_|\\=|\\'|\\s]", String.Empty);

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

            productName = Regex.Replace(productName, "[^A-Z|a-z|0-9|~|`|\\[|\\]|\\{|\\}|\\-|_|\\=|\\'|\\s]", String.Empty);
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
                        assign += " " + (joynum * CommonConstants.DX32 + i);
                        assign += " " + (int)Invoke.Default;
                        assign += " " + "-2";
                        assign += " " + "0";
                        assign += " " + "0x0";
                        assign += " " + dx[i].assign[ii].GetSoundID();
                        assign += "\n";
                        assign += dx[i].assign[ii].GetCallback();
                        assign += " " + (numOfDevices * CommonConstants.DX32 + joynum * CommonConstants.DX32 + i);
                        assign += " " + (int)Invoke.Default;
                        assign += " " + "-2";
                        assign += " " + "0";
                        assign += " " + "0x0";
                        assign += " " + dx[i].assign[ii].GetSoundID();
                        assign += "\n";
                        continue;
                    }

                    assign += dx[i].assign[ii].GetCallback();

                    if (ii == CommonConstants.DX_PRESS | ii == CommonConstants.DX_RELEASE)
                        assign += " " + (joynum * CommonConstants.DX32 + i);
                    if (ii == CommonConstants.DX_PRESS_SHIFT | ii == CommonConstants.DX_RELEASE_SHIFT)
                        assign += " " + (numOfDevices * CommonConstants.DX32 + joynum * CommonConstants.DX32 + i);

                    assign += " " + (int)dx[i].assign[ii].GetInvoke();
                    assign += " " + "-2";

                    if (ii == CommonConstants.DX_PRESS | ii == CommonConstants.DX_PRESS_SHIFT)
                        assign += " " + "0";
                    if (ii == CommonConstants.DX_RELEASE | ii == CommonConstants.DX_RELEASE_SHIFT)
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

                    if (ii == CommonConstants.DX_PRESS_SHIFT) 
                        result += " SHIFT";
                    if (ii == CommonConstants.DX_RELEASE)
                        result += " RELEASE";
                    if (ii == CommonConstants.DX_RELEASE_SHIFT) 
                        result += " RELEASE SHIFT";

                    if (dx[i].assign[ii].GetInvoke() == Invoke.Down && ii != CommonConstants.DX_RELEASE && ii != CommonConstants.DX_RELEASE_SHIFT)
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

        object ICloneable.Clone() => Clone();

        public JoyAssgn Clone()
        {
            return new JoyAssgn(this);
        }
    }
}
