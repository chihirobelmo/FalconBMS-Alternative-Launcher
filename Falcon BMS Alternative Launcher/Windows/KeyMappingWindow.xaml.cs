using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
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
        private DeviceControl deviceControlRef;

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

        public KeyMappingWindow(DeviceControl deviceControl, KeyAssgn SelectedCallback)
        {
            InitializeComponent();

            this.SelectedCallback = SelectedCallback;

            this.deviceControlRef = deviceControl;
            this.keyFile = deviceControl.GetKeyBindings();

            CallbackName.Content = SelectedCallback.GetKeyDescription();

            Select_PinkyShift.IsChecked = true;
            Select_DX_Release.IsChecked = true;

            Reset();
        }

        public static void ShowKeyMappingWindow(Window owner, DeviceControl deviceControl, KeyAssgn SelectedCallback)
        {
            KeyMappingWindow ownWindow = new KeyMappingWindow(deviceControl, SelectedCallback);
            ownWindow.Owner = owner;
            ownWindow.ShowDialog();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            getNeutralPosition();
            KeyMappingTimer.Tick += KeyMappingtimerCode;
            KeyMappingTimer.Interval = TimeSpan.FromMilliseconds(50);
            KeyMappingTimer.Start();
        }

        private void getNeutralPosition()
        {
            JoyAssgn[] joyAssgns = deviceControlRef.GetJoystickMappingsForButtonsAndHats();

            for (int i = 0; i < joyAssgns.Length; i++)
                neutralButtons[i] = new NeutralButtons(joyAssgns[i]);
        }
        private void Reset()
        {
            JoyAssgn[] joyAssgns = deviceControlRef.GetJoystickMappingsForButtonsAndHats();

            neutralButtons = new NeutralButtons[joyAssgns.Length];

            tmpJoyStick = new JoyAssgn[joyAssgns.Length];
            for (int i = 0; i < joyAssgns.Length; i++)
            {
                tmpJoyStick[i] = joyAssgns[i].MakeTempCloneForKeyMappingDialog();
            }
            tmpCallback = SelectedCallback.Clone();
        }

        private void KeyMappingtimerCode(object sender, EventArgs e)
        {
            if (sw.ElapsedMilliseconds > CommonConstants.FLUSHTIME1)
                AwaitingInputs.Content = "";
            if (sw.ElapsedMilliseconds > CommonConstants.FLUSHTIME2)
                AwaitingInputs.Content = "   AWAITING INPUTS";

            try
            {
                if (sw.ElapsedMilliseconds > CommonConstants.FLUSHTIME2)
                {
                    //REVIEW: risk of losing unsaved changes, if this happens?
                    //Microsoft.DirectX.DirectInput.DeviceList devList =
                    //Microsoft.DirectX.DirectInput.Manager.GetDevices(
                    //Microsoft.DirectX.DirectInput.DeviceClass.GameControl,
                    //Microsoft.DirectX.DirectInput.EnumDevicesFlags.AttachedOnly
                    //);
                    //
                    //if (devList.Count != MainWindow.deviceControl.joyAssign.Length)
                    //{
                    //    mainWindow.ReloadDevices();
                    //    Reset();
                    //    getNeutralPosition();
                    //}

                    sw.Reset();
                    sw.Start();
                }

                KeyboardButtonMonitor();
                JoystickButtonMonitor();
                ShowAssignedStatus();
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
            }
        }

        private void ShowAssignedStatus()
        {
            JoyAssgn[] joyAssgns = deviceControlRef.GetJoystickMappingsForButtonsAndHats();

            string str = "";
            str += tmpCallback.GetKeyAssignmentStatus() + "; ";
            if (str == "; ")
                str = "";
            for (int i = 0; i < joyAssgns.Length; i++)
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
            JoyAssgn[] joyAssgns = deviceControlRef.GetJoystickMappingsForButtonsAndHats();

            byte[] buttons;
            int[] povs;

            bool EitherOneOfShiftPressed = false;

            for (int i = 0; i < joyAssgns.Length; i++)
            {
                buttons = joyAssgns[i].GetButtons();

                for (int ii = 0; ii < CommonConstants.DX_MAX_BUTTONS; ii++)
                {
                    if (buttons[ii] == CommonConstants.PRS128 && joyAssgns[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false ||
                        buttons[ii] == CommonConstants.PRS128 && joyAssgns[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasShift"      && pressedByHand == false)
                    {
                        EitherOneOfShiftPressed = true;
                    }
                }
            }

            for (int i = 0; i < joyAssgns.Length; i++)
            {
                buttons = joyAssgns[i].GetButtons();

                for (int ii = 0; ii < CommonConstants.DX_MAX_BUTTONS; ii++)
                {
                    if (EitherOneOfShiftPressed)
                        Select_PinkyShift.IsChecked = false;
                    else
                        Select_PinkyShift.IsChecked = true;

                    if (buttons[ii] == neutralButtons[i].buttons[ii])
                        continue;

                    if (buttons[ii] == CommonConstants.PRS0)
                    {
                        if (ii + 1 < CommonConstants.DX_MAX_BUTTONS && buttons[ii + 1] == CommonConstants.PRS0)
                        {
                            getNeutralPosition();
                            continue;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (joyAssgns[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false ||
                        joyAssgns[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasShift"      && pressedByHand == false)
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
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), Pinky.UnShift, Behaviour.Press, Invoke.Default, tmpCallback.GetSoundID());
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), Pinky.Shift, Behaviour.Press, Invoke.Default, tmpCallback.GetSoundID());
                    }
                    else
                    {
                        tmpJoyStick[i].dx[ii].Assign(tmpCallback.GetCallback(), pinkyStatus, behaviourStatus, invokeStatus, tmpCallback.GetSoundID());
                    }

                    getNeutralPosition();
                    return;
                }

                povs = joyAssgns[i].GetPointOfView();
                for (int ii = 0; ii < tmpJoyStick[i].pov.Length; ii++)
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

            //QWERTY comm menu avoid.
            if ((Microsoft.DirectX.DirectInput.Key)catchedScanCode == Microsoft.DirectX.DirectInput.Key.Q && !Shift && !Ctrl && !Alt)
                return;
            if ((Microsoft.DirectX.DirectInput.Key)catchedScanCode == Microsoft.DirectX.DirectInput.Key.W && !Shift && !Ctrl && !Alt)
                return;
            if ((Microsoft.DirectX.DirectInput.Key)catchedScanCode == Microsoft.DirectX.DirectInput.Key.E && !Shift && !Ctrl && !Alt)
                return;
            if ((Microsoft.DirectX.DirectInput.Key)catchedScanCode == Microsoft.DirectX.DirectInput.Key.R && !Shift && !Ctrl && !Alt)
                return;
            if ((Microsoft.DirectX.DirectInput.Key)catchedScanCode == Microsoft.DirectX.DirectInput.Key.T && !Shift && !Ctrl && !Alt)
                return;
            if ((Microsoft.DirectX.DirectInput.Key)catchedScanCode == Microsoft.DirectX.DirectInput.Key.Y && !Shift && !Ctrl && !Alt)
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
            JoyAssgn[] joyAssgns = deviceControlRef.GetJoystickMappingsForButtonsAndHats();

            for (int i = 0; i < joyAssgns.Length; i++)
            {
                tmpJoyStick[i] = joyAssgns[i].MakeTempCloneForKeyMappingDialog();
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
                    Select_Invoke.Background = CommonConstants.GREYBLUE;
                    break;
                case Invoke.Down:
                    invokeStatus = Invoke.Up;
                    Select_Invoke.Content = "INVOKE KEYUP";
                    Select_Invoke.Background = CommonConstants.GREYBLUE;
                    break;
                case Invoke.Up:
                    invokeStatus = Invoke.Default;
                    Select_Invoke.Content = "INVOKE BOTH";
                    Select_Invoke.Background = CommonConstants.WHITEILUM;
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
            JoyAssgn[] joyAssgns = deviceControlRef.GetJoystickMappingsForButtonsAndHats();

            for (int i = 0; i < tmpJoyStick.Length; i++)
            {
                joyAssgns[i].CopyButtonsAndHatsFromCurrentProfile(tmpJoyStick[i]);
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
                    Select_Press.Background = CommonConstants.GREYBLUE;
                    invokeStatus = Invoke.Down;
                    Select_DX_Release.IsChecked = false;
                    break;
                case Press.Release:
                    pressStatus = Press.Press;
                    Select_Press.Content = "PRESS";
                    Select_Press.Background = CommonConstants.WHITEILUM;
                    invokeStatus = Invoke.Default;
                    Select_DX_Release.IsChecked = true;
                    break;
            }
        }
    }
}
