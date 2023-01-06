using System;

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

        // for Datagrid Display //
        public string Visibility { get; set; }
        public string Mapping => " " + description.Replace("\"","");

        public string Key {
            get => GetKeyAssignmentStatus();
            set => keyboard = value;
        }

        public string GetCallback() { return callback; }
        public string GetKeycombo() { return keycombo; }
        public string GetKeycomboMod() { return keycomboMod; }
        public string GetKeyDescription() { return description; }

        /// <summary>
        /// Save given key file code line in "BMS - FULL.key" and split them to parts.
        /// </summary>
        public KeyAssgn(string stBuffer)
        {
            string[] stArrayData = stBuffer.Split(' ');

            callback = stArrayData[0];
            soundID = stArrayData[1];
            none = stArrayData[2];
            keyboard = stArrayData[3];
            modifier = stArrayData[4];
            keycombo = stArrayData[5];
            keycomboMod = stArrayData[6];
            visibility = stArrayData[7];
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

            if (stArrayData.Length >= 9)
                description = stArrayData[8];
            if (stArrayData.Length > 9)
                for (int i = 9; i < stArrayData.Length; i++)
                    description += " " + stArrayData[i];

            if (callback == "SimHotasPinkyShift" || callback == "SimHotasShift")
                visibility = "White";
            if (description == "\"======== 2.19     THROTTLE QUADRANT SYSTEM ==\"")
                description = "\"======== 2.19     THROTTLE QUADRANT SYSTEM ========\"";
        }

        public KeyAssgn() { }

        public void getOtherKeyInstance(KeyAssgn otherInstance)
        {
            callback    = otherInstance.callback;
            soundID     = otherInstance.soundID;
            none        = otherInstance.none;
            keyboard    = otherInstance.keyboard;
            modifier    = otherInstance.modifier;
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
        }

        /// <summary>
        /// Return TRUE is the line has been broken.
        /// </summary>
        /// <returns></returns>
        public bool CheckFileCollapsing()
        {
            bool shutdown = false;
            return shutdown;
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

                string scancodestr = keycombo.Remove(0, 2);
                int scancode10 = Convert.ToInt32(scancodestr, 16);

                // int -> enum
                Key int2enum = (Key)scancode10;

                assignmentStatus += int2enum + "\t: ";
            }

            if (keyboard.Remove(0, 2) != "FFFFFFFF")
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

                string scancodestr = keyboard.Remove(0, 2);
                int scancode10 = Convert.ToInt32(scancodestr, 16);

                // int -> enum
                Key int2enum = (Key)scancode10;

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

        public string ReadJoyAssignment(int joynum)
        {
            string ans = "";
            if (MainWindow.deviceControl.joyAssign.Length <= joynum)
                return "";
            ans = MainWindow.deviceControl.joyAssign[joynum].KeyMappingPreviewDX(this);
            // PRIMARY DEVICE POV
            InGameAxAssgn rollAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Roll.ToString()];
            InGameAxAssgn throttleAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Throttle.ToString()];
            if (rollAxis.GetDeviceNumber() == joynum || throttleAxis.GetDeviceNumber() == joynum)
            {
                string tmp = MainWindow.deviceControl.joyAssign[joynum].KeyMappingPreviewPOV(this);
                if (ans != "" & tmp != "")
                    ans += "\n";
                ans += tmp;
            }
            return ans;
        }

        public string ReadJoyAssignment(int joynum, JoyAssgn[] joyAssign)
        {
            string ans = "";
            if (joyAssign.Length <= joynum)
                return "";
            ans = joyAssign[joynum].KeyMappingPreviewDX(this);
            if(ans != "")
                ans = "JOY " + joynum + " " + joyAssign[joynum].KeyMappingPreviewDX(this).Replace("\n", ", ");
            // PRIMARY DEVICE POV
            InGameAxAssgn rollAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Roll.ToString()];
            InGameAxAssgn throttleAxis = (InGameAxAssgn)MainWindow.inGameAxis[AxisName.Throttle.ToString()];
            if (rollAxis.GetDeviceNumber() == joynum || throttleAxis.GetDeviceNumber() == joynum) 
            {
                string tmp = "";
                tmp = joyAssign[joynum].KeyMappingPreviewPOV(this);
                if (tmp != "")
                    tmp = "JOY " + joynum + " " + joyAssign[joynum].KeyMappingPreviewPOV(this).Replace("\n", ", ");
                if (ans != "" & tmp != "")
                    ans += "; ";
                ans += tmp;
            }
            if (ans != "")
                ans += "; ";
            return ans;
        }

        object ICloneable.Clone() => Clone();

        public KeyAssgn Clone()
        {
            return (KeyAssgn)MemberwiseClone();
        }
    }
}
