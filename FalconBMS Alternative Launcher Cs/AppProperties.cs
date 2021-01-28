using System.Windows;

using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher
{
    public class AppProperties
    {
        private MainWindow mainWindow;
        public int bandWidthDefault = 1024;

        public AppProperties(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // Load Buttons
            mainWindow.MiscPlatform.IsChecked             = Properties.Settings.Default.Platform;
            mainWindow.CmdAcmi.IsChecked                  = Properties.Settings.Default.CMD_ACMI;
            mainWindow.CmdWindow.IsChecked                = Properties.Settings.Default.CMD_WINDOW;
            mainWindow.CmdNomovie.IsChecked               = Properties.Settings.Default.CMD_NOMOVIE;
            mainWindow.CmdEf.IsChecked                    = Properties.Settings.Default.CMD_EF;
            mainWindow.CmdMono.IsChecked                  = Properties.Settings.Default.CMD_MONO;
            bandWidthDefault                          = Properties.Settings.Default.CMD_BW;
            mainWindow.ApplicationOverride.IsChecked       = Properties.Settings.Default.NoOverride;
            mainWindow.MiscRollLinkedNws.IsChecked        = Properties.Settings.Default.Misc_RLNWS;
            mainWindow.MiscMouseCursorAnchor.IsChecked    = Properties.Settings.Default.Misc_MouseCursorAnchor;
            mainWindow.MiscTrackIrz.IsChecked             = Properties.Settings.Default.Misc_TrackIRZ;
            mainWindow.MiscExMouseLook.IsChecked          = Properties.Settings.Default.Misc_ExMouseLook;
            mainWindow.MiscOverrideSelfCancel.IsChecked   = Properties.Settings.Default.Misc_OverrideSelfCancel;
            mainWindow.MiscSmartScalingOverride.IsChecked = Properties.Settings.Default.Misc_SmartScalingOverride;
            mainWindow.MiscNaturalHeadMovement.IsChecked  = Properties.Settings.Default.Misc_NaturalHeadMovement;
            mainWindow.MiscPilotModel.IsChecked           = Properties.Settings.Default.Misc_PilotModel;

            // Button Status Default
            mainWindow.SelectDxRelease.IsChecked  = true;
            mainWindow.SelectPinkyShift.IsChecked  = true;
            mainWindow.CmdBw.Content               = "BW : " + bandWidthDefault;
            mainWindow.AbThrottle.Visibility       = Visibility.Hidden;
            mainWindow.AbThrottleRight.Visibility = Visibility.Hidden;
        }

        public void SaveUiSetup()
        {
            Properties.Settings.Default.Platform                  = (bool)mainWindow.MiscPlatform.IsChecked;
            Properties.Settings.Default.CMD_ACMI                  = (bool)mainWindow.CmdAcmi.IsChecked;
            Properties.Settings.Default.CMD_WINDOW                = (bool)mainWindow.CmdWindow.IsChecked;
            Properties.Settings.Default.CMD_NOMOVIE               = (bool)mainWindow.CmdNomovie.IsChecked;
            Properties.Settings.Default.CMD_EF                    = (bool)mainWindow.CmdEf.IsChecked;
            Properties.Settings.Default.CMD_MONO                  = (bool)mainWindow.CmdMono.IsChecked;
            Properties.Settings.Default.CMD_BW                    = bandWidthDefault;
            Properties.Settings.Default.NoOverride                = (bool)mainWindow.ApplicationOverride.IsChecked;
            Properties.Settings.Default.Misc_RLNWS                = (bool)mainWindow.MiscRollLinkedNws.IsChecked;
            Properties.Settings.Default.Misc_MouseCursorAnchor    = (bool)mainWindow.MiscMouseCursorAnchor.IsChecked;
            Properties.Settings.Default.Misc_TrackIRZ             = (bool)mainWindow.MiscTrackIrz.IsChecked;
            Properties.Settings.Default.Misc_ExMouseLook          = (bool)mainWindow.MiscExMouseLook.IsChecked;
            Properties.Settings.Default.Misc_OverrideSelfCancel   = (bool)mainWindow.MiscOverrideSelfCancel.IsChecked;
            Properties.Settings.Default.Misc_SmartScalingOverride = (bool)mainWindow.MiscSmartScalingOverride.IsChecked;
            Properties.Settings.Default.Misc_NaturalHeadMovement  = (bool)mainWindow.MiscNaturalHeadMovement.IsChecked;
            Properties.Settings.Default.Misc_PilotModel           = (bool)mainWindow.MiscPilotModel.IsChecked;
            Properties.Settings.Default.Save();
        }

        public void CMD_BW_Click()
        {
            bandWidthDefault *= 2;
            if (bandWidthDefault > 10000)
            {
                bandWidthDefault = 512;
            }
            mainWindow.CmdBw.Content = "BW : " + bandWidthDefault;
        }
    }
}
