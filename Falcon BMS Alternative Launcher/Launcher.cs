using System.IO;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher
{
    public class Launcher
    {
        protected AppRegInfo appReg;
        protected MainWindow mainWindow;

        protected string url;

        public Launcher(AppRegInfo appReg, MainWindow mainWindow)
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

        public void SecretStartsFrom437(bool flg)
        {
            if (flg)
            {
                mainWindow.Label_Secret.Visibility = Visibility.Visible;
                mainWindow.Misc_Secret.Visibility  = Visibility.Visible;
            }
            else
            {
                mainWindow.Label_Secret.Visibility = Visibility.Hidden;
                mainWindow.Misc_Secret.Visibility  = Visibility.Hidden;
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

    public class Launcher432 : Launcher
    {
        public Launcher432(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(false);
            PlatformChangeSince433(AvailablePlatform.X86);
            AVCSince433(false);
            DISXuntil434(true);
            RTTsince435(false);
            NewAxisFrom435(false);
            SecretStartsFrom437(false);

            mainWindow.Version_Number.Content = "4.32";
        }

        public override void execute(object sender, bool flg)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                case "Launch_BMS_Large":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();

                    string appPlatform = appReg.GetInstallDir() + "/Bin/x86/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    if (flg)
                        mainWindow.minimizeWindowUntilProcessEnds(process);
                    else
                        mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_DISX":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/Display Extraction.exe");
                    break;
                case "Launch_IVCC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x86/IVC/");
                    System.Diagnostics.Process.Start("IVC Client.exe");
                    break;
                case "Launch_IVCS":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/IVC/IVC Server.exe");
                    break;
                case "Launch_AVC":
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/Editor.exe");
                    break;
            }
        }
    }

    public class Launcher433 : Launcher
    {
        public Launcher433(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.BOTH);
            AVCSince433(true);
            DISXuntil434(true);
            RTTsince435(false);
            NewAxisFrom435(false);
            SecretStartsFrom437(false);

            mainWindow.Version_Number.Content = "4.33";
        }

        public override void execute(object sender, bool flg)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                case "Launch_BMS_Large":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();

                    string appPlatform = "";
                    if (mainWindow.Misc_Platform.IsChecked == true)
                        appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    else
                        appPlatform = appReg.GetInstallDir() + "/Bin/x86/Falcon BMS.exe";
                    if (File.Exists(appPlatform) == false)
                    {
                        mainWindow.Misc_Platform.IsChecked = false;
                        appPlatform = appReg.GetInstallDir() + "/Bin/x86/Falcon BMS.exe";
                        return;
                    }
                    if (File.Exists(appPlatform) == false)
                        return;
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    if (flg)
                        mainWindow.minimizeWindowUntilProcessEnds(process);
                    else
                        mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_DISX":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/Display Extraction.exe");
                    break;
                case "Launch_IVCC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x86/IVC/");
                    System.Diagnostics.Process.Start("IVC Client.exe");
                    break;
                case "Launch_IVCS":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/IVC/IVC Server.exe");
                    break;
                case "Launch_AVC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x86/");
                    process = System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/Editor.exe");
                    break;
            }
        }
    }

    public class Launcher434 : Launcher
    {
        public Launcher434(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.X64);
            AVCSince433(true);
            DISXuntil434(true);
            RTTsince435(false);
            NewAxisFrom435(false);
            SecretStartsFrom437(false);

            mainWindow.Version_Number.Content = "4.34";
        }

        public override void execute(object sender, bool flg)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                case "Launch_BMS_Large":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();
                        
                    string appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    if (flg)
                        mainWindow.minimizeWindowUntilProcessEnds(process);
                    else
                        mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_DISX":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/Display Extraction.exe");
                    break;
                case "Launch_IVCC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x64/IVC/");
                    System.Diagnostics.Process.Start("IVC Client.exe");
                    break;
                case "Launch_IVCS":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/IVC/IVC Server.exe");
                    break;
                case "Launch_AVC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x86/");
                    process = System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/Editor.exe");
                    break;
            }
        }
    }

    public class Launcher435 : Launcher
    {
        public Launcher435(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.X64);
            AVCSince433(true);
            DISXuntil434(false);
            RTTsince435(true);
            NewAxisFrom435(true);
            SecretStartsFrom437(false);

            mainWindow.Version_Number.Content = "4.35";
        }

        public override void execute(object sender, bool flg)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                case "Launch_BMS_Large":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();

                    string appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    if (flg)
                        mainWindow.minimizeWindowUntilProcessEnds(process);
                    else
                        mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_RTTC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Tools/RTTRemote/");
                    System.Diagnostics.Process.Start("RTTClient64.exe");
                    break;
                case "Launch_RTTS":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Tools/RTTRemote/");
                    System.Diagnostics.Process.Start("RTTServer64.exe");
                    break;
                case "Launch_IVCC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x64/IVC/");
                    System.Diagnostics.Process.Start("IVC Client.exe");
                    break;
                case "Launch_IVCS":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/IVC/IVC Server.exe");
                    break;
                case "Launch_AVC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x86/");
                    process = System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/Editor.exe");
                    break;
            }
        }
    }

    public class Launcher436 : Launcher435
    {
        public Launcher436(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.X64);
            AVCSince433(true);
            DISXuntil434(false);
            RTTsince435(true);
            NewAxisFrom435(true);
            SecretStartsFrom437(false);

            mainWindow.Version_Number.Content = "4.36";
        }
    }

        public class Launcher436Internal : Launcher
    {
        public Launcher436Internal(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.X64);
            AVCSince433(true);
            DISXuntil434(false);
            RTTsince435(true);
            NewAxisFrom435(true);
            SecretStartsFrom437(false);

            mainWindow.Version_Number.Content = "4.36 I";
        }

        public override void execute(object sender, bool flg)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                case "Launch_BMS_Large":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();

                    string appPlatform = appReg.GetInstallDir() + "/Bin/x86//Hub.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    if (flg)
                        mainWindow.minimizeWindowUntilProcessEnds(process);
                    else
                        mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_RTTC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Tools/RTTRemote/");
                    System.Diagnostics.Process.Start("RTTClient64.exe");
                    break;
                case "Launch_RTTS":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Tools/RTTRemote/");
                    System.Diagnostics.Process.Start("RTTServer64.exe");
                    break;
                case "Launch_IVCC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x64/IVC/");
                    System.Diagnostics.Process.Start("IVC Client.exe");
                    break;
                case "Launch_IVCS":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/IVC/IVC Server.exe");
                    break;
                case "Launch_AVC":
                    Directory.SetCurrentDirectory(appReg.GetInstallDir() + "/Bin/x86/");
                    process = System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/Editor.exe");
                    break;
            }
        }
    }

    public class Launcher437 : Launcher436
    {
        public Launcher437(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.X64);
            AVCSince433(true);
            DISXuntil434(false);
            RTTsince435(true);
            NewAxisFrom435(true);
            SecretStartsFrom437(false);

            mainWindow.Version_Number.Content = "4.37";
        }
    }

    public class Launcher437Internal : Launcher436Internal
    {
        public Launcher437Internal(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.X64);
            AVCSince433(true);
            DISXuntil434(false);
            RTTsince435(true);
            NewAxisFrom435(true);
            SecretStartsFrom437(true);

            mainWindow.Version_Number.Content = "4.37 I";
        }
    }
}
