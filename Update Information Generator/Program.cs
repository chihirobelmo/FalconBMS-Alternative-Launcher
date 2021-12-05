using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FalconBMS.Launcher.Update;

namespace Update_Information_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Download dl = new Download();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-s":
                        if (i + 1 >= args.Length)
                            break;
                        Setup setup = new Setup(args[i+1]);
                        setup.calcSHA1();
                        setup.removePath();
                        dl.bms.setup = setup;
                        break;
                    case "-u":
                        if (i + 1 >= args.Length)
                            break;
                        IncrementalUpdate iun = new IncrementalUpdate(args[i + 1]);
                        iun.calcSHA1();
                        iun.removePath();
                        dl.bms.addUpdate(iun);
                        break;
                    default:
                        break;
                }
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Download));
            StreamWriter sw = new StreamWriter("Setup.xml", false, new UTF8Encoding(false));
            serializer.Serialize(sw, dl);

            sw.Close();
        }
    }
}
