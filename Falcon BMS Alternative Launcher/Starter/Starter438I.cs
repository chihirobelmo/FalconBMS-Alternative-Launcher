using System.IO;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher.Starter
{
    public class Starter438Internal : Starter437Internal
    {
        public Starter438Internal(AppRegInfo appReg, MainWindow mainWindow) : base(appReg, mainWindow)
        {
            Bandwidth(false);
            NewAxisFrom433(true);
            PlatformChangeSince433(AvailablePlatform.X64);
            AVCSince433(true);
            DISXuntil434(false);
            RTTsince435(true);
            NewAxisFrom435(true);
            VRsince437(true);

            mainWindow.Version_Number.Content = "4.38 I";
        }
    }
}
