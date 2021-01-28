using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using FalconBMS.Launcher.Input;

using Microsoft.DirectX.DirectInput;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for KeyMappingWindow.xaml
    /// </summary>
    public partial class KeyMappingWindow
    {
        private DeviceControl deviceControl;
        private KeyFile keyFile;
        private KeyAssign selectedCallback;

        private JoyAssign[] tmpJoyStick;
        private KeyAssign tmpCallback;

        private DirectInputKeyboard directInputDevice = new DirectInputKeyboard();
        private DispatcherTimer keyMappingTimer = new DispatcherTimer();
        private Stopwatch sw = Stopwatch.StartNew();
        private NeutralButtons[] neutralButtons;
        private Invoke invokeStatus = Invoke.Default;

        private bool pressedByHand;

        public KeyMappingWindow(KeyAssign selectedCallback, KeyFile keyFile, DeviceControl deviceControl)
        {
            InitializeComponent();
            CallbackName.Content = selectedCallback.GetKeyDescription();
            SelectPinkyShift.IsChecked = true;
            SelectDxRelease.IsChecked = true;
            this.selectedCallback = selectedCallback;
            this.keyFile = keyFile;
            this.deviceControl = deviceControl;
            neutralButtons = new NeutralButtons[deviceControl.devList.Count];

            tmpJoyStick = new JoyAssign[deviceControl.devList.Count];
            for (int i = 0; i < deviceControl.devList.Count; i++)
            {
                tmpJoyStick[i] = deviceControl.joyAssign[i].Clone();
            }
            tmpCallback = this.selectedCallback.Clone();
        }

        public static void ShowKeyMappingWindow(KeyAssign selectedCallback, KeyFile keyFile, DeviceControl deviceControl, object sender)
        {
            KeyMappingWindow ownWindow = new KeyMappingWindow(selectedCallback, keyFile, deviceControl);
            ownWindow.ShowDialog();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            GetNeutralPosition();
            keyMappingTimer.Tick += KeyMappingtimerCode;
            keyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            keyMappingTimer.Start();
        }

        private void GetNeutralPosition()
        {
            for (int i = 0; i < deviceControl.devList.Count; i++)
                neutralButtons[i] = new NeutralButtons(deviceControl.joyStick[i]);
        }

        private void KeyMappingtimerCode(object sender, EventArgs e)
        {
            KeyboardButtonMonitor();
            JoystickButtonMonitor();
            ShowAssignedStatus();
        }

        private void ShowAssignedStatus()
        {
            string str = "";
            str += tmpCallback.GetKeyAssignmentStatus() + "; ";
            if (str == "; ")
                str = "";
            for (int i = 0; i < deviceControl.devList.Count; i++)
                str += tmpCallback.ReadJoyAssignment(i, tmpJoyStick);
            MappedButton.Content = str;

            if (str != "")
            {
                AwaitingInputs.Content = "";
                return;
            }
            if (sw.ElapsedMilliseconds > 1000)
            {
                AwaitingInputs.Content = "";
            }
            if (sw.ElapsedMilliseconds > 1666)
            {
                AwaitingInputs.Content = "   AWAITING INPUTS";
                sw.Reset();
                sw.Start();
            }
        }

        private void JoystickButtonMonitor()
        {
            for (int i = 0; i < deviceControl.devList.Count; i++)
            {
                byte[] buttons;
                int[] povs;

                buttons = deviceControl.joyStick[i].CurrentJoystickState.GetButtons();
                for (int ii = 0; ii < 32; ii++)
                {
                    if (buttons[ii] == 128 && deviceControl.joyAssign[i].dx[ii].assign[0].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false)
                    {
                        SelectPinkyShift.IsChecked = false;
                    }
                    if (buttons[ii] == 0 && deviceControl.joyAssign[i].dx[ii].assign[0].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false)
                    {
                        SelectPinkyShift.IsChecked = true;
                    }

                    if (buttons[ii] == neutralButtons[i].Buttons[ii])
                        continue;
                    if (buttons[ii] == 0)
                    {
                        GetNeutralPosition();
                        continue;
                    }

                    if (deviceControl.joyAssign[i].dx[ii].assign[0].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false)
                    {
                        continue;
                    }

                    Pinky pinkyStatus = Pinky.UnShift;
                    Behaviour behaviourStatus = Behaviour.Press;
                    if (SelectPinkyShift.IsChecked == false)
                        pinkyStatus = Pinky.Shift;
                    if (SelectDxRelease.IsChecked == false)
                        behaviourStatus = Behaviour.Release;

                    // Construct DX button instance.
                    if (tmpCallback.GetCallback() == "SimHotasPinkyShift")
                    {
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), Pinky.UnShift, Behaviour.Press, Invoke.Default, 0);
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), Pinky.Shift, Behaviour.Press, Invoke.Default, 0);
                    }
                    else
                    {
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), pinkyStatus, behaviourStatus, invokeStatus, 0);
                    }
                    //while (buttons[ii] != neutralButtons[i].buttons[ii])
                    //{
                    //    buttons = deviceControl.joyStick[i].CurrentJoystickState.GetButtons();
                    //}
                    GetNeutralPosition();
                    return;
                }
                povs = deviceControl.joyStick[i].CurrentJoystickState.GetPointOfView();
                buttons = deviceControl.joyStick[i].CurrentJoystickState.GetButtons();
                for (int ii = 0; ii < 4; ii++)
                {
                    if (povs[ii] == neutralButtons[i].Povs[ii])
                        continue;
                    if (povs[ii] == -1)
                    {
                        GetNeutralPosition();
                        continue;
                    }

                    Pinky pinkyStatus = Pinky.UnShift;
                    if (SelectPinkyShift.IsChecked == false)
                        pinkyStatus = Pinky.Shift;

                    // Construct POV button instance.
                    tmpJoyStick[i].pov[ii].Assign(povs[ii], tmpCallback.GetCallback(), pinkyStatus, 0);
                    //while (povs[ii] != neutralButtons[i].povs[ii])
                    //{
                    //    povs = deviceControl.joyStick[i].CurrentJoystickState.GetPointOfView();
                    //}
                    GetNeutralPosition();
                    return;
                }
            }
        }

        private void KeyboardButtonMonitor()
        {
            directInputDevice.GetCurrentKeyboardState();
            for (int i = 1; i < 238; i++)
                if (directInputDevice.KeyboardState[(Microsoft.DirectX.DirectInput.Key)i])
                    KeyMappingGrid_KeyDown();
        }

        private void KeyMappingGrid_KeyDown()
        {
            bool shift = false;
            bool ctrl = false;
            bool alt = false;
            int catchedScanCode = 0;
            directInputDevice.GetCurrentKeyboardState();
            for (int i = 1; i < 238; i++)
            {
                if (directInputDevice.KeyboardState[(Microsoft.DirectX.DirectInput.Key)i])
                {
                    if (i == (int)Microsoft.DirectX.DirectInput.Key.LeftShift |
                        i == (int)Microsoft.DirectX.DirectInput.Key.RightShift)
                    {
                        shift = true;
                        continue;
                    }
                    if (i == (int)Microsoft.DirectX.DirectInput.Key.LeftControl |
                        i == (int)Microsoft.DirectX.DirectInput.Key.RightControl)
                    {
                        ctrl = true;
                        continue;
                    }
                    if (i == (int)Microsoft.DirectX.DirectInput.Key.LeftAlt |
                        i == (int)Microsoft.DirectX.DirectInput.Key.RightAlt)
                    {
                        alt = true;
                        continue;
                    }
                    catchedScanCode = i;
                }
            }
            if (catchedScanCode == 0)
                return;

            Pinky pinkyStatus = Pinky.UnShift;
            if (SelectPinkyShift.IsChecked == false)
                pinkyStatus = Pinky.Shift;

            if (pinkyStatus == Pinky.UnShift)
                tmpCallback.SetKeyboard(catchedScanCode, shift, ctrl, alt);
            if (pinkyStatus == Pinky.Shift)
                tmpCallback.Setkeycombo(catchedScanCode, shift, ctrl, alt);
        }

        private class NeutralButtons
        {
            public byte[] Buttons { get; set; }
            public int[] Povs { get; set; }

            public NeutralButtons(Device joyStick)
            {
                Buttons = joyStick.CurrentJoystickState.GetButtons();
                Povs = joyStick.CurrentJoystickState.GetPointOfView();
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            keyMappingTimer.Stop();
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            }
            catch
            { }
        }

        private void ClearDX_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < deviceControl.devList.Count; i++)
            {
                tmpJoyStick[i] = deviceControl.joyAssign[i].Clone();
            }
            string target = tmpCallback.GetCallback();
            foreach (JoyAssign joy in tmpJoyStick)
                joy.UnassigntargetCallback(target);
        }

        private void ClearKey_Click(object sender, RoutedEventArgs e)
        {
            tmpCallback.UnassignKeyboard();
        }

        private void Select_Invoke_Click(object sender, RoutedEventArgs e)
        {
            switch (invokeStatus)
            {
                case Invoke.Default:
                    invokeStatus = Invoke.Down;
                    SelectInvoke.Content = "INVOKE KEYDN";
                    SelectInvoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
                    break;
                case Invoke.Down:
                    invokeStatus = Invoke.Up;
                    SelectInvoke.Content = "INVOKE KEYUP";
                    SelectInvoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
                    break;
                case Invoke.Up:
                    invokeStatus = Invoke.Default;
                    SelectInvoke.Content = "INVOKE BOTH";
                    SelectInvoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF7, 0xF7, 0xF7));
                    break;
            }
        }

        private class DirectInputKeyboard
        {
            Device device;
            KeyboardState keyState;
            public KeyboardState KeyboardState => keyState;

            public DirectInputKeyboard()
            {
                device = new Device(SystemGuid.Keyboard);
                device.Acquire();
            }
            public void GetCurrentKeyboardState()
            {
                keyState = device.GetCurrentKeyboardState();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            deviceControl.joyAssign = tmpJoyStick;
            selectedCallback.GetOtherKeyInstance(tmpCallback);

            // Unassign the previous mapping that was assigned to this key/key combo.
            KeyAssign oldKey = keyFile.keyAssign.FirstOrDefault(x => x != selectedCallback && x.GetKeyAssignmentStatus() == selectedCallback.GetKeyAssignmentStatus());
            if (oldKey != null)
            {
                oldKey.UnassignKeyboard();
            }

            Close();
        }

        private void Select_PinkyShift_Click(object sender, RoutedEventArgs e)
        {
            if (SelectPinkyShift.IsChecked == false)
                pressedByHand = true;
            else
                pressedByHand = false;
        }

        private Press pressStatus = Press.Press;
        public enum Press
        {
            Press,
            Hold,
            Release
        }

        private void Select_Press_Click(object sender, RoutedEventArgs e)
        {
            switch (pressStatus)
            {
                case Press.Press:
                    pressStatus = Press.Release;
                    SelectPress.Content = "RELEASE";
                    SelectPress.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
                    invokeStatus = Invoke.Down;
                    SelectDxRelease.IsChecked = false;
                    break;
                case Press.Release:
                    pressStatus = Press.Press;
                    SelectPress.Content = "PRESS";
                    SelectPress.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF7, 0xF7, 0xF7));
                    invokeStatus = Invoke.Default;
                    SelectDxRelease.IsChecked = true;
                    break;
            }
        }
    }
}
