using MahApps.Metro.Controls;
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
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.DirectX.DirectInput;

namespace FalconBMS_Alternative_Launcher_Cs
{
    /// <summary>
    /// Interaction logic for KeyMappingWindow.xaml
    /// </summary>
    public partial class KeyMappingWindow
    {
        private DeviceControl deviceControl;
        private KeyFile keyFile;
        private KeyAssgn SelectedCallback;

        private JoyAssgn[] tmpJoyStick;
        private KeyAssgn tmpCallback;

        private DirectInputKeyboard directInputDevice = new DirectInputKeyboard();
        private DispatcherTimer KeyMappingTimer = new DispatcherTimer();
        private Stopwatch sw = Stopwatch.StartNew();
        private NeutralButtons[] neutralButtons;
        private Invoke invokeStatus = Invoke.Default;

        public KeyMappingWindow(KeyAssgn SelectedCallback, KeyFile keyFile, DeviceControl deviceControl)
        {
            InitializeComponent();
            this.CallbackName.Content = SelectedCallback.GetKeyDescription();
            this.Select_PinkyShift.IsChecked = true;
            this.Select_DX_Release.IsChecked = true;
            this.SelectedCallback = SelectedCallback;
            this.keyFile = keyFile;
            this.deviceControl = deviceControl;
            this.neutralButtons = new NeutralButtons[deviceControl.devList.Count];

            tmpJoyStick = new JoyAssgn[deviceControl.devList.Count];
            for (int i = 0; i < deviceControl.devList.Count; i++)
            {
                tmpJoyStick[i] = deviceControl.joyAssign[i].Clone();
            }
            this.tmpCallback = this.SelectedCallback.Clone();
        }

        static public void ShowKeyMappingWindow(KeyAssgn SelectedCallback, KeyFile keyFile, DeviceControl deviceControl, object sender)
        {
            KeyMappingWindow ownWindow = new KeyMappingWindow(SelectedCallback, keyFile, deviceControl);
            ownWindow.ShowDialog();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            getNeutralPosition();
            KeyMappingTimer.Tick += KeyMappingtimerCode;
            KeyMappingTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            KeyMappingTimer.Start();
        }

        private void getNeutralPosition()
        {
            for (int i = 0; i < this.deviceControl.devList.Count; i++)
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
            str += this.tmpCallback.GetKeyAssignmentStatus() + "; ";
            if (str == "; ")
                str = "";
            for (int i = 0; i < deviceControl.devList.Count; i++)
                str += this.tmpCallback.ReadJoyAssignment(i, tmpJoyStick);
            this.MappedButton.Content = str;

            if (str != "")
            {
                this.AwaitingInputs.Content = "";
                return;
            }
            if (sw.ElapsedMilliseconds > 1000)
            {
                this.AwaitingInputs.Content = "";
            }
            if (sw.ElapsedMilliseconds > 1666)
            {
                this.AwaitingInputs.Content = "   AWAITING INPUTS";
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
                    if (buttons[ii] == neutralButtons[i].buttons[ii])
                        continue;
                    if (buttons[ii] == 0)
                    {
                        getNeutralPosition();
                        continue;
                    }

                    Pinky pinkyStatus = Pinky.UnShift;
                    Behaviour behaviourStatus = Behaviour.Press;
                    if (Select_PinkyShift.IsChecked == false)
                        pinkyStatus = Pinky.Shift;
                    if (Select_DX_Release.IsChecked == false)
                        behaviourStatus = Behaviour.Release;

                    // Construct DX button instance.
                    if (this.tmpCallback.GetCallback() == "SimHotasPinkyShift")
                    {
                        tmpJoyStick[i].dx[ii].Assign(this.tmpCallback.GetCallback(), Pinky.UnShift, Behaviour.Press, Invoke.Default, 0);
                        tmpJoyStick[i].dx[ii].Assign(this.tmpCallback.GetCallback(), Pinky.Shift, Behaviour.Press, Invoke.Default, 0);
                    }
                    else
                    {
                        tmpJoyStick[i].dx[ii].Assign(this.tmpCallback.GetCallback(), pinkyStatus, behaviourStatus, this.invokeStatus, 0);
                    }
                    //while (buttons[ii] != neutralButtons[i].buttons[ii])
                    //{
                    //    buttons = deviceControl.joyStick[i].CurrentJoystickState.GetButtons();
                    //}
                    getNeutralPosition();
                    return;
                }
                povs = deviceControl.joyStick[i].CurrentJoystickState.GetPointOfView();
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
                    tmpJoyStick[i].pov[ii].Assign(povs[ii], this.tmpCallback.GetCallback(), pinkyStatus, 0);
                    //while (povs[ii] != neutralButtons[i].povs[ii])
                    //{
                    //    povs = deviceControl.joyStick[i].CurrentJoystickState.GetPointOfView();
                    //}
                    getNeutralPosition();
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
            if (this.Select_PinkyShift.IsChecked == false)
                pinkyStatus = Pinky.Shift;

            if (pinkyStatus == Pinky.UnShift)
                this.tmpCallback.SetKeyboard(catchedScanCode, Shift, Ctrl, Alt);
            if (pinkyStatus == Pinky.Shift)
                this.tmpCallback.Setkeycombo(catchedScanCode, Shift, Ctrl, Alt);
            //this.Close();
        }

        private class NeutralButtons
        {
            public byte[] buttons { get; set; }
            public int[] povs { get; set; }

            public NeutralButtons(Device joyStick)
            {
                this.buttons = joyStick.CurrentJoystickState.GetButtons();
                this.povs = joyStick.CurrentJoystickState.GetPointOfView();
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            KeyMappingTimer.Stop();
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ClearDX_Click(object sender, RoutedEventArgs e)
        {
            string target = this.tmpCallback.GetCallback();
            foreach (JoyAssgn joy in this.tmpJoyStick)
                joy.UnassigntargetCallback(target);
            //this.Close();
        }

        private void ClearKey_Click(object sender, RoutedEventArgs e)
        {
            this.tmpCallback.UnassignKeyboard();
            //this.Close();
        }

        private void Select_Invoke_Click(object sender, RoutedEventArgs e)
        {
            switch (invokeStatus)
            {
                case Invoke.Default:
                    this.invokeStatus = Invoke.Down;
                    this.Select_Invoke.Content = "INVOKE KEYDN";
                    this.Select_Invoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
                    break;
                case Invoke.Down:
                    this.invokeStatus = Invoke.Up;
                    this.Select_Invoke.Content = "INVOKE KEYUP";
                    this.Select_Invoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xD9, 0xEA));
                    break;
                case Invoke.Up:
                    this.invokeStatus = Invoke.Default;
                    this.Select_Invoke.Content = "INVOKE BOTH";
                    this.Select_Invoke.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF7, 0xF7, 0xF7));
                    break;
            }
        }

        private class DirectInputKeyboard
        {
            Microsoft.DirectX.DirectInput.Device device;
            Microsoft.DirectX.DirectInput.KeyboardState keyState;
            public KeyboardState KeyboardState
            {
                get { return keyState; }
            }
            public DirectInputKeyboard()
            {
                device = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
                device.Acquire();
            }
            public void GetCurrentKeyboardState()
            {
                keyState = device.GetCurrentKeyboardState();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.deviceControl.joyAssign = this.tmpJoyStick;
            this.SelectedCallback.getOtherKeyInstance(this.tmpCallback);

            // Unassign the previous mapping that was assigned to this key/key combo.
            var oldKey = this.keyFile.keyAssign.FirstOrDefault(x => (x != this.SelectedCallback) && x.GetKeyAssignmentStatus() == this.SelectedCallback.GetKeyAssignmentStatus());
            if (oldKey != null)
            {
                oldKey.UnassignKeyboard();
            }

            this.Close();
        }
    }
}
