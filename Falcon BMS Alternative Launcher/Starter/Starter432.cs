using System.IO;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Starter
{
    public class Starter432 : AbstractStarter
    {
        public Starter432(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(false);
            PlatformChangeSince433(AvailablePlatform.X86);
            AVCSince433(false);
            DISXuntil434(true);
            RTTsince435(false);
            NewAxisFrom435(false);
            VRsince437(false);

            mainWindow.Version_Number.Content = "4.32";
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

}
