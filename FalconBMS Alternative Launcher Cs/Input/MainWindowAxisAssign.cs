using System;
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
            AxisName.ThrottleRight,
            AxisName.ToeBrake,
            AxisName.ToeBrakeRight,
            AxisName.TrimRoll,
            AxisName.TrimPitch,
            AxisName.TrimYaw,
            AxisName.RadarAntennaElevation,
            AxisName.CursorX,
            AxisName.CursorY,
            AxisName.RangeKnob,
            AxisName.HudBrightness,
            AxisName.ReticleDepression,
            AxisName.HmsBrightness,
            AxisName.FlirBrightness,
            AxisName.Intercom,
            AxisName.CommChannel1,
            AxisName.CommChannel2,
            AxisName.MslVolume,
            AxisName.ThreatVolume,
            AxisName.AiVsIvc,
            AxisName.Fov,
            AxisName.CameraDistance,
            AxisName.HsiCourseKnob,
            AxisName.HsiHeadingKnob,
            AxisName.AltimeterKnob,
            AxisName.IlsVolumeKnob
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
        public const int Maxin = 65536;

        /// <summary>
        /// Checks and shows each device each axis inputs 60 times per seconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AxisMovingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (inGameAxis.Count == 0)
                    return;
                invertNum = 0;
                foreach (AxisName nme in axisNameList)
                {
                    if (((InGameAxAssign)inGameAxis[nme.ToString()]).GetDeviceNumber() == -1)
                        continue;

                    tblabel = FrameworkElement.FindName("Label_" + nme) as Label;
                    tbprogressbar = FrameworkElement.FindName("Axis_" + nme) as ProgressBar;

                    switch (nme)
                    {
                        case AxisName.Throttle:
                        case AxisName.ThrottleRight:
                        case AxisName.ToeBrake:
                        case AxisName.ToeBrakeRight:
                        case AxisName.Intercom:
                        case AxisName.CommChannel1:
                        case AxisName.CommChannel2:
                        case AxisName.MslVolume:
                        case AxisName.ThreatVolume:
                        case AxisName.AiVsIvc:
                        case AxisName.IlsVolumeKnob:
                            if (!((InGameAxAssign)inGameAxis[nme.ToString()]).GetInvert())
                                invertNum = -1;
                            else
                                invertNum = 1;
                            break;
                        default:
                            if (!((InGameAxAssign)inGameAxis[nme.ToString()]).GetInvert())
                                invertNum = 1;
                            else
                                invertNum = -1;
                            break;
                    }
                    if (invertNum == 1)
                    {
                        tbprogressbar.Minimum = 0;
                        tbprogressbar.Maximum = Maxin;
                    }
                    else
                    {
                        tbprogressbar.Minimum = -Maxin;
                        tbprogressbar.Maximum = 0;
                    }

                    if (((InGameAxAssign)inGameAxis[nme.ToString()]).GetDeviceNumber() == -2)
                    {
                        tbprogressbar.Value = (Maxin / 2 + wheelValue * 1024 / 120) * invertNum;
                        tblabel.Content = "MOUSE : WH";
                        continue;
                    }

                    int output = ApplyDeadZone
                        (
                            Windows.MainWindow.deviceControl.JoyAxisState(((InGameAxAssign)inGameAxis[nme.ToString()]).GetDeviceNumber(), ((InGameAxAssign)inGameAxis[nme.ToString()]).GetPhysicalNumber()),
                            ((InGameAxAssign)inGameAxis[nme.ToString()]).GetDeadZone(),
                            ((InGameAxAssign)inGameAxis[nme.ToString()]).GetSaturation()
                        );
                    tbprogressbar.Value = output * invertNum;

                    string joyActualName = Windows.MainWindow.deviceControl.joyStick[((InGameAxAssign)inGameAxis[nme.ToString()]).GetDeviceNumber()].DeviceInformation.InstanceName;
                    string joyName = "JOY  " + ((InGameAxAssign)inGameAxis[nme.ToString()]).GetDeviceNumber();

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

                    int axisNumber = ((InGameAxAssign)inGameAxis[nme.ToString()]).GetPhysicalNumber();
                    tblabel.Content = joyName + " : " + ((AxisNumName)axisNumber).ToString().Replace('_', ' ');
                    tblabel.Content = ((string)tblabel.Content).Replace("Axis ", "  ");
                    tblabel.Content = ((string)tblabel.Content).Replace("Rotation ", "R");
                    tblabel.Content = ((string)tblabel.Content).Replace("Slider 0", "S1");
                    tblabel.Content = ((string)tblabel.Content).Replace("Slider 1", "S2");

                    if (nme != AxisName.Throttle & nme != AxisName.ThrottleRight)
                        continue;

                    tblabelab = FrameworkElement.FindName("AB_" + nme) as Label;
                    tblabelab.Visibility = Visibility.Hidden;

                    tbprogressbar.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0x38, 0x78, 0xA8));
                    if (Maxin + tbprogressbar.Value < Windows.MainWindow.deviceControl.throttlePos.GetIdle())
                    {
                        tbprogressbar.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 240, 0, 0));
                        tblabelab.Visibility = Visibility.Visible;
                        tblabelab.Content = "IDLE CUTOFF";
                    }
                    if (Maxin + tbprogressbar.Value > Windows.MainWindow.deviceControl.throttlePos.GetAb())
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
                    x2 = Maxin / 2;
                    x3 = Maxin / 2;
                    break;
                case AxCurve.Small:
                    x2 = Maxin / 2 - Maxin / 2 * 0.01;
                    x3 = Maxin / 2 + Maxin / 2 * 0.01;
                    break;
                case AxCurve.Medium:
                    x2 = Maxin / 2 - Maxin / 2 * 0.05;
                    x3 = Maxin / 2 + Maxin / 2 * 0.05;
                    break;
                case AxCurve.Large:
                    x2 = Maxin / 2 - Maxin / 2 * 0.1;
                    x3 = Maxin / 2 + Maxin / 2 * 0.1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deadzone), deadzone, null); // TODO: Add error message to pass to console/logs here.
            }
            switch (saturation)
            {
                case AxCurve.None:
                    x1 = 0;
                    x4 = Maxin;
                    break;
                case AxCurve.Small:
                    x1 = 0 + Maxin / 2 * 0.01;
                    x4 = Maxin - Maxin / 2 * 0.01;
                    break;
                case AxCurve.Medium:
                    x1 = 0 + Maxin / 2 * 0.05;
                    x4 = Maxin - Maxin / 2 * 0.05;
                    break;
                case AxCurve.Large:
                    x1 = 0 + Maxin / 2 * 0.1;
                    x4 = Maxin - Maxin / 2 * 0.1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(saturation), saturation, null); // TODO: Add error message to pass to console/logs here.
            }

            a1 = Maxin / 2 / (x2 - x1);
            b1 = -a1 * x1;
            a2 = Maxin / 2 / (x4 - x3);
            b2 = Maxin / 2 - a2 * x3;

            if (input < Maxin / 2)
            {
                y = a1 * x + b1;
                if (y < 0)
                    y = 0;
                if (y > Maxin / 2)
                    y = Maxin / 2;
            }
            if (input >= Maxin / 2)
            {
                y = a2 * x + b2;
                if (y < Maxin / 2)
                    y = Maxin / 2;
                if (y > Maxin)
                    y = Maxin;
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

            string whocalledwindow = ((Button)sender).Name;

            InGameAxAssign axisAssign = new InGameAxAssign();

            axisAssign = AxisAssignWindow.ShowAxisAssignWindow((InGameAxAssign)inGameAxis[whocalledwindow], sender);

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
                inGameAxis[nme.ToString()] = new InGameAxAssign();
            for (int i = 0; i <= Windows.MainWindow.deviceControl.joyAssign.Length - 1; i++)
            {
                for (int ii = 0; ii <= 7; ii++)
                {
                    if (inGameAxis[Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAxisName()] == null)
                        continue;
                    if (ReferenceEquals(Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAxisName(), ""))
                        continue;
                    if (((InGameAxAssign)inGameAxis[Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAxisName()]).GetDate() > Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAssignDate())
                        continue;
                    inGameAxis[Windows.MainWindow.deviceControl.joyAssign[i].axis[ii].GetAxisName()] = new InGameAxAssign(i, ii, Windows.MainWindow.deviceControl.joyAssign[i].axis[ii]);
                }
            }
            if (ReferenceEquals(Windows.MainWindow.deviceControl.mouseWheelAssign.GetAxisName(), ""))
                return;
            if (((InGameAxAssign)inGameAxis[Windows.MainWindow.deviceControl.mouseWheelAssign.GetAxisName()]).GetDate() > Windows.MainWindow.deviceControl.mouseWheelAssign.GetAssignDate())
                return;
            inGameAxis[Windows.MainWindow.deviceControl.mouseWheelAssign.GetAxisName()] = new InGameAxAssign(-2, -1, Windows.MainWindow.deviceControl.mouseWheelAssign);
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
                tbprogressbar.Maximum = Maxin;
            }
        }
        
        /// <summary>
        /// Aquire/Unaquire all devices
        /// </summary>
        /// <param name="flg"></param>
        public static void AquireAll(bool flg)
        {
            if (flg)
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