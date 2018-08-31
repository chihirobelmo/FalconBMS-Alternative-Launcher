using MahApps.Metro.Controls;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace FalconBMS_Alternative_Launcher_Cs
{
    /// <summary>
    /// Interaction logic for AxisAssignWindow.xaml
    /// </summary>
    public partial class AxisAssignWindow : MetroWindow
    {
        public AxisAssignWindow(InGameAxAssgn axisAssign, object sender)
        {
            InitializeComponent();

            this.axisAssign = axisAssign;
            this.whoCalledWindow = ((System.Windows.Controls.Button)sender).Name;

            this.devNumTmp = axisAssign.GetDeviceNumber();
            this.phyAxNumTmp = axisAssign.GetPhysicalNumber();

            this.MouseWheel += Detect_MouseWheel;
        }

        static public InGameAxAssgn ShowMiniWindow(InGameAxAssgn axisAssign, object sender)
        {
            AxisAssignWindow ownWindow = new AxisAssignWindow(axisAssign, sender);
            ownWindow.ShowDialog();
            axisAssign = ownWindow.axisAssign;
            return axisAssign;
        }

        System.Windows.Threading.DispatcherTimer AxisDetectionTimer = new System.Windows.Threading.DispatcherTimer();
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        private JoyAxisNeutralValue[] Joynum;
        public class JoyAxisNeutralValue
        {
            public int[] NeutralValue = new int[8];
        }

        private InGameAxAssgn axisAssign;
        private string whoCalledWindow;

        private int devNumTmp = -1;
        private int phyAxNumTmp = -1;
        
        private int invertNum = 0;
        private int wheelValue;

        const int MAXIN = 65536;

        private int AB = MAXIN;
        private int IDLE = 0;


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
            if (MainWindow.FLG_YAME64 == true)
            {
                this.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                BackGroundImage.Opacity = 0;
            }

            Retry.Visibility = Visibility.Hidden;
            SetAB.Visibility = Visibility.Hidden;
            Idle.Visibility = Visibility.Hidden;

            check_ABIDLE.Visibility = Visibility.Hidden;

            AxisName.Content = whoCalledWindow.Replace("_", " ");

            switch (whoCalledWindow)
            {
                case "Roll":
                    DirectionDecrease.Content = "Left Wing Down";
                    DirectionIncrease.Content = "Right Wing Down";
                    Invert.Visibility = Visibility.Collapsed;
                    break;
                case "Trim_Roll":
                    DirectionDecrease.Content = "Left Wing Down";
                    DirectionIncrease.Content = "Right Wing Down";
                    break;
                case "Pitch":
                    DirectionDecrease.Content = "Pitch Down";
                    DirectionIncrease.Content = "Pitch Up";
                    Invert.Visibility = Visibility.Collapsed;
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
                    Label_DeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "Toe_Brake":
                case "Toe_Brake_Right":
                    DirectionDecrease.Content = "Release";
                    DirectionIncrease.Content = "Apply";
                    DeadZone.Visibility = Visibility.Collapsed;
                    Label_DeadZone.Visibility = Visibility.Collapsed;
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
                    Label_DeadZone.Visibility = Visibility.Collapsed;
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
                    Label_DeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "FOV":
                    DirectionDecrease.Content = "Narrow";
                    DirectionIncrease.Content = "Wide";
                    DeadZone.Visibility = Visibility.Collapsed;
                    Label_DeadZone.Visibility = Visibility.Collapsed;
                    break;
                case "Camera_Distance":
                    DirectionDecrease.Content = "Close";
                    DirectionIncrease.Content = "Leave";
                    DeadZone.Visibility = Visibility.Collapsed;
                    Label_DeadZone.Visibility = Visibility.Collapsed;
                    break;
            }

            AxisValueProgress.Value = 0;
            AxisValueProgress.Minimum = 0;
            AxisValueProgress.Maximum = MAXIN;

            Saturation.SelectedIndex = (int)axisAssign.GetSaturation();
            DeadZone.SelectedIndex = (int)axisAssign.GetDeadzone();
            Invert.IsChecked = axisAssign.GetInvert();

            if (axisAssign.GetDeviceNumber() > -1 | 
                axisAssign.GetDeviceNumber() == -2)
            {
                status = Status.ShowAxisStatus;
                Retry.Content = "CLEAR";
                Retry.Visibility = Visibility.Visible;
                if (whoCalledWindow == "Throttle" | whoCalledWindow == "Throttle_Right")
                {
                    SetAB.Visibility = Visibility.Visible;
                    Idle.Visibility = Visibility.Visible;
                    AB = MainWindow.deviceControl.throttlePos.GetAB();
                    IDLE = MainWindow.deviceControl.throttlePos.GetIDLE();
                }
            }

            Joynum = new JoyAxisNeutralValue[MainWindow.deviceControl.devList.Count];
            for (int i = 0; i < MainWindow.deviceControl.devList.Count; i++)
            {
                Joynum[i] = new JoyAxisNeutralValue();
            }

            AxisDetectionTimer.Tick += AxisDetectionTimerCode;
            AxisDetectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            AxisDetectionTimer.Start();
            
        }

        private void AxisDetectionTimerCode(object sender, EventArgs e)
        {
            if (status == Status.GetNeutralPosition)
            {
                for (int i = 0; i <= MainWindow.deviceControl.devList.Count - 1; i++)
                    for (int ii = 0; ii < 8; ii++)
                        Joynum[i].NeutralValue[ii] = MainWindow.deviceControl.JoyAxisState(i, ii);
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
                        if (MainWindow.deviceControl.JoyAxisState(i, ii) < (Joynum[i].NeutralValue[ii] + MAXIN / 4) &
                            MainWindow.deviceControl.JoyAxisState(i, ii) > (Joynum[i].NeutralValue[ii] - MAXIN / 4))
                            continue;
                        devNumTmp = i;
                        phyAxNumTmp = ii;
                        status = Status.ShowAxisStatus;
                        Retry.Content = "RETRY";
                        Retry.Visibility = Visibility.Visible;

                        if (whoCalledWindow != "Throttle" &
                            whoCalledWindow != "Throttle_Right")
                            continue;
                        SetAB.Visibility = Visibility.Visible;
                        Idle.Visibility = Visibility.Visible;
                        AB = MainWindow.deviceControl.throttlePos.GetAB();
                        IDLE = MainWindow.deviceControl.throttlePos.GetIDLE();
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
                    AxisValueProgress.Maximum = MAXIN;
                }
                else
                {
                    AxisValueProgress.Minimum = -MAXIN;
                    AxisValueProgress.Maximum = 0;
                }

                if (devNumTmp == -2)
                {
                    AxisValueProgress.Value = (32768 + (wheelValue * 1024 /120 )) * invertNum;
                    AssignedJoystick.Content = "   MouseWheel";
                    return;
                }

                int output = MainWindow.ApplyDeadZone
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
                check_ABIDLE.Visibility = Visibility.Hidden;
                if (MAXIN + AxisValueProgress.Value < IDLE)
                {
                    AxisValueProgress.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 240, 0, 0));
                    check_ABIDLE.Visibility = Visibility.Visible;
                    check_ABIDLE.Content = "IDLE CUTOFF";
                }
                if (MAXIN + AxisValueProgress.Value > AB)
                {
                    AxisValueProgress.Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0, 240, 0));
                    check_ABIDLE.Visibility = Visibility.Visible;
                    check_ABIDLE.Content = "AB";
                }
            }
        }
        
        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            status = Status.GetNeutralPosition;
            AssignedJoystick.Content = "   AWAITING INPUTS";

            AxisValueProgress.Minimum = 0;
            AxisValueProgress.Maximum = MAXIN;
            AxisValueProgress.Value = 0;

            Retry.Visibility = Visibility.Hidden;
            SetAB.Visibility = Visibility.Hidden;
            Idle.Visibility = Visibility.Hidden;

            wheelValue = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (status == Status.WaitInput)
            {
                JoyAssgn.AxAssgn axisInfo = new JoyAssgn.AxAssgn();
                axisAssign = new InGameAxAssgn(-1, -1, axisInfo);
                if (whoCalledWindow == "Throttle" | whoCalledWindow == "Throttle_Right")
                    MainWindow.deviceControl.throttlePos = new ThrottlePosition();
            }
            if (status == Status.ShowAxisStatus)
            {
                JoyAssgn.AxAssgn axisInfo = new JoyAssgn.AxAssgn();
                axisAssign = new InGameAxAssgn(
                    devNumTmp, 
                    phyAxNumTmp, 
                    (bool)Invert.IsChecked, 
                    (AxCurve)DeadZone.SelectedIndex, 
                    (AxCurve)Saturation.SelectedIndex
                    );
                if (whoCalledWindow == "Throttle" | whoCalledWindow == "Throttle_Right")
                    MainWindow.deviceControl.throttlePos = new ThrottlePosition(AB,IDLE);
            }
            AxisDetectionTimer.Stop();
            sw.Stop();
            this.Close();
        }

        private void AssignWindow_Closed(object sender, EventArgs e)
        {
            AxisDetectionTimer.Stop();
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
            int ABposition;
            ABposition = MAXIN - MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp);
            ABposition += 256;
            if (MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp) > MAXIN)
                ABposition = MAXIN;
            this.AB = ABposition;
        }

        private void SetIDLE_Click(object sender, RoutedEventArgs e)
        {
            if (status != Status.ShowAxisStatus)
                return;
            int IDLEposition;
            IDLEposition = MAXIN - MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp);
            IDLEposition -= 256;
            if (MainWindow.deviceControl.JoyAxisState(devNumTmp, phyAxNumTmp) < 0)
                IDLEposition = 0;
            this.IDLE = IDLEposition;
        }
        
        private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
