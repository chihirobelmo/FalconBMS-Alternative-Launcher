using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class AppProperties
    {
        private MainWindow mainWindow;
        public int bandWidthDefault = 1024;

        public AppProperties(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // Load Buttons
            mainWindow.Misc_Platform.IsChecked             = Properties.Settings.Default.Platform;
            mainWindow.CMD_ACMI.IsChecked                  = Properties.Settings.Default.CMD_ACMI;
            mainWindow.CMD_WINDOW.IsChecked                = Properties.Settings.Default.CMD_WINDOW;
            mainWindow.CMD_NOMOVIE.IsChecked               = Properties.Settings.Default.CMD_NOMOVIE;
            mainWindow.CMD_EF.IsChecked                    = Properties.Settings.Default.CMD_EF;
            mainWindow.CMD_MONO.IsChecked                  = Properties.Settings.Default.CMD_MONO;
            this.bandWidthDefault                          = Properties.Settings.Default.CMD_BW;
            mainWindow.ApplicationOverride.IsChecked       = Properties.Settings.Default.NoOverride;
            mainWindow.Misc_RollLinkedNWS.IsChecked        = Properties.Settings.Default.Misc_RLNWS;
            mainWindow.Misc_MouseCursorAnchor.IsChecked    = Properties.Settings.Default.Misc_MouseCursorAnchor;
            mainWindow.Misc_TrackIRZ.IsChecked             = Properties.Settings.Default.Misc_TrackIRZ;
            mainWindow.Misc_ExMouseLook.IsChecked          = Properties.Settings.Default.Misc_ExMouseLook;
            mainWindow.Misc_OverrideSelfCancel.IsChecked   = Properties.Settings.Default.Misc_OverrideSelfCancel;
            mainWindow.Misc_SmartScalingOverride.IsChecked = Properties.Settings.Default.Misc_SmartScalingOverride;

            // Button Status Default
            mainWindow.Select_DX_Release.IsChecked  = true;
            mainWindow.Select_PinkyShift.IsChecked  = true;
            mainWindow.CMD_BW.Content               = "BW : " + this.bandWidthDefault.ToString();
            mainWindow.AB_Throttle.Visibility       = Visibility.Hidden;
            mainWindow.AB_Throttle_Right.Visibility = Visibility.Hidden;
        }

        public void SaveUISetup()
        {
            Properties.Settings.Default.Platform                  = (bool)mainWindow.Misc_Platform.IsChecked;
            Properties.Settings.Default.CMD_ACMI                  = (bool)mainWindow.CMD_ACMI.IsChecked;
            Properties.Settings.Default.CMD_WINDOW                = (bool)mainWindow.CMD_WINDOW.IsChecked;
            Properties.Settings.Default.CMD_NOMOVIE               = (bool)mainWindow.CMD_NOMOVIE.IsChecked;
            Properties.Settings.Default.CMD_EF                    = (bool)mainWindow.CMD_EF.IsChecked;
            Properties.Settings.Default.CMD_MONO                  = (bool)mainWindow.CMD_MONO.IsChecked;
            Properties.Settings.Default.CMD_BW                    = this.bandWidthDefault;
            Properties.Settings.Default.NoOverride                = (bool)mainWindow.ApplicationOverride.IsChecked;
            Properties.Settings.Default.Misc_RLNWS                = (bool)mainWindow.Misc_RollLinkedNWS.IsChecked;
            Properties.Settings.Default.Misc_MouseCursorAnchor    = (bool)mainWindow.Misc_MouseCursorAnchor.IsChecked;
            Properties.Settings.Default.Misc_TrackIRZ             = (bool)mainWindow.Misc_TrackIRZ.IsChecked;
            Properties.Settings.Default.Misc_ExMouseLook          = (bool)mainWindow.Misc_ExMouseLook.IsChecked;
            Properties.Settings.Default.Misc_OverrideSelfCancel   = (bool)mainWindow.Misc_OverrideSelfCancel.IsChecked;
            Properties.Settings.Default.Misc_SmartScalingOverride = (bool)mainWindow.Misc_SmartScalingOverride.IsChecked;
            Properties.Settings.Default.Save();
        }

        public void CMD_BW_Click()
        {
            this.bandWidthDefault *= 2;
            if (this.bandWidthDefault > 10000)
            {
                this.bandWidthDefault = 512;
            }
            mainWindow.CMD_BW.Content = "BW : " + this.bandWidthDefault.ToString();
        }
    }
}
