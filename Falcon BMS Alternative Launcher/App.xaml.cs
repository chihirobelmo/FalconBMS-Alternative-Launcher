using FalconBMS.Launcher.Windows;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Security.Principal;
using System.Security.AccessControl;
using ControlzEx.Standard;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Documents;
using System.Collections.Generic;

namespace FalconBMS.Launcher
{
    /// <summary>
    /// App.xaml
    /// </summary>
    public partial class App
    {

        private const string UniquePipeName = "falcon-al-pipe";

        private const string UniqueMutexName = "d3946cb1-8a0a-4ae3-83a0-ccd66940512a";

        private Mutex mutex;

        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            bool isOwned;
             this.mutex = new Mutex(true, UniqueMutexName, out isOwned);

             GC.KeepAlive(this.mutex);

            if (isOwned)
            {
                // create main window
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                // Spawn a thread which will be waiting for our event
                WaitToReceiveArguments();

            }
            else
            {
                SendArgumentsToRunningInstance(e.Args);
                this.Shutdown();
            }

        }

        public App()
        {
            Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            Diagnostics.WriteLogFile(false, "Log Start");
            Diagnostics.Log("Application Initialization completed successfully.", Diagnostics.LogLevels.Info);
        }

        private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is NotImplementedException)
            {
                MessageBox.Show("This feature has not yet been implemented.", "Feature not Implemented",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                e.Handled = true;
            }

            else
            {
                // Skip this step if debugging so the debugger can catch errors.
                if (Debugger.IsAttached) return;

                MessageBox.Show("An unknown error has occured. Contact support if this problem persists.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Diagnostics.Log(e.Exception);
                Diagnostics.WriteLogFile();

                e.Handled = true;
            }
        }


        private void WaitToReceiveArguments()
        {
            var thread = new Thread(() =>
            {
                PipeSecurity ps = new PipeSecurity();
                SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
                PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
                ps.AddAccessRule(par);
                var pipeServer = new NamedPipeServerStream(UniquePipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.None, 512, 512, ps);

                do
                {
                    try
                    {
                        StreamReader sr = new StreamReader(pipeServer);
                        pipeServer.WaitForConnection();

                        List<string> args = new List<string>();

                        while (sr.Peek() > 0)
                        {
                            args.Add(sr.ReadLine());
                        }

                        Current.Dispatcher.BeginInvoke((Action)(() => ((MainWindow)Current.MainWindow).BringToForeground(args.ToArray())));
                    }

                    catch (Exception ex) { throw ex; }

                    finally
                    {
                        if (pipeServer.IsConnected) { pipeServer.Disconnect();  }
                    }
                } while (true);
            });
                
                
                thread.IsBackground = true;
                thread.Start();
        }

        private void SendArgumentsToRunningInstance(string[] args)
        {
            var pipeClient = new NamedPipeClientStream(".",
              UniquePipeName, PipeDirection.Out, PipeOptions.None);

            if (pipeClient.IsConnected != true) { pipeClient.Connect(); }

            StreamWriter sw = new StreamWriter(pipeClient);

            foreach (string arg in args)
            {
                sw.WriteLine(arg);
            }

            sw.Flush();
            pipeClient.Close();
        }
    }
}

