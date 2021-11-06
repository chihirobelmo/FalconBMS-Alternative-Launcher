using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Xml;

using FalconBMS.Launcher.Input;


namespace FalconBMS.Launcher.Windows
{
    public class RSSReader
    {
        public static void Read(string url)
        {
            XmlReader rdr = XmlReader.Create(url);
            SyndicationFeed feed = SyndicationFeed.Load(rdr);
        }
    }
}
