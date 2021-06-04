using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace FalconBMS.Launcher
{
    public class TheaterList
    {
        /// <summary>
        /// Read theater.lst and apply the list to Combobox.
        /// </summary>
        public static void PopulateAndSave(AppRegInfo appReg, ComboBox Combo)
        {
            string filename = appReg.GetInstallDir() + "/Data/Terrdata/theaterdefinition/theater.lst";
            string fbackupname = appReg.GetInstallDir() + "/User/Config/Backup/theater.lst";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, false);
            File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            var theaterFiles = Directory.GetFiles(appReg.GetInstallDir() + "/Data", "*.tdf", SearchOption.AllDirectories);
            System.Array.Sort(theaterFiles);

            // Write all TDFs to the theater list, slicing the install dir off.
            var dataDirLength = appReg.GetInstallDir().Length + "/Data/".Length;
            File.WriteAllLines(filename, theaterFiles.Select(t => t.Substring(dataDirLength)).ToArray());

            List<string> theaters = new List<string>();
            foreach (string tdf in theaterFiles)
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

            for (int ii = 0; ii < theaters.Count; ii++)
            {
                Combo.Items.Add(theaters[ii]);
                if (theaters[ii] == appReg.GetCurrentTheater())
                    Combo.SelectedIndex = ii;
            }
        }
    }
}
