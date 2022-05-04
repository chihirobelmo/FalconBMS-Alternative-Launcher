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
        private MainWindow mainWindow;

        private KeyFile       keyFile;
        private KeyAssgn      SelectedCallback;

        private JoyAssgn[] tmpJoyStick;
        private KeyAssgn   tmpCallback;

        private DirectInputKeyboard directInputDevice = new DirectInputKeyboard();
        private DispatcherTimer     KeyMappingTimer   = new DispatcherTimer();

        private Stopwatch sw = Stopwatch.StartNew();

        private NeutralButtons[] neutralButtons;

        private Invoke invokeStatus = Invoke.Default;

        private bool pressedByHand;

        public KeyMappingWindow(MainWindow mainWindow, KeyAssgn SelectedCallback, KeyFile keyFile, DeviceControl deviceControl)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;

            CallbackName.Content = SelectedCallback.GetKeyDescription();

            Select_PinkyShift.IsChecked = true;
            Select_DX_Release.IsChecked = true;

            this.SelectedCallback = SelectedCallback;
            this.keyFile = keyFile;

            Reset();
        }

        public static void ShowKeyMappingWindow(MainWindow mainWindow, KeyAssgn SelectedCallback, KeyFile keyFile, DeviceControl deviceControl, object sender)
        {
            KeyMappingWindow ownWindow = new KeyMappingWindow(mainWindow, SelectedCallback, keyFile, deviceControl);
            ownWindow.ShowDialog();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            getNeutralPosition();
            KeyMappingTimer.Tick += KeyMappingtimerCode;
            KeyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 32);
            KeyMappingTimer.Start();
        }

        private void getNeutralPosition()
        {
            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
                neutralButtons[i] = new NeutralButtons(MainWindow.deviceControl.joyAssign[i]);
        }
        private void Reset()
        {
            neutralButtons = new NeutralButtons[MainWindow.deviceControl.joyAssign.Length];

            tmpJoyStick = new JoyAssgn[MainWindow.deviceControl.joyAssign.Length];
            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
            {
                tmpJoyStick[i] = MainWindow.deviceControl.joyAssign[i].Clone();
            }
            tmpCallback = SelectedCallback.Clone();
        }

        private void KeyMappingtimerCode(object sender, EventArgs e)
        {
            if (sw.ElapsedMilliseconds > 1000)
                AwaitingInputs.Content = "";
            if (sw.ElapsedMilliseconds > 1666)
                AwaitingInputs.Content = "   AWAITING INPUTS";

            if (sw.ElapsedMilliseconds > 1666)
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
                    getNeutralPosition();
                }

                sw.Reset();
                sw.Start();
            }

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
            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
                str += tmpCallback.ReadJoyAssignment(i, tmpJoyStick);
            MappedButton.Content = str;

            if (str != "")
            {
                AwaitingInputs.Content = "";
                return;
            }
        }

        private void JoystickButtonMonitor()
        {
            byte[] buttons;
            int[] povs;

            bool EitherOneOfShiftPressed = false;

            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
            {
                buttons = MainWindow.deviceControl.joyAssign[i].GetButtons();

                for (int ii = 0; ii < CommonConstants.DX128; ii++)
                {
                    if (buttons[ii] == CommonConstants.PRS128 && MainWindow.deviceControl.joyAssign[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false ||
                        buttons[ii] == CommonConstants.PRS128 && MainWindow.deviceControl.joyAssign[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasShift"      && pressedByHand == false)
                    {
                        EitherOneOfShiftPressed = true;
                    }
                }
            }

            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
            {
                buttons = MainWindow.deviceControl.joyAssign[i].GetButtons();

                for (int ii = 0; ii < CommonConstants.DX128; ii++)
                {
                    if (EitherOneOfShiftPressed)
                        Select_PinkyShift.IsChecked = false;
                    else
                        Select_PinkyShift.IsChecked = true;

                    if (buttons[ii] == neutralButtons[i].buttons[ii])
                        continue;
                    if (buttons[ii] == CommonConstants.PRS0)
                    {
                        getNeutralPosition();
                        continue;
                    }

                    if (MainWindow.deviceControl.joyAssign[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false ||
                        MainWindow.deviceControl.joyAssign[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasShift"      && pressedByHand == false)
                    {
                        continue;
                    }

                    Pinky pinkyStatus = Pinky.UnShift;
                    Behaviour behaviourStatus = Behaviour.Press;
                    if (Select_PinkyShift.IsChecked == false)
                        pinkyStatus = Pinky.Shift;
                    if (Select_DX_Release.IsChecked == false)
                        behaviourStatus = Behaviour.Release;

                    // Construct DX button instance.
                    if (tmpCallback.GetCallback() == "SimHotasPinkyShift" || tmpCallback.GetCallback() == "SimHotasShift")
                    {
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), Pinky.UnShift, Behaviour.Press, Invoke.Default, 0);
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), Pinky.Shift, Behaviour.Press, Invoke.Default, 0);
                    }
                    else
                    {
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), pinkyStatus, behaviourStatus, invokeStatus, 0);
                    }

                    getNeutralPosition();
                    return;
                }
                povs = MainWindow.deviceControl.joyAssign[i].GetPointOfView();
                buttons = MainWindow.deviceControl.joyAssign[i].GetButtons();
                for (int ii = 0; ii < 4; ii++)
                {
                    if (povs[ii] == neutralButtons[i].povs[ii])
                        continue;
                    if (povs[ii] == -1)
                    {
                        getNeutralPosition();
                        continue;
                    }

                    Pinky pinkyStatus = Pinky.UnShift;
                    if (Select_PinkyShift.IsChecked == false)
                        pinkyStatus = Pinky.Shift;

                    // Construct POV button instance.
                    tmpJoyStick[i].pov[ii].Assign(povs[ii], tmpCallback.GetCallback(), pinkyStatus, 0);

                    getNeutralPosition();
                    return;
                }
            }
        }

        private void KeyboardButtonMonitor()
        {
            directInputDevice.GetCurrentKeyboardState();
            for (int i = 1; i < CommonConstants.KEYBOARD_KEYLENGTH; i++)
                if (directInputDevice.KeyboardState[(Microsoft.DirectX.DirectInput.Key)i])
                    KeyMappingGrid_KeyDown();
        }

        private void KeyMappingGrid_KeyDown()
        {
            bool Shift = false;
            bool Ctrl = false;
            bool Alt = false;
            int catchedScanCode = 0;
            directInputDevice.GetCurrentKeyboardState();
            for (int i = 1; i < 238; i++)
            {
                if (directInputDevice.KeyboardState[(Microsoft.DirectX.DirectInput.Key)i])
                {
                    if (i == (int)Microsoft.DirectX.DirectInput.Key.LeftShift |
                        i == (int)Microsoft.DirectX.DirectInput.Key.RightShift)
                    {
                        Shift = true;
                        continue;
                    }
                    if (i == (int)Microsoft.DirectX.DirectInput.Key.LeftControl |
                        i == (int)Microsoft.DirectX.DirectInput.Key.RightControl)
                    {
                        Ctrl = true;
                        continue;
                    }
                    if (i == (int)Microsoft.DirectX.DirectInput.Key.LeftAlt |
                        i == (int)Microsoft.DirectX.DirectInput.Key.RightAlt)
                    {
                        Alt = true;
                        continue;
                    }
                    catchedScanCode = i;
                }
            }
            if (catchedScanCode == 0)
                return;

            Pinky pinkyStatus = Pinky.UnShift;
            if (Select_PinkyShift.IsChecked == false)
                pinkyStatus = Pinky.Shift;

            if (pinkyStatus == Pinky.UnShift)
                tmpCallback.SetKeyboard(catchedScanCode, Shift, Ctrl, Alt);
            if (pinkyStatus == Pinky.Shift)
                tmpCallback.Setkeycombo(catchedScanCode, Shift, Ctrl, Alt);
        }

        private class NeutralButtons
        {
            public byte[] buttons { get; set; }
            public int[] povs { get; set; }

            public NeutralButtons(JoyAssgn joyStick)
            {
                buttons = joyStick.GetButtons();
                povs = joyStick.GetPointOfView();
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            KeyMappingTimer.Stop();
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
            for (int i = 0; i < MainWindow.deviceControl.joyAssign.Length; i++)
            {
                tmpJoyStick[i] = MainWindow.deviceControl.joyAssign[i].Clone();
            }
            string target = tmpCallback.GetCallback();
            foreach (JoyAssgn joy in tmpJoyStick)
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
                    Select_Invoke.Content = "INVOKE KEYDN";
                    Select_Invoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
                    break;
                case Invoke.Down:
                    invokeStatus = Invoke.Up;
                    Select_Invoke.Content = "INVOKE KEYUP";
                    Select_Invoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
                    break;
                case Invoke.Up:
                    invokeStatus = Invoke.Default;
                    Select_Invoke.Content = "INVOKE BOTH";
                    Select_Invoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF7, 0xF7, 0xF7));
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
            for (int i = 0; i < tmpJoyStick.Length; i++)
            {
                MainWindow.deviceControl.joyAssign[i].Load(tmpJoyStick[i]);
            }
            SelectedCallback.getOtherKeyInstance(tmpCallback);

            // Unassign the previous mapping that was assigned to this key/key combo.
            KeyAssgn oldKey = keyFile.keyAssign.FirstOrDefault(x => x != SelectedCallback && x.GetKeyAssignmentStatus() == SelectedCallback.GetKeyAssignmentStatus());
            if (oldKey != null)
            {
                oldKey.UnassignKeyboard();
            }

            Close();
        }

        private void Select_PinkyShift_Click(object sender, RoutedEventArgs e)
        {
            if (Select_PinkyShift.IsChecked == false)
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
                    Select_Press.Content = "RELEASE";
                    Select_Press.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
                    invokeStatus = Invoke.Down;
                    Select_DX_Release.IsChecked = false;
                    break;
                case Press.Release:
                    pressStatus = Press.Press;
                    Select_Press.Content = "PRESS";
                    Select_Press.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF7, 0xF7, 0xF7));
                    invokeStatus = Invoke.Default;
                    Select_DX_Release.IsChecked = true;
                    break;
            }
        }
    }
}
