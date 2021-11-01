using System.IO;
using System.Windows;

using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher
{
    public class Launcher
    {
        protected AppRegInfo appReg;
        protected MainWindow mainWindow;

        public Launcher(AppRegInfo appReg, MainWindow mainWindow)
        {
            this.appReg     = appReg;
            this.mainWindow = mainWindow;
        }

        public virtual void execute(object sender) { }

        public virtual string getCommandLine() { return ""; }
    }

    public class Launcher432 : Launcher
    {
        public Launcher432(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.Misc_Platform.IsChecked  = false;

            mainWindow.Misc_Platform.Visibility = Visibility.Hidden;
            mainWindow.Misc_Megane.Visibility       = Visibility.Hidden;

            mainWindow.Label_Platform.Content = "Platform : BMS 4.35 64-bit.";

            mainWindow.Launch_AVC.Visibility = Visibility.Hidden;
            mainWindow.Label_AVC.Visibility  = Visibility.Hidden;

            mainWindow.Name_FLIR_Brightness.Visibility  = Visibility.Hidden;
            mainWindow.Label_FLIR_Brightness.Visibility = Visibility.Hidden;
            mainWindow.Axis_FLIR_Brightness.Visibility  = Visibility.Hidden;
            mainWindow.FLIR_Brightness.Visibility       = Visibility.Hidden;

            mainWindow.Name_AI_vs_IVC.Visibility  = Visibility.Hidden;
            mainWindow.Label_AI_vs_IVC.Visibility = Visibility.Hidden;
            mainWindow.Axis_AI_vs_IVC.Visibility  = Visibility.Hidden;
            mainWindow.AI_vs_IVC.Visibility       = Visibility.Hidden;

            mainWindow.Tab_HSI_and_Altimeter.Visibility    = Visibility.Collapsed;
            mainWindow.Misc_NaturalHeadMovement.Visibility = Visibility.Collapsed;
        }

        public override void execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();

                    string appPlatform = appReg.GetInstallDir() + "/Bin/x86/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
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

        public override string getCommandLine()
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
            strCmdText += "-bw " + mainWindow.getBWValue();
            return strCmdText;
        }
    }

    public class Launcher433 : Launcher
    {
        public Launcher433(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.Tab_HSI_and_Altimeter.Visibility    = Visibility.Collapsed;
            mainWindow.Misc_NaturalHeadMovement.Visibility = Visibility.Collapsed;

            mainWindow.Misc_Megane.Visibility = Visibility.Hidden;
            mainWindow.Label_Megane.Visibility = Visibility.Hidden;
        }

        public override void execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
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
                    System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x86/Editor.exe");
                    break;
            }
        }

        public override string getCommandLine()
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
            strCmdText += "-bw " + mainWindow.getBWValue();
            return strCmdText;
        }
    }

    public class Launcher434 : Launcher
    {
        public Launcher434(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.Misc_Platform.IsChecked  = true;

            mainWindow.Misc_Platform.Visibility = Visibility.Hidden;
            mainWindow.Misc_Megane.Visibility   = Visibility.Hidden;

            mainWindow.Label_Megane.Visibility = Visibility.Hidden;

            mainWindow.Label_Platform.Content   = "Platform : BMS 4.34 is 64-bit appreciation.";

            mainWindow.CMD_BW.Visibility = Visibility.Hidden;
        }

        public override void execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();
                        
                    string appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
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
                    System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/Editor.exe");
                    break;
            }
        }

        public override string getCommandLine()
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
    }

    public class Launcher435 : Launcher
    {
        public Launcher435(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.Misc_Platform.IsChecked  = true;
            mainWindow.Misc_Platform.Visibility = Visibility.Hidden;
            mainWindow.Misc_Megane.Visibility   = Visibility.Hidden;

            mainWindow.Label_Platform.Content   = "Platform : BMS 4.35 is 64-bit appreciation.";
            mainWindow.Label_Megane.Visibility = Visibility.Hidden;

            mainWindow.Launch_DISX.Visibility = Visibility.Hidden;
            mainWindow.Label_DISX.Visibility  = Visibility.Hidden;

            mainWindow.CMD_BW.Visibility = Visibility.Hidden;
        }

        public override void execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();

                    string appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
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
                    System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/Editor.exe");
                    break;
            }
        }

        public override string getCommandLine()
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
    }

    public class Launcher436Internal : Launcher
    {
        public Launcher436Internal(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.Misc_Platform.IsChecked  = true;

            mainWindow.Misc_Platform.Visibility = Visibility.Hidden;
            mainWindow.Misc_Megane.Visibility   = Visibility.Hidden;

            mainWindow.Label_Megane.Visibility = Visibility.Hidden;
            mainWindow.Label_Platform.Visibility = Visibility.Hidden;

            mainWindow.Launch_DISX.Visibility = Visibility.Hidden;
            mainWindow.Label_DISX.Visibility  = Visibility.Hidden;

            mainWindow.CMD_BW.Visibility = Visibility.Hidden;
        }

        public override void execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();

                    string appPlatform = appReg.GetInstallDir() + "/Bin/x86/Hub.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
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
                    System.Diagnostics.Process.Start("Avionics Configurator.exe");
                    break;
                case "Launch_EDIT":
                    System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Bin/x64/Editor.exe");
                    break;
            }
        }

        public override string getCommandLine()
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
    }
}
