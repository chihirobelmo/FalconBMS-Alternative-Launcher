using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class KeyAssgn
    {
        protected string callback;          // 1st: callback(ex: "SimDoNothing")
        protected string soundID;           // 2nd: -1
        protected string none;              // 3rd: 0 
        protected string keyboard;          // 4th: Scancode(ex: 0x1E => 30 => A).
        protected string modifier;          // 5th: Modification keys(ex: Shift,Ctrl,Alt).
        protected string keycombo;          // 6th: (Same as keyboard)
        protected string keycomboMod;       // 7th: (Same as modifier but for keycombo)
        protected string visibility;        // 8th: 1=Visible -1=Headline -0=Locked -2=hidden
        protected string description;       // 9th: The description

        // for Datagrid Display //
        public string Visibility { get; set; }
        public string Mapping { get { return " " + this.description.Replace("\"",""); } }
        public string Key {
            get { return this.GetKeyAssignmentStatus(); }
            set { this.keyboard = value; }
        }

        public string GetCallback() { return this.callback; }
        public string GetKeycombo() { return this.keycombo; }
        public string GetKeycomboMod() { return this.keycomboMod; }

        /// <summary>
        /// Get Visibility information (1=Visible -1=Headline -0=Locked -2=hidden)
        /// </summary>
        public string GetVisibility() { return this.visibility; }

        /// <summary>
        /// Returns each line to write to BMS - FULL.key file.
        /// </summary>
        /// <returns></returns>
        public string GetKeyLine()
        {
            string line = "";
            if (this.visibility == "Blue")
                line += "#=======================================" +
                    "============================================\n";
            line += this.callback;
            line += " " + this.soundID;
            line += " " + this.none;
            line += " " + this.keyboard;
            line += " " + this.modifier;
            line += " " + this.keycombo;
            line += " " + this.keycomboMod;
            if (this.visibility == "Hidden")
                line += " -2";
            if (this.visibility == "Blue")
                line += " -1";
            if (this.visibility == "Green")
                line += " -0";
            if (this.visibility == "White")
                line += " 1";
            line += " " + this.description;
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
            this.modifier = code.ToString();

            this.keyboard = "0x" + scancode10.ToString("X");
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
            this.keycomboMod = code.ToString();

            this.keycombo = "0x" + scancode10.ToString("X");
        }

        /// <summary>
        /// Unassign the line.
        /// </summary>
        public void UnassignKeyboard()
        {
            this.keyboard = "0xFFFFFFFF";
            this.modifier = "0";
            this.keycombo = "0";
            this.keycomboMod = "0";
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
        /// Save given key file code line in "BMS - FULL.key" and split them to parts.
        /// </summary>
        public KeyAssgn(string stBuffer)
        {
            string[] stArrayData = stBuffer.Split(' ');

            this.callback = (string)stArrayData[0];
            this.soundID = (string)stArrayData[1];
            this.none = (string)stArrayData[2];
            this.keyboard = (string)stArrayData[3];
            this.modifier = (string)stArrayData[4];
            this.keycombo = (string)stArrayData[5];
            this.keycomboMod = (string)stArrayData[6];
            this.visibility = (string)stArrayData[7];
            if (this.visibility == "-2")
                this.visibility = "Hidden";
            else if (this.visibility == "-1")
                this.visibility = "Blue";
            else if (this.visibility == "-0")
                this.visibility = "Green";
            else if (this.visibility == "1")
                this.visibility = "White";
            else
                this.visibility = "White";
            this.description = "";

            if (stArrayData.Length >= 9)
                this.description = (string)stArrayData[8];
            if (stArrayData.Length > 9)
                for (int i = 9; i < stArrayData.Length; i++)
                    this.description += " " + (string)stArrayData[i];
        }
        
        /// <summary>
        /// Make new instance.
        /// </summary>
        public KeyAssgn()
        {
        }

        /// <summary>
        /// Return overall key assignment status (ex: Alt c : Shift g)
        /// </summary>
        public string GetKeyAssignmentStatus()
        {
            string assignmentStatus = "";

            // keycombo //
            if (this.keycombo != "0")
            {
                // keycomboMod //
                switch (this.keycomboMod)
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

                string scancodestr = this.keycombo.Remove(0, 2);
                int scancode10 = Convert.ToInt32(scancodestr, 16);

                // int -> enum
                var int2enum = (Microsoft.DirectX.DirectInput.Key)scancode10;

                assignmentStatus += int2enum.ToString() + "\t: ";
            }

            if (this.keyboard.Remove(0, 2) != "FFFFFFFF")
            {
                // modifier //
                switch (this.modifier)
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

                string scancodestr = this.keyboard.Remove(0, 2);
                int scancode10 = Convert.ToInt32(scancodestr, 16);

                // int -> enum
                var int2enum = (Microsoft.DirectX.DirectInput.Key)scancode10;

                assignmentStatus += int2enum.ToString();
            }

            return assignmentStatus;
        }

        // Z_Joy_<asssigned joystick number> = "DX1 DX16 POV1UP" //
        public string Z_Joy_0 { get { return ReadJoyAssignment(0); } }
        public string Z_Joy_1 { get { return ReadJoyAssignment(1); } }
        public string Z_Joy_2 { get { return ReadJoyAssignment(2); } }
        public string Z_Joy_3 { get { return ReadJoyAssignment(3); } }
        public string Z_Joy_4 { get { return ReadJoyAssignment(4); } }
        public string Z_Joy_5 { get { return ReadJoyAssignment(5); } }
        public string Z_Joy_6 { get { return ReadJoyAssignment(6); } }
        public string Z_Joy_7 { get { return ReadJoyAssignment(7); } }
        public string Z_Joy_8 { get { return ReadJoyAssignment(8); } }
        public string Z_Joy_9 { get { return ReadJoyAssignment(9); } }
        public string Z_Joy_10 { get { return ReadJoyAssignment(10); } }
        public string Z_Joy_11 { get { return ReadJoyAssignment(11); } }
        public string Z_Joy_12 { get { return ReadJoyAssignment(12); } }
        public string Z_Joy_13 { get { return ReadJoyAssignment(13); } }
        public string Z_Joy_14 { get { return ReadJoyAssignment(14); } }
        public string Z_Joy_15 { get { return ReadJoyAssignment(15); } }

        public string ReadJoyAssignment(int joynum)
        {
            string ans = "";
            if (MainWindow.deviceControl.devList.Count <= joynum)
                return "";
            ans = MainWindow.deviceControl.joyAssign[joynum].KeyMappingPreviewDX(this);
            // PRIMARY DEVICE POV
            if (((InGameAxAssgn)MainWindow.inGameAxis["Roll"]).GetDeviceNumber() == joynum)
            {
                string tmp = MainWindow.deviceControl.joyAssign[joynum].KeyMappingPreviewPOV(this);
                if (ans != "" & tmp != "")
                    ans += "\n";
                ans += tmp;
            }
            return ans;
        }
    }
}
