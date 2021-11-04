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
    /// CallsignWindow.xaml
    /// 
    ///  if the user launches BMS the first time after installing it, Viper.pop will be created and it defines FULL.key as a profile.
    ///  AL can edit<callsign>.pop and change the selected key file to any other file.like: “BMS - AUTO.key” for AL use.I tried it and it worked.
    ///  However, AL can rewrite key files, pop files, and axis settings only before launching BMS
    ///  This means the first time we launch BMS with AL, AL doesn’t have a pop file to edit.
    ///  Therefore AL can not provide a proper joystick setup for the first installation of BMS
    ///  So AL can not work properly for their first setup of BMS and it should make “Huh? why it’s not working” troubles.
    ///  I also tried generating pop files by AL but somehow it didn’t work well, maybe some format I tried to generate was not correct, and BMS recreate fresh <callsign>.pop that looks for FULL.key
    /// 
    ///  I found without LBK file it reverts callsign to "Viper".
    ///  LBK file has some sort of cryptograph so I can't just rename Viper.lbk to create unique callsign.lbk.
    /// 
    /// </summary>
    public partial class CallsignWindow
    {
        private AppRegInfo appReg;
        public CallsignWindow(AppRegInfo appReg)
        {
            this.appReg = appReg;
            InitializeComponent();
        }

        public static bool ShowCallsignWindow(AppRegInfo appReg)
        {
            CallsignWindow ownWindow = new CallsignWindow(appReg);
            ownWindow.ShowDialog();
            return (ownWindow.TextBox_Callsign.Text == "Viper") || (ownWindow.TextBox_PilotName.Text == "Joe Pilot");
        }

        private void Callsign_Changed(object sender, TextChangedEventArgs e)
        {
            if (TextBox_Callsign.Text != "Viper")
                Label_Error_Callsign.Visibility = Visibility.Collapsed;
            if (TextBox_Callsign.Text.Length > 12)
            {
                TextBox_Callsign.Text = TextBox_Callsign.Text.Remove(12);
            }
            TextBox_Callsign.Text = Regex.Replace(TextBox_Callsign.Text, "[^A-Z|a-z|0-9|~|`|\\[|\\]|\\{|\\}|\\-|_|\\=|\\'|\\s]", String.Empty);
        }

        private void PilotName_Changed(object sender, TextChangedEventArgs e)
        {
            if (TextBox_PilotName.Text != "Joe Pilot")
                Label_Error_PilotName.Visibility = Visibility.Collapsed;
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
            Label_Error_Callsign.Visibility = Visibility.Collapsed;
            Label_Error_PilotName.Visibility = Visibility.Collapsed;
        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        public static bool IsAlphabetOrNumber(char c)
        {
            return (c >= 'A' && c <= 'z') || (c >= '0' && c <= '9') ? true : false;
        }

        private void Button_Register_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Callsign.Text == "Viper")
            {
                Label_Error_Callsign.Visibility = Visibility.Visible;
                if (TextBox_PilotName.Text == "Joe Pilot")
                {
                    Label_Error_PilotName.Visibility = Visibility.Visible;
                    return;
                }
                return;
            }
            if (TextBox_PilotName.Text == "Joe Pilot")
            {
                Label_Error_PilotName.Visibility = Visibility.Visible;
                return;
            }
            appReg.ChangeName(TextBox_Callsign.Text, TextBox_PilotName.Text);
            Close();
        }
    }
}
