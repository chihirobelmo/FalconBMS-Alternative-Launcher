using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace FalconBMS.Launcher.Core
{
    public class TheaterList
    {
        /// <summary>
        /// Read theater.lst and apply the list to Combobox.
        /// </summary>
        public static void Populate(AppRegInfo appReg, ComboBox Combo)
        {
            string filename = appReg.GetInstallDir() + "/Data/Terrdata/theaterdefinition/theater.lst";
            if (File.Exists(filename) == false)
                return;
            IEnumerable<string> theaterPaths = File.ReadLines(filename, Encoding.UTF8)
                .Select(line => line.Trim()) // Trim whitespace
                .Where(line => line.Length > 0 && !line.StartsWith("#")) // Throw out empty lines, comments, etc.
                .Select(line => appReg.GetInstallDir() + "\\Data\\" + line) // Construct TDF file path
                .Where(tdf => File.Exists(tdf)); // Throw out paths we can't find

            List<string> theaters = new List<string>();
            foreach (string tdf in theaterPaths)
            {
                IEnumerable<string> lines = File.ReadLines(tdf, Encoding.UTF8);
                foreach (string str in lines)
                {
                    if (str.Contains("name "))
                    {
                        theaters.Add(str.Replace("name ", "").Trim());
                        break;
                    }
                }
            }
            theaters.Sort();

            for (int ii = 0; ii < theaters.Count; ii++)
            {
                Combo.Items.Add(theaters[ii]);
                if (theaters[ii] == appReg.GetCurrentTheater())
                    Combo.SelectedIndex = ii;
            }
        }
    }
}
