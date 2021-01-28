using System.IO;
using System.Windows;

using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Core
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

        public virtual void Execute(object sender) { }

        public virtual string GetCommandLine() { return ""; }
    }

    public class Launcher432 : Launcher
    {
        public Launcher432(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.MiscPlatform.IsChecked = false;
            mainWindow.MiscPlatform.Visibility = Visibility.Hidden;
            mainWindow.LabelPlatform.Content = "Platform : BMS 4.32 is 32-bit appreciation.";

            mainWindow.LaunchAvc.Visibility = Visibility.Hidden;
            mainWindow.LabelAvc.Visibility = Visibility.Hidden;

            mainWindow.NameFlirBrightness.Visibility = Visibility.Hidden;
            mainWindow.LabelFlirBrightness.Visibility = Visibility.Hidden;
            mainWindow.AxisFlirBrightness.Visibility = Visibility.Hidden;
            mainWindow.FlirBrightness.Visibility = Visibility.Hidden;

            mainWindow.NameAiVsIvc.Visibility = Visibility.Hidden;
            mainWindow.LabelAiVsIvc.Visibility = Visibility.Hidden;
            mainWindow.AxisAiVsIvc.Visibility = Visibility.Hidden;
            mainWindow.AiVsIvc.Visibility = Visibility.Hidden;

            mainWindow.TabHsiAndAltimeter.Visibility = Visibility.Collapsed;
            mainWindow.MiscNaturalHeadMovement.Visibility = Visibility.Collapsed;
        }

        public override void Execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = GetCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.ExecuteOverride();

                    string appPlatform = appReg.GetInstallDir() + "/Bin/x86/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.MinimizeWindowUntilProcessEnds(process);
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

        public override string GetCommandLine()
        {
            string strCmdText = "";
            if (mainWindow.CmdAcmi.IsChecked == false)
                strCmdText += "-acmi ";
            if (mainWindow.CmdWindow.IsChecked == false)
                strCmdText += "-window ";
            if (mainWindow.CmdNomovie.IsChecked == false)
                strCmdText += "-nomovie ";
            if (mainWindow.CmdEf.IsChecked == false)
                strCmdText += "-ef ";
            if (mainWindow.CmdMono.IsChecked == false)
                strCmdText += "-mono ";
            strCmdText += "-bw " + mainWindow.GetBwValue();
            return strCmdText;
        }
    }

    public class Launcher433 : Launcher
    {
        public Launcher433(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.TabHsiAndAltimeter.Visibility = Visibility.Collapsed;
            mainWindow.MiscNaturalHeadMovement.Visibility = Visibility.Collapsed;
        }

        public override void Execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = GetCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.ExecuteOverride();

                    string appPlatform = "";
                    if (mainWindow.MiscPlatform.IsChecked == true)
                        appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    else
                        appPlatform = appReg.GetInstallDir() + "/Bin/x86/Falcon BMS.exe";
                    if (File.Exists(appPlatform) == false)
                    {
                        mainWindow.MiscPlatform.IsChecked = false;
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
                    mainWindow.MinimizeWindowUntilProcessEnds(process);
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

        public override string GetCommandLine()
        {
            string strCmdText = "";
            if (mainWindow.CmdAcmi.IsChecked == false)
                strCmdText += "-acmi ";
            if (mainWindow.CmdWindow.IsChecked == false)
                strCmdText += "-window ";
            if (mainWindow.CmdNomovie.IsChecked == false)
                strCmdText += "-nomovie ";
            if (mainWindow.CmdEf.IsChecked == false)
                strCmdText += "-ef ";
            if (mainWindow.CmdMono.IsChecked == false)
                strCmdText += "-mono ";
            strCmdText += "-bw " + mainWindow.GetBwValue();
            return strCmdText;
        }
    }

    public class Launcher434 : Launcher
    {
        public Launcher434(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.MiscPlatform.IsChecked = true;
            mainWindow.MiscPlatform.Visibility = Visibility.Hidden;
            mainWindow.LabelPlatform.Content = "Platform : BMS 4.34 is 64-bit appreciation.";

            mainWindow.CmdBw.Visibility = Visibility.Hidden;
        }

        public override void Execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = GetCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.ExecuteOverride();
                        
                    string appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.MinimizeWindowUntilProcessEnds(process);
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

        public override string GetCommandLine()
        {
            string strCmdText = "";
            if (mainWindow.CmdAcmi.IsChecked == false)
                strCmdText += "-acmi ";
            if (mainWindow.CmdWindow.IsChecked == false)
                strCmdText += "-window ";
            if (mainWindow.CmdNomovie.IsChecked == false)
                strCmdText += "-nomovie ";
            if (mainWindow.CmdEf.IsChecked == false)
                strCmdText += "-ef ";
            if (mainWindow.CmdMono.IsChecked == false)
                strCmdText += "-mono ";
            return strCmdText;
        }
    }

    public class Launcher435 : Launcher
    {
        public Launcher435(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            mainWindow.MiscPlatform.IsChecked = true;
            mainWindow.MiscPlatform.Visibility = Visibility.Hidden;
            mainWindow.LabelPlatform.Content = "Platform : BMS 4.35 is 64-bit appreciation.";

            mainWindow.LaunchDisx.Visibility = Visibility.Hidden;
            mainWindow.LabelDisx.Visibility  = Visibility.Hidden;

            mainWindow.CmdBw.Visibility = Visibility.Hidden;
        }

        public override void Execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_BMS":
                    string strCmdText = GetCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.ExecuteOverride();

                    string appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                    process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    mainWindow.Close();
                    break;
                case "Launch_CFG":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Config.exe");
                    mainWindow.MinimizeWindowUntilProcessEnds(process);
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

        public override string GetCommandLine()
        {
            string strCmdText = "";
            if (mainWindow.CmdAcmi.IsChecked == false)
                strCmdText += "-acmi ";
            if (mainWindow.CmdWindow.IsChecked == false)
                strCmdText += "-window ";
            if (mainWindow.CmdNomovie.IsChecked == false)
                strCmdText += "-nomovie ";
            if (mainWindow.CmdEf.IsChecked == false)
                strCmdText += "-ef ";
            if (mainWindow.CmdMono.IsChecked == false)
                strCmdText += "-mono ";
            return strCmdText;
        }
    }
}
