using System.IO;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Starter
{
    public class Starter433 : AbstractStarter
    {
        public Starter433(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.BOTH);
            AVCSince433(true);
            DISXuntil434(true);
            RTTsince435(false);
            NewAxisFrom435(false);
            VRsince437(false);

            mainWindow.Version_Number.Content = "4.33";
        }

        public override void execute(object sender)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_UPD":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Updater.exe");
                    mainWindow.Close();
                    break;
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
                    MainWindow.bmsHasBeenLaunched = true;
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

}
