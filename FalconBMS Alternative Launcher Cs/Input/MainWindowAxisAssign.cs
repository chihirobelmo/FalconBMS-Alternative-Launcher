﻿using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using FalconBMS.Launcher.Core;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Input
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Have same names and relations to Assign Buttons.
        /// </summary>
        public static AxisName[] axisNameList = {
            AxisName.Roll,
            AxisName.Pitch,
            AxisName.Yaw,
            AxisName.Throttle,
            AxisName.Throttle_Right,
            AxisName.Toe_Brake,
            AxisName.Toe_Brake_Right,
            AxisName.Trim_Roll,
            AxisName.Trim_Pitch,
            AxisName.Trim_Yaw,
            AxisName.Radar_Antenna_Elevation,
            AxisName.Cursor_X,
            AxisName.Cursor_Y,
            AxisName.Range_Knob,
            AxisName.HUD_Brightness,
            AxisName.Reticle_Depression,
            AxisName.HMS_Brightness,
            AxisName.FLIR_Brightness,
            AxisName.Intercom,
            AxisName.COMM_Channel_1,
            AxisName.COMM_Channel_2,
            AxisName.MSL_Volume,
            AxisName.Threat_Volume,
            AxisName.AI_vs_IVC,
            AxisName.FOV,
            AxisName.Camera_Distance,
            AxisName.HSI_Course_Knob,
            AxisName.HSI_Heading_Knob,
            AxisName.Altimeter_Knob,
            AxisName.ILS_Volume_Knob
        };
        
        /// <summary>
        /// Has each axisNameList as Index. Shows In-Game axis assignment state.
        /// </summary>
        public static Hashtable inGameAxis = new Hashtable();

        /// <summary>
        /// Mouse Wheel Input Value
        /// </summary>
        private int wheelValue;

        /// <summary>
        /// invert TRUE or FALSE (TODO: This has to be Boolean)
        /// </summary>
        private int invertNum;

        /// <summary>
        /// I forgot what was these...
        /// </summary>
        private Label tblabel;
        private Label tblabelab;
        private ProgressBar tbprogressbar;

        /// <summary>
        /// MAX INPUT = 16bit
        /// </summary>
        public const int MAXIN = 65536;

        /// <summary>
        /// Checks and shows each device each axis inputs 60 times per seconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void AxisMovingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (inGameAxis.Count == 0)
                    return;
                invertNum = 0;
                foreach (AxisName nme in axisNameList)
                {
                    if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() == -1)
                        continue;
                    tblabel = FindName("Label_" + nme) as Label;
                    tbprogressbar = FindName("Axis_" + nme) as ProgressBar;

                    switch (nme)
                    {
                        case AxisName.Throttle:
                        case AxisName.Throttle_Right:
                        case AxisName.Toe_Brake:
                        case AxisName.Toe_Brake_Right:
                        case AxisName.Intercom:
                        case AxisName.COMM_Channel_1:
                        case AxisName.COMM_Channel_2:
                        case AxisName.MSL_Volume:
                        case AxisName.Threat_Volume:
                        case AxisName.AI_vs_IVC:
                        case AxisName.ILS_Volume_Knob:
                            if (!((InGameAxAssgn)inGameAxis[nme.ToString()]).GetInvert())
                                invertNum = -1;
                            else
                                invertNum = 1;
                            break;
                        default:
                            if (!((InGameAxAssgn)inGameAxis[nme.ToString()]).GetInvert())
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

                    if (((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber() == -2)
                    {
                        tbprogressbar.Value = (MAXIN / 2 + wheelValue * 1024 / 120) * invertNum;
                        tblabel.Content = "MOUSE : WH";
                        continue;
                    }

                    int output = ApplyDeadZone
                        (
                            Windows.MainWindow.deviceControl.JoyAxisState(((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber(), ((InGameAxAssgn)inGameAxis[nme.ToString()]).GetPhysicalNumber()),
                            ((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeadzone(),
                            ((InGameAxAssgn)inGameAxis[nme.ToString()]).GetSaturation()
                        );
                    tbprogressbar.Value = output * invertNum;

                    string joyActualName = Windows.MainWindow.deviceControl.joyStick[((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber()].DeviceInformation.InstanceName;
                    string joyName = "JOY  " + ((InGameAxAssgn)inGameAxis[nme.ToString()]).GetDeviceNumber();

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
                    if (joyActualName.ToLower().Contains("g940"))
                        joyName = "LG940";
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

                    int axisNumber = ((InGameAxAssgn)inGameAxis[nme.ToString()]).GetPhysicalNumber();
                    tblabel.Content = joyName + " : " + ((AxisNumName)axisNumber).ToString().Replace('_', ' ');
                    tblabel.Content = ((string)tblabel.Content).Replace("Axis ", "  ");
                    tblabel.Content = ((string)tblabel.Content).Replace("Rotation ", "R");
                    tblabel.Content = ((string)tblabel.Content).Replace("Slider 0", "S1");
                    tblabel.Content = ((string)tblabel.Content).Replace("Slider 1", "S2");

                    if (nme != AxisName.Throttle & nme != AxisName.Throttle_Right)
                        continue;

                    tblabelab = FindName("AB_" + nme) as Label;
                    tblabelab.Visibility = Visibility.Hidden;

                    tbprogressbar.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0x38, 0x78, 0xA8));
                    if (MAXIN + tbprogressbar.Value < Windows.MainWindow.deviceControl.throttlePos.GetIDLE())
                    {
                        tbprogressbar.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 240, 0, 0));
                        tblabelab.Visibility = Visibility.Visible;
                        tblabelab.Content = "IDLE CUTOFF";
                    }
                    if (MAXIN + tbprogressbar.Value > Windows.MainWindow.deviceControl.throttlePos.GetAB())
                    {
                        tbprogressbar.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0, 240, 0));
                        tblabelab.Visibility = Visibility.Visible;
                        tblabelab.Content = "AB";
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                Diagnostics.Log(ex);

                //StreamWriter sw = new StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, Encoding.GetEncoding("shift_jis"));
                //sw.Write(ex.Message);
                //sw.Close();

                MessageBox.Show($"Error log saved to {Diagnostics.AppDataPath}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                Close();
            }
        }
        
        /// <summary>
        /// Shows axis output with DEADZONE and SATURATION enabled in BMS
        /// </summary>
        /// <param name="input"></param>
        /// <param name="deadzone"></param>
        /// <param name="saturation"></param>
        /// <returns></returns>
        public static int ApplyDeadZone(int input, AxCurve deadzone, AxCurve saturation)
        {
            double x = input, y = 0;
            double x1 = 0, x2 = 0, x3 = 0, x4 = 0, a1 = 0, a2 = 0, b1 = 0, b2 = 0;

            switch (deadzone)
            {
                case AxCurve.None:
                    x2 = MAXIN / 2;
                    x3 = MAXIN / 2;
                    break;
                case AxCurve.Small:
                    x2 = MAXIN / 2 - MAXIN / 2 * 0.01;
                    x3 = MAXIN / 2 + MAXIN / 2 * 0.01;
                    break;
                case AxCurve.Medium:
                    x2 = MAXIN / 2 - MAXIN / 2 * 0.05;
                    x3 = MAXIN / 2 + MAXIN / 2 * 0.05;
                    break;
                case AxCurve.Large:
                    x2 = MAXIN / 2 - MAXIN / 2 * 0.1;
                    x3 = MAXIN / 2 + MAXIN / 2 * 0.1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deadzone), deadzone, null); // TODO: Add error message to pass to console/logs here.
            }
            switch (saturation)
            {
                case AxCurve.None:
                    x1 = 0;
                    x4 = MAXIN;
                    break;
                case AxCurve.Small:
                    x1 = 0 + MAXIN / 2 * 0.01;
                    x4 = MAXIN - MAXIN / 2 * 0.01;
                    break;
                case AxCurve.Medium:
                    x1 = 0 + MAXIN / 2 * 0.05;
                    x4 = MAXIN - MAXIN / 2 * 0.05;
                    break;
                case AxCurve.Large:
                    x1 = 0 + MAXIN / 2 * 0.1;
                    x4 = MAXIN - MAXIN / 2 * 0.1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(saturation), saturation, null); // TODO: Add error message to pass to console/logs here.
            }

            a1 = MAXIN / 2 / (x2 - x1);
            b1 = -a1 * x1;
            a2 = MAXIN / 2 / (x4 - x3);
            b2 = MAXIN / 2 - a2 * x3;

            if (input < MAXIN / 2)
            {
                y = a1 * x + b1;
                if (y < 0)
                    y = 0;
                if (y > MAXIN / 2)
                    y = MAXIN / 2;
            }
            if (input >= MAXIN / 2)
            {
                y = a2 * x + b2;
                if (y < MAXIN / 2)
                    y = MAXIN / 2;
                if (y > MAXIN)
                    y = MAXIN;
            }

            int output = (int)y;
            return output;
        }
        
        /// <summary>
        /// Callback When clicked "Assign" Button. Opens AxisAssignWindow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            AxisMovingTimer.Stop();

            string whocalledwindow = ((System.Windows.Controls.Button)sender).Name;

            InGameAxAssgn axisAssign = new InGameAxAssgn();

            axisAssign = AxisAssignWindow.ShowAxisAssignWindow((InGameAxAssgn)inGameAxis[whocalledwindow], sender);

            // Reset PhysicalAxis previously assigned to same axis
            // In case of axis has been unassigned and saved.
            for (int i = 0; i < Windows.MainWindow.deviceControl.devList.Count; i++)
                Windows.MainWindow.deviceControl.joyAssign[i].ResetPreviousAxis(whocalledwindow);
            if (Windows.MainWindow.deviceControl.mouseWheelAssign.GetAxisName() == whocalledwindow)
                Windows.MainWindow.deviceControl.mouseWheelAssign = new AxAssgn();

            // When axis has been assigned.
            if (axisAssign.GetDeviceNumber() > -1)
                Windows.MainWindow.deviceControl.joyAssign[axisAssign.GetDeviceNumber()].axis[axisAssign.GetPhysicalNumber()]
                    = new AxAssgn(whocalledwindow, axisAssign);
            if (axisAssign.GetDeviceNumber() == -2)
            {
                wheelValue = 0;
                Windows.MainWindow.deviceControl.mouseWheelAssign = new AxAssgn(whocalledwindow, axisAssign);
            }

            joyAssign_2_inGameAxis();
            ResetAssgnWindow();

            AxisMovingTimer.Start();
        }
        
        /// <summary>
        /// Ah what was this...
        /// </summary>
        public void joyAssign_2_inGameAxis()
        {
            foreach (AxisName nme in axisNameList)
                inGameAxis[nme.ToString()] = new InGameAxAssgn();
            for (int i = 0; i <= Windows.MainWindow.deviceControl.joyAssign.Length - 1; i++)
            {
                for (int ii = 0; ii <= 7; ii++)
                {
                    if (inGameAxis[Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAxisName()] == null)
                        continue;
                    if (ReferenceEquals(Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAxisName(), ""))
                        continue;
                    if (((InGameAxAssgn)inGameAxis[Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAxisName()]).getDate() > Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAssignDate())
                        continue;
                    inGameAxis[Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAxisName()] = new InGameAxAssgn(i, ii, Windows.MainWindow.deviceControl.joyAssign[i].axis[ii]);
                }
            }
            if (ReferenceEquals(Windows.MainWindow.deviceControl.mouseWheelAssign.GetAxisName(), ""))
                return;
            if (((InGameAxAssgn)inGameAxis[Windows.MainWindow.deviceControl.mouseWheelAssign.GetAxisName()]).getDate() > Windows.MainWindow.deviceControl.mouseWheelAssign.GetAssignDate())
                return;
            inGameAxis[Windows.MainWindow.deviceControl.mouseWheelAssign.GetAxisName()] = new InGameAxAssgn(-2, -1, Windows.MainWindow.deviceControl.mouseWheelAssign);
        }
        
        /// <summary>
        /// Reset Axis Assign Window. No Input No assign.
        /// </summary>
        public void ResetAssgnWindow()
        {
            foreach (AxisName nme in axisNameList)
            {
                Label tblabel = FindName("Label_" + nme) as Label;
                ProgressBar tbprogressbar = FindName("Axis_" + nme) as ProgressBar;

                tblabel.Content = nme.ToString().Replace("_", " ") + " :";
                tblabel.Content = "";

                tbprogressbar.Value = 0;
                tbprogressbar.Minimum = 0;
                tbprogressbar.Maximum = MAXIN;
            }
        }
        
        /// <summary>
        /// Aquire/Unaquire all devices
        /// </summary>
        /// <param name="FLG"></param>
        public static void AquireAll(bool FLG)
        {
            if (FLG)
            {
                for (int i = 0; i < Windows.MainWindow.deviceControl.devList.Count; i++)
                {
                    Windows.MainWindow.deviceControl.joyStick[i].Acquire();
                }
                return;
            }
            for (int i = 0; i < Windows.MainWindow.deviceControl.devList.Count; i++)
            {
                Windows.MainWindow.deviceControl.joyStick[i].Unacquire();
            }
        }
        
        /// <summary>
        /// Detect mouse Wheel as the name is.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Detect_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Windows.MainWindow.deviceControl.mouseWheelAssign.GetAxisName() != "")
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