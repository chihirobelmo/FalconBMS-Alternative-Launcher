using FalconBMS.Launcher.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Concurrent;

namespace FalconBMS.Launcher
{
    public class TheaterList
    {
        public static ObservableCollection<string> AvailableTheaters { get; set; } = new ObservableCollection<string>();
        public static ConcurrentDictionary<string, string> AvailableTheatersCampaignDirs { get; set; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Read theater.lst and apply the list to Combobox.
        /// </summary>
        public static void PopulateAndSave(AppRegInfo appReg, ComboBox Combo)
        {
            if (!Directory.Exists(appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER))
                Directory.CreateDirectory(appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER);

            string filename = appReg.GetInstallDir() + "/Data/Terrdata/TheaterDefinition/theater.lst";
            string fbackupname = appReg.GetInstallDir() + CommonConstants.BACKUPFOLDER + "theater.lst";
            if (!File.Exists(fbackupname) & File.Exists(filename))
                File.Copy(filename, fbackupname, false);
            File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);

            var theaterFiles = Directory.GetFiles(appReg.GetInstallDir() + "/Data", "*.tdf", SearchOption.AllDirectories);
            System.Array.Sort(theaterFiles);

            // KoreaKTO should be at the Top or MC will have a problem. I'd say MC should fix this!
            theaterFiles = theaterFiles.OrderByDescending(y => y.EndsWith("Data\\TerrData\\TheaterDefinition\\Korea KTO.tdf")).ToArray();

            // Write all TDFs to the theater list, slicing the install dir off.
            var dataDirLength = appReg.GetInstallDir().Length + "/Data/".Length;
            File.WriteAllLines(filename, theaterFiles.Select(t => t.Substring(dataDirLength)).ToArray());

            List<string> theaters = new List<string>();
            foreach (string tdf in theaterFiles)
            {
                // For EMF theater
                if (tdf.Contains("F4Patch"))
                    continue;

                IEnumerable<string> lines = File.ReadLines(tdf, Encoding.UTF8);
                string theaterName ="", campaignDir ="";
                foreach (string str in lines)
                {
                    if (str.Contains("name "))
                    {
                        theaterName = str.Replace("name ", "").Trim();
                    }
                    if(str.Contains("campaigndir "))
                    {
                        campaignDir = str.Replace("campaigndir ", "").Trim();
                    }

                    if (!string.IsNullOrEmpty(theaterName) && !string.IsNullOrEmpty(campaignDir))
                    {
                        theaters.Add(theaterName);
                        AvailableTheatersCampaignDirs.TryAdd(theaterName, campaignDir);
                        break;
                    }
                }
            }

            AvailableTheaters.Clear();
            Combo.SelectedIndex = -1;
            Combo.Items.Clear();

            for (int ii = 0; ii < theaters.Count; ii++)
            {
                AvailableTheaters.Add(theaters[ii]);
                Combo.Items.Add(theaters[ii]);
                if (theaters[ii] == appReg.GetCurrentTheater())
                    Combo.SelectedIndex = ii;
            }
        }
    }
}
