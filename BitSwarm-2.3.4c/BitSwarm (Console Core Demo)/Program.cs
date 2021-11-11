using System;
using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;

using SuRGeoNix.BitSwarmLib;
using SuRGeoNix.BitSwarmLib.BEP;

using BitSwarmOptions = SuRGeoNix.BitSwarmLib.Options;

namespace SuRGeoNix.BitSwarmConsole
{
    class Program
    {
        static BitSwarm         bitSwarm;
        static BitSwarmOptions  bitSwarmOptions;
        static Torrent          torrent;

        static bool             sessionFinished;
        static int              prevHeight;
        static object           lockRefresh     = new object();

        static View             view            = View.Stats;

        enum View
        {
            Peers,
            Stats,
            Torrent
        }

        private static void Main(string[] args)
        {
            var parser          = new Parser(with => with.HelpWriter = null);
            var parserResult    = parser.ParseArguments<Options>(args);
            parserResult.WithParsed<Options>(options => Run(options)).WithNotParsed(errs => PrintHelp(parserResult, errs));
        }
        private static void PrintHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h => { return HelpText.DefaultParsingErrorsHandler(result, h); }, e => e, false, 160);
            helpText.Heading = $"[BitSwarm v{BitSwarm.Version}]";
            helpText.AddPostOptionsText("\r\n" + "CONFIG: \t\t\t\t  Creates config file with default options (Place it under -> . | $HOME/.bitswarm | /etc/bitswarm | %APPDATA%/BitSwarm)\r\n.        \t\t\t\t  If [OPTIONS] are defined will override config values (Notice: you can use env variables even on unix %var%)\r\n\r\n\t" + $"bitswarm config\r\n");
            helpText.AddPostOptionsText("\r\n" + "USAGE:  \r\n\r\n\t" + $"bitswarm [OPTIONS] Torrent|Magnet|Hash|Session");            
            Console.WriteLine(helpText);
        }
        private static void PrintMenu() { Console.WriteLine("[1: Stats] [2: Torrent] [3: Peers] [4: Peers (w/Refresh)] [Ctrl-C: Exit]".PadLeft(100, ' ')); }

        private static void Run(Options userOptions)
        {
            try
            {
                Console.WriteLine($"[BitSwarm v{BitSwarm.Version}] Initializing ...");

                if (userOptions.Input == "config")
                {
                    BitSwarmOptions.CreateConfig(new BitSwarmOptions());
                    Console.WriteLine($"[BitSwarm v{BitSwarm.Version}] Config {BitSwarmOptions.ConfigFile} created.");
                    return;
                }
                else if ((bitSwarmOptions = BitSwarmOptions.LoadConfig()) != null)
                    Console.WriteLine($"[BitSwarm v{BitSwarm.Version}] Config {BitSwarmOptions.ConfigFile} loaded.");

                if (bitSwarmOptions == null) bitSwarmOptions = new BitSwarmOptions();
                if (!Options.ParseOptionsToBitSwarm(userOptions, ref bitSwarmOptions)) return;

                // BitSwarm [Create | Subscribe | Open | Start]
                bitSwarm = new BitSwarm(bitSwarmOptions);

                bitSwarm.MetadataReceived   += BitSwarm_MetadataReceived;   // Receives torrent data [on torrent file will fire directly, on magnetlink will fire on metadata received]
                bitSwarm.StatsUpdated       += BitSwarm_StatsUpdated;       // Stats refresh every 2 seconds
                bitSwarm.StatusChanged      += BitSwarm_StatusChanged;      // Paused/Stopped or Finished

                bitSwarm.Open(userOptions.Input);
                bitSwarm.Start();

                // Stats | Torrent | Peers Views [Until Stop or Finish]
                ConsoleKeyInfo cki;
                Console.TreatControlCAsInput = true;
                prevHeight = Console.WindowHeight;

                while (!sessionFinished)
                {
                    try
                    {
                        cki = Console.ReadKey();

                        if (sessionFinished) break;
                        if ((cki.Modifiers & ConsoleModifiers.Control) != 0 && cki.Key == ConsoleKey.C) break;

                        lock (lockRefresh)
                        switch (cki.Key)
                        {
                            case ConsoleKey.D1:
                                view = View.Stats;
                                Console.Clear();
                                Console.WriteLine(bitSwarm.DumpStats());
                                PrintMenu();
                                break;

                            case ConsoleKey.D2:
                                view = View.Torrent;
                                Console.Clear();
                                Console.WriteLine(bitSwarm.DumpTorrent());
                                PrintMenu();

                                break;

                            case ConsoleKey.D3:
                                view = View.Torrent;
                                Console.Clear();
                                Console.WriteLine(bitSwarm.DumpPeers());
                                PrintMenu();

                                break;

                            case ConsoleKey.D4:
                                view = View.Peers;
                                Console.Clear();
                                Console.WriteLine(bitSwarm.DumpPeers());
                                PrintMenu();

                                break;

                            default:
                                break;
                        }
                    } catch (Exception) { }
                }

                // Dispose (force) BitSwarm
                if (bitSwarm != null) bitSwarm.Dispose(true);

            } catch (Exception e) { Console.WriteLine($"[ERROR] {e.Message}"); }
        }

        private static void BitSwarm_StatusChanged(object source, BitSwarm.StatusChangedArgs e)
        {
            if (e.Status == 0 && torrent != null && torrent.file.name != null)
            {
                Console.WriteLine($"\r\nDownload of {torrent.file.name} success!\r\n\r\n");
                Console.WriteLine(bitSwarm.DumpTorrent());
                Console.WriteLine($"\r\nDownload of {torrent.file.name} success!\r\n\r\n");
            }
            else if (e.Status == 2)
                Console.WriteLine("An error has been occured :( \r\n" + e.ErrorMsg);

            bitSwarm?.Dispose();
            bitSwarm = null;
            sessionFinished = true;
        }
        private static void BitSwarm_MetadataReceived(object source, BitSwarm.MetadataReceivedArgs e)
        {
            lock (lockRefresh)
            {
                torrent = e.Torrent;
                view    = View.Torrent;
                Console.Clear();
                Console.WriteLine(bitSwarm.DumpTorrent());
                PrintMenu();
            }

            System.Threading.Thread tmp = new System.Threading.Thread(() =>
            {
                lock (lockRefresh)
                {
                    System.Threading.Thread.Sleep(3000);
                    Console.Clear();
                    Console.WriteLine(bitSwarm.DumpStats());
                    PrintMenu();
                    view = View.Stats;
                }
            });
            tmp.IsBackground = true;
            tmp.Start();
        }
        private static void BitSwarm_StatsUpdated(object source, BitSwarm.StatsUpdatedArgs e)
        {
            try
            {
                if (view != View.Stats && view != View.Peers) return;

                if (Console.WindowHeight != prevHeight) { prevHeight = Console.WindowHeight; Console.Clear(); }

                lock (lockRefresh)
                if (view == View.Peers)
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(bitSwarm.DumpPeers());
                }
                else if (view == View.Stats)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(bitSwarm.DumpStats());
                
                }

                PrintMenu();

            } catch (Exception) { }
        }
    }
}