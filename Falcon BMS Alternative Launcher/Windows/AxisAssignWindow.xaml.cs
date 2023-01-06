using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using FalconBMS.Launcher.Input;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for AxisAssignWindow.xaml
    /// </summary>
    public partial class AxisAssignWindow
    {
        public AxisAssignWindow(MainWindow mainWindow, InGameAxAssgn axisAssign, object sender)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.axisAssign = axisAssign;

            whoCalledWindow = ((System.Windows.Controls.Button)sender).Name;

            MouseWheel += Detect_MouseWheel;
        }

        public static InGameAxAssgn ShowAxisAssignWindow(MainWindow mainWindow, InGameAxAssgn axisAssign, object sender)
        {
            AxisAssignWindow ownWindow = new AxisAssignWindow(mainWindow, axisAssign, sender);
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

        private MainWindow mainWindow;

        private InGameAxAssgn axisAssign;
        private string whoCalledWindow;

        private int devNumTmp = -1;
        private int phyAxNumTmp = -1;
        private int invertNum;
        private int wheelValue;

        private int AB   = CommonConstants.AXISMAX;
        private int IDLE = CommonConstants.AXISMIN;

        private Status status = Status.GetNeutralPosition;

        private enum Status
        {
            GetNeutralPosition = -1,
            WaitInput = 0,
            ShowAxisStatus = 1
        }

        private void AssignWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Retry.Visibility = Visibility.Hidden;
            SetAB.Visibility = Visibility.Hidden;
            Idle.Visibility = Visibility.Hidden;

            check_ABIDLE.Visibility = Visibility.Hidden;

            Label_AxisName.Content = whoCalledWindow.Replace("_", " ");

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
                case "HSI_Course_Knob":
                case "HSI_Heading_Knob":
                case "Altimeter_Knob":
                    DirectionDecrease.Content = "Decrease";
                    DirectionIncrease.Content = "Increase";
                    DeadZone.Visibility = Visibility.Collapsed;
                    Label_DeadZone.Visibility = Visibility.Collapsed;
                    break;
            }

            AxisValueProgress.Value   = CommonConstants.AXISMIN;
            AxisValueProgress.Minimum = CommonConstants.AXISMIN;
            AxisValueProgress.Maximum = CommonConstants.AXISMAX;

            Reset();

            AxisDetectionTimer.Tick += AxisDetectionTimerCode;
            AxisDetectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            AxisDetectionTimer.Start();
            
        }

        public void Reset()
        {
            AxisValueProgress.Value = CommonConstants.AXISMIN;

            axisAssign = (InGameAxAssgn)MainWindow.inGameAxis[whoCalledWindow];

            if (axisAssign.GetDeviceNumber() > -1 | axisAssign.GetDeviceNumber() == -2)
            {
                status = Status.ShowAxisStatus;
                Retry.Content = "CLEAR";
                Retry.Visibility = Visibility.Visible;
                if (whoCalledWindow == AxisName.Throttle.ToString())
                {
                    SetAB.Visibility = Visibility.Visible;
                    Idle.Visibility = Visibility.Visible;

                    if (axisAssign.GetDeviceNumber() >= 0)
                    {
                        AB = MainWindow.deviceControl.joyAssign[axisAssign.GetDeviceNumber()].detentPosition.GetAB();
                        IDLE = MainWindow.deviceControl.joyAssign[axisAssign.GetDeviceNumber()].detentPosition.GetIDLE();
                    }
                }
            }

            Saturation.SelectedIndex = (int)axisAssign.GetSaturation();
            DeadZone.SelectedIndex = (int)axisAssign.GetDeadzone();
            Invert.IsChecked = axisAssign.GetInvert();

            devNumTmp = axisAssign.GetDeviceNumber();
            phyAxNumTmp = axisAssign.GetPhysicalNumber();

            Joynum = new JoyAxisNeutralValue[MainWindow.deviceControl.joyAssign.Length];
            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
                Joynum[i] = new JoyAxisNeutralValue();

            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
                for (int ii = 0; ii < 8; ii++)
                    Joynum[i].NeutralValue[ii] = MainWindow.deviceControl.joyAssign[i].JoyAxisState(ii);
        }

        private void GetNeutralPosition()
        {
            Reset();

            status = Status.WaitInput;
            AssignedJoystick.Content = "   AWAITING INPUTS";
            sw.Start();
        }

        private void WaitInput()
        {
            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
            {
                for (int ii = 0; ii < 8; ii++)
                {
                    if (MainWindow.deviceControl.joyAssign[i].JoyAxisState(ii) < Joynum[i].NeutralValue[ii] + CommonConstants.AXISMAX / 4 &
                        MainWindow.deviceControl.joyAssign[i].JoyAxisState(ii) > Joynum[i].NeutralValue[ii] - CommonConstants.AXISMAX / 4)
                        continue;
                    devNumTmp = i;
                    phyAxNumTmp = ii;
                    status = Status.ShowAxisStatus;
                    Retry.Content = "RETRY";
                    Retry.Visibility = Visibility.Visible;

                    if (whoCalledWindow != AxisName.Throttle.ToString())
                        continue;
                    SetAB.Visibility = Visibility.Visible;
                    Idle.Visibility = Visibility.Visible;

                    InGameAxAssgn axis = (InGameAxAssgn)MainWindow.inGameAxis[whoCalledWindow];
                    if (axis.GetDeviceNumber() >= 0)
                    {
                        AB = MainWindow.deviceControl.joyAssign[axis.GetDeviceNumber()].detentPosition.GetAB();
                        IDLE = MainWindow.deviceControl.joyAssign[axis.GetDeviceNumber()].detentPosition.GetIDLE();
                    }
                }
            }
        }

        private void InvertAxisDisp()
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
                AxisValueProgress.Minimum = CommonConstants.AXISMIN;
                AxisValueProgress.Maximum = CommonConstants.AXISMAX;
            }
            else
            {
                AxisValueProgress.Minimum = -CommonConstants.AXISMAX;
                AxisValueProgress.Maximum = CommonConstants.AXISMIN;
            }
        }

        private void ShowAxisStatus()
        {
            // no joystick assigned
            if (devNumTmp == -1)
            {
                status = Status.GetNeutralPosition;
                return;
            }

            InvertAxisDisp();

            // mouse wheel assigned
            if (devNumTmp == -2)
            {
                AxisValueProgress.Value = (32768 + wheelValue * 1024 / 120) * invertNum;
                AssignedJoystick.Content = "   MouseWheel";
                return;
            }

            int output = MainWindow.ApplyDeadZone
                            (
                                MainWindow.deviceControl.joyAssign[devNumTmp].JoyAxisState(phyAxNumTmp),
                                (AxCurve)DeadZone.SelectedIndex,
                                (AxCurve)Saturation.SelectedIndex
                            );
            AxisValueProgress.Value = output * invertNum;
            AssignedJoystick.Content = "   "
                + ((AxisNumName)phyAxNumTmp).ToString().Replace('_', ' ') + " : "
                + MainWindow.deviceControl.joyStick[devNumTmp].DeviceInformation.ProductName;

            if (whoCalledWindow != AxisName.Throttle.ToString() & whoCalledWindow != AxisName.Throttle_Right.ToString())
                return;
            AxisValueProgress.Foreground = CommonConstants.LIGHTBLUE;
            check_ABIDLE.Visibility = Visibility.Hidden;
            if ( (Invert.IsChecked == false && CommonConstants.AXISMAX + AxisValueProgress.Value < IDLE) || (Invert.IsChecked == true && CommonConstants.AXISMIN + AxisValueProgress.Value < IDLE))
            {
                AxisValueProgress.Foreground = CommonConstants.LIGHTRED;
                check_ABIDLE.Visibility = Visibility.Visible;
                check_ABIDLE.Content = "IDLE CUTOFF";
            }
            if ( (Invert.IsChecked == false && CommonConstants.AXISMAX + AxisValueProgress.Value > AB) || (Invert.IsChecked == true && CommonConstants.AXISMIN + AxisValueProgress.Value > AB) )
            {
                AxisValueProgress.Foreground = CommonConstants.LIGHTGREEN;
                check_ABIDLE.Visibility = Visibility.Visible;
                check_ABIDLE.Content = "AB";
            }
        }

        private void AxisDetectionTimerCode(object sender, EventArgs e)
        {
            if (sw.ElapsedMilliseconds > CommonConstants.FLUSHTIME1)
                AssignedJoystick.Content = "";
            if (sw.ElapsedMilliseconds > CommonConstants.FLUSHTIME2)
            {
                AssignedJoystick.Content = "   AWAITING INPUTS";
            }

            try
            {
                if (sw.ElapsedMilliseconds > CommonConstants.FLUSHTIME2)
                {
                    Microsoft.DirectX.DirectInput.DeviceList devList =
                    Microsoft.DirectX.DirectInput.Manager.GetDevices(
                    Microsoft.DirectX.DirectInput.DeviceClass.GameControl,
                    Microsoft.DirectX.DirectInput.EnumDevicesFlags.AttachedOnly
                    );

                    if (devList.Count != MainWindow.deviceControl.joyAssign.Length)
                    {
                        mainWindow.ReloadDevices();
                        Reset();
                        mainWindow.UpdateAxisStatus();
                    }

                    sw.Reset();
                    sw.Start();
                }

                switch (status)
                {
                    case Status.GetNeutralPosition:

                        GetNeutralPosition();

                        break;

                    case Status.WaitInput:

                        WaitInput();

                        break;

                    case Status.ShowAxisStatus:

                        ShowAxisStatus();

                        break;

                    default:
                        break;
                }
            }
            catch
            {
                Console.WriteLine("Error on Axis Moving Timer");
            }
        }
        
        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            status = Status.GetNeutralPosition;
            AssignedJoystick.Content = "   AWAITING INPUTS";

            AxisValueProgress.Minimum = CommonConstants.AXISMIN;
            AxisValueProgress.Maximum = CommonConstants.AXISMAX;
            AxisValueProgress.Value   = CommonConstants.AXISMIN;

            Retry.Visibility = Visibility.Hidden;
            SetAB.Visibility = Visibility.Hidden;
            Idle.Visibility  = Visibility.Hidden;

            wheelValue = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (status == Status.WaitInput)
            {
                AxAssgn axisInfo = new AxAssgn();
                axisAssign = new InGameAxAssgn(new JoyAssgn(), -1, axisInfo);
            }
            if (status == Status.ShowAxisStatus)
            {
                if (devNumTmp > CommonConstants.JOYNUMUNASSIGNED)
                {
                    axisAssign = new InGameAxAssgn(
                            MainWindow.deviceControl.joyAssign[devNumTmp],
                            phyAxNumTmp,
                            (bool)Invert.IsChecked,
                            (AxCurve)DeadZone.SelectedIndex,
                            (AxCurve)Saturation.SelectedIndex
                        );
                    if (whoCalledWindow == AxisName.Throttle.ToString())
                        MainWindow.deviceControl.joyAssign[devNumTmp].detentPosition = new DetentPosition(AB, IDLE);
                }
                else if (devNumTmp == CommonConstants.JOYNUMMOUSEWHEEL)
                {
                    axisAssign = new InGameAxAssgn(
                            MainWindow.deviceControl.mouse,
                            0,
                            (bool)Invert.IsChecked,
                            (AxCurve)DeadZone.SelectedIndex,
                            (AxCurve)Saturation.SelectedIndex
                        );
                    if (whoCalledWindow == AxisName.Throttle.ToString())
                        MainWindow.deviceControl.mouse.detentPosition = new DetentPosition(AB, IDLE);
                }
            }
            AxisDetectionTimer.Stop();
            sw.Stop();
            Close();
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
            
            if (Invert.IsChecked == true)
                AB = CommonConstants.AXISMIN + MainWindow.deviceControl.joyAssign[devNumTmp].JoyAxisState(phyAxNumTmp);
            else
                AB = CommonConstants.AXISMAX - MainWindow.deviceControl.joyAssign[devNumTmp].JoyAxisState(phyAxNumTmp);

            if (AB > CommonConstants.AXISMAX)
                AB = CommonConstants.AXISMAX;
            if (AB < CommonConstants.AXISMIN)
                AB = CommonConstants.AXISMIN;
        }

        private void SetIDLE_Click(object sender, RoutedEventArgs e)
        {
            if (status != Status.ShowAxisStatus)
                return;

            if (Invert.IsChecked == true)
                IDLE = CommonConstants.AXISMIN + MainWindow.deviceControl.joyAssign[devNumTmp].JoyAxisState(phyAxNumTmp);
            else
                IDLE = CommonConstants.AXISMAX - MainWindow.deviceControl.joyAssign[devNumTmp].JoyAxisState(phyAxNumTmp);

            if (IDLE > CommonConstants.AXISMAX)
                IDLE = CommonConstants.AXISMAX;
            if (IDLE < CommonConstants.AXISMIN)
                IDLE = CommonConstants.AXISMIN;
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
                // Don't write anything here.
            }
        }
    }
}
