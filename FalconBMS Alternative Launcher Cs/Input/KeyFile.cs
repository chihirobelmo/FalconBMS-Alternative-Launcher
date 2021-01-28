using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace FalconBMS.Launcher.Input
{
    public class KeyFile
    {
        public KeyAssign[] keyAssign;

        public KeyFile(string filename, AppRegInfo appReg)
        {
            string stParentName = Path.GetDirectoryName(filename);

            // Do BMS - FULL.key file exists at User/Config?
            if (File.Exists(filename) == false)
            {
                MessageBoxResult result = MessageBox.Show
                    ("App could not find " + filename, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string[] lines = File.ReadAllLines(filename, Encoding.UTF8);

            keyAssign = new KeyAssign[lines.Length];

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
                keyAssign[i] = new KeyAssign(stBuffer);

                // What if the line format was broken?

                if (keyAssign[i].CheckFileCollapsing() == false)
                    continue;

                MessageBoxResult result = MessageBox.Show
                    ("App found " + appReg.GetKeyFileName() + " broken\nWould you like to restore it to the default?", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.OK)
                {
                    string fnamestock = appReg.GetInstallDir() + "\\Docs\\Key Files & Input\\" + appReg.GetKeyFileName();
                    string fname = appReg.GetInstallDir() + "\\User\\Config\\" + appReg.GetKeyFileName(); ;
                    if (File.Exists(fnamestock))
                    {
                        File.Copy(fnamestock, fname, true);
                        Application.Current.Shutdown();
                        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                        return;
                    }
                    MessageBox.Show("App could not find " + appReg.GetKeyFileName() + " at\nDocs\\Key Files & Input\\", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Application.Current.Shutdown();
                    return;
                }
                Application.Current.Shutdown();
                return;
            }
            Array.Resize(ref keyAssign, i+1);
        }

        public KeyFile(IReadOnlyList<KeyAssign> keyAssign)
        {
            this.keyAssign = new KeyAssign[keyAssign.Count];
            for (int i = 0; i < keyAssign.Count; i++)
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
