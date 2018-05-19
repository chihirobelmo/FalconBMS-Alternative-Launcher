using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class TheaterList
    {
        /// <summary>
        /// Read theater.lst and apply the list to Combobox.
        /// </summary>
        public TheaterList(AppRegInfo appReg, ComboBox Combo)
        {
            String filename = appReg.GetInstallDir() + "/Data/Terrdata/theaterdefinition/theater.lst";
            if (File.Exists(filename) == false)
                return;
            string[] definitionfile = File.ReadAllLines(filename, Encoding.UTF8);

            var list = new List<string>();
            foreach (string tdf in definitionfile)
            {
                if (File.Exists(appReg.GetInstallDir() + "\\Data\\" + tdf) == false)
                    continue;
                string[] line = File.ReadAllLines(appReg.GetInstallDir() + "\\Data\\" + tdf, Encoding.UTF8);
                string theatername = "";
                foreach (string str in line)
                {
                    if (!str.Contains("name "))
                        continue;
                    theatername = str.Replace("name ", "").Trim();
                    break;
                }
                list.Add(theatername);
            }
            for (int ii = 0; ii < list.Count; ii++)
            {
                Combo.Items.Add(list[ii]);
                if (list[ii] == appReg.GetCurrentTheater())
                    Combo.SelectedIndex = ii;
            }
        }
    }
}
