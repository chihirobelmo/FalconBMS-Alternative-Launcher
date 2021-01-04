using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FalconBMS_Alternative_Launcher_Cs
{
    public class KeyFile
    {
        public KeyAssgn[] keyAssign;

        public KeyFile(string Filename, AppRegInfo appReg)
        {
            string stParentName = Path.GetDirectoryName(Filename);

            // Do BMS - FULL.key file exists at User/Config?
            if (File.Exists(Filename) == false)
            {
                MessageBoxResult result = MessageBox.Show
                    ("App could not find " + Filename, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string[] lines = File.ReadAllLines(Filename, Encoding.UTF8);

            keyAssign = new KeyAssgn[lines.Length];

            int i = -1;
            foreach (string stBuffer in lines)
            {
                string[] stArrayData = stBuffer.Split(' ');

                if (stArrayData.Length < 7)
                    continue;
                if (stBuffer.Substring(0, 1) == "#")
                    continue;
                if (stArrayData[3] == "-2" | stArrayData[3] == "-3")
                    continue;

                // Okay now this line is confirmed to be a line that shows keyboard assignment.

                i += 1;
                keyAssign[i] = new KeyAssgn(stBuffer);

                // What if the line format was broken?

                if (keyAssign[i].CheckFileCollapsing() == false)
                    continue;

                MessageBoxResult result = MessageBox.Show
                    ("App found " + appReg.getKeyFileName() + " broken\nWould you like to restore it to the default?", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.OK)
                {
                    string fnamestock = appReg.GetInstallDir() + "\\Docs\\Key Files & Input\\" + appReg.getKeyFileName();
                    string fname = appReg.GetInstallDir() + "\\User\\Config\\" + appReg.getKeyFileName(); ;
                    if (File.Exists(fnamestock) == true)
                    {
                        File.Copy(fnamestock, fname, true);
                        Application.Current.Shutdown();
                        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                        return;
                    }
                    MessageBox.Show("App could not find " + appReg.getKeyFileName() + " at\nDocs\\Key Files & Input\\", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Application.Current.Shutdown();
                    return;
                }
                Application.Current.Shutdown();
                return;
            }
            Array.Resize(ref keyAssign, i+1);
        }

        public KeyFile(KeyAssgn[] keyAssign)
        {
            this.keyAssign = new KeyAssgn[keyAssign.Length];
            for (int i = 0; i < keyAssign.Length; i++)
            {
                this.keyAssign[i] = keyAssign[i].Clone();
            }
        }

        public KeyFile Clone()
        {
            return new KeyFile(keyAssign);
        }
    }
}
