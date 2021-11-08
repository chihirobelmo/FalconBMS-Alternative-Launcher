using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FalconBMS.Launcher.Windows
{
    public class LatestBMSStatus
    {
        public string release;
        public string tutorialUpdate;
        public string tutorialInstall;

        public int setupCount;

        public XmlNodeList setupZip;
        public XmlNodeList setupExe;
        public XmlNodeList setupSize;
        public XmlNodeList setupHash;
        public XmlNodeList setupTracker;

        public int updateCount;

        public XmlNodeList updeteSize;
        public XmlNodeList updateExe;
        public XmlNodeList updateHash;
        public XmlNodeList updateTracker;

        public string destination;
        public LatestBMSStatus()
        {
            try
            {
                string url = "https://raw.githubusercontent.com/chihirobelmo/FalconBMS-Alternative-Launcher/master/Falcon%20BMS%20Alternative%20Launcher/Update/BMS_Latest.xml";

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(@url);

                var item = xmlDoc.SelectNodes("item/release");
                release = item[0].InnerText;

                var tutorial = xmlDoc.SelectNodes("item/tutorial/update");
                tutorialUpdate = tutorial[0].InnerText;

                var tutorial2 = xmlDoc.SelectNodes("item/tutorial/install");
                tutorialInstall = tutorial2[0].InnerText;

                var setup = xmlDoc.SelectNodes("item/setup/zip");
                setupCount = setup.Count;

                setupZip     = xmlDoc.SelectNodes("item/setup/zip");
                setupExe     = xmlDoc.SelectNodes("item/setup/exe");
                setupSize    = xmlDoc.SelectNodes("item/setup/size");
                setupHash    = xmlDoc.SelectNodes("item/setup/hash");
                setupTracker = xmlDoc.SelectNodes("item/setup/tracker");

                var update  = xmlDoc.SelectNodes("item/update");
                updateCount = update.Count;

                updeteSize    = xmlDoc.SelectNodes("item/update/size");
                updateExe     = xmlDoc.SelectNodes("item/update/exe");
                updateHash    = xmlDoc.SelectNodes("item/update/hash");
                updateTracker = xmlDoc.SelectNodes("item/update/tracker");

                var destinationNode = xmlDoc.SelectNodes("item/destination");
                destination = destinationNode[0].InnerText;
            }
            catch
            { 
            }
        }
    }
}
