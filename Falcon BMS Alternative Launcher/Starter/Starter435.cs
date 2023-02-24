using System.IO;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Starter
{
    public class Starter435 : AbstractStarter
    {
        public Starter435(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.X64);
            AVCSince433(true);
            DISXuntil434(false);
            RTTsince435(true);
            NewAxisFrom435(true);
            VRsince437(false);

            mainWindow.Version_Number.Content = "4.35";
        }

        public override void execute(object sender, bool flg)
        {
            System.Diagnostics.Process process;
            switch (((System.Windows.Controls.Button)sender).Name)
            {
                case "Launch_UPD":
                    process = System.Diagnostics.Process.Start(appReg.GetInstallDir() + "/Updater.exe");
                    mainWindow.minimizeWindowUntilProcessEnds(process);
                    break;
                case "Launch_BMS_Large":
                    string strCmdText = getCommandLine();

                    // OVERRIDE SETTINGS.
                    mainWindow.executeOverride();

                    string testPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS Test.exe";
                    if (File.Exists(testPlatform) && MessageBox.Show("Start Test Exe?", "Launcher", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        process = System.Diagnostics.Process.Start(testPlatform, strCmdText);
                    }
                    else
                    {
                        string appPlatform = appReg.GetInstallDir() + "/Bin/x64/Falcon BMS.exe";
                        process = System.Diagnostics.Process.Start(appPlatform, strCmdText);
                    }
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

}
