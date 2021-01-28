using System;

using Microsoft.DirectX.DirectInput;

namespace FalconBMS.Launcher.Input
{
    public class KeyAssign
    {
        protected string callback = "SimDoNothing";            // 1st: callback(ex: "SimDoNothing")
        protected string soundId = "-1";                       // 2nd: -1
        protected string none = "0";                           // 3rd: 0 
        protected string keyboard = "0xFFFFFFFF";              // 4th: Scan code(ex: 0x1E => 30 => A).
        protected string modifier = "0";                       // 5th: Modification keys(ex: Shift,Ctrl,Alt).
        protected string keycombo = "0";                       // 6th: (Same as keyboard)
        protected string keycomboMod = "0";                    // 7th: (Same as modifier but for key combo)
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
        public KeyAssign(string stBuffer)
        {
            string[] stArrayData = stBuffer.Split(' ');

            callback = stArrayData[0];
            soundId = stArrayData[1];
            none = stArrayData[2];
            keyboard = stArrayData[3];
            modifier = stArrayData[4];
            keycombo = stArrayData[5];
            keycomboMod = stArrayData[6];
            visibility = stArrayData[7];
            switch (visibility)
            {
                case "-2":
                    visibility = "Hidden";
                    break;
                case "-1":
                    visibility = "Blue";
                    break;
                case "-0":
                    visibility = "Green";
                    break;
                case "1":
                    visibility = "White";
                    break;
                default:
                    visibility = "Green";
                    description += "!Alt Launcher ERROR!";
                    break;
            }

            description = "";

            if (stArrayData.Length >= 9)
                description = stArrayData[8];
            if (stArrayData.Length > 9)
                for (int i = 9; i < stArrayData.Length; i++)
                    description += " " + stArrayData[i];

            if (callback == "SimHotasPinkyShift")
                visibility = "White";
            if (description == "\"======== 2.19     THROTTLE QUADRANT SYSTEM ==\"")
                description = "\"======== 2.19     THROTTLE QUADRANT SYSTEM ========\"";
        }

        public KeyAssign() { }

        public void GetOtherKeyInstance(KeyAssign otherInstance)
        {
            callback    = otherInstance.callback;
            soundId     = otherInstance.soundId;
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
            line += " " + soundId;
            line += " " + none;
            line += " " + keyboard;
            line += " " + modifier;
            line += " " + keycombo;
            line += " " + keycomboMod;
            switch (visibility)
            {
                case "Hidden":
                    line += " -2";
                    break;
                case "Blue":
                    line += " -1";
                    break;
                case "Green":
                    line += " -0";
                    break;
                case "White":
                    line += " 1";
                    break;
                case "-2":
                    line += " -2";
                    break;
                case "-1":
                    line += " -1";
                    break;
                case "-0":
                    line += " -0";
                    break;
                case "1":
                    line += " 1";
                    break;
            }

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
                Key int2Enum = (Key)scancode10;

                assignmentStatus += int2Enum + "\t: ";
            }

            if (keyboard.Remove(0, 2) == "FFFFFFFF") return assignmentStatus;
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
                Key int2Enum = (Key)scancode10;

                assignmentStatus += int2Enum.ToString();
            }

            return assignmentStatus;
        }

        // Z_Joy_<asssigned joystick number> = "DX1 DX16 POV1UP" //
        public string ZJoy0 => ReadJoyAssignment(0);
        public string ZJoy1 => ReadJoyAssignment(1);
        public string ZJoy2 => ReadJoyAssignment(2);
        public string ZJoy3 => ReadJoyAssignment(3);
        public string ZJoy4 => ReadJoyAssignment(4);
        public string ZJoy5 => ReadJoyAssignment(5);
        public string ZJoy6 => ReadJoyAssignment(6);
        public string ZJoy7 => ReadJoyAssignment(7);
        public string ZJoy8 => ReadJoyAssignment(8);
        public string ZJoy9 => ReadJoyAssignment(9);
        public string ZJoy10 => ReadJoyAssignment(10);
        public string ZJoy11 => ReadJoyAssignment(11);
        public string ZJoy12 => ReadJoyAssignment(12);
        public string ZJoy13 => ReadJoyAssignment(13);
        public string ZJoy14 => ReadJoyAssignment(14);
        public string ZJoy15 => ReadJoyAssignment(15);

        public string ReadJoyAssignment(int joynum)
        {
            string ans;
            if (Windows.MainWindow.deviceControl.joyAssign.Length <= joynum)
                return "";
            ans = Windows.MainWindow.deviceControl.joyAssign[joynum].KeyMappingPreviewDx(this);
            // PRIMARY DEVICE POV
            if (((InGameAxAssign)MainWindow.inGameAxis["Roll"]).GetDeviceNumber() == joynum || ((InGameAxAssign)MainWindow.inGameAxis["Throttle"]).GetDeviceNumber() == joynum)
            {
                string tmp = Windows.MainWindow.deviceControl.joyAssign[joynum].KeyMappingPreviewPov(this);
                if (ans != "" & tmp != "")
                    ans += "\n";
                ans += tmp;
            }
            return ans;
        }

        public string ReadJoyAssignment(int joynum, JoyAssign[] joyAssign)
        {
            string ans;
            if (joyAssign.Length <= joynum)
                return "";
            ans = joyAssign[joynum].KeyMappingPreviewDx(this);
            if(ans != "")
                ans = "JOY " + joynum + " " + joyAssign[joynum].KeyMappingPreviewDx(this).Replace("\n", ", ");
            // PRIMARY DEVICE POV
            if (((InGameAxAssign)MainWindow.inGameAxis["Roll"]).GetDeviceNumber() == joynum || ((InGameAxAssign)MainWindow.inGameAxis["Throttle"]).GetDeviceNumber() == joynum) 
            {
                string tmp;
                tmp = joyAssign[joynum].KeyMappingPreviewPov(this);
                if (tmp != "")
                    tmp = "JOY " + joynum + " " + joyAssign[joynum].KeyMappingPreviewPov(this).Replace("\n", ", ");
                if (ans != "" & tmp != "")
                    ans += "; ";
                ans += tmp;
            }
            if (ans != "")
                ans += "; ";
            return ans;
        }

        public KeyAssign Clone()
        {
            return (KeyAssign)MemberwiseClone();
        }
    }
}
