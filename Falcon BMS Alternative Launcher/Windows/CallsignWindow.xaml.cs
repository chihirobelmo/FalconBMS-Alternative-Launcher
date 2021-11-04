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
using System.Text.RegularExpressions;

using FalconBMS.Launcher.Input;

namespace FalconBMS.Launcher.Windows
{
    /// <summary>
    /// CallsignWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CallsignWindow
    {
        public CallsignWindow()
        {
            InitializeComponent();
        }

        public static void ShowCallsignWindow()
        {
            CallsignWindow ownWindow = new CallsignWindow();
            ownWindow.ShowDialog();
        }

        private void Callsign_Changed(object sender, TextChangedEventArgs e)
        {
            if (TextBox_Callsign.Text.Length > 12)
            {
                TextBox_Callsign.Text = TextBox_Callsign.Text.Remove(12);
            }
            TextBox_Callsign.Text = Regex.Replace(TextBox_Callsign.Text, "[^A-Z|a-z|0-9|~|`|\\[|\\]|\\{|\\}|\\-|_|\\=|\\'|\\s]", String.Empty);
        }

        private void PilotName_Changed(object sender, TextChangedEventArgs e)
        {
            if (TextBox_PilotName.Text.Length > 20)
            {
                TextBox_PilotName.Text = TextBox_PilotName.Text.Remove(20);
            }
        }

        private void AssignWindow_Closed(object sender, EventArgs e)
        {

        }

        private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void AssignWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Register_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        public static bool IsAlphabetOrNumber(char c)
        {
            return (c >= 'A' && c <= 'z') || (c >= '0' && c <= '9') ? true : false;
        }
    }
}
