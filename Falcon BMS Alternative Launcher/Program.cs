using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace FalconBMS.Launcher
{
    public class Program
    {
        /// <summary>
        ///     Main
        /// </summary>
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            App.Main();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is NotImplementedException)
            {
                Exception ex = e.ExceptionObject as Exception;

                MessageBox.Show("This feature has not yet been implemented.", "Feature not Implemented",
                    MessageBoxButton.OK, MessageBoxImage.Information);
               
                
                //ex.Handled = true;
            }

            else
            {
                // Skip this step if debugging so the debugger can catch errors.
                if (Debugger.IsAttached) return;

                MessageBox.Show("An unknown error has occured. Contact support if this problem persists.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                //e.Handled = true;
            }
        }

        private static void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            
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
