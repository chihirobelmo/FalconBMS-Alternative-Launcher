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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

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

        public static void ShowCallsignWindow(AppRegInfo appReg)
        {
            CallsignWindow ownWindow = new CallsignWindow(appReg);
            ownWindow.ShowDialog();
            return;
        }

        private void Callsign_Changed(object sender, TextChangedEventArgs e)
        {
            if (TextBox_Callsign.Text != CommonConstants.DEFAULTCALLSIGN)
                Label_Error_Callsign.Visibility = Visibility.Collapsed;
            if (TextBox_Callsign.Text.Length > 12)
            {
                TextBox_Callsign.Text = TextBox_Callsign.Text.Remove(12);
            }
            TextBox_Callsign.Text = Regex.Replace(TextBox_Callsign.Text, "[^A-Z|a-z|0-9|~|`|\\[|\\]|\\{|\\}|\\-|_|\\=|\\'|\\s]", String.Empty);
        }

        private void PilotName_Changed(object sender, TextChangedEventArgs e)
        {
            if (TextBox_PilotName.Text != CommonConstants.DEFAULTPILOTNAME)
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

        private void Button_Register_Click(object sender, RoutedEventArgs e)
        {
            string pilotName = TextBox_PilotName.Text.Trim();
            string pilotCallsign = TextBox_Callsign.Text.Trim();

            bool ok = true;
            if (0 == String.Compare(pilotCallsign, CommonConstants.DEFAULTCALLSIGN, StringComparison.OrdinalIgnoreCase))
            {
                Label_Error_Callsign.Visibility = Visibility.Visible;
                ok = false;
            }
            if (0 == String.Compare(pilotName, CommonConstants.DEFAULTPILOTNAME, StringComparison.OrdinalIgnoreCase))
            {
                Label_Error_PilotName.Visibility = Visibility.Visible;
                ok = false;
            }
            if (!ok) return;

            appReg.ChangeName(pilotCallsign, pilotName);

            // Launch external tool to create logbook (lbk) file.
            const char dq = '\"';
            string pilotNameDQ = $"{dq}{pilotName}{dq}";
            string pilotCallsignDQ = $"{dq}{pilotCallsign}{dq}";
            string configFolder = appReg.GetInstallDir() + CommonConstants.CONFIGFOLDERBACKSLASH;

            string lbkPath = System.IO.Path.Combine(configFolder, pilotCallsign + ".lbk");
            string lbkPathDQ = $"{dq}{lbkPath}{dq}";

            // Bugfix: don't overwrite existing lbk! This situation can happen for variety of reasons.. after changing
            // current pilot callsign in-game.. or after a fresh reinstall, and copying over Callsign.pop, ini and lbk
            if (File.Exists(lbkPath))
                return;

            string command = $"-o {lbkPathDQ} write-default --name {pilotNameDQ} --callsign {pilotCallsignDQ}";
            //string command = 
            //    "-o \"" 
            //    + appReg.GetInstallDir() 
            //    + CommonConstants.CONFIGFOLDERBACKSLASH 
            //    + TextBox_Callsign.Text 
            //    + ".lbk\" write-default --name \"" 
            //    + TextBox_PilotName.Text 
            //    + "\" --callsign \"" 
            //    + TextBox_Callsign.Text 
            //    + "\"";
            Diagnostics.Log(command);

            string thisExeDirectory = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string logcatExePath = System.IO.Path.Combine(thisExeDirectory, CommonConstants.LOGCAT);
            
            if (!File.Exists(logcatExePath))
            {
                Diagnostics.Log("Could not find bms-logcat.exe", Diagnostics.LogLevels.Warning);
                Close();
                return;
            }

            Process logcatExe = Process.Start(CommonConstants.LOGCAT, command);
            logcatExe.WaitForExit();

            if (logcatExe.ExitCode != 0)
                Diagnostics.Log("Error code returned from bms-logcat.exe: " +logcatExe.ExitCode, Diagnostics.LogLevels.Warning);

            Close();
            return;
        }
    }
}
