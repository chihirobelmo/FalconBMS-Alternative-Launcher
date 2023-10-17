using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using System.Text.RegularExpressions;

using Microsoft.DirectX.DirectInput;
using System.Diagnostics;

namespace FalconBMS.Launcher.Input
{
    public class JoyAssgn
    {
        protected Device hwDevice = null;

        // Member
        protected string productName = null;
        protected Guid productGUID = Guid.Empty;
        protected Guid instanceGUID = Guid.Empty;

        // Method
        public string GetProductName() { return productName ?? throw new NullReferenceException(); }
        public string GetProductFileName() { return GetProductName().Replace("/", "-"); }
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
        public PovAssgn[] pov = new PovAssgn[CommonConstants.DX_MAX_HATS];

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
        public DxAssgn[] dx = new DxAssgn[CommonConstants.DX_MAX_BUTTONS];

        // Support secondary "profile" for F-15 (buttons and pov only; no axes) but keep existing xml seri structure intact, for backward-compatibility.
        //HACK: this will double the size of the XML serializtion footprint, but retain back-compat with existing users' XML datafiles
        public struct ProfileContainer
        {
            public PovAssgn[] pov;
            public DxAssgn[] dx;
        }
        public ProfileContainer profileDefaultF16;
        public ProfileContainer profileF15ABCD;

        private string currentProfile = null;

        public JoyAssgn()
        {
            Console.WriteLine();
        } //parameterless ctor needed for XML deserialization

        public JoyAssgn(bool allocStorage)
        {
            if (!allocStorage) return;

            for (int i = 0; i < axis.Length; i++)
                axis[i] = new AxAssgn();

            for (int i = 0; i < dx.Length; i++)
                dx[i] = new DxAssgn();
            for (int i = 0; i < pov.Length; i++)
                pov[i] = new PovAssgn();

            // Use ref-copy for the default profile.
            profileDefaultF16.dx = dx;//ref-copy
            profileDefaultF16.pov = pov;//ref-copy

            // But allocate new storage for the secondary profiles.
            profileF15ABCD.dx = new DxAssgn[CommonConstants.DX_MAX_BUTTONS];
            profileF15ABCD.pov = new PovAssgn[CommonConstants.DX_MAX_HATS];

            for (int i = 0; i < CommonConstants.DX_MAX_BUTTONS; i++)
                profileF15ABCD.dx[i] = new DxAssgn();
            for (int i = 0; i < 4; i++)
                profileF15ABCD.pov[i] = new PovAssgn();

            _Debug_ValidateCurrentProfile();
        }

        public JoyAssgn(Device device) : this(allocStorage:true)
        {
            this.hwDevice = device;
            this.SetDeviceInstance(device.DeviceInformation);
        }
        void SetDeviceInstance(DeviceInstance deviceInstance)
        {
            productName = deviceInstance.ProductName;
            productName = Regex.Replace(productName, "[^A-Z|a-z|0-9|~|`|\\[|\\]|\\{|\\}|\\-|_|\\=|\\'|\\s]", String.Empty);

            productGUID = deviceInstance.ProductGuid;
            instanceGUID = deviceInstance.InstanceGuid;
        }

        public void SelectAvionicsProfile(string avionicsProfile = null)
        {
            _Debug_ValidateCurrentProfile();

            if (currentProfile == avionicsProfile)
                return;

            // A little shell-game with pointers, to avoid changing existing logic in 100 places..
            if (currentProfile == null && avionicsProfile == CommonConstants.F15_TAG)
            {
                // Swap from F-16 to F-15.
                this.profileDefaultF16.dx = this.dx;
                this.profileDefaultF16.pov = this.pov;

                this.dx = profileF15ABCD.dx;
                this.pov = profileF15ABCD.pov;
            }
            else if (currentProfile == CommonConstants.F15_TAG && avionicsProfile == null)
            {
                // Swap from F-15 to F-16.
                this.profileF15ABCD.dx = this.dx;
                this.profileF15ABCD.pov = this.pov;

                this.dx = profileDefaultF16.dx;
                this.pov = profileDefaultF16.pov;
            }
            else throw new InvalidProgramException();

            currentProfile = avionicsProfile;
            _Debug_ValidateCurrentProfile();
            return;
        }

        public int JoyAxisState(int joyAxisNumber)
        {
            int input = 0;
            if (hwDevice == null)
                return 0;
            try
            {
                switch (joyAxisNumber)
                {
                    case 0:
                        input = hwDevice.CurrentJoystickState.X;
                        break;
                    case 1:
                        input = hwDevice.CurrentJoystickState.Y;
                        break;
                    case 2:
                        input = hwDevice.CurrentJoystickState.Z;
                        break;
                    case 3:
                        input = hwDevice.CurrentJoystickState.Rx;
                        break;
                    case 4:
                        input = hwDevice.CurrentJoystickState.Ry;
                        break;
                    case 5:
                        input = hwDevice.CurrentJoystickState.Rz;
                        break;
                    case 6:
                        input = hwDevice.CurrentJoystickState.GetSlider()[0];
                        break;
                    case 7:
                        input = hwDevice.CurrentJoystickState.GetSlider()[1];
                        break;
                }
                return input;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("(Catching exception from hwDevice.CurrentJoystickState.)");
                return 0;
            }
        }

        /// <summary>
        /// UnAssign any DX/POV assignment if the callback name was same.
        /// </summary>
        public void UnassigntargetCallback(string callbackname)
        {
            _Debug_ValidateCurrentProfile();

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
        /// DXnumber: total DXnumber per device BMS can handle.
        /// </summary>
        public string GetKeyLineDX(int indexInDeviceSortingOrder, int countDevices)
        {
            _Debug_ValidateCurrentProfile();

            const int DXnumber = CommonConstants.DX_MAX_BUTTONS;

            string assign = "";
            assign += "\n#======== " + GetProductName() + " ========\n";
            for (int i = 0; i < dx.Length; i++)
            {
                for (int ii = 0; ii < dx[i].assign.Length; ii++)
                {
                    if (dx[i].assign[ii].GetCallback() == CommonConstants.SIMDONOTHING)
                        continue;
                    if (dx[i].assign[ii].GetCallback() == "SimHotasPinkyShift" || dx[i].assign[ii].GetCallback() == "SimHotasShift")
                    {
                        if (ii != 0)
                            continue;
                        assign += dx[i].assign[ii].GetCallback();
                        assign += " " + (indexInDeviceSortingOrder * DXnumber + i);
                        assign += " " + (int)Invoke.Default;
                        assign += " " + "-2";
                        assign += " " + "0";
                        assign += " " + "0x0";
                        assign += " " + dx[i].assign[ii].GetSoundID();
                        assign += "\n";
                        assign += dx[i].assign[ii].GetCallback();
                        assign += " " + (countDevices * DXnumber + indexInDeviceSortingOrder * DXnumber + i);
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
                        assign += " " + (indexInDeviceSortingOrder * DXnumber + i);
                    if (ii == CommonConstants.DX_PRESS_SHIFT | ii == CommonConstants.DX_RELEASE_SHIFT)
                        assign += " " + (countDevices * DXnumber + indexInDeviceSortingOrder * DXnumber + i);

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
        /// Serialize the POV hat assignments to key file.
        /// </summary>
        public string GetKeyLinePOV(int povBase, int hatId)
        {
            _Debug_ValidateCurrentProfile();

            StringBuilder povBlock = new StringBuilder(2000);
            povBlock.AppendLine("\n");
            povBlock.AppendLine($"#======== {GetProductName()} : POV #{povBase} ========");

            for (int dirId = 0; dirId < pov[hatId].direction.Length; dirId++)
            {
                for (int shiftState = 0; shiftState < 2; shiftState++)
                {
                    string callback = pov[hatId].direction[dirId].GetCallback((Pinky)shiftState);
                    int povNumShifted = ((Pinky)shiftState == Pinky.Shift) ? (povBase + 2) : povBase;
                    int povDir = dirId;
                    int soundId = pov[hatId].direction[dirId].GetSoundID((Pinky)shiftState);

                    povBlock.AppendLine($"{callback} {povNumShifted} -1 -3 {povDir} 0x0 {soundId}");
                }
            }
            return povBlock.ToString();
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
            _Debug_ValidateCurrentProfile();

            string result;
            result = "";

            for (int i = 0; i < dx.Length; i++)
            {
                for (int ii = 0; ii < dx[i].assign.Length; ii++)
                {
                    if (dx[i].assign[ii].GetCallback() == CommonConstants.SIMDONOTHING)
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
            _Debug_ValidateCurrentProfile();

            string result = "";
            
            for (int i = 0; i < pov.Length; i++)
            {
                for (int ii = 0; ii < pov[i].direction.Length; ii++)
                {
                    string direction = pov[i].GetDirection(ii);
                    for (int iii = 0; iii < 2; iii++)
                    {
                        if (pov[i].direction[ii].GetCallback((Pinky)iii) == CommonConstants.SIMDONOTHING)
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

        public void CopyButtonsAndHatsFromCurrentProfile(JoyAssgn otherJoy)
        {
            //axis = otherJoy.axis;
            //detentPosition = otherJoy.detentPosition;

            Debug.Assert(otherJoy.dx.Length == this.dx.Length);
            Debug.Assert(otherJoy.pov.Length == this.pov.Length);

            Array.Copy(otherJoy.dx, this.dx, this.dx.Length);
            Array.Copy(otherJoy.pov, this.pov, this.pov.Length);
            return;
        }

        public void LoadAxesButtonsAndHatsFrom(string xmlPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(JoyAssgn));

            using (StreamReader sr = File.OpenText(xmlPath))
            {
                JoyAssgn xmlJoy = (JoyAssgn)serializer.Deserialize(sr);

                this.axis = xmlJoy.axis;
                this.detentPosition = xmlJoy.detentPosition;

                // Upgrade-path: these profile* subnodes will be null, when loading an older XML file.
                if (xmlJoy.profileDefaultF16.dx == null)
                {
                    Debug.Assert(xmlJoy.profileDefaultF16.dx == null);
                    Debug.Assert(xmlJoy.profileDefaultF16.pov == null);
                    Debug.Assert(xmlJoy.profileF15ABCD.dx == null);
                    Debug.Assert(xmlJoy.profileF15ABCD.pov == null);

                    this.dx = xmlJoy.dx;
                    this.pov = xmlJoy.pov;

                    // Wire up profileDefaultF16 subnodes to be by-ref copies of the default profile (because it's the one currently selected, at inital startup-time).
                    this.profileDefaultF16.dx = xmlJoy.dx;
                    this.profileDefaultF16.pov = xmlJoy.pov;

                    // But allocate new storage for the secondary (F15) profile.
                    xmlJoy.profileF15ABCD.dx = new DxAssgn[CommonConstants.DX_MAX_BUTTONS];
                    for (int i = 0; i < CommonConstants.DX_MAX_BUTTONS; i++)
                        xmlJoy.profileF15ABCD.dx[i] = new DxAssgn();

                    xmlJoy.profileF15ABCD.pov = new PovAssgn[CommonConstants.DX_MAX_HATS];
                    for (int i = 0; i < CommonConstants.DX_MAX_HATS; i++)
                        xmlJoy.profileF15ABCD.pov[i] = new PovAssgn();

                    this.profileF15ABCD.dx = xmlJoy.profileF15ABCD.dx;
                    this.profileF15ABCD.pov = xmlJoy.profileF15ABCD.pov;
                }
                else
                {
                    Debug.Assert(xmlJoy.profileDefaultF16.dx != null);
                    Debug.Assert(xmlJoy.profileDefaultF16.pov != null);
                    Debug.Assert(xmlJoy.profileF15ABCD.dx != null);
                    Debug.Assert(xmlJoy.profileF15ABCD.pov != null);

                    // Not upgrade-path: wire up this.dx/pov to point to profileDefaultF16 subnodes (because it's the one currently selected, at inital startup-time).
                    this.dx = xmlJoy.profileDefaultF16.dx;
                    this.pov = xmlJoy.profileDefaultF16.pov;

                    this.profileDefaultF16.dx = xmlJoy.profileDefaultF16.dx;
                    this.profileDefaultF16.pov = xmlJoy.profileDefaultF16.pov;

                    this.profileF15ABCD.dx = xmlJoy.profileF15ABCD.dx;
                    this.profileF15ABCD.pov = xmlJoy.profileF15ABCD.pov;
                }
            }
            _Debug_ValidateCurrentProfile();
            return;
        }


        public JoyAssgn MakeTempCloneForKeyMappingDialog()
        {
            _Debug_ValidateCurrentProfile();

            JoyAssgn joy = new JoyAssgn();
            joy.CopyButtonsAndHatsFromCurrentProfile(this);
            joy.currentProfile = "temp";
            return joy;
        }

        public Device GetDevice()
        {
            return hwDevice;
        }

        public JoystickState GetDeviceState()
        {
            try
            {
                return hwDevice.CurrentJoystickState;
            }
            catch 
            {
                return new JoystickState();
            }
        }

        public byte[] GetButtons()
        {
            try
            {
                return hwDevice.CurrentJoystickState.GetButtons();
            }
            catch 
            {
                return new byte[128];
            }
        }

        public int[] GetPointOfView()
        {
            try
            {
                return hwDevice.CurrentJoystickState.GetPointOfView();
            }
            catch
            {
                return new int[8];
            }
        }

        private void _Debug_ValidateCurrentProfile()
        {
#if DEBUG
            if (currentProfile == "temp")
                return;

            if (currentProfile == null)
            {
                Debug.Assert(ReferenceEquals(this.dx, this.profileDefaultF16.dx));
                Debug.Assert(ReferenceEquals(this.pov, this.profileDefaultF16.pov));
            }
            else if (currentProfile == CommonConstants.F15_TAG)
            {
                Debug.Assert(ReferenceEquals(this.dx, this.profileF15ABCD.dx));
                Debug.Assert(ReferenceEquals(this.pov, this.profileF15ABCD.pov));
            }
            else throw new InvalidProgramException();
#endif
        }

    }
}
