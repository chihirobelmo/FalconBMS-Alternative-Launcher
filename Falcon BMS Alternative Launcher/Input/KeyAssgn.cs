using System;
using System.Text;
using FalconBMS.Launcher.Windows;

using Microsoft.DirectX.DirectInput;

namespace FalconBMS.Launcher.Input
{
    public class KeyAssgn : ICloneable
    {
        protected string callback = CommonConstants.SIMDONOTHING;            // 1st: callback(ex: CommonConstants.SIMDONOTHING)
        protected string soundID = "-1";                       // 2nd: -1
        protected string none = "0";                           // 3rd: 0 
        protected string keyboard = "0xFFFFFFFF";              // 4th: Scancode(ex: 0x1E => 30 => A).
        protected string modifier = "0";                       // 5th: Modification keys(ex: Shift,Ctrl,Alt).
        protected string keycombo = "0";                       // 6th: (Same as keyboard)
        protected string keycomboMod = "0";                    // 7th: (Same as modifier but for keycombo)
        protected string visibility = "-0";                    // 8th: 1=Visible -1=Headline -0=Locked -2=hidden
        protected string description = "!Alt Launcher ERROR!"; // 9th: The description

        protected int numericScancode;
        protected int numericModFlags;

        // for Datagrid Display //
        public string Visibility { get; set; }
        public string Mapping => " " + description.Replace("\"","");

        public string Key {
            get => GetKeyAssignmentStatus();
        }

        public string GetCallback() { return callback; }
        public string GetKeycombo() { return keycombo; }
        public string GetKeycomboMod() { return keycomboMod; }
        public string GetKeyDescription() { return description; }
        public int GetSoundID() { return Int32.Parse(soundID); }

        public int GetScancode() { return numericScancode; }
        public int GetModFlags() { return numericModFlags; }

        public KeyAssgn() { }

        public KeyAssgn(params string[] stringParams)
        {
            callback = stringParams[0];
            soundID = stringParams[1];
            none = stringParams[2];
            keyboard = stringParams[3];
            numericScancode = Convert.ToInt32(keyboard, fromBase:16);
            modifier = stringParams[4];
            numericModFlags = Convert.ToInt32(modifier, fromBase:10);
            keycombo = stringParams[5];
            keycomboMod = stringParams[6];
            visibility = stringParams[7];
            if (visibility == "-2")
                visibility = "Hidden";
            else if (visibility == "-1")
                visibility = "Blue";
            else if (visibility == "-0")
                visibility = "Green";
            else if (visibility == "1")
                visibility = "White";
            else
            {
                visibility = "Green";
                description += "!Alt Launcher ERROR!";
            }
            description = "";

            if (stringParams.Length >= 9)
                description = stringParams[8];
            if (stringParams.Length > 9)
                for (int i = 9; i < stringParams.Length; i++)
                    description += " " + stringParams[i];

            if (callback == "SimHotasPinkyShift" || callback == "SimHotasShift")
                visibility = "White";
        }

        public void CopyOtherKeyAssgn(KeyAssgn otherInstance)
        {
            callback    = otherInstance.callback;
            soundID     = otherInstance.soundID;
            none        = otherInstance.none;
            keyboard    = otherInstance.keyboard;
            numericScancode = otherInstance.numericScancode;
            modifier    = otherInstance.modifier;
            numericModFlags = otherInstance.numericModFlags;
            keycombo    = otherInstance.keycombo;
            keycomboMod = otherInstance.keycomboMod;
            visibility  = otherInstance.visibility;
            description = otherInstance.description;
        }

        /// <summary>
        /// Get Visibility information (1=Visible -1=Headline -0=Locked -2=hidden)
        /// </summary>
        public string GetVisibility() { return visibility; }

        /// <summary>
        /// Returns each line to write to BMS - FULL.key file.
        /// </summary>
        /// <returns></returns>
        public string GetKeyLine()
        {
            string line = "";
            if (visibility == "Blue")
                line += "#=======================================" +
                    "============================================\n";
            line += callback;
            line += " " + soundID;
            line += " " + none;
            line += " " + keyboard;
            line += " " + modifier;
            line += " " + keycombo;
            line += " " + keycomboMod;
            if (visibility == "Hidden")
                line += " -2";
            if (visibility == "Blue")
                line += " -1";
            if (visibility == "Green")
                line += " -0";
            if (visibility == "White")
                line += " 1";
            if (visibility == "-2")
                line += " -2";
            if (visibility == "-1")
                line += " -1";
            if (visibility == "-0")
                line += " -0";
            if (visibility == "1")
                line += " 1";
            line += " " + description;
            line += "\n";
            return line;
        }

        /// <summary>
        /// Convert keyboard input to KEY file line format.
        /// </summary>
        /// <param name="scancode10"></param>
        /// <param name="shift"></param>
        /// <param name="ctrl"></param>
        /// <param name="alt"></param>
        public void SetKeyboard(int scancode10, bool shift, bool ctrl, bool alt)
        {
            int code = 0;
            if (shift)
                code += 1;
            if (ctrl)
                code += 2;
            if (alt)
                code += 4;
            modifier = code.ToString();

            keyboard = "0x" + scancode10.ToString("X");

            numericScancode = scancode10;
            numericModFlags = code;
        }

        /// <summary>
        /// Convert Shift/Ctrl/Alt key combination to KEY file line format.
        /// </summary>
        /// <param name="scancode10"></param>
        /// <param name="shift"></param>
        /// <param name="ctrl"></param>
        /// <param name="alt"></param>
        public void Setkeycombo(int scancode10, bool shift, bool ctrl, bool alt)
        {
            //if (this.keyboard == "0xFFFFFFFF")
                //return;

            int code = 0;
            if (shift)
                code += 1;
            if (ctrl)
                code += 2;
            if (alt)
                code += 4;
            keycomboMod = code.ToString();

            keycombo = "0x" + scancode10.ToString("X");
        }

        /// <summary>
        /// Unassign the line.
        /// </summary>
        public void UnassignKeyboard()
        {
            keyboard = "0xFFFFFFFF";
            modifier = "0";
            keycombo = "0";
            keycomboMod = "0";

            numericScancode = 0;
            numericModFlags = 0;
        }

        /// <summary>
        /// Return overall key assignment status (ex: Alt c : Shift g)
        /// </summary>
        public string GetKeyAssignmentStatus()
        {
            string assignmentStatus = "";

            // keycombo //
            if (keycombo != "0")
            {
                // keycomboMod //
                switch (keycomboMod)
                {
                    case "0":
                        assignmentStatus += "";
                        break;
                    case "1":
                        assignmentStatus += "Shift ";
                        break;
                    case "2":
                        assignmentStatus += "Ctrl ";
                        break;
                    case "3":
                        assignmentStatus += "Ctrl+Shift ";
                        break;
                    case "4":
                        assignmentStatus += "Alt ";
                        break;
                    case "5":
                        assignmentStatus += "Alt+Shift ";
                        break;
                    case "6":
                        assignmentStatus += "Ctrl+Alt ";
                        break;
                    case "7":
                        assignmentStatus += "Ctrl+Shift+Alt ";
                        break;
                }

                int scancode10 = Convert.ToInt32(keycombo, fromBase:16);

                // int -> enum
                Key int2enum = (Key)scancode10;

                assignmentStatus += int2enum + "\t: ";
            }

            if (keyboard != "0xFFFFFFFF")
            {
                // modifier //
                switch (modifier)
                {
                    case "0":
                        assignmentStatus += "";
                        break;
                    case "1":
                        assignmentStatus += "Shift ";
                        break;
                    case "2":
                        assignmentStatus += "Ctrl ";
                        break;
                    case "3":
                        assignmentStatus += "Ctrl+Shift ";
                        break;
                    case "4":
                        assignmentStatus += "Alt ";
                        break;
                    case "5":
                        assignmentStatus += "Alt+Shift ";
                        break;
                    case "6":
                        assignmentStatus += "Ctrl+Alt ";
                        break;
                    case "7":
                        assignmentStatus += "Ctrl+Shift+Alt ";
                        break;
                }

                int scancode10 = Convert.ToInt32(keyboard, fromBase:16);

                // int -> enum
                Key int2enum = (Key)scancode10;

                if (int2enum.ToString() == "-1")
                { return assignmentStatus; }

                assignmentStatus += int2enum.ToString();
            }

            return assignmentStatus;
        }

        // Z_Joy_<asssigned joystick number> = "DX1 DX16 POV1UP" //
        public string Z_Joy_0 => ReadJoyAssignment(0);
        public string Z_Joy_1 => ReadJoyAssignment(1);
        public string Z_Joy_2 => ReadJoyAssignment(2);
        public string Z_Joy_3 => ReadJoyAssignment(3);
        public string Z_Joy_4 => ReadJoyAssignment(4);
        public string Z_Joy_5 => ReadJoyAssignment(5);
        public string Z_Joy_6 => ReadJoyAssignment(6);
        public string Z_Joy_7 => ReadJoyAssignment(7);
        public string Z_Joy_8 => ReadJoyAssignment(8);
        public string Z_Joy_9 => ReadJoyAssignment(9);
        public string Z_Joy_10 => ReadJoyAssignment(10);
        public string Z_Joy_11 => ReadJoyAssignment(11);
        public string Z_Joy_12 => ReadJoyAssignment(12);
        public string Z_Joy_13 => ReadJoyAssignment(13);
        public string Z_Joy_14 => ReadJoyAssignment(14);
        public string Z_Joy_15 => ReadJoyAssignment(15);

        public string ReadJoyAssignment(int joyId)
        {
            JoyAssgn[] joyAssgns = MainWindow.deviceControl.GetJoystickMappingsForButtonsAndHats();

            if (joyId >= joyAssgns.Length)
                return "";

            StringBuilder sb = new StringBuilder();
            sb.Append(joyAssgns[joyId].KeyMappingPreviewDX(this));

            // PRIMARY DEVICE POV
            InGameAxAssgn rollAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Roll.ToString()];
            InGameAxAssgn throttleAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Throttle.ToString()];
            if (rollAxis.GetDeviceNumber() == joyId || throttleAxis.GetDeviceNumber() == joyId)
            {
                string tmp = joyAssgns[joyId].KeyMappingPreviewPOV(this);
                if (!string.IsNullOrEmpty(tmp))
                    sb.Append("\n"+tmp);
            }
            return sb.ToString();
        }

        public string ReadJoyAssignment(int joyId, JoyAssgn[] joyAssgns)
        {
            if (joyId >= joyAssgns.Length)
                return "";

            StringBuilder sb = new StringBuilder();

            string tmp1 = joyAssgns[joyId].KeyMappingPreviewDX(this);
            if (!string.IsNullOrEmpty(tmp1))
                sb.Append("JOY " + joyId + " " + tmp1.Replace("\n", ", "));

            // PRIMARY DEVICE POV
            InGameAxAssgn rollAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Roll.ToString()];
            InGameAxAssgn throttleAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Throttle.ToString()];
            if (rollAxis.GetDeviceNumber() == joyId || throttleAxis.GetDeviceNumber() == joyId) 
            {
                string tmp2 = joyAssgns[joyId].KeyMappingPreviewPOV(this);
                if (!string.IsNullOrEmpty(tmp2))
                    sb.Append("; " + "JOY " + joyId + " " + tmp2.Replace("\n", ", "));
            }
            return sb.ToString();
        }

        object ICloneable.Clone() => Clone();

        public KeyAssgn Clone()
        {
            return (KeyAssgn)MemberwiseClone();
        }
    }
}
