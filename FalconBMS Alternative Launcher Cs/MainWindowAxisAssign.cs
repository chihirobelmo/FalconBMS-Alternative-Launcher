using MahApps.Metro.Controls;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FalconBMS_Alternative_Launcher_Cs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static string[] axisNum2Name = {
            "Axis X",
            "Axis Y",
            "Axis Z",
            "Rotation X",
            "Rotation Y",
            "Rotation Z",
            "Slider 0",
            "Slider 1"
        };
        /// <summary>
        /// Have same names and relations to Assign Buttons.
        /// </summary>
        public static string[] axisNameList = {
            "Roll",
            "Pitch",
            "Yaw",
            "Throttle",
            "Throttle_Right",
            "Toe_Brake",
            "Toe_Brake_Right",
            "Trim_Roll",
            "Trim_Pitch",
            "Trim_Yaw",
            "Radar_Antenna_Elevation",
            "Cursor_X",
            "Cursor_Y",
            "Range_Knob",
            "HUD_Brightness",
            "Reticle_Depression",
            "HMS_Brightness",
            "FLIR_Brightness",
            "Intercom",
            "COMM_Channel_1",
            "COMM_Channel_2",
            "MSL_Volume",
            "Threat_Volume",
            "AI_vs_IVC",
            "FOV",
            "Camera_Distance"
        };

        public static JoyAssgn[] joyAssign;

        public static JoyAssgn.AxAssgn mouseWheelAssign = new JoyAssgn.AxAssgn();
        private int wheelValue;

        /// <summary>
        /// Has each axisNameList as Index. Shows In-Game axis assignment state.
        /// </summary>
        public static Hashtable inGameAxis = new Hashtable();
        public class InGameAxAssgn
        {
            protected int devNum = -1;      // DeviceNumber(-2=MouseWheel)
            protected int phyAxNum = -1;    // PhysicalAxisNumber
                                            // 0=X 1=Y 2=Z 3=Rx 4=Ry 5=Rz 6=Slider0 7=Slider1
            protected bool invert = false;
            protected AxCurve saturation = AxCurve.None;
            protected AxCurve deadzone = AxCurve.None;
            protected System.DateTime assgnDate = DateTime.Parse("12/12/1998 12:00:00");

            public InGameAxAssgn() { }

            public InGameAxAssgn(int devNum, int phyAxNum, JoyAssgn.AxAssgn axis)
            {
                this.devNum = devNum;
                this.phyAxNum = phyAxNum;
                this.invert = axis.GetInvert();
                this.saturation = axis.GetSaturation();
                this.deadzone = axis.GetDeadZone();
                this.assgnDate = axis.GetAssignDate();
            }

            public InGameAxAssgn(int devNum, int phyAxNum, bool invert, AxCurve deadzone, AxCurve saturation)
            {
                this.devNum = devNum;
                this.phyAxNum = phyAxNum;
                this.invert = invert;
                this.deadzone = deadzone;
                this.saturation = saturation;
            }

            public int GetDeviceNumber() { return this.devNum; }
            public int GetPhysicalNumber() { return this.phyAxNum; }
            public bool GetInvert() { return this.invert; }
            public AxCurve GetDeadzone() { return this.deadzone; }
            public AxCurve GetSaturation() { return this.saturation; }
            public DateTime getDate() { return this.assgnDate; }
        }

        public static ThrottlePosition throttlePos = new ThrottlePosition();
        public class ThrottlePosition
        {
            // Member
            protected int aB = MAXIN;
            protected int iDLE = 0;

            // Property for XML
            public int AB { get { return this.aB; } set { this.aB = value; } }
            public int IDLE { get { return this.iDLE; } set { this.iDLE = value; } }

            // Constructor
            public ThrottlePosition(int aB, int iDLE) { this.aB = aB; this.iDLE = iDLE; }
            public ThrottlePosition() { }

            // Method
            public int GetAB() { return this.aB; }
            public int GetIDLE() { return this.iDLE; }
        }




        private int invertNum = 0;
        private Label tblabel;
        private Label tblabelab;
        private ProgressBar tbprogressbar;
        
        const int MAXIN = 65536;

        public void AxisMovingTimer_Tick(object sender, EventArgs e)
        {
            if (inGameAxis.Count == 0)
                return;
            invertNum = 0;
            foreach (String nme in axisNameList)
            {
                if (((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber() == -1)
                    continue;
                tblabel = this.FindName("Label_" + nme) as Label;
                tbprogressbar = this.FindName("Axis_" + nme) as ProgressBar;

                switch (nme)
                {
                    case "Throttle":
                    case "Throttle_Right":
                    case "Toe_Brake":
                    case "Toe_Brake_Right":
                    case "Intercom":
                    case "COMM_Channel_1":
                    case "COMM_Channel_2":
                    case "MSL_Volume":
                    case "Threat_Volume":
                    case "AI_vs_IVC":
                        if (!((InGameAxAssgn)inGameAxis[nme]).GetInvert())
                            invertNum = -1;
                        else
                            invertNum = 1;
                        break;
                    default:
                        if (!((InGameAxAssgn)inGameAxis[nme]).GetInvert())
                            invertNum = 1;
                        else
                            invertNum = -1;
                        break;
                }
                if (invertNum == 1)
                {
                    tbprogressbar.Minimum = 0;
                    tbprogressbar.Maximum = MAXIN;
                }
                else
                {
                    tbprogressbar.Minimum = -MAXIN;
                    tbprogressbar.Maximum = 0;
                }

                if (((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber() == -2)
                {
                    tbprogressbar.Value = (MAXIN / 2 + (wheelValue * 1024 / 120)) * invertNum;
                    tblabel.Content = "Wheel";
                    continue;
                }

                int output = ApplyDeadZone
                    (
                        JoyAxisState(((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber(), ((InGameAxAssgn)inGameAxis[nme]).GetPhysicalNumber()),
                        ((InGameAxAssgn)inGameAxis[nme]).GetDeadzone(),
                        ((InGameAxAssgn)inGameAxis[nme]).GetSaturation()
                    );
                tbprogressbar.Value = output * invertNum;
                /* tblabel.Content = 
                    nme.Replace("_", " ") + " : " + 
                    axisNum2Name[((InGameAxAssgn)inGameAxis[nme]).GetPhysicalNumber()] + " | " + 
                    joyStick[((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber()].DeviceInformation.InstanceName; */

                string joyActualName = joyStick[((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber()].DeviceInformation.InstanceName;
                string joyName = "JOY  " + ((InGameAxAssgn)inGameAxis[nme]).GetDeviceNumber();

                if (joyActualName.Contains("Thrustmaster HOTAS Cougar"))
                    joyName = "HOTAS";
                if (joyActualName.Contains("Thrustmaster Combined - HOTAS Warthog"))
                    joyName = "TMWHC";
                if (joyActualName.Contains("Joystick - HOTAS Warthog"))
                    joyName = "TMWHJ";
                if (joyActualName.Contains("Throttle - HOTAS Warthog"))
                    joyName = "TMWHT";
                if (joyActualName.ToLower().Contains("t.16000m"))
                    joyName = "T16KM";
                if (joyActualName.ToLower().Contains("t.flight hotas x"))
                    joyName = "TMTHX";
                if (joyActualName.ToLower().Contains("t - rudder"))
                    joyName = "TTFRP";
                if (joyActualName.ToLower().Contains("fssb"))
                    joyName = "FSSBR";
                if (joyActualName.ToLower().Contains("tusba"))
                    joyName = "TUSBA";
                if (joyActualName.ToLower().Contains("rusba"))
                    joyName = "RUSBA";
                if (joyActualName.ToLower().Contains("fusba"))
                    joyName = "FUSBA";
                if (joyActualName.ToLower().Contains("crosswind"))
                    joyName = "MFGCW";
                if (joyActualName.ToLower().Contains("x36"))
                    joyName = "STX36";
                if (joyActualName.ToLower().Contains("x45"))
                    joyName = "STX45";
                if (joyActualName.ToLower().Contains("x52"))
                    joyName = "STX52";
                if (joyActualName.ToLower().Contains("x52 pro"))
                    joyName = "SX52P";
                if (joyActualName.ToLower().Contains("x55"))
                    joyName = "STX55";
                if (joyActualName.ToLower().Contains("x56"))
                    joyName = "STX56";
                if (joyActualName.ToLower().Contains("x65"))
                    joyName = "SX65F";
                if (joyActualName.ToLower().Contains("ch fighter"))
                    joyName = "CHPFS";
                if (joyActualName.ToLower().Contains("ch combat"))
                    joyName = "CHPCS";
                if (joyActualName.ToLower().Contains("ch ") && joyActualName.ToLower().Contains("throttle"))
                    joyName = "CHPPT";
                if (joyActualName.ToLower().Contains("ch ") && joyActualName.ToLower().Contains("pedals"))
                    joyName = "CHPPP";

                tblabel.Content = joyName + " : " + axisNum2Name[((InGameAxAssgn)inGameAxis[nme]).GetPhysicalNumber()];
                tblabel.Content = ((string)tblabel.Content).Replace("Axis ", "  ");
                tblabel.Content = ((string)tblabel.Content).Replace("Rotation ", "R");
                tblabel.Content = ((string)tblabel.Content).Replace("Slider 0", "S1");
                tblabel.Content = ((string)tblabel.Content).Replace("Slider 1", "S2");

                if (nme != "Throttle" & nme != "Throttle_Right")
                    continue;

                tblabelab = this.FindName("AB_" + nme) as Label;
                tblabelab.Visibility = Visibility.Hidden;

                tbprogressbar.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0x38, 0x78, 0xA8));
                if (MAXIN + tbprogressbar.Value < throttlePos.GetIDLE())
                {
                    tbprogressbar.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 240, 0, 0));
                    tblabelab.Visibility = Visibility.Visible;
                    tblabelab.Content = "IDLE CUTOFF";
                }
                if (MAXIN + tbprogressbar.Value > throttlePos.GetAB())
                {
                    tbprogressbar.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0, 240, 0));
                    tblabelab.Visibility = Visibility.Visible;
                    tblabelab.Content = "AB";
                }
            }
        }





        public static int ApplyDeadZone(int input, AxCurve deadzone, AxCurve saturation)
        {
            double x = (double)input, y = 0;
            double x1 = 0, x2 = 0, x3 = 0, x4 = 0, a1 = 0, a2 = 0, b1 = 0, b2 = 0;

            switch (deadzone)
            {
                case AxCurve.None:
                    x2 = MAXIN / 2;
                    x3 = MAXIN / 2;
                    break;
                case AxCurve.Small:
                    x2 = (MAXIN / 2) - ((MAXIN / 2) * 0.01);
                    x3 = (MAXIN / 2) + ((MAXIN / 2) * 0.01);
                    break;
                case AxCurve.Medium:
                    x2 = (MAXIN / 2) - ((MAXIN / 2) * 0.05);
                    x3 = (MAXIN / 2) + ((MAXIN / 2) * 0.05);
                    break;
                case AxCurve.Large:
                    x2 = (MAXIN / 2) - ((MAXIN / 2) * 0.1);
                    x3 = (MAXIN / 2) + ((MAXIN / 2) * 0.1);
                    break;
            }
            switch (saturation)
            {
                case AxCurve.None:
                    x1 = 0;
                    x4 = MAXIN;
                    break;
                case AxCurve.Small:
                    x1 = 0 + ((MAXIN / 2) * 0.01);
                    x4 = MAXIN - ((MAXIN / 2) * 0.01);
                    break;
                case AxCurve.Medium:
                    x1 = 0 + ((MAXIN / 2) * 0.05);
                    x4 = MAXIN - ((MAXIN / 2) * 0.05);
                    break;
                case AxCurve.Large:
                    x1 = 0 + ((MAXIN / 2) * 0.1);
                    x4 = MAXIN - ((MAXIN / 2) * 0.1);
                    break;
            }

            a1 = (MAXIN / 2) / (x2 - x1);
            b1 = -a1 * x1;
            a2 = (MAXIN / 2) / (x4 - x3);
            b2 = (MAXIN / 2) - a2 * x3;

            if (input < (MAXIN / 2))
            {
                y = a1 * x + b1;
                if (y < 0)
                    y = 0;
                if (y > (MAXIN / 2))
                    y = (MAXIN / 2);
            }
            if (input >= (MAXIN / 2))
            {
                y = a2 * x + b2;
                if (y < (MAXIN / 2))
                    y = (MAXIN / 2);
                if (y > MAXIN)
                    y = MAXIN;
            }

            int output = (int)y;
            return output;
        }





        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            AxisMovingTimer.Stop();

            string whocalledwindow = ((System.Windows.Controls.Button)sender).Name;

            InGameAxAssgn axisAssign = new InGameAxAssgn();

            axisAssign = AxisAssignWindow.ShowMiniWindow((InGameAxAssgn)inGameAxis[whocalledwindow], sender);

            // Reset PhysicalAxis previously assigned to same axis
            // In case of axis has been unassigned and saved.
            for (int i = 0; i < devList.Count; i++)
                joyAssign[i].ResetPreviousAxis(whocalledwindow);
            if (mouseWheelAssign.GetAxisName() == whocalledwindow)
                mouseWheelAssign = new JoyAssgn.AxAssgn();

            // When axis has been assigned.
            if (axisAssign.GetDeviceNumber() > -1)
                joyAssign[axisAssign.GetDeviceNumber()].axis[axisAssign.GetPhysicalNumber()]
                    = new JoyAssgn.AxAssgn(whocalledwindow, axisAssign);
            if (axisAssign.GetDeviceNumber() == -2)
            {
                wheelValue = 0;
                mouseWheelAssign = new JoyAssgn.AxAssgn(whocalledwindow, axisAssign);
            }

            joyAssign_2_inGameAxis();
            ResetAssgnWindow();

            AxisMovingTimer.Start();
        }





        public void joyAssign_2_inGameAxis()
        {
            foreach (String nme in axisNameList)
                inGameAxis[nme] = new InGameAxAssgn();
            for (int i = 0; i <= joyAssign.Length - 1; i++)
            {
                for (int ii = 0; ii <= 7; ii++)
                {
                    if (object.ReferenceEquals(joyAssign[i].axis[ii].GetAxisName(), ""))
                        continue;
                    if (((InGameAxAssgn)inGameAxis[joyAssign[i].axis[ii].GetAxisName()]).getDate() > joyAssign[i].axis[ii].GetAssignDate())
                        continue;
                    inGameAxis[joyAssign[i].axis[ii].GetAxisName()] = new InGameAxAssgn(i, ii, joyAssign[i].axis[ii]);
                }
            }
            if (object.ReferenceEquals(mouseWheelAssign.GetAxisName(), ""))
                return;
            if (((InGameAxAssgn)inGameAxis[mouseWheelAssign.GetAxisName()]).getDate() > mouseWheelAssign.GetAssignDate())
                return;
            inGameAxis[mouseWheelAssign.GetAxisName()] = new InGameAxAssgn(-2, -1, mouseWheelAssign);
        }





        public void ResetAssgnWindow()
        {
            foreach (String nme in axisNameList)
            {
                Label tblabel = this.FindName("Label_" + nme) as Label;
                ProgressBar tbprogressbar = this.FindName("Axis_" + nme) as ProgressBar;

                tblabel.Content = nme.Replace("_", " ") + " :";
                tblabel.Content = "";

                tbprogressbar.Value = 0;
                tbprogressbar.Minimum = 0;
                tbprogressbar.Maximum = MAXIN;
            }
        }





        public static void AquireAll(bool FLG)
        {
            if (FLG)
            {
                for (int i = 0; i < MainWindow.devList.Count; i++)
                {
                    MainWindow.joyStick[i].Acquire();
                }
                return;
            }
            for (int i = 0; i < MainWindow.devList.Count; i++)
            {
                MainWindow.joyStick[i].Unacquire();
            }
        }
        




        private void Detect_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (mouseWheelAssign.GetAxisName() != "")
            {
                wheelValue += e.Delta;
                // (32768 * 120 / 1240 ) = 3840 
                if (wheelValue < -3840)
                    wheelValue = -3840;
                if (wheelValue > 3840)
                    wheelValue = 3840;
            }
        }





    }
}