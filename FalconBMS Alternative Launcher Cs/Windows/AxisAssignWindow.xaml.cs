using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using FalconBMS.Launcher.Input;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for AxisAssignWindow.xaml
    /// </summary>
    public partial class AxisAssignWindow
    {
        public AxisAssignWindow(InGameAxAssign axisAssign, object sender)
        {
            InitializeComponent();

            this.axisAssign = axisAssign;
            whoCalledWindow = ((Button)sender).Name;

            devNumTmp = axisAssign.GetDeviceNumber();
            phyAxNumTmp = axisAssign.GetPhysicalNumber();

            MouseWheel += Detect_MouseWheel;
        }

        public static InGameAxAssign ShowAxisAssignWindow(InGameAxAssign axisAssign, object sender)
        {
            AxisAssignWindow ownWindow = new AxisAssignWindow(axisAssign, sender);
            ownWindow.ShowDialog();
            axisAssign = ownWindow.axisAssign;
            return axisAssign;
        }

        DispatcherTimer axisDetectionTimer = new DispatcherTimer();
        Stopwatch sw = Stopwatch.StartNew();

        private JoyAxisNeutralValue[] joynum;
        public class JoyAxisNeutralValue
        {
            public int[] neutralValue = new int[8];
        }

        private InGameAxAssign axisAssign;
        private string whoCalledWindow;

        private int devNumTmp = -1;
        private int phyAxNumTmp = -1;
        private int invertNum;
        private int wheelValue;

        const int Maxin = 65536;
        private int ab = Maxin;
        private int idle;

        private Status status = Status.GetNeutralPosition;

        private enum Status
        {
            GetNeutralPosition = -1,
            WaitInput = 0,
            ShowAxisStatus = 1
        }

        private void AssignWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // YAME 64 VERSION
            if (MainWindow.flgYame64)
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                BackGroundImage.Opacity = 0;
            }

            Retry.Visibility = Visibility.Hidden;
            SetAb.Visibility = Visibility.Hidden;
            Idle.Visibility = Visibility.Hidden;

            CheckAbidle.Visibility = Visibility.Hidden;

            AxisName.Content = whoCalledWindow.Replace("_", " ");

            switch (whoCalledWindow)
            {
                case "Roll":
                    DirectionDecrease.Content = "Left Wing Down";
                    DirectionIncrease.Content = "Right Wing Down";
                    break;
                case "Trim_Roll":
                    DirectionDecrease.Content = "Left Wing Down";
                    DirectionIncrease.Content = "Right Wing Down";
                    break;
                case "Pitch":
                    DirectionDecrease.Content = "Pitch Down";
                    DirectionIncrease.Content = "Pitch Up";
                    break;
                case "Trim_Pitch":
                    DirectionDecrease.Content = "Pitch Down";
                    DirectionIncrease.Content = "Pitch Up";
                    break;
                case "Yaw":
                case "Trim_Yaw":
                    DirectionDecrease.Content = "Yaw Left";
                    DirectionIncrease.Content = "Yaw Right";
                    break;
                case "Throttle":
                case "Throttle_Right":
                    DirectionDecrease.Content = "Afterward";
                    DirectionIncrease.Content = "Forward";
                    Invert.Visibility = Visibility.Collapsed;
                    DeadZone.Visibility = Visibility.Collapsed;
                    LabelDeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "Toe_Brake":
                case "Toe_Brake_Right":
                    DirectionDecrease.Content = "Release";
                    DirectionIncrease.Content = "Apply";
                    DeadZone.Visibility = Visibility.Collapsed;
                    LabelDeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "Radar_Antenna_Elevation":
                    DirectionDecrease.Content = "Elevation Down";
                    DirectionIncrease.Content = "Elevation Up";
                    break;
                case "Cursor_X":
                    DirectionDecrease.Content = "Cursor Left";
                    DirectionIncrease.Content = "Cursor Right";
                    break;
                case "Cursor_Y":
                    DirectionDecrease.Content = "Cursor Afterward";
                    DirectionIncrease.Content = "Cursor Forward";
                    break;
                case "Range_Knob":
                    DirectionDecrease.Content = "Clock Wise";
                    DirectionIncrease.Content = "Counter CW";
                    break;
                case "HMS_Brightness":
                case "FLIR_Brightness":
                case "HUD_Brightness":
                case "Reticle_Depression":
                    DirectionDecrease.Content = "Dark";
                    DirectionIncrease.Content = "Bright";
                    DeadZone.Visibility = Visibility.Collapsed;
                    LabelDeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "Intercom":
                case "COMM_Channel_1":
                case "COMM_Channel_2":
                case "MSL_Volume":
                case "Threat_Volume":
                case "AI_vs_IVC":
                    DirectionDecrease.Content = "Volume Down";
                    DirectionIncrease.Content = "Volume Up";
                    DeadZone.Visibility = Visibility.Collapsed;
                    LabelDeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "FOV":
                    DirectionDecrease.Content = "Narrow";
                    DirectionIncrease.Content = "Wide";
                    DeadZone.Visibility = Visibility.Collapsed;
                    LabelDeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "Camera_Distance":
                    DirectionDecrease.Content = "Close";
                    DirectionIncrease.Content = "Leave";
                    DeadZone.Visibility = Visibility.Collapsed;
                    LabelDeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "HSI_Course_Knob":
                case "HSI_Heading_Knob":
                case "Altimeter_Knob":
                    DirectionDecrease.Content = "Decrease";
                    DirectionIncrease.Content = "Increase";
                    DeadZone.Visibility = Visibility.Collapsed;
                    LabelDeadZone.Visibility = Visibility.Collapsed;
                    break;
            }

            AxisValueProgress.Value = 0;
            AxisValueProgress.Minimum = 0;
            AxisValueProgress.Maximum = Maxin;

            Saturation.SelectedIndex = (int)axisAssign.GetSaturation();
            DeadZone.SelectedIndex = (int)axisAssign.GetDeadZone();
            Invert.IsChecked = axisAssign.GetInvert();

            if (axisAssign.GetDeviceNumber() > -1 | 
                axisAssign.GetDeviceNumber() == -2)
            {
                status = Status.ShowAxisStatus;
                Retry.Content = "CLEAR";
                Retry.Visibility = Visibility.Visible;
                if (whoCalledWindow == "Throttle" | whoCalledWindow == "Throttle_Right")
                {
                    SetAb.Visibility = Visibility.Visible;
                    Idle.Visibility = Visibility.Visible;
                    ab = MainWindow.deviceControl.throttlePos.GetAb();
                    idle = MainWindow.deviceControl.throttlePos.GetIdle();
                }
            }

            joynum = new JoyAxisNeutralValue[MainWindow.deviceControl.devList.Count];
            for (int i = 0; i < MainWindow.deviceControl.devList.Count; i++)
            {
                joynum[i] = new JoyAxisNeutralValue();
            }

            axisDetectionTimer.Tick += AxisDetectionTimerCode;
            axisDetectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            axisDetectionTimer.Start();
            
        }

        private void AxisDetectionTimerCode(object sender, EventArgs e)
        {
            if (status == Status.GetNeutralPosition)
            {
                for (int i = 0; i <= MainWindow.deviceControl.devList.Count - 1; i++)
                    for (int ii = 0; ii < 8; ii++)
                        joynum[i].neutralValue[ii] = MainWindow.deviceControl.JoyAxisState(i, ii);
                status = Status.WaitInput;
                AssignedJoystick.Content = "   AWAITING INPUTS";
                sw.Start();
            }
            else if (status == Status.WaitInput)
            {
                for (int i = 0; i <= MainWindow.deviceControl.devList.Count - 1; i++)
                {
                    for (int ii = 0; ii < 8; ii++)
                    {
                        if (MainWindow.deviceControl.JoyAxisState(i, ii) < joynum[i].neutralValue[ii] + Maxin / 4 &
                            MainWindow.deviceControl.JoyAxisState(i, ii) > joynum[i].neutralValue[ii] - Maxin / 4)
                            continue;
                        devNumTmp = i;
                        phyAxNumTmp = ii;
                        status = Status.ShowAxisStatus;
                        Retry.Content = "RETRY";
                        Retry.Visibility = Visibility.Visible;

                        if (whoCalledWindow != "Throttle" & whoCalledWindow != "Throttle_Right")
                            continue;
                        SetAb.Visibility = Visibility.Visible;
                        Idle.Visibility = Visibility.Visible;
                        ab = MainWindow.deviceControl.throttlePos.GetAb();
                        idle = MainWindow.deviceControl.throttlePos.GetIdle();
                    }
                }
                if (sw.ElapsedMilliseconds > 1000)
                {
                    AssignedJoystick.Content = "";
                }
                if (sw.ElapsedMilliseconds > 1666)
                {
                    AssignedJoystick.Content = "   AWAITING INPUTS";
                    sw.Reset();
                    sw.Start();
                }
            }
            else if (status == Status.ShowAxisStatus)
            {
                switch (whoCalledWindow)
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
                        if (Invert.IsChecked == false || Invert.IsChecked == null)
                            invertNum = -1;
                        else
                            invertNum = 1;
                        break;
                    default:
                        if (Invert.IsChecked == false || Invert.IsChecked == null)
                            invertNum = 1;
                        else
                            invertNum = -1;
                        break;
                }
                if (invertNum == 1)
                {
                    AxisValueProgress.Minimum = 0;
                    AxisValueProgress.Maximum = Maxin;
                }
                else
                {
                    AxisValueProgress.Minimum = -Maxin;
                    AxisValueProgress.Maximum = 0;
                }

                if (devNumTmp == -2)
                {
                    AxisValueProgress.Value = (32768 + wheelValue * 1024 /120) * invertNum;
                    AssignedJoystick.Content = "   MouseWheel";
                    return;
                }

                int output = Input.MainWindow.ApplyDeadZone
                    (
                        MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp), 
                        (AxCurve)DeadZone.SelectedIndex, 
                        (AxCurve)Saturation.SelectedIndex
                    );
                AxisValueProgress.Value = output * invertNum;
                AssignedJoystick.Content = "   "
                    + ((AxisNumName)phyAxNumTmp).ToString().Replace('_',' ') + " : "
                    + MainWindow.deviceControl.joyStick[devNumTmp].DeviceInformation.ProductName;

                if (whoCalledWindow != "Throttle" & whoCalledWindow != "Throttle_Right")
                    return;
                AxisValueProgress.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0x38, 0x78, 0xA8));
                CheckAbidle.Visibility = Visibility.Hidden;
                if (Maxin + AxisValueProgress.Value < idle)
                {
                    AxisValueProgress.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 240, 0, 0));
                    CheckAbidle.Visibility = Visibility.Visible;
                    CheckAbidle.Content = "IDLE CUTOFF";
                }
                if (Maxin + AxisValueProgress.Value > ab)
                {
                    AxisValueProgress.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0, 240, 0));
                    CheckAbidle.Visibility = Visibility.Visible;
                    CheckAbidle.Content = "AB";
                }
            }
        }
        
        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            status = Status.GetNeutralPosition;
            AssignedJoystick.Content = "   AWAITING INPUTS";

            AxisValueProgress.Minimum = 0;
            AxisValueProgress.Maximum = Maxin;
            AxisValueProgress.Value = 0;

            Retry.Visibility = Visibility.Hidden;
            SetAb.Visibility = Visibility.Hidden;
            Idle.Visibility = Visibility.Hidden;

            wheelValue = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            switch (status)
            {
                case Status.WaitInput:
                {
                    AxAssgn axisInfo = new AxAssgn();
                    axisAssign = new InGameAxAssign(-1, -1, axisInfo);
                    if (whoCalledWindow == "Throttle" | whoCalledWindow == "Throttle_Right")
                        MainWindow.deviceControl.throttlePos = new ThrottlePosition();

                    break;
                }
                case Status.ShowAxisStatus:
                {
                    AxAssgn axisInfo = new AxAssgn();
                    axisAssign = new InGameAxAssign(
                        devNumTmp, 
                        phyAxNumTmp, 
                        (bool)Invert.IsChecked, 
                        (AxCurve)DeadZone.SelectedIndex, 
                        (AxCurve)Saturation.SelectedIndex
                    );
                    if (whoCalledWindow == "Throttle" | whoCalledWindow == "Throttle_Right")
                        MainWindow.deviceControl.throttlePos = new ThrottlePosition(ab,idle);

                    break;
                }
                case Status.GetNeutralPosition:
                    // TODO: Handle Neutral Position Status.
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            axisDetectionTimer.Stop();
            sw.Stop();
            Close();
        }

        private void AssignWindow_Closed(object sender, EventArgs e)
        {
            axisDetectionTimer.Stop();
            sw.Stop();
        }

        private void Detect_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (status == Status.WaitInput)
            {
                switch (whoCalledWindow)
                {
                    case "Roll":
                    case "Pitch":
                    case "Yaw":
                    case "Throttle":
                    case "Throttle_Right":
                        return;
                }
                devNumTmp = -2;
                status = Status.ShowAxisStatus;
                Retry.Content = "RETRY";
                Retry.Visibility = Visibility.Visible;
            }
            if (status == Status.ShowAxisStatus)
            {
                if (devNumTmp == -2)
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
        
        private void Saturation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        
        private void DeadZone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void SetAB_Click(object sender, RoutedEventArgs e)
        {
            if (status != Status.ShowAxisStatus)
                return;
            int aBposition;
            aBposition = Maxin - MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp);
            if (MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp) > Maxin)
                aBposition = Maxin;
            ab = aBposition;
        }

        private void SetIDLE_Click(object sender, RoutedEventArgs e)
        {
            if (status != Status.ShowAxisStatus)
                return;
            int idlEposition;
            idlEposition = Maxin - MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp);
            if (MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp) < 0)
                idlEposition = 0;
            idle = idlEposition;
        }
        
        private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            }
            catch
            {

            }
        }
    }
}
