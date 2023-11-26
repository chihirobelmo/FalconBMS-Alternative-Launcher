using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using System.Runtime.InteropServices;
using FalconBMS.Launcher.Windows;

namespace FalconBMS.Launcher
{
    public class Program
    {
        internal static string thisExeDir = null;
        internal static MainWindow mainWin = null;
        internal static Window activeWin = null;

        [STAThread]
        public static void Main()
        {
            try
            {
                // Set cwd to the EXE location.
                string thisExe = Assembly.GetExecutingAssembly().Location;
                thisExeDir = Path.GetDirectoryName(thisExe);

                Environment.CurrentDirectory = thisExeDir;

                // Launch the WPF app.
                AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
                App.Main();
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                Diagnostics.ShowErrorMsgbox(ex);
            }
            finally
            {
                Diagnostics.Log("Process exiting - closing logfile.");
                Diagnostics.FinalizeLogfile();
            }
            return;
        }

        internal static bool? ShowDialogAndMakeActive(Window dialog)
        {
            Window prevActive = Program.activeWin;
            dialog.Owner = prevActive;
            try
            {
                Program.activeWin = dialog;
                bool? result = dialog.ShowDialog();
                return result;
            }
            finally
            {
                Program.activeWin = prevActive;
            }
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";

            try
            {
                if (assemblyName.CultureInfo == null)
                {
                    assemblyName.CultureInfo = CultureInfo.InvariantCulture;
                }

                if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
                {
                    path = $@"{assemblyName.CultureInfo}\{path}";
                }
            }

            catch (NullReferenceException ex)
            {
                Debug.Print($"Null Reference Exception Occured: {ex.InnerException} \n {ex.Message}");
            }


            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
