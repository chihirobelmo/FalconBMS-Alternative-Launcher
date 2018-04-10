using MahApps.Metro.Controls;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FalconBMS_Alternative_Launcher_Cs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private KeyAssgn[] keyAssign;

        public void ReadKeyFile(string Filename)
        {
            string stParentName = System.IO.Path.GetDirectoryName(Filename);

            if (File.Exists(Filename) == false)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show
                    ("App could not find " + Filename, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.OK)
                    System.Windows.Application.Current.Shutdown();
                return;
            }

            string[] lines = File.ReadAllLines(Filename,Encoding.UTF8);

            keyAssign = new KeyAssgn[lines.Length];

            int i = 0;
            foreach (string stBuffer in lines)
            {
                string[] stArrayData = stBuffer.Split(' ');

                if (stArrayData.Length < 7)
                    continue;
                if (stBuffer.Substring(0, 1) == "#")
                    continue;
                if (stArrayData[3] == "-2" | stArrayData[3] == "-3")
                    continue;

                keyAssign[i] = new KeyAssgn(stBuffer);

                if (keyAssign[i].CheckFileCollapsing() == true)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show
                        ("App found BMS - FULL.key broken\nWould you like to restore it to the default?", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                    if (result == MessageBoxResult.OK)
                    {
                        string fnamestock = appReg.GetInstallDir() + "\\Docs\\Key Files & Input\\BMS - Full.key";
                        string fname = appReg.GetInstallDir() + "\\User\\Config\\BMS - Full.key";
                        if (File.Exists(fnamestock) == true)
                        {
                            System.IO.File.Copy(fnamestock, fname, true);
                            Application.Current.Shutdown();
                            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                            Array.Resize(ref keyAssign, i);
                            return;
                        }
                        else
                        {
                            MessageBoxResult result2 = System.Windows.MessageBox.Show
                                ("App could not find BMS - FULL.key at\nDocs\\Key Files & Input\\BMS - Full.key", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            if (result == MessageBoxResult.OK)
                            {
                                System.Windows.Application.Current.Shutdown();
                                Array.Resize(ref keyAssign, i);
                                return;
                            }
                        }
                    }
                    else
                    {
                        System.Windows.Application.Current.Shutdown();
                        Array.Resize(ref keyAssign, i);
                        return;
                    }
                }

                i += 1;
            }
            Array.Resize(ref keyAssign, i);
        }
        




        public void WriteDataGrid()
        {
            foreach (KeyAssgn Assgn in keyAssign)
                Assgn.Visibility = Assgn.GetVisibility();

            //string target = "MFD";

            //foreach (KeyAssgn Assgn in keyAssign)
            //{
            //    if (Assgn.Mapping.Trim().Contains(target))
            //        Assgn.Visibility = Assgn.GetVisibility();
            //    else
            //    {
            //        Assgn.Visibility = "Hidden";
            //    }
            //}

            this.KeyMappingGrid.ItemsSource = keyAssign;
        }





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
            if (target >= devList.Count)
            {
                e.Cancel = true;
                return;
            }
            e.Column.Header = joyAssign[target].GetProductName();
            e.Column.Width = 128;
            e.Column.DisplayIndex = 3 + target;
        }






        private void DataGrid_MouseButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
            KeyMappingGrid.ScrollIntoView(KeyMappingGrid.Items[currentIndex]);
            KeyMappingGrid.SelectedIndex = currentIndex;
            if (KeyMappingGrid.CurrentColumn == null)
                return;
            int Rows = KeyMappingGrid.SelectedIndex;
            int Columns = KeyMappingGrid.CurrentColumn.DisplayIndex;

            if (Columns == 1)
            {
                if (Rows < 0)
                    return;
                if (keyAssign[currentIndex].Visibility != "White")
                    return;
                keyAssign[currentIndex].UnassignKeyboard();
            }
            if (Columns > 1)
            {
                if (Rows < 0)
                    return;
                string target = keyAssign[currentIndex].GetCallback();
                joyAssign[Columns - 3].UnassigntargetCallback(target);
            }
            KeyMappingGrid.Items.Refresh();
            KeyMappingGrid.UnselectAllCells();
            statusSearch = Search.Search;
        }
        
        private byte[] buttons;
        private int[] povs;
        private NeutralButtons[] neutralButtons;

        public class NeutralButtons
        {
            public byte[] buttons { get; set; }
            public int[] povs { get; set; }

            public NeutralButtons(Device joyStick)
            {
                this.buttons = joyStick.CurrentJoystickState.GetButtons();
                this.povs = joyStick.CurrentJoystickState.GetPointOfView();
            }
        }
        
        private Status statusAssign = Status.GetNeutralPos;
        private enum Status
        {
            GetNeutralPos = 0,
            WaitingforInput = 1
        }
        private Search statusSearch = Search.Search;
        private enum Search
        {
            Assign = 0,
            Search = 1
        }

        private void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            statusSearch = Search.Assign;
            Label_AssgnStatus.Content = "AWAITING INPUTS";
        }

        private void KeyMappingGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            KeyMappingGrid.UnselectAllCells();
            statusSearch = Search.Search;
            Label_AssgnStatus.Content = "KEYSEARCH MODE";
        }

        public void KeyMappingTimer_Tick(object sender, EventArgs e)
        {
            directInputDevice.GetCurrentKeyboardState();
            for (int i = 1; i < 238; i++)
                if (directInputDevice.KeyboardState[(Microsoft.DirectX.DirectInput.Key)i])
                    KeyMappingGrid_KeyDown();

            int Rows = KeyMappingGrid.SelectedIndex;
            if (Rows == -1 | statusSearch == Search.Search)
            {
                JumptoAssignedKey();
                return;
            }
            if (KeyMappingGrid.CurrentColumn == null)
                return;
            if (keyAssign[Rows].GetVisibility() != "White")
                return;


            switch (statusAssign)
            {
                case Status.GetNeutralPos:
                    for (int i = 0; i < devList.Count; i++)
                        neutralButtons[i] = new NeutralButtons(joyStick[i]);
                    statusAssign = Status.WaitingforInput;
                    break;
                case Status.WaitingforInput:
                    for (int i = 0; i < devList.Count; i++)
                    {
                        buttons = joyStick[i].CurrentJoystickState.GetButtons();
                        for (int ii = 0; ii < 32; ii++)
                        {
                            if (buttons[ii] == neutralButtons[i].buttons[ii])
                                continue;
                            statusAssign = Status.GetNeutralPos;
                            if (buttons[ii] == 0)
                                continue;
                            
                            Pinky pinkyStatus = Pinky.UnShift;
                            Behaviour behaviourStatus = Behaviour.Press;
                            if (Select_PinkyShift.IsChecked == false)
                                pinkyStatus = Pinky.Shift;
                            if (Select_DX_Release.IsChecked == false)
                                behaviourStatus = Behaviour.Release;

                            // Construct DX button instance.
                            joyAssign[i].dx[ii].Assign(keyAssign[Rows].GetCallback(), pinkyStatus, behaviourStatus, invokeStatus, 0);

                            KeyMappingGrid.Items.Refresh();
                            KeyMappingGrid.UnselectAllCells();
                        }
                        povs = joyStick[i].CurrentJoystickState.GetPointOfView();
                        for (int ii = 0; ii < 4; ii++)
                        {
                            if (povs[ii] == neutralButtons[i].povs[ii])
                                continue;
                            statusAssign = Status.GetNeutralPos;
                            if (povs[ii] == -1)
                                continue;

                            Pinky pinkyStatus = Pinky.UnShift;
                            if (Select_PinkyShift.IsChecked == false)
                                pinkyStatus = Pinky.Shift;

                            // Construct POV button instance.
                            joyAssign[i].pov[ii].Assign(povs[ii], keyAssign[Rows].GetCallback(), pinkyStatus, 0);

                            KeyMappingGrid.Items.Refresh();
                            KeyMappingGrid.UnselectAllCells();
                        }
                    }
                    break;
            }
        }

        public void JumptoAssignedKey()
        {
            string target = "";
            switch (statusAssign)
            {
                case Status.GetNeutralPos:
                    for (int i = 0; i < devList.Count; i++)
                        neutralButtons[i] = new NeutralButtons(joyStick[i]);
                    statusAssign = Status.WaitingforInput;
                    break;
                case Status.WaitingforInput:
                    for (int i = 0; i < devList.Count; i++)
                    {
                        buttons = joyStick[i].CurrentJoystickState.GetButtons(); //Microsoft.DirectX.DirectInput.InputLostException: 'アプリケーションでエラーが発生しました。'
                        for (int ii = 0; ii < 32; ii++)
                        {
                            if (buttons[ii] == neutralButtons[i].buttons[ii])
                                continue;
                            statusAssign = Status.GetNeutralPos;
                            if (buttons[ii] == 0) // RELEASE SHIFT
                                continue;
                            
                            Pinky pinkyStatus = Pinky.UnShift;
                            Behaviour behaviourStatus = Behaviour.Press;
                            if (Select_PinkyShift.IsChecked == false)
                                pinkyStatus = Pinky.Shift;
                            if (Select_DX_Release.IsChecked == false)
                                behaviourStatus = Behaviour.Release;

                            target = joyAssign[i].dx[ii].assign[(int)pinkyStatus + (int)behaviourStatus].GetCallback();

                            Label_AssgnStatus.Content = "DX" + (ii+1) + "\t: " + joyAssign[i].GetProductName();
                        }
                        povs = joyStick[i].CurrentJoystickState.GetPointOfView();
                        for (int ii = 0; ii < 4; ii++)
                        {
                            if (povs[ii] == neutralButtons[i].povs[ii])
                                continue;
                            statusAssign = Status.GetNeutralPos;
                            if (povs[ii] == -1)
                                continue;
                            
                            Pinky pinkyStatus = Pinky.UnShift;
                            if (Select_PinkyShift.IsChecked == false)
                                pinkyStatus = Pinky.Shift;

                            target = joyAssign[i].pov[ii].direction[povs[ii] / 4500].GetCallback(pinkyStatus);

                            string direction = joyAssign[i].pov[ii].GetDirection(povs[ii]);
                            Label_AssgnStatus.Content = "POV" + (ii + 1) + "." + direction + "\t: " + joyAssign[i].GetProductName();
                        }
                    }
                    break;
            }
            
            if (target == "")
                return;
            if (target == "SimDoNothing")
                return;
            for (int i = 0; i < keyAssign.Length; i++)
            {
                if (keyAssign[i].GetCallback() == target)
                {
                    Label_AssgnStatus.Content += "   / " + keyAssign[i].Mapping;

                    KeyMappingGrid.UpdateLayout();
                    KeyMappingGrid.ScrollIntoView(KeyMappingGrid.Items[i]);
                    KeyMappingGrid.SelectedIndex = i;
                    statusSearch = Search.Search;
                }
            }
        }

        private void KeyMappingGrid_KeyDown()
        {
            if (currentIndex < 0)
            {
                currentIndex = 1;
                statusSearch = Search.Search;
                return;
            }
            if (SearchBox.IsSelectionActive == true)
                return;
            if (SearchBox.IsFocused == true)
                return;
            if (SearchBox.IsKeyboardFocused == true)
                return;

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
            if (statusSearch == Search.Search)
            {
                KeyAssgn keytmp = new KeyAssgn("SimDoNothing - 1 0 0XFFFFFFFF 0 0 0 - 1 \"nothing\"");
                keytmp.SetKeyboard(catchedScanCode, Shift, Ctrl, Alt);
                Label_AssgnStatus.Content = "INPUT " + keytmp.GetKeyAssignmentStatus();
                for (int i = 0; i < keyAssign.Length; i++)
                {
                    if (keytmp.GetKeyAssignmentStatus() != keyAssign[i].GetKeyAssignmentStatus())
                        continue;

                    Label_AssgnStatus.Content += "\t/" + keyAssign[i].Mapping;

                    KeyMappingGrid.UpdateLayout();
                    KeyMappingGrid.ScrollIntoView(KeyMappingGrid.Items[i]);
                    KeyMappingGrid.SelectedIndex = i;
                }
                return;
            }
            if (KeyMappingGrid.SelectedIndex == -1)
                return;
            if (keyAssign[currentIndex].GetVisibility() != "White")
                return;

            Pinky pinkyStatus = Pinky.UnShift;
            if (Select_PinkyShift.IsChecked == false)
                pinkyStatus = Pinky.Shift;

            KeyMappingGrid.ScrollIntoView(KeyMappingGrid.Items[currentIndex]);
            KeyMappingGrid.SelectedIndex = currentIndex;
            if (pinkyStatus == Pinky.UnShift)
                keyAssign[currentIndex].SetKeyboard(catchedScanCode, Shift, Ctrl, Alt);
            if (pinkyStatus == Pinky.Shift)
                keyAssign[currentIndex].Setkeycombo(catchedScanCode, Shift, Ctrl, Alt);

            for (int i = 0; i < keyAssign.Length; i++)
            {
                if (keyAssign[i].GetKeyAssignmentStatus() != keyAssign[currentIndex].GetKeyAssignmentStatus())
                    continue;
                if (i == currentIndex)
                    continue;
                if (keyAssign[i].GetVisibility() != "White")
                    continue;
                keyAssign[i].UnassignKeyboard();
            }

            KeyMappingGrid.Items.Refresh();
            KeyMappingGrid.UnselectAllCells();
            statusSearch = Search.Search;
        }

        DirectInputKeyboard directInputDevice = new DirectInputKeyboard();

        class DirectInputKeyboard
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
            foreach (KeyAssgn keys in keyAssign)
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



        
        
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Keyboard.ClearFocus();

            if (SearchBox.Text == "")
                return;

            KeyMappingGrid.UnselectAllCells();
            KeyMappingGrid.ItemsSource = null;

            //if (SearchBox.Text == "")
            //{
            //    foreach (KeyAssgn Assgn in keyAssign)
            //        Assgn.Visibility = Assgn.GetVisibility();
            //    KeyMappingGrid.Items.Refresh();
            //    KeyMappingGrid.UnselectAllCells();
            //    return;
            //}

            string target = SearchBox.Text;

            //foreach (KeyAssgn Assgn in keyAssign)
            //{
            //    if (Assgn.Mapping.Trim().Contains(target))
            //        Assgn.Visibility = Assgn.GetVisibility();
            //    else
            //    {
            //        Assgn.Visibility = "Hidden";
            //    }
            //}
            
            this.KeyMappingGrid.ItemsSource = keyAssign;
            KeyMappingGrid.Items.Refresh();

            int i = 0;
            foreach (KeyAssgn keys in keyAssign)
            {
                if (keys.Mapping.Trim().Contains(target))
                {
                    KeyMappingGrid.ScrollIntoView(KeyMappingGrid.Items[KeyMappingGrid.Items.Count - 1]);
                    KeyMappingGrid.UpdateLayout();
                    KeyMappingGrid.ScrollIntoView(KeyMappingGrid.Items[i]);

                    return;
                }
                i += 1;
            }
        }

        private int currentIndex;
        private void KeyMappingGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentIndex = KeyMappingGrid.SelectedIndex;
        }
    }
}