using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace FalconBMS.Launcher
{
    /// <summary>
    /// App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is NotImplementedException)
            {
                MessageBox.Show("This feature has not yet been implemented.", "Feature not Implemented",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                e.Handled = true;


                Diagnostics.Log(e.Exception);
            }

            else
            {
                // Skip this step if debugging so the debugger can catch errors.
                if (Debugger.IsAttached) return;

                MessageBox.Show("An unknown error has occured. Contact support if this problem persists.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                e.Handled = true;

                Diagnostics.Log(e.Exception);
            }
        }
    }
}
