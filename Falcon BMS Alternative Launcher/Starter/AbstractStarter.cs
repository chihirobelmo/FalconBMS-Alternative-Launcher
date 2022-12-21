using System.IO;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Starter
{
    public class AbstractStarter
    {
        protected AppRegInfo appReg;
        protected MainWindow mainWindow;

        protected string url;

        public AbstractStarter(AppRegInfo appReg, MainWindow mainWindow)
        {
            this.appReg     = appReg;
            this.mainWindow = mainWindow;
        }

        public virtual void execute(object sender) 
        {
            execute(sender, false);
        }
        public virtual void execute(object sender, bool flg) { }

        public virtual string getCommandLine()
        {
            string strCmdText = "";
            if (mainWindow.CMD_ACMI.IsChecked == false)
                strCmdText += "-acmi ";
            if (mainWindow.CMD_WINDOW.IsChecked == false)
                strCmdText += "-window ";
            if (mainWindow.CMD_NOMOVIE.IsChecked == false)
                strCmdText += "-nomovie ";
            if (mainWindow.CMD_EF.IsChecked == false)
                strCmdText += "-ef ";
            if (mainWindow.CMD_MONO.IsChecked == false)
                strCmdText += "-mono ";
            return strCmdText;
        }

        public void NewAxisFrom433(bool flg)
        {
            if (flg)
            {
                mainWindow.Name_FLIR_Brightness.Visibility = Visibility.Visible;
                mainWindow.Label_FLIR_Brightness.Visibility = Visibility.Visible;
                mainWindow.Axis_FLIR_Brightness.Visibility = Visibility.Visible;
                mainWindow.FLIR_Brightness.Visibility = Visibility.Visible;

                mainWindow.Name_AI_vs_IVC.Visibility = Visibility.Visible;
                mainWindow.Label_AI_vs_IVC.Visibility = Visibility.Visible;
                mainWindow.Axis_AI_vs_IVC.Visibility = Visibility.Visible;
                mainWindow.AI_vs_IVC.Visibility = Visibility.Visible;

                mainWindow.Grid_HSI.Visibility = Visibility.Visible;
                mainWindow.Grid_Altimeter.Visibility = Visibility.Visible;
                mainWindow.Misc_NaturalHeadMovement.Visibility = Visibility.Visible;
            }
            else
            {
                mainWindow.Name_FLIR_Brightness.Visibility = Visibility.Hidden;
                mainWindow.Label_FLIR_Brightness.Visibility = Visibility.Hidden;
                mainWindow.Axis_FLIR_Brightness.Visibility = Visibility.Hidden;
                mainWindow.FLIR_Brightness.Visibility = Visibility.Hidden;

                mainWindow.Name_AI_vs_IVC.Visibility = Visibility.Hidden;
                mainWindow.Label_AI_vs_IVC.Visibility = Visibility.Hidden;
                mainWindow.Axis_AI_vs_IVC.Visibility = Visibility.Hidden;
                mainWindow.AI_vs_IVC.Visibility = Visibility.Hidden;

                mainWindow.Grid_HSI.Visibility = Visibility.Collapsed;
                mainWindow.Grid_Altimeter.Visibility = Visibility.Collapsed;
                mainWindow.Misc_NaturalHeadMovement.Visibility = Visibility.Collapsed;
            }
        }

        public void AVCSince433(bool flg)
        {
            if (flg)
            {
                mainWindow.Launch_AVC.Visibility = Visibility.Visible;
                mainWindow.Label_AVC.Visibility  = Visibility.Visible;
            }
            else
            {
                mainWindow.Launch_AVC.Visibility = Visibility.Hidden;
                mainWindow.Label_AVC.Visibility  = Visibility.Hidden;
            }
        }

        public void VRsince437(bool flg)
        {
            if (flg && SteamVR.HasSteamVR)
            {
                mainWindow.Label_VR.Visibility = Visibility.Visible;
                mainWindow.Misc_VR.Visibility  = Visibility.Visible;
            }
            else
            {
                mainWindow.Label_VR.Visibility = Visibility.Hidden;
                mainWindow.Misc_VR.Visibility  = Visibility.Hidden;
                mainWindow.Misc_VR.IsChecked = false;
            }
        }

        public void DISXuntil434(bool flg)
        {
            if (flg)
            {
                mainWindow.Launch_DISX.Visibility = Visibility.Visible;
                mainWindow.Label_DISX.Visibility  = Visibility.Visible;
            }
            else
            {
                mainWindow.Launch_DISX.Visibility = Visibility.Hidden;
                mainWindow.Label_DISX.Visibility  = Visibility.Hidden;
            }
        }

        public void NewAxisFrom435(bool flg)
        {
            if (flg)
            {
                mainWindow.Name_ILS_Volume_Knob.Visibility  = Visibility.Visible;
                mainWindow.Label_ILS_Volume_Knob.Visibility = Visibility.Visible;
                mainWindow.Axis_ILS_Volume_Knob.Visibility  = Visibility.Visible;
                mainWindow.ILS_Volume_Knob.Visibility       = Visibility.Visible;
            }
            else
            {
                mainWindow.Name_ILS_Volume_Knob.Visibility  = Visibility.Hidden;
                mainWindow.Label_ILS_Volume_Knob.Visibility = Visibility.Hidden;
                mainWindow.Axis_ILS_Volume_Knob.Visibility  = Visibility.Hidden;
                mainWindow.ILS_Volume_Knob.Visibility       = Visibility.Hidden;
            }
        }

        public void RTTsince435(bool flg)
        {
            if (flg)
            {
                mainWindow.Launch_RTTC.Visibility = Visibility.Visible;
                mainWindow.Label_RTTC.Visibility  = Visibility.Visible;
                mainWindow.Launch_RTTS.Visibility = Visibility.Visible;
                mainWindow.Label_RTTS.Visibility  = Visibility.Visible;
            }
            else
            {
                mainWindow.Launch_RTTC.Visibility = Visibility.Hidden;
                mainWindow.Label_RTTC.Visibility  = Visibility.Hidden;
                mainWindow.Launch_RTTS.Visibility = Visibility.Hidden;
                mainWindow.Label_RTTS.Visibility  = Visibility.Hidden;
            }
        }

        public void Bandwidth(bool flg)
        {
            if (flg)
            {
                mainWindow.CMD_BW.Visibility = Visibility.Visible;
            }
            else
            {
                mainWindow.CMD_BW.Visibility = Visibility.Hidden;
            }
        }

        public void PlatformChangeSince433(AvailablePlatform mode)
        {
            switch (mode)
            {
                case AvailablePlatform.X86:
                    mainWindow.Misc_Platform.IsChecked   = false;
                    mainWindow.Misc_Platform.Visibility  = Visibility.Hidden;
                    mainWindow.Label_Platform.Visibility = Visibility.Hidden;
                    break;
                case AvailablePlatform.X64:
                    mainWindow.Misc_Platform.IsChecked   = true;
                    mainWindow.Misc_Platform.Visibility  = Visibility.Hidden;
                    mainWindow.Label_Platform.Visibility = Visibility.Hidden;
                    break;
                case AvailablePlatform.BOTH:
                    mainWindow.Misc_Platform.IsChecked   = Properties.Settings.Default.Platform;
                    mainWindow.Misc_Platform.Visibility  = Visibility.Visible;
                    mainWindow.Label_Platform.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        public enum AvailablePlatform
        {
            X86 = 0,
            X64 = 1,
            BOTH = 2
        }
    }

}
