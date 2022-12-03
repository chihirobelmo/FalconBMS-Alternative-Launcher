using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using FalconBMS.Launcher.Input;

using Microsoft.DirectX.DirectInput;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Let's write DataGrid cells at KeyMapping page a keyfile informarion.
        /// </summary>
        public void WriteDataGrid()
        {
            foreach (KeyAssgn Assgn in keyFile.keyAssign)
                Assgn.Visibility = Assgn.GetVisibility();

            //string target = "MFD";

            //foreach (KeyAssgn Assgn in keyFile.keyAssign)
            //{
            //    if (Assgn.Mapping.Trim().Contains(target))
            //        Assgn.Visibility = Assgn.GetVisibility();
            //    else
            //    {
            //        Assgn.Visibility = "Hidden";
            //    }
            //}

            KeyMappingGrid.ItemsSource = keyFile.keyAssign;
        }
        public void RefreshJoystickColumn()
        {
            KeyMappingGrid.Items.Refresh();

            statusAssign = Status.GetNeutralPos;
        }

        /// <summary>
        /// Initialize Datagrid Columns.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Mapping":
                    e.Column.Header = "Mapping";
                    e.Column.DisplayIndex = 0;
                    break;
                case "Key":
                    e.Column.Header = "Key";
                    e.Column.DisplayIndex = 1;
                    break;
                case "Visibility":
                    // Do not show
                    e.Column.DisplayIndex = 2;
                    e.Cancel = true;
                    break;
            }
            Category.SelectedIndex = 0;
            if (!e.PropertyName.Contains("Z_Joy_"))
                return;
            int target = int.Parse(e.PropertyName.Replace("Z_Joy_", ""));
            if (target >= deviceControl.joyAssign.Length)
            {
                e.Cancel = true;
                return;
            }
            e.Column.Header = deviceControl.joyAssign[target].GetProductName();
            //e.Column.Width = 128;
            e.Column.DisplayIndex = 3 + target;
        }
        
        /// <summary>
        /// Unassign keyboard key or joystick button when double clicked a Datagrid cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_MouseButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
            KeyMappingTimer.Stop();
            NewDeviceDetectTimer.Stop();

            KeyAssgn selectedItem = (KeyAssgn)KeyMappingGrid.SelectedItem;
            if (selectedItem == null)
                return;
            if (KeyMappingGrid.CurrentColumn == null)
                return;
            if (selectedItem.GetVisibility() != "White")
                return;
            if (selectedItem.GetCallback() == "SimDoNothing")
                return;

            KeyMappingWindow.ShowKeyMappingWindow(this, selectedItem, keyFile, deviceControl, sender);
            ResortDevices();

            KeyMappingGrid.Items.Refresh();
            KeyMappingGrid.UnselectAllCells();

            NewDeviceDetectTimer.Start();
            KeyMappingTimer.Start();
        }
        
        private byte[] buttons;
        private int[] povs;
        private NeutralButtons[] neutralButtons;

        /// <summary>
        /// What was your joystick buttons were like when you clicked somewhere? Are they pressed or released?
        /// </summary>
        public class NeutralButtons
        {
            public byte[] buttons { get; set; }
            public int[] povs { get; set; }

            public NeutralButtons(JoyAssgn joyStick)
            {
                buttons = joyStick.GetButtons();
                povs = joyStick.GetPointOfView();
            }
        }
        
        /// <summary>
        /// Is KeyMapping page currently trying to get neutral button positions? or waiting for your input?
        /// </summary>
        private Status statusAssign = Status.GetNeutralPos;
        private enum Status
        {
            GetNeutralPos = 0,
            WaitingforInput = 1
        }

        /// <summary>
        /// Check your keyboard/joysticks button behaviour every 60 frames per seconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void KeyMappingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                directInputDevice.GetCurrentKeyboardState();
                for (int i = 1; i < 238; i++)
                    if (directInputDevice.KeyboardState[(Microsoft.DirectX.DirectInput.Key)i])
                        KeyMappingGrid_KeyDown();
                JumptoAssignedKey();
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                System.IO.StreamWriter sw = new System.IO.StreamWriter(appReg.GetInstallDir() + "\\Error.txt", false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(ex.Message);
                sw.Close();

                MessageBox.Show("Error Log Saved To " + appReg.GetInstallDir() + "\\Error.txt", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information);

                Close();
            }
        }

        /// <summary>
        /// You pressed a joystick button to search which callback is it assigned to? OK let's go there.
        /// </summary>
        public void JumptoAssignedKey()
        {
            string target = "";
            switch (statusAssign)
            {
                case Status.GetNeutralPos:
                    for (int i = 0; i < deviceControl.joyAssign.Length; i++)
                        neutralButtons[i] = new NeutralButtons(deviceControl.joyAssign[i]);
                    statusAssign = Status.WaitingforInput;
                    break;
                case Status.WaitingforInput:

                    bool EitherOneOfShiftPressed = false;

                    for (int i = 0; i < deviceControl.joyAssign.Length; i++)
                    {
                        buttons = deviceControl.joyAssign[i].GetButtons();
                        for (int ii = 0; ii < CommonConstants.DX128; ii++)
                        {
                            if (buttons[ii] == CommonConstants.PRS128 && deviceControl.joyAssign[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false ||
                                buttons[ii] == CommonConstants.PRS128 && deviceControl.joyAssign[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasShift" && pressedByHand == false)
                                EitherOneOfShiftPressed = true;
                        }
                    }

                    for (int i = 0; i < deviceControl.joyAssign.Length; i++)
                    {
                        buttons = deviceControl.joyAssign[i].GetButtons();
                        for (int ii = 0; ii < CommonConstants.DX128; ii++)
                        {
                            if (EitherOneOfShiftPressed)
                                Select_PinkyShift.IsChecked = false;
                            else
                                Select_PinkyShift.IsChecked = true;

                            if (neutralButtons[i] == null)
                                break; // KEY SEARCH WILL NOT WORK IF ENTER HERE
                            if (buttons[ii] == neutralButtons[i].buttons[ii])
                                continue;
                            statusAssign = Status.GetNeutralPos;
                            if (buttons[ii] == CommonConstants.PRS0) // RELEASE SHIFT
                                Select_DX_Release.IsChecked = false;
                            else
                                Select_DX_Release.IsChecked = true;

                            if (deviceControl.joyAssign[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasPinkyShift" && pressedByHand == false ||
                                deviceControl.joyAssign[i].dx[ii].assign[CommonConstants.DX_PRESS].GetCallback() == "SimHotasShift"      && pressedByHand == false)
                                continue;

                            Pinky pinkyStatus = Pinky.UnShift;
                            Behaviour behaviourStatus = Behaviour.Press;
                            if (Select_PinkyShift.IsChecked == false)
                                pinkyStatus = Pinky.Shift;
                            if (Select_DX_Release.IsChecked == false)
                                behaviourStatus = Behaviour.Release;

                            target = deviceControl.joyAssign[i].dx[ii].assign[(int)pinkyStatus + (int)behaviourStatus].GetCallback();
                            if (target == "SimDoNothing" && behaviourStatus == Behaviour.Release)
                            { }
                            else
                            {
                                Label_AssgnStatus.Content = "DX" + (ii + 1) + "\t: " + deviceControl.joyAssign[i].GetProductName();
                            }
                        }
                        povs = deviceControl.joyAssign[i].GetPointOfView();
                        for (int ii = 0; ii < 4; ii++)
                        {
                            if (neutralButtons[i] == null)
                                break; // KEY SEARCH WILL NOT WORK IF ENTER HERE
                            if (povs[ii] == neutralButtons[i].povs[ii])
                                continue;
                            statusAssign = Status.GetNeutralPos;
                            if (povs[ii] == -1)
                                continue;
                            
                            Pinky pinkyStatus = Pinky.UnShift;
                            if (Select_PinkyShift.IsChecked == false)
                                pinkyStatus = Pinky.Shift;

                            target = deviceControl.joyAssign[i].pov[ii].direction[povs[ii] / CommonConstants.POV45].GetCallback(pinkyStatus);

                            string direction = deviceControl.joyAssign[i].pov[ii].GetDirection(povs[ii]);
                            Label_AssgnStatus.Content = "POV" + (ii + 1) + "." + direction + "\t: " + deviceControl.joyAssign[i].GetProductName();
                        }
                    }
                    break;
            }
            if (target == "")
                return;
            if (target == "SimDoNothing")
                return;
            // If the key assignment was found, jump to the mapping for it and highlight it.
            KeyAssgn key = keyFile.keyAssign.FirstOrDefault(x => x.GetCallback() == target);
            if (key != null)
            {
                Label_AssgnStatus.Content += "   / " + key.Mapping;

                KeyMappingGrid.ScrollIntoView(key);
                KeyMappingGrid.SelectedIndex = KeyMappingGrid.Items.IndexOf(key);
            }
        }

        /// <summary>
        /// You pressed keyboard keys? I will check which key was pressed with Shift/Ctrl/Alt.
        /// </summary>
        private void KeyMappingGrid_KeyDown()
        {
            if (SearchBox.IsSelectionActive)
                return;
            if (SearchBox.IsFocused)
                return;
            if (SearchBox.IsKeyboardFocused)
                return;

            bool Shift = false;
            bool Ctrl = false;
            bool Alt = false;

            int catchedScanCode = 0;

            directInputDevice.GetCurrentKeyboardState();

            for (int i = 1; i < CommonConstants.KEYBOARD_KEYLENGTH; i++)
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
            if (Select_PinkyShift.IsChecked == false)
                return;

            KeyAssgn keytmp = new KeyAssgn("SimDoNothing - 1 0 0XFFFFFFFF 0 0 0 - 1 \"nothing\"");
            keytmp.SetKeyboard(catchedScanCode, Shift, Ctrl, Alt);
            Label_AssgnStatus.Content = "INPUT " + keytmp.GetKeyAssignmentStatus();

            // If the key assignment was found, jump to the mapping for it and highlight it.
            KeyAssgn key = keyFile.keyAssign.FirstOrDefault(x => x.GetKeyAssignmentStatus() == keytmp.GetKeyAssignmentStatus());
            if (key != null)
            {
                Label_AssgnStatus.Content += "\t/" + key.Mapping;

                KeyMappingGrid.UpdateLayout();
                KeyMappingGrid.ScrollIntoView(key);
                KeyMappingGrid.SelectedIndex = KeyMappingGrid.Items.IndexOf(key);
            }
        }

        /// <summary>
        /// So this was... keyboard, I suppose.
        /// </summary>
        DirectInputKeyboard directInputDevice = new DirectInputKeyboard();
        class DirectInputKeyboard
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

        /// <summary>
        /// Invoke Status.
        /// </summary>
        private Invoke invokeStatus = Invoke.Default;
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

        /// <summary>
        /// Let's jump to a category you have selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string target = "";
            switch (Category.SelectedIndex)
            {
                case 0:
                    target = "BMS - Full";
                    break;
                case 1:
                    target = "1. UI & 3RD PARTY SOFTWARE";
                    break;
                case 2:
                    target = "2. LEFT CONSOLE";
                    break;
                case 3:
                    target = "======== 2.19     THROTTLE QUADRANT SYSTEM ========";
                    break;
                case 4:
                    target = "3. LEFT AUX CONSOLE";
                    break;
                case 5:
                    target = "4. CENTER CONSOLE";
                    break;
                case 6:
                    target = "======== 4.05     LEFT MFD ========";
                    break;
                case 7:
                    target = "======== 4.10     RIGHT MFD ========";
                    break;
                case 8:
                    target = "5. RIGHT CONSOLE";
                    break;
                case 9:
                    target = "======== 5.11     FLIGHT STICK  ========";
                    break;
                case 10:
                    target = "6. MISCELLANEOUS";
                    break;
                case 11:
                    target = "7. VIEWS";
                    break;
                case 12:
                    target = "8. RADIO COMMS";
                    break;
            }

            int i = 0;
            foreach (KeyAssgn keys in keyFile.keyAssign)
            {
                if (keys.Mapping.Trim() == target)
                {
                    KeyMappingGrid.ScrollIntoView(KeyMappingGrid.Items[KeyMappingGrid.Items.Count - 1]);
                    KeyMappingGrid.UpdateLayout();
                    KeyMappingGrid.ScrollIntoView(KeyMappingGrid.Items[i]);
                }
                i += 1;
            }
        }

        /// <summary>
        /// User has changed the text in the search box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            KeyMappingGrid.UnselectAllCells();
            string filter = SearchBox.Text.Trim().ToLower();
            bool isFilterEmpty = string.IsNullOrWhiteSpace(filter);

            // Enable the category selector only if there's no search filter active.
            Category.IsEnabled = isFilterEmpty;

            KeyMappingGrid.Items.Filter = x => isFilterEmpty || ((KeyAssgn) x).Mapping.Trim().ToLower().Contains(filter);
        }
    }
}