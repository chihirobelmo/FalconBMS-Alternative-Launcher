using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;

using BencodeNET.Objects;
using SuRGeoNix.Partfiles;

using SuRGeoNix.BitSwarmLib.BEP;

using static SuRGeoNix.BitSwarmLib.BEP.Torrent.TorrentData;

namespace SuRGeoNix.BitSwarmLib
{
    /// <summary>
    /// Bittorrent protocol v2 implementation
    /// </summary>
    public class BitSwarm
    {
        /// <summary>
        /// Library version
        /// </summary>
        public static string Version { get; private set; } = Regex.Replace(Assembly.GetExecutingAssembly().GetName().Version.ToString(), @"\.0$", "");

        #region Enums
        /// <summary>
        /// BitSwarm's valid input types otherwise unknown
        /// </summary>
        public enum InputType
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            Torrent,
            Magnet,
            Sha1,
            Base32,
            Session,
            Unkown
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
        private enum SleepModeState
        {
            Automatic,
            Manual,
            Disabled
        }
        private enum Status
        {
            RUNNING     = 0,
            PAUSED      = 1,
            STOPPED     = 2
        }

        internal enum PeersStorage
        {
            DHT,
            PEX,
            TRACKER
        }
        #endregion

        #region Event Handlers
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Fires when all included pieces have been received and allows to include more incomplete files and prevent it from finishing (e.Cancel = true)
        /// </summary>
        public event FinishingHandler OnFinishing;
        public delegate void FinishingHandler(object source, FinishingArgs e);
        public class FinishingArgs
        {
            public bool Cancel { get; set; }
            public FinishingArgs(bool cancel)
            {
                Cancel = cancel;
            }
        }

        /// <summary>
        /// Fires when metadata are available (e.Torrent). If input is a torrent or session file will fire direclty after Open. Can be used to specify included files
        /// </summary>
        public event MetadataReceivedHandler MetadataReceived;
        public delegate void MetadataReceivedHandler(object source, MetadataReceivedArgs e);
        public class MetadataReceivedArgs
        {
            public Torrent Torrent { get; set; }
            public MetadataReceivedArgs(Torrent torrent)
            {
                Torrent = torrent;
            }
        }
        
        /// <summary>
        /// Fires when finished (e.Status). If Status is 0 the download has been completed, if Status is 1 the sessions has been stopped and if Status is 2 an error has been occurred (e.ErrorMsg)
        /// </summary>
        public event StatusChangedHandler StatusChanged;
        public delegate void StatusChangedHandler(object source, StatusChangedArgs e);
        public class StatusChangedArgs : EventArgs
        {
            public int      Status      { get; set; } // 0: Stopped, 1: Finished, 2: Error + Msg
            public string   ErrorMsg    { get; set; }
            public StatusChangedArgs(int status, string errorMsg = "")
            {
                Status     = status;
                ErrorMsg   = errorMsg;
            }
        }

        /// <summary>
        /// Fires after receiving metdata, every 2 seconds with the fresh stats for connections/bytes/speed (e.Stats)
        /// </summary>
        public event StatsUpdatedHandler StatsUpdated;
        public delegate void StatsUpdatedHandler(object source, StatsUpdatedArgs e);
        public class StatsUpdatedArgs : EventArgs
        {
            public Stats Stats { get; set; }
            public StatsUpdatedArgs(Stats stats)
            {
                Stats = stats;
            }
        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        #endregion

        #region Public Properties Exposed
        public   Options        Options;        // Live Changes
        internal Options        OptionsClone;   // Live Changes not allowed (will be reloaded on restart if required)

        public Stats            Stats;
        public bool             isRunning       => status == Status.RUNNING;
        public bool             isPaused        => status == Status.PAUSED;
        public bool             isStopped       => status == Status.STOPPED;
        public bool             isDHTRunning    => dht.status == DHT.Status.RUNNING;
        public string           LoadedSessionFile   { get; set; }

        public bool             FocusAreInUse       { get; set; }
        public Tuple<int, int>  FocusArea           { get { lock (lockerTorrent) return focusArea; } set { lock (lockerTorrent) focusArea = value; } }

        internal Tuple<int, int> focusArea;
        internal Tuple<int, int> lastFocusArea;
        #endregion

        #region Declaration
        // Lockers
        readonly object  lockerTorrent       = new object();
        readonly object  lockerMetadata      = new object();

        // Generators (Hash / Random)
        private SHA1                    sha1                = new SHA1Managed();
        private static Random           rnd                 = new Random();
        internal byte[]                 peerID;

        // Main [Torrent / Trackers / Peers / Options]
        ThreadPool bstp;
        internal ConcurrentDictionary<string, int>  peersStored         {get; private set; }
        internal ConcurrentStack<Peer>              peersForDispatch    {get; private set; }

        public Torrent                  torrent;

        private List<Tracker>           trackers;
        private List<Uri>               trackersUnsorted;
        private Tracker.Options         trackerOpt;
        
        private DHT                     dht;                            
        private DHT.Options             dhtOpt;

        internal Logger                 log;
        private Logger                  logDHT;
        private Thread                  beggar;
        private Status                  status;

        private long                    metadataLastRequested;
        private bool                    wasPaused;
        private int                     curSecond           = 0;
        private int                     prevSecond          = 0;
        internal long                   prevStatsTicks      = 0;

        private long                    totalBytesDownloaded;
        private long                    totalBytesDownloadedPrev;
        #endregion

        #region Initiliaze | Setup
        private void Initiliaze()
        {
            OptionsClone = (Options) Options.Clone();

            // https://github.com/dotnet/runtime/issues/25792 ?

            OptionsClone.FolderComplete      = Environment.ExpandEnvironmentVariables(OptionsClone.FolderComplete);
            OptionsClone.FolderIncomplete    = Environment.ExpandEnvironmentVariables(OptionsClone.FolderIncomplete);
            OptionsClone.FolderTorrents      = Environment.ExpandEnvironmentVariables(OptionsClone.FolderTorrents);
            OptionsClone.FolderSessions      = Environment.ExpandEnvironmentVariables(OptionsClone.FolderSessions);

            if (OptionsClone.TrackersPath != null)
            OptionsClone.TrackersPath        = Environment.ExpandEnvironmentVariables(OptionsClone.TrackersPath);

            if (!Directory.Exists(OptionsClone.FolderComplete))  Directory.CreateDirectory(OptionsClone.FolderComplete);
            if (!Directory.Exists(OptionsClone.FolderIncomplete))Directory.CreateDirectory(OptionsClone.FolderIncomplete);
            if (!Directory.Exists(OptionsClone.FolderTorrents))  Directory.CreateDirectory(OptionsClone.FolderTorrents);
            if (!Directory.Exists(OptionsClone.FolderSessions))  Directory.CreateDirectory(OptionsClone.FolderSessions);

            bstp    = new ThreadPool();
            Stats   = new Stats();
            status  = Status.STOPPED;

            peerID                          = new byte[20]; rnd.NextBytes(peerID);
            peersStored                     = new ConcurrentDictionary<string, int>();
            peersForDispatch                = new ConcurrentStack<Peer>();
            trackers                        = new List<Tracker>();

            torrent                         = new Torrent(this);
            torrent.metadata.progress       = new Bitfield(20); // Max Metadata Pieces
            torrent.metadata.pieces         =  2;                // Consider 2 Pieces Until The First Response

            if (Options.Verbosity > 0)
            {
                log                         = new Logger(Path.Combine(OptionsClone.FolderComplete, "Logs", "session" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log"    ), true);
                if (Options.EnableDHT && Options.LogDHT)
                    logDHT                  = new Logger(Path.Combine(OptionsClone.FolderComplete, "Logs", "session" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_DHT.log"), true);
            }
        }
        private void Setup()
        {
            // DHT
            if (Options.EnableDHT)
            {
                dhtOpt                      = DHT.GetDefaultOptions();
                dhtOpt.Beggar               = this;
                dhtOpt.LogFile              = logDHT;
                dhtOpt.Verbosity            = Options.LogDHT ? Options.Verbosity : 0;
                dht                         = new DHT(torrent.file.infoHash, dhtOpt);
            }

            // Tracker
            trackerOpt                      = new Tracker.Options();
            trackerOpt.PeerId               = peerID;
            trackerOpt.InfoHash             = torrent.file.infoHash;
            trackerOpt.ConnectTimeout       = Options.HandshakeTimeout;
            trackerOpt.ReceiveTimeout       = Options.HandshakeTimeout;
            Tracker.Beggar                  = this;

            trackerOpt.LogFile              = log;
            trackerOpt.Verbosity            = Options.LogTracker ? Options.Verbosity : 0;

            // Peer
            BDictionary extensions = new BDictionary();
            extensions.Add("ut_metadata", Peer.Messages.EXT_UT_METADATA);
            if (Options.EnablePEX) extensions.Add("ut_pex", Peer.Messages.EXT_UT_PEX);

            Peer.HANDSHAKE_BYTES            = Utils.ArrayMerge(Peer.BIT_PROTO, Peer.EXT_PROTO, Utils.StringHexToArray(torrent.file.infoHash), peerID);
            Peer.EXT_BDIC                   = (new BDictionary() { {"e", 0 }, { "m", extensions }, {"reqq", 250 }, {"v", $"BitSwarm {Version}" } }).EncodeAsBytes();
            Peer.Beggar                     = this;

            // Fill from TorrentFile | MagnetLink + TrackersPath to Trackers
            FillTrackersFromTorrent();

            // TODO: Local ISP SRV _bittorrent-tracker.<> http://bittorrent.org/beps/bep_0022.html

            // Metadata already done?
            if (  (torrent.file.length > 0      || (torrent.file.lengths != null && torrent.file.lengths.Count > 0))  
                && torrent.file.pieceLength > 0 && (torrent.file.pieces  != null && torrent.file.pieces. Count > 0))
            {
                torrent.metadata.isDone = true;
                Stats.PiecesIncluded    = torrent.data.pieces;
                Stats.BytesIncluded     = torrent.data.totalSize;
                Log("Dumping Torrent\r\n" + DumpTorrent());
                MetadataReceived?.Invoke(this, new MetadataReceivedArgs(torrent));
            }
        }
        #endregion

        #region Public Methods Exposed [Ctor | Open | Start | Pause | Dispose | IncludeFiles | Dumps]
        /// <summary>
        /// Creates a new instance of BitSwarm (subscribe to events before Open/Start)
        /// </summary>
        /// <param name="opt">BitSwarm's main options (can be changed later)</param>
        public BitSwarm(Options opt = null) { Options = (opt == null) ? new Options() : opt; }

        /// <summary>
        /// Opens a Torrent File, Magnet Link, SHA-1 Hex Hash, Base32 Hash or a previously Saved Session (.bsf).
        /// Searches for -infoHash-.bsf file in OptionsStatic.FolderSessions and restores it automatically
        /// </summary>
        /// <param name="input">Torrent File, Magnet Link, SHA-1 Hex Hash, Base32 Hash or Saved Session File</param>
        public void Open(string input)
        {
            if (input == null || input.Trim() == "") throw new Exception("No input has been provided");
            input = input.Trim();

            Initiliaze();

            bool isSessionFile = false;
            //bool isTorrentFile = false;
            BDictionary bInfo  = null;

            // Check Torrent | Session
            if (File.Exists(input))
            {
                FileInfo fi = new FileInfo(input);

                if (fi.Extension.ToLower() == ".bsf")
                    isSessionFile = true;
                else if (fi.Extension.ToLower() == ".torrent")
                    bInfo = torrent.FillFromTorrentFile(input);
                else
                    throw new Exception("Unknown file format has been provided");
            }

            // Check MagnetLink | SHA-1 Hash | Base32
            else
            {
                if (Regex.IsMatch(input, @"^magnet:", RegexOptions.IgnoreCase))
                    torrent.FillFromMagnetLink(new Uri(input));
                else if (Regex.IsMatch(input, @"^[0-9a-f]+$", RegexOptions.IgnoreCase))
                {
                    torrent.file.infoHash = input;
                    if (torrent.file.infoHash.Length != 40) throw new Exception("[SHA-1] No valid hash found");

                }
                else if (Regex.IsMatch(input, @"^[2-7a-z]+=*$", RegexOptions.IgnoreCase))
                {
                    torrent.file.infoHash = Utils.ArrayToStringHex(Utils.FromBase32String(input));
                    if (torrent.file.infoHash.Length != 40) throw new Exception("[BASE32] No valid hash found");
                }
                else
                    throw new Exception("Unknown string input has been provided");
            }

            string sessionFilePath = !isSessionFile ? Path.Combine(OptionsClone.FolderSessions, torrent.file.infoHash.ToUpper() + ".bsf") : ""; //  string.Join("_", torrent.file.name.Split(Path.GetInvalidFileNameChars()));

            // Input file is Session or a saved Session found in temp (for provided hash)
            if (isSessionFile || File.Exists(sessionFilePath))
            {
                LoadedSessionFile = isSessionFile ? input : sessionFilePath;
                torrent = Torrent.LoadSession(LoadedSessionFile);
                if (torrent == null) throw new Exception($"Unable to load session file {LoadedSessionFile}");
                torrent.bitSwarm = this;
                torrent.FillFromSession();
            }

            // Input file is Torrent
            else if (bInfo != null)
                torrent.FillFromInfo(bInfo);

            Setup();
        }
        /// <summary>
        /// Starts BitSwarm
        /// </summary>
        public void Start()
        {
            if (status == Status.RUNNING || (torrent.data.progress != null && torrent.data.progress.GetFirst0() == - 1)) return;
            
            wasPaused           = status == Status.PAUSED;
            status              = Status.RUNNING;
            Stats.EndGameMode   = false;
            torrent.data.isDone = false;

            Utils.EnsureThreadDoneNoAbort(beggar);

            beggar = new Thread(() =>
            {
                Beggar();

                if (torrent.data.isDone)
                    StatusChanged?.Invoke(this, new StatusChangedArgs(0));
                else
                    StatusChanged?.Invoke(this, new StatusChangedArgs(1));
            });

            beggar.IsBackground = true;
            beggar.Start();
        }
        /// <summary>
        /// Pauses BitSwarm &amp; Disposes Threadpool
        /// </summary>
        public void Pause()
        {
            if (status == Status.PAUSED) return;

            status = Status.PAUSED;
            Utils.EnsureThreadDoneNoAbort(beggar, 1500);
            bstp.Dispose();
        }
        /// <summary>
        /// Stops BitSwarm &amp; Disposes
        /// </summary>
        /// <param name="force">To avoid waiting for proper disposing</param>
        public void Dispose(bool force = false)
        {
            /* TODO
             * In case of Stop and not Abort we could do an end game in the current working pieces to avoid loosing those bytes
             *      Another solution would be to ensure proper dispose/close and serialize also working pieces (and load them back to session) | more possibilities for corrupted data but will be validated in SHA-1 validation
             */

            try
            {
                status = Status.STOPPED;

                if (!force)
                    Utils.EnsureThreadDoneNoAbort(beggar, 1500);
                else
                    bstp.Stop = true;

                lock (lockerTorrent)
                {
                    if (torrent != null) torrent.Dispose();
                    if (logDHT  != null) logDHT. Dispose();
                    if (log     != null) log.    Dispose();
                    bstp.Threads = null;
                    peersStored?.Clear();
                    peersForDispatch?.Clear();
                }
            } catch (Exception) { }
        }
        /// <summary>
        /// Forces BitSwarm to download only the specified files (can be called at any time)
        /// </summary>
        /// <param name="includeFiles">Downloads only the specified files (from torrent.file.paths)</param>
        public void IncludeFiles(List<string> includeFiles)
        {
            if (!torrent.metadata.isDone) return;

            Bitfield newProgress = new Bitfield(torrent.data.pieces);
            newProgress.SetAll();

            Stats.PiecesIncluded    = 0;
            Stats.BytesIncluded     = 0;
            long curDistance        = 0;

            lock (lockerTorrent)
            {
                for (int i=0; i<torrent.file.paths.Count; i++)
                {
                    // Is Included
                    if (includeFiles.Contains(torrent.file.paths[i]))
                    {
                        Stats.BytesIncluded += torrent.file.lengths[i];

                        // Was Excluded?    Prev -> New
                        if (!torrent.data.filesIncludes.Contains(torrent.file.paths[i]))
                            newProgress.CopyFrom(torrent.data.progressPrev, (int) (curDistance/torrent.file.pieceLength), (int) ((curDistance + torrent.file.lengths[i])/torrent.file.pieceLength));
                        // Was Included?    Cur  -> New
                        else
                            newProgress.CopyFrom(torrent.data.progress,     (int) (curDistance/torrent.file.pieceLength), (int) ((curDistance + torrent.file.lengths[i])/torrent.file.pieceLength));
                    }

                    // Is Excluded
                    else
                    {
                        // Was Included?    Cur -> Prev
                        if (torrent.data.filesIncludes.Contains(torrent.file.paths[i]))
                            torrent.data.progressPrev.CopyFrom(torrent.data.progress, (int) (curDistance/torrent.file.pieceLength), (int) ((curDistance + torrent.file.lengths[i])/torrent.file.pieceLength));
                    }

                    curDistance += torrent.file.lengths[i];
                }
                
                torrent.data.filesIncludes  = includeFiles;
                torrent.data.progress       = newProgress;
                CloneProgress();

                for (int i=0; i<torrent.data.filesIncludes.Count; i++)
                    Log($"[FILE INCLUDED] {torrent.data.filesIncludes[i]}");

                Stats.PiecesIncluded += (int) ((Stats.BytesIncluded/torrent.file.pieceLength) + 1);
            }
        }
        internal void CloneProgress()
        {
            torrent.data.requests = torrent.data.progress.Clone();

            // Might keep alot pieceProgress open (Working Pieces) | Memory issue? | From the other hand we dont loose those blocks
            foreach (var pieceBlocks in torrent.data.pieceProgress.Values)
                pieceBlocks.requests = pieceBlocks.progress.Clone();
        }

        /// <summary>
        /// Checks if the provided input could be handled by BitSwarm
        /// </summary>
        /// <param name="input">Torrent File, Magnet Link, SHA-1 Hex Hash, Base32 Hash or Saved Session File</param>
        /// <returns>Identified InputType or InputType.Unknown</returns>
        public static InputType ValidateInput(string input)
        {
            if (input == null || input.Trim() == "")        return InputType.Unkown;

            input = input.Trim();

            if (File.Exists(input))
            {
                FileInfo fi = new FileInfo(input);
                if (fi.Extension.ToLower() == ".bsf")       return InputType.Session;
                if (fi.Extension.ToLower() == ".torrent")   return InputType.Torrent;

                return InputType.Unkown;
            }

            if (Regex.IsMatch(input, @"^magnet:",       RegexOptions.IgnoreCase)) return InputType.Magnet;
            if (Regex.IsMatch(input, @"^[0-9abcdef]+$", RegexOptions.IgnoreCase) && input.Length == 40) return InputType.Sha1;
            if (Regex.IsMatch(input, @"^[a-z2-7]+$",    RegexOptions.IgnoreCase) && Utils.ArrayToStringHex(Utils.FromBase32String(input)).Length == 40) return InputType.Base32;

            return InputType.Unkown;
        }

        public string DumpPeers(int sortBy = 2)
        {
            Dictionary<string, string> peersDump = new Dictionary<string, string>();

            lock (peersForDispatch)
                foreach (var thread in bstp.Threads)
                {
                    if (!thread.isLongRun || thread.peer == null) continue;

                    Peer peer = thread.peer;

                    if (peer.status == Peer.Status.DOWNLOADING)
                    {
                        string verA = "";
                        string verB = "";

                        if (peer.stageYou != null && peer.stageYou.version != null)
                        {
                            int vPos = peer.stageYou.version.LastIndexOf('/');
                            if (vPos == -1) vPos = peer.stageYou.version.LastIndexOf(' ');

                            verA = vPos == -1 ? peer.stageYou.version : peer.stageYou.version.Substring(0, vPos); 
                            verB = vPos == -1 ? "" : peer.stageYou.version.Substring(vPos + 1);

                            if (verA.Length > 25) verA = verA.Substring(0, 25);
                            if (verB.Length > 11) verA = verA.Substring(0, 11);
                        }

                        long   downRate =   peer.GetDownRate()/1024;
                        string peerDump =   $"[{peer.host}".PadRight(40, ' ') + ":" + PadStats($"{peer.port}]", 6) + " " + 
                                            $"[{verA}".PadRight(26, ' ') + PadStats($"{verB}]", 12) + " " + 
                                            $"[{String.Format("{0:n0}", downRate).PadLeft(6, ' ')} KB/s]";

                        //if (sortBy == 2)
                        peersDump.Add(peerDump, downRate.ToString());
                    }
                }

            string dump = "";
            //if (sortBy == 2)
            foreach (var peerDump in peersDump.OrderBy(x => int.Parse(x.Value)))
                dump += peerDump.Key + "             \r\n";

            
            dump += "\r\n" + ($"[{Stats.PeersDownloading}/{Stats.PeersChoked} D/W] [{String.Format("{0:n0}", Stats.DownRate/1024).PadLeft(6, ' ')} KB/s]").PadLeft(100, ' ') + "        \r\n";

            return dump;
        }
        public string DumpTorrent()
        {
            if (torrent == null || !torrent.metadata.isDone) return "Metadata not ready";

            string str = "";
            str += "===============\n";
            str += "Torrent Details\n";
            str += "===============\n";
            str += $"(Pieces/Blocks: {torrent.data.pieces}/{torrent.data.blocks} | Piece Size: {torrent.data.pieceSize}/{torrent.data.totalSize % torrent.data.pieceSize} | Block Size: {torrent.data.blockSize}/{torrent.data.blockLastSize})\n";
            str += "\n";
            str += $"Torrent Hash : {torrent.file.infoHash}\n";
            str += $"Torrent Name : {torrent.file.name}\n";
            str += $"Torrent Size : {Utils.BytesToReadableString(torrent.data.totalSize)}\n";
            str += "\n";
            str += "-----\n";
            str += "Files\n";
            str += "-----\n";

            str += $"- {OptionsClone.FolderComplete}\n";

            for (int i=0; i<torrent.file.paths.Count; i++)
                str += $"+ {PadStats(Utils.BytesToReadableString(torrent.file.lengths[i]), 8)} | {torrent.file.paths[i]}\n";

            return str;
        }
        public string DumpStats()
        {
            string mode = "NRM";
            if (Stats.SleepMode)
                mode = "SLP";
            else if (Stats.BoostMode)
                mode = "BST";
            else if (Stats.EndGameMode)
                mode = "END";

            int alignCenter = torrent.file.name.Length + 2 + ((100 - torrent.file.name.Length) / 2);
            string stats = $"{("[" + torrent.file.name + "]").PadLeft(alignCenter,'=').PadRight(100, '=')}\n";

            string includedBytes = $" ({Utils.BytesToReadableString(Stats.BytesCurDownloaded)} / {Utils.BytesToReadableString(Stats.PiecesIncluded * torrent.data.pieceSize)})"; // Not the actual file sizes but pieces size (- first/last chunks)
            stats += "\n";
            stats += $"BitSwarm  " +
                $"{PadStats(TimeSpan.FromSeconds(curSecond).ToString(@"hh\:mm\:ss"), 15)} | " + 
                $"{PadStats("ETA " + TimeSpan.FromSeconds(Stats.AvgETA).ToString(@"hh\:mm\:ss"), 20)} | " +
                $"{Utils.BytesToReadableString(Stats.BytesDownloaded + Stats.BytesDownloadedPrevSession)} / {Utils.BytesToReadableString(torrent.data.totalSize)}{(Stats.PiecesIncluded == torrent.data.pieces ? "" : includedBytes)}           ";
                

            stats += "\n";
            stats += $"v{Version}".PadRight(9, ' ') +
                $"{PadStats(String.Format("{0:n0}", Stats.DownRate/1024), 11)} KB/s | " +
                $"{PadStats(String.Format("{0:n1}", ((Stats.DownRate * 8)/1000.0)/1000.0), 15)} Mbps | " +
                $"Max: {String.Format("{0:n0}", Stats.MaxRate/1024)} KB/s, {String.Format("{0:n0}", ((Stats.MaxRate * 8)/1000.0)/1000.0)} Mbps            ";

            stats += "\n";
            stats += $"         " +
                $"{PadStats($"Mode: {mode}", 15)}  | " +
                $"{PadStats(" ", 20)} | " +
                $"Avg: {String.Format("{0:n0}", Stats.AvgRate/1024)} KB/s, {String.Format("{0:n0}", ((Stats.AvgRate * 8)/1000.0)/1000.0)} Mbps            ";

            int progressLen = Stats.Progress.ToString().Length;
            stats += "\n";
            for (int i=0; i<100; i++)
            {
                if (i == 50 - progressLen) { stats += Stats.Progress + "%"; i += progressLen + 1; }
                stats += i < Stats.Progress ? "|" : "-";
            }

            stats += "\n";
            stats += $"[PEERS ] " +
                $"{PadStats($"{Stats.PeersDownloading}/{Stats.PeersChoked}", 11)} D/W  | " +
                $"{PadStats($"{Stats.PeersConnecting}/{Stats.PeersInQueue}/{Stats.PeersTotal}", 14)} C/Q/T | " +
                $"{Stats.PEXPeers}/{Stats.TRKPeers}/{Stats.DHTPeers} PEX/TRK/DHT {(dht.status == DHT.Status.RUNNING ? "(On)" : "(Off)")}                ";

            stats += "\n";
            stats += $"[PIECES] " +
                $"{PadStats($"{torrent.data.progress.setsCounter}/{torrent.data.pieces}", 11)} D/T  | " +
                $"{PadStats($"{torrent.data.requests.setsCounter}", 14)} REQ   | " +
                $"{Utils.BytesToReadableString(Stats.BytesDropped)} / {Stats.AlreadyReceived} BLK (Drops)             ";


            stats += "\n";
            stats += $"[ERRORS] " +
                $"{PadStats($"{Stats.PieceTimeouts}", 11)} TMO  | " +
                $"{PadStats($"{Stats.Rejects}", 14)} RJS   | " +
                $"{Stats.SHA1Failures} SHA                ";

            stats += "\n";
            for (int i=0; i<100; i++) stats += "=";
            stats += "\n";

            return stats;
        }
        static string PadStats(string str, int num) { return str.PadLeft(num, ' '); }
        #endregion

        #region Feeders [Torrent -> Trackers | Trackers -> Peers | DHT -> Peers | Client -> Stats]
        private  void FillStats()
        {
            try
            {
                // Stats
                Stats.CurrentTime           = DateTime.UtcNow.Ticks;

                // Progress
                int includedSetsCounter     = torrent.data.progress.setsCounter - (torrent.data.pieces - Stats.PiecesIncluded);            
                Stats.BytesCurDownloaded    = includedSetsCounter * torrent.data.pieceSize;
                if (Stats.BytesCurDownloaded < 0) Stats.BytesCurDownloaded = 0;
                Stats.Progress              = includedSetsCounter > 0 ? (int) (includedSetsCounter * 100.0 / Stats.PiecesIncluded) : 0;

                // Rates
                double secondsDiff          = ((Stats.CurrentTime - prevStatsTicks) / 10000000.0); // For more accurancy
                totalBytesDownloaded        = Stats.BytesDownloaded + Stats.BytesDropped; // Included or Not?
                Stats.DownRate              = (int) ((totalBytesDownloaded - totalBytesDownloadedPrev) / secondsDiff); // Change this (2 seconds) if you change scheduler
                Stats.AvgRate               = (int) ( totalBytesDownloaded / curSecond);
                if (Stats.DownRate > Stats.MaxRate) Stats.MaxRate = Stats.DownRate;

                // ETAs
                if (torrent.data.pieces != Stats.PiecesIncluded && curSecond > 0 && Stats.BytesCurDownloaded > 1)
                {
                    Stats.AvgETA = (int) ( (Stats.PiecesIncluded * torrent.data.pieceSize) / (Stats.BytesCurDownloaded / curSecond ) );
                }
                else
                {
                    if (torrent.data.totalSize - Stats.BytesDownloaded == 0)
                    {
                        Stats.ETA       = 0;
                        Stats.AvgETA    = 0;
                    } 
                    else
                    {
                        if (Stats.BytesDownloaded - totalBytesDownloadedPrev == 0)
                            Stats.ETA  *= 2; // Kind of infinite | int overflow nice
                        else 
                            Stats.ETA   = (int) ( (torrent.data.totalSize - Stats.BytesDownloaded) / ((Stats.BytesDownloaded - totalBytesDownloadedPrev) / secondsDiff) );

                        if (Stats.BytesDownloaded != 0 && curSecond > 0 && Stats.BytesDownloaded > 1)
                            Stats.AvgETA = (int) ( (torrent.data.totalSize - Stats.BytesDownloaded) / (Stats.BytesDownloaded / curSecond ) );
                    }
                }

                // Save Cur to Prev
                totalBytesDownloadedPrev    = totalBytesDownloaded;
                prevStatsTicks              = Stats.CurrentTime;

                // Stats & Clean-up
                Stats.PeersTotal        = peersStored.Count;
                Stats.PeersInQueue      = peersForDispatch.Count;
                Stats.PeersConnecting   = bstp.ShortRun;
                Stats.PeersConnected    = bstp.LongRun;
                Stats.PeersChoked       = 0;
                Stats.PeersUnChoked     = 0;
                Stats.PeersDownloading  = 0;

                lock (peersForDispatch)
                    foreach (var thread in bstp.Threads)
                    {
                        if (thread.isLongRun && thread.peer != null)
                        {
                            if (thread.peer.status == Peer.Status.READY)
                            {
                                if (thread.peer.stageYou.unchoked)
                                    Stats.PeersUnChoked++;
                                else
                                    Stats.PeersChoked++;
                            }
                            else if (thread.peer.status == Peer.Status.DOWNLOADING)
                            {
                                Stats.PeersDownloading++;
                            }
                        }
                    }

                // Stats -> UI
                StatsUpdated?.Invoke(this, new StatsUpdatedArgs(Stats));

                // Stats -> Log
                if (Options.LogStats) Log("Dumping Stats\r\n" + DumpStats());

            } catch (Exception e) { Log($"[CRITICAL ERROR] Msg: {e.Message}\r\n{e.StackTrace}"); }
        }
        private  void FillTrackersFromTorrent()
        {
            trackersUnsorted = new List<Uri>();

            for (int i=0; i<torrent.file.trackers.Count; i++)
                trackersUnsorted.Add(torrent.file.trackers[i]);

            if (OptionsClone.TrackersPath != null && File.Exists(OptionsClone.TrackersPath))
            {
                string[] trackersExternal = File.ReadAllLines(OptionsClone.TrackersPath);

                foreach (var tracker in trackersExternal)
                    try { trackersUnsorted.Add(new Uri(tracker)); } catch (Exception) { }
            }

            foreach (Uri uri in trackersUnsorted)
            {
                if (uri.Scheme.ToLower() == "http" || uri.Scheme.ToLower() == "https" || uri.Scheme.ToLower() == "udp")
                {
                    bool found = false;
                    foreach (var tracker in trackers)
                        if (tracker.uri.Scheme == uri.Scheme && tracker.uri.DnsSafeHost == uri.DnsSafeHost && tracker.uri.Port == uri.Port) { found = true; break; }
                    if (found) continue;

                    if (Options.LogTracker) Log($"[Torrent] [Tracker] [ADD] {uri}");
                    trackers.Add(new Tracker(uri, trackerOpt));
                }
                else
                    if (Options.LogTracker) Log($"[Torrent] [Tracker] [ADD] {uri} Protocol not implemented");
            }
        }
        private  void StartTrackers()
        {
            for (int i=0; i<trackers.Count; i++)
            {
                int cacheI = i;

                System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                {
                    trackers[cacheI].Announce(Options.PeersFromTracker);
                }), null);
            }
        }
        internal void FillPeers(Dictionary<string, int> newPeers, PeersStorage storage)
        {
            int countNew = 0;

            lock (peersForDispatch)
                foreach (KeyValuePair<string, int> peerKV in newPeers)
                {
                    if (!peersStored.ContainsKey(peerKV.Key) && !peersBanned.Contains(peerKV.Key))
                    {
                        peersStored.TryAdd(peerKV.Key, peerKV.Value);
                        Peer peer = new Peer(peerKV.Key, peerKV.Value);
                        peersForDispatch.Push(peer); //if (!bstp.Dispatch(peer)) peersForDispatch.Push(peer);
                        countNew++;   
                    }
                }

            if (peersForDispatch.Count > 0 && bstp.ShortRun < bstp.MinThreads)
            {
                for (int i=0; i< Math.Min(peersForDispatch.Count, bstp.MinThreads); i++)
                    bstp.Dispatch(null);
            }

            if (storage == PeersStorage.TRACKER)
                Stats.TRKPeers += countNew;
            else if (storage == PeersStorage.DHT)
                Stats.DHTPeers += countNew;
            else if (storage == PeersStorage.PEX)
                Stats.PEXPeers += countNew;

            if (Options.Verbosity > 0 && countNew > 0) Log($"[{storage.ToString()}] {countNew} Adding Peers");
        }
        internal void ReFillPeers()
        {
            lock (peersForDispatch)
            {
                if (peersForDispatch.Count > 0)
                    Console.WriteLine("Peers queue not empty! -> " + peersStored.Count);

                peersForDispatch.Clear();

                HashSet<string> peersRunning = new HashSet<string>();

                if (bstp.Threads != null)
                foreach (var thread in bstp.Threads)
                    if (thread.thread != null && thread.peer != null) peersRunning.Add(thread.peer.host);

                foreach (var peerKV in peersStored)
                {
                    if (!peersRunning.Contains(peerKV.Key) && !peersBanned.Contains(peerKV.Key))
                    {
                        Peer peer = new Peer(peerKV.Key, peerKV.Value);
                        peersForDispatch.Push(peer);
                    }
                }

                if (peersForDispatch.Count > 0 && bstp.ShortRun < bstp.MinThreads)
                {
                    for (int i=0; i< Math.Min(peersForDispatch.Count, bstp.MinThreads); i++)
                        bstp.Dispatch(null);
                }

            }
        }
        #endregion


        #region ******** BEGGAR *********
        private void Beggar()
        {
            try
            {
                if (Utils.IsWindows) Utils.TimeBeginPeriod(5);

                // BoostThreads & MaxThreads should be OptionsStatic however we dont use them anywhere else (using just Options to allow change after pause/start)
                bstp.Initialize(Math.Max(Options.MinThreads, Math.Min(Options.BoostThreads, Options.MaxThreads)), Options.MaxThreads, peersForDispatch);
                if (Options.BoostThreads > Options.MinThreads) { Stats.BoostMode = true; if (Options.Verbosity > 0) Log($"[MODE] Boost Activated"); }

                if (wasPaused)
                {
                    CloneProgress();
                    ReFillPeers();
                }

                if (Options.EnableTrackers) StartTrackers();
                if (Options.EnableDHT) { logDHT?.RestartTime(); dht.Start(); }

                curSecond               = 0;
                prevSecond              = 0;
                totalBytesDownloadedPrev= 0;
                Stats.MaxRate           = 0;
                Stats.StartTime         = DateTime.UtcNow.Ticks;
                Stats.CurrentTime       = Stats.StartTime;
                prevStatsTicks          = Stats.StartTime;
                metadataLastRequested   = -1;
                int queueEmptySec       = -1;

                log?.RestartTime();
                if (Options.Verbosity > 0) Log("[BEGGAR  ] " + status);

                while (status == Status.RUNNING)
                {
                    // Every 100ms Ensure BSTP ShortRun Will Be Filled (from peersForDispatch)
                    if (peersForDispatch.Count > 0 && bstp.ShortRun < bstp.MinThreads)
                        for (int i=0; i<Math.Min(peersForDispatch.Count, bstp.MinThreads); i++) bstp.Dispatch(null);

                    // Every Second
                    if (curSecond != prevSecond && curSecond > 1)
                    {
                        prevSecond = curSecond;

                        if (queueEmptySec == -1 && peersForDispatch.Count == 0)
                            queueEmptySec = curSecond;
                        else if (peersForDispatch.Count > 0)
                            queueEmptySec = -1;

                        // Every 10 Seconds of Empty Queue -> ReFill
                        else if (!Stats.SleepMode && queueEmptySec != -1 && curSecond - queueEmptySec > 10)
                        {
                            ReFillPeers();
                            queueEmptySec = -1;
                        }

                        if (!torrent.metadata.isDone)
                        {
                            // Every 1 Second [Check Request Timeouts (Metadata)]
                            if (metadataLastRequested != -1 && Stats.CurrentTime - metadataLastRequested > Options.MetadataTimeout * 10000) { OptionsClone.MetadataParallelReq += 2; Log($"[REQ ] [M] Timeout"); }
                        }
                        else
                        {
                            // Every 1 Second [Check Boost Mode]

                            // [Boost Mode]
                            if (Stats.BoostMode)
                            {
                                bool prevBoostMode  = Stats.BoostMode;
                                Stats.BoostMode     = curSecond <= Options.BoostTime;
                                if (Stats.BoostMode!= prevBoostMode)
                                {
                                    if (Options.Verbosity > 0) Log("[MODE] Boost" + (Stats.BoostMode ? "On" : "Off"));
                                    if (!Stats.BoostMode) bstp.SetMinThreads(Options.MinThreads);
                                }
                            }

                            // Every 2 Second [Stats, Clean-up, Sleep Mode]
                            if (curSecond % 2 == 0)
                            {
                                // [Stats, Clean-up] + [Keep Alive->Interested in FillStats]
                                FillStats();

                                // [Sleep Mode Auto = MaxRate x 3/4]
                                if (Options.SleepModeLimit == -1 && curSecond > Options.BoostTime * 2 && Stats.MaxRate > 0)
                                {
                                    int curLimit = ((Stats.MaxRate * 3)/4) / 1024;
                                    if (Options.SleepModeLimit != curLimit)
                                    {
                                        Options.SleepModeLimit = curLimit;
                                        if (Options.Verbosity > 0) Log($"[MODE] Sleep - New Limit at {Options.SleepModeLimit}");
                                    }
                                }
                                
                                // [Sleep Mode On/Off] | MinThreads/DHT/Trackers | Offset -+ 200 to avoid On/Off very often
                                if (Options.SleepModeLimit > 0 && !Stats.BoostMode && !Stats.EndGameMode)
                                {
                                    bool prevSleepMode  = Stats.SleepMode;
                                    Stats.SleepMode     = Stats.DownRate > 0 && Stats.DownRate / 1024 > Options.SleepModeLimit;

                                    if (Stats.SleepMode != prevSleepMode)
                                    {
                                        if (!prevSleepMode)
                                            Stats.SleepMode = Stats.DownRate > 0 && Stats.DownRate / 1024 > Options.SleepModeLimit + 200;
                                        else
                                            Stats.SleepMode = Stats.DownRate > 0 && Stats.DownRate / 1024 > Options.SleepModeLimit - 200;
                                    }

                                    if (Stats.SleepMode != prevSleepMode)
                                    {
                                        if (Options.Verbosity > 0) Log("[MODE] Sleep" + (Stats.SleepMode ? "On" : "Off"));

                                        if (Stats.SleepMode)
                                        {
                                            if (Options.EnableDHT)
                                            {
                                                if (Options.Verbosity > 0) Log("[DHT] Stopping (SleepMode)");
                                                dht.Stop();
                                            }
                                            bstp.SetMinThreads(Options.MinThreads / 2); // If we enable dispatching in every loop
                                        }
                                        else
                                        {
                                            bstp.SetMinThreads(Options.MinThreads); // If we enable dispatching in every loop

                                            if (Options.EnableDHT)
                                            {
                                                if (Options.Verbosity > 0) Log("[DHT] Restarting (SleepMode Off)");
                                                dht.Start();
                                            }
                                        }
                                    }
                                }

                            } // Every 2 Seconds

                        } // !Metadata Received
                            
                        // Every 3 Seconds  [EndGame Mode | Check DHT Stop/Start - 40 seconds]
                        if (curSecond % 3 == 0)
                        {
                            // [EndGame Mode] | Proper way in RequestPiece here is a backup plan
                            if (!Stats.EndGameMode && torrent.data.pieces > 0 && (torrent.data.pieces - torrent.data.progress.setsCounter) * torrent.data.blocks < 300)
                            {
                                if (Options.Verbosity > 0) Log($"[MODE] End Game On");
                                Stats.SleepMode     = false;
                                Stats.EndGameMode   = true;
                            }

                            // [Check DHT Stop/Start - 40 seconds]
                            if (Options.EnableDHT)
                            {
                                if (dht.status == DHT.Status.RUNNING && !Stats.BoostMode && !Stats.EndGameMode && Stats.CurrentTime - dht.StartedAt > 40 * 1000 * 10000)
                                {
                                    if (Options.Verbosity > 0) Log("[DHT] Stopping (40 secs of running)");
                                    dht.Stop();
                                }
                                else if (dht.status == DHT.Status.STOPPED && !Stats.SleepMode && Stats.PeersInQueue < 100 && Stats.CurrentTime - dht.StoppedAt > 40 * 1000 * 10000)
                                {
                                    if (Options.Verbosity > 0) Log("[DHT] Restarting (40 secs of idle)");
                                    dht.Start();
                                }
                            }
                        }

                        // Every 31 Seconds [Re-request Trackers]
                        if (curSecond % 31 == 0)
                        {
                            if (Options.EnableTrackers && ((!Stats.SleepMode && Stats.PeersInQueue < 100) || Stats.BoostMode))
                            {
                                if (Options.Verbosity > 0) Log("[Trackers] Restarting (every X seconds)");
                                StartTrackers();
                            }
                        }

                    } // Scheduler [Every Second]

                    Thread.Sleep(100);

                    Stats.CurrentTime = DateTime.UtcNow.Ticks;

                    if (Stats.CurrentTime - (Stats.StartTime + (curSecond * (long)10000000)) > 0) curSecond++;

                } // While

                if (Options.EnableDHT) dht.Stop();

                Stats.EndTime = Stats.CurrentTime;

                bstp.Stop = true;
                if (bstp.Threads != null)
                    foreach (var thread in bstp.Threads)
                        if (thread.thread != null && thread.peer != null) thread.peer.Disconnect();

                bstp.Dispose();

                Log("[BEGGAR  ] " + status);

            } catch (ThreadAbortException) {
            } catch (Exception e) { Log($"[CRITICAL ERROR] Msg: {e.Message}\r\n{e.StackTrace}"); StatusChanged?.Invoke(this, new StatusChangedArgs(2, e.Message + "\r\n"+ e.StackTrace)); }

            if (Utils.IsWindows) Utils.TimeEndPeriod(5);
        }
        #endregion


        #region Piece/Block [Timeout | Request | Receive | Reject | Save]

        // PieceBlock [Request Metadata/Torrent] | EndGame/FocusPoint/Normal
        internal void RequestPiece(Peer peer, bool imBeggar = false)
        {
            if (!isRunning) return;

            int piece, block, blockSize;

            // Metadata Requests (Until Done)
            if (!torrent.metadata.isDone)
            {
                if (peer.stageYou.metadataRequested) return;

                // Ugly Prison TBR | Start Game with Unknown Bitfield Size So they can start and not be prisoned here
                if (peer.stageYou.extensions.ut_metadata == 0)
                {
                    while(isRunning && !torrent.metadata.isDone) Thread.Sleep(25);
                    return;
                }

                // Ugly Prison TBR | Start Game with Unknown Bitfield Size So they can start and not be prisoned here
                else if (OptionsClone.MetadataParallelReq < 1)
                {
                    while(isRunning && !torrent.metadata.isDone && OptionsClone.MetadataParallelReq < 1) Thread.Sleep(25);
                    RequestPiece(peer);
                    return;
                }

                //lock (lockerMetadata)
                if (torrent.metadata.totalSize == 0)
                {
                    OptionsClone.MetadataParallelReq -= 2;
                    peer.RequestMetadata(0, 1);
                    Log($"[{peer.host.PadRight(15, ' ')}] [REQ ][M]\tPiece: 0, 1");
                }
                else
                {
                    piece = torrent.metadata.progress.GetFirst0();
                    if (piece < 0) return;

                    int piece2 = (piece + 1) >= torrent.metadata.progress.size ? -1 : torrent.metadata.progress.GetFirst0(piece + 1);

                    if (piece > torrent.metadata.pieces - 1 || piece2 > torrent.metadata.pieces - 1) return;

                    if (piece2 >= 0)
                    {
                        OptionsClone.MetadataParallelReq -= 2;
                        peer.RequestMetadata(piece, piece2);
                    }
                    else
                    {
                        OptionsClone.MetadataParallelReq--;
                        peer.RequestMetadata(piece);
                    }

                    Log($"[{peer.host.PadRight(15, ' ')}] [REQ ][M]\tPiece: {piece},{piece2}");
                }

                metadataLastRequested = DateTime.UtcNow.Ticks;
                peer.stageYou.metadataRequested = true;

                return;
            }

            // Torrent Requests
            if (peer.stageYou.haveNone || (!peer.stageYou.haveAll && peer.stageYou.bitfield == null) || !peer.stageYou.unchoked) return;

            //if (peer.PiecesRequested > 0) { Log($"CRITCAL PiecesRequested > 0 {peer.host}"); return; }

            List<Tuple<int, int, int>> requests = new List<Tuple<int, int, int>>();

            if (Stats.EndGameMode)
            {
                // Find [piece, block] combination of the last pieces
                List<int> piecesLeft;
                if (peer.stageYou.haveAll)
                    lock (lockerTorrent) piecesLeft = torrent.data.progress.GetAll0();
                else
                    lock (lockerTorrent) piecesLeft = torrent.data.progress.GetAll0(peer.stageYou.bitfield);

                if (piecesLeft.Count == 0) { peer.Disconnect(); return; }

                List<Tuple<int, int>> piecesBlocksLeft = new List<Tuple<int, int>>();

                foreach (int pieceLeft in piecesLeft)
                {
                    CreatePieceProgress(pieceLeft);
                    List<int> pieceBlocksLeft = torrent.data.pieceProgress[pieceLeft].progress.GetAll0();
                    foreach (int blockLeft in pieceBlocksLeft)
                        piecesBlocksLeft.Add(new Tuple<int, int>(pieceLeft, blockLeft));
                }

                // Choose Randomly | Review Randomness (probably an issue here)
                int requestsCounter = Math.Min(Options.BlockRequests, piecesBlocksLeft.Count);
                for (int i=0; i<requestsCounter; i++)
                {
                    int curPieceBlock = rnd.Next(i * (piecesBlocksLeft.Count / requestsCounter), (i + 1) * (piecesBlocksLeft.Count / requestsCounter));

                    piece = piecesBlocksLeft[curPieceBlock].Item1;
                    block = piecesBlocksLeft[curPieceBlock].Item2;
                    blockSize = GetBlockSize(piece, block);

                    if (Options.Verbosity > 1) Log($"[{peer.host.PadRight(15, ' ')}] [REQE][P]\tPiece: {piece} Block: {block} Offset: {block * torrent.data.blockSize} Size: {blockSize} Requests: {peer.PiecesRequested}");

                    requests.Add(new Tuple<int, int, int>(piece, block * torrent.data.blockSize, blockSize));
                }

                if (requests.Count > 0) { peer.RequestPiece(requests); Thread.Sleep(15); } // Avoid Reaching Upload Limits

                return;
            }

            lock (lockerTorrent)
                while (requests.Count < Options.BlockRequests) // Piece
                {
                    // PIECE: From Our Bitfield
                    if (peer.stageYou.haveAll)
                    {
                        if (focusArea == null)
                        {
                            piece = torrent.data.requests.GetFirst0();

                            if (piece < 0)
                            {
                                // In case we have missed to reset properly requests from 1 to 0 (on timeouts/rejects) -> End Game should be activated to solve the issue
                                if (!Stats.EndGameMode && torrent.data.requests.GetFirst0() < 0)
                                {
                                    Log("[MODE] FORCED End Game");
                                    Stats.SleepMode = false;
                                    Stats.EndGameMode = true;
                                    if (requests.Count == 0) RequestPiece(peer);
                                }

                                break;
                            }
                        }
                        else
                        {
                            // Should Also check for END Game

                            piece = torrent.data.requests.GetFirst0(focusArea.Item1, focusArea.Item2);

                            if (piece < 0)
                            {
                                Log($"[FOCUS {focusArea.Item1} - {focusArea.Item2}] Done");
                                lastFocusArea = new Tuple<int, int>(focusArea.Item1, focusArea.Item2);
                                focusArea = null;
                                continue;
                            }
                        }
                    }

                    // PIECE: From Peer's Bitfield
                    else
                    {
                        if (focusArea == null)
                            piece = torrent.data.requests.GetFirst01(peer.stageYou.bitfield);
                        else
                        {
                            piece = torrent.data.requests.GetFirst01(peer.stageYou.bitfield, focusArea.Item1, focusArea.Item2);

                            if (piece < 0)
                            {
                                if (torrent.data.requests.GetFirst0(focusArea.Item1, focusArea.Item2) < 0)
                                {
                                    Log($"[FOCUS {focusArea.Item1} - {focusArea.Item2}] Done");
                                    lastFocusArea = new Tuple<int, int>(focusArea.Item1, focusArea.Item2);
                                    focusArea = null;
                                    continue;
                                }

                                Log($"[FOCUS {focusArea.Item1} - {focusArea.Item2}] No Pieces Peer {peer.host}");
                                piece = torrent.data.requests.GetFirst01(peer.stageYou.bitfield);
                            }
                        }

                        if (piece < 0) { Log($"[DROP] No Pieces Peer {peer.host}"); peer.Disconnect(); return; }
                    }
                    
                    if (piece < 0) { Log($"CRITICAL {peer.host}"); peer.Disconnect(); return; }

                    CreatePieceProgress(piece);

                    // Possible validation torrent.data.pieceProgress[piece].requests.GetFirst0() < 0 (Piece Bit 0 , Block Bits 1s)

                    while (requests.Count < Options.BlockRequests) // Block
                    {
                        block = torrent.data.pieceProgress[piece].requests.GetFirst0();

                        // Piece Done
                        if (block < 0) break;

                        torrent.data.pieceProgress[piece].requests.SetBit(block);

                        blockSize = GetBlockSize(piece, block);

                        if (Options.Verbosity > 1) Log($"[{peer.host.PadRight(15, ' ')}] [" + (focusArea == null ? "REQ " : "REQF") +  $"][P]\tPiece: {piece} Block: {block} Offset: {block * torrent.data.blockSize} Size: {blockSize} Requests: {peer.PiecesRequested}");
                        requests.Add(new Tuple<int, int, int>(piece, block * torrent.data.blockSize, blockSize));

                        // Piece Done
                        if (torrent.data.pieceProgress[piece].requests.GetFirst0() == -1) { SetRequestsBit(piece); break; }
                    }
                }

            if (requests.Count > 0) peer.RequestPiece(requests); else Log($"{peer.host} No Pieces Requested");
        }
        internal void RequestFastPiece(Peer peer)
        {
            if (!isRunning) return;

            if (!torrent.metadata.isDone || peer.stageYou.haveNone) return; // TODO: Probably should also verify that they have the piece?

            List<Tuple<int, int, int>> requests = new List<Tuple<int, int, int>>();

            int piece = -1;
            int block, blockSize;

            lock (lockerTorrent)
            {
                for (int i=0; i<Options.BlockRequests; i++)
                {
                    for (int l=peer.allowFastPieces.Count-1; l>=0; l--)
                    {
                        if (torrent.data.requests.GetBit(peer.allowFastPieces[l]))
                            peer.allowFastPieces.RemoveAt(l);
                        else
                            { piece = peer.allowFastPieces[l];  break; }
                    }

                    if (peer.allowFastPieces.Count == 0) break;

                    CreatePieceProgress(piece);
                    block = torrent.data.pieceProgress[piece].requests.GetFirst0();
                    if (block < 0) { Log($"Shouldn't be here! Piece: {piece}"); break; }

                    torrent.data.pieceProgress[piece].requests.SetBit(block);
                    if (torrent.data.pieceProgress[piece].requests.GetFirst0() == -1) SetRequestsBit(piece);

                    blockSize = GetBlockSize(piece, block);

                    if (Options.Verbosity > 1) Log($"[{peer.host.PadRight(15, ' ')}] [REQA][P]\tPiece: {piece} Block: {block} Offset: {block * torrent.data.blockSize} Size: {blockSize} Requests: {peer.PiecesRequested}");

                    requests.Add(new Tuple<int, int, int>(piece, block * torrent.data.blockSize, blockSize));
                }
            }

            if (requests.Count > 0) peer.RequestPiece(requests);
        }

        // PieceBlock [Metadata Receive | Reject]
        internal void MetadataPieceReceived(byte[] data, int piece, int offset, int totalSize, Peer peer)
        {
            Log($"[{peer.host.PadRight(15, ' ')}] [RECV][M]\tPiece: {piece} Offset: {offset} Size: {totalSize}");

            lock (lockerMetadata)
            {
                OptionsClone.MetadataParallelReq  += 2;
                peer.stageYou.metadataRequested     = false;

                if (torrent.metadata.totalSize == 0)
                {
                    torrent.metadata.totalSize  = totalSize;
                    torrent.metadata.progress   = new Bitfield((totalSize/Peer.MAX_DATA_SIZE) + 1);
                    torrent.metadata.pieces     = (totalSize/Peer.MAX_DATA_SIZE) + 1;

                    Partfiles.Options opt   = new Partfiles.Options();
                    opt.Folder              = OptionsClone.FolderTorrents;
                    opt.PartFolder          = OptionsClone.FolderTorrents;
                    opt.AutoCreate          = false;
                    opt.Overwrite           = true;
                    opt.PartOverwrite       = true;

                    string metadataFileName = torrent.file.name == null ? torrent.file.infoHash + ".torrent" : Utils.GetValidFileName(torrent.file.name) + ".torrent";
                    torrent.metadata.file   = new Partfile(metadataFileName, Peer.MAX_DATA_SIZE, totalSize, opt);
                }

                if (torrent.metadata.progress.GetBit(piece)) { Log($"[{peer.host.PadRight(15, ' ')}] [RECV][M]\tPiece: {piece} Already received"); return; }

                if (piece == torrent.metadata.pieces - 1)
                {
                    torrent.metadata.file.WriteLast(piece, data, offset, data.Length - offset);
                } else
                {
                    torrent.metadata.file.Write(piece, data, offset);
                }

                torrent.metadata.progress.SetBit(piece);
            
                if (torrent.metadata.progress.setsCounter == torrent.metadata.pieces)
                {
                    // TODO: Validate Torrent's SHA-1 Hash with Metadata Info
                    OptionsClone.MetadataParallelReq = -1000;
                    Log($"Creating Metadata File {torrent.metadata.file.Filename}");
                    torrent.metadata.file.Create();
                    torrent.FillFromMetadata();
                    torrent.metadata.isDone = true; // Should be before (DumpTorrent & Invoke -> includeFiles etc)

                    Stats.PiecesIncluded    = torrent.data.pieces;
                    Stats.BytesIncluded     = torrent.data.totalSize;

                    Log("Dumping Torrent\r\n" + DumpTorrent());

                    // Invoke? hm...
                    MetadataReceived?.Invoke(this, new MetadataReceivedArgs(torrent));
                }
            }
        }
        internal void MetadataPieceRejected(int piece, string src)
        {
            OptionsClone.MetadataParallelReq += 2;
            Log($"[{src.PadRight(15, ' ')}] [RECV][M]\tPiece: {piece} Rejected");
        }

        // PieceBlock [Torrent  Receive]
        internal void PieceReceived(byte[] data, int piece, int offset, Peer peer)
        {
            if (!isRunning) return;

            // [Already Received | SHA-1 Validation Failed] => leave
            int  block = offset / torrent.data.blockSize;
            bool containsKey;
            bool pieceProgress;
            bool blockProgress;

            lock (lockerTorrent)
            {   
                containsKey     = torrent.data.pieceProgress.ContainsKey(piece);
                pieceProgress   = torrent.data.progress.GetBit(piece);
                blockProgress   = containsKey ? torrent.data.pieceProgress[piece].progress.GetBit(block) : false;

                // Piece Done | Block Done
                if (   (!containsKey && pieceProgress )     // Piece Done
                    || ( containsKey && blockProgress ) )   // Block Done
                { 
                    Stats.BytesDropped   += data.Length; 
                    Stats.AlreadyReceived++;
                    if (Options.Verbosity > 0) Log($"[{peer.host.PadRight(15, ' ')}] [RECV][P]\tPiece: {piece} Block: {block} Offset: {offset} Size: {data.Length} Already received"); 

                    return; 
                }

                // Cancel Received Piece From Downloaders | Possible not worth the cpu (few peers will actually cancel them)
                //foreach (var downpeer in peers)
                //    if (downpeer.status == Peer.Status.DOWNLOADING && downpeer.host != peer.host)
                //    {
                //        foreach (var downpiece in downpeer.lastPieces)
                //            if (downpiece.Item1 == piece && downpiece.Item2 == offset) { downpeer.CancelPieces(downpiece.Item1, downpiece.Item2, downpiece.Item3); break; }
                //    }

                if (Options.Verbosity > 1) Log($"[{peer.host.PadRight(15, ' ')}] [RECV][P]\tPiece: {piece} Block: {block} Offset: {offset} Size: {data.Length} Requests: {peer.PiecesRequested}");
                Stats.BytesDownloaded += data.Length;

                // Keep track of received data in case of SHA1 failure to ban the responsible peer
                recvBlocksTracking[$"{piece}|{block}"] = peer.host;
                
                // Parse Block Data to Piece Data
                Buffer.BlockCopy(data, 0, torrent.data.pieceProgress[piece].data, offset, data.Length);

                // SetBit Block | Leave if more blocks required for Piece
                torrent.data.pieceProgress[piece].progress.     SetBit(block);

                // In case of Timed out (to avoid re-requesting)
                torrent.data.pieceProgress[piece].requests.     SetBit(block);
                if (torrent.data.pieceProgress[piece].requests. GetFirst0() == -1) SetRequestsBit(piece);

                if (torrent.data.pieceProgress[piece].progress. GetFirst0() != -1) return;

                // SHA-1 Validation for Piece Data | Failed? -> Re-request whole Piece! | Lock, No thread-safe!
                byte[] pieceHash;
                pieceHash = sha1.ComputeHash(torrent.data.pieceProgress[piece].data);
                if (!Utils.ArrayComp(torrent.file.pieces[piece], pieceHash))
                {
                    // Failure twice in a row | Ban peers with Diff blocks (possible to ban good ones | also in case of not diff / failed block came from same peer with same invalid data -> we ban the most famous one)
                    if (sha1FailedPieces.ContainsKey(piece))
                    {
                        if (Options.Verbosity > 0) Log($"[{peer.host.PadRight(15, ' ')}] [RECV][P]\tPiece: {piece} Block: {block} Offset: {offset} Size: {data.Length} Size2: {torrent.data.pieceProgress[piece].data.Length} SHA-1 failed | Doing Diff (both)");
                        SHA1FailedPiece sfp = new SHA1FailedPiece(piece, torrent.data.pieceProgress[piece].data, recvBlocksTracking, torrent.data.blocks);
                        List<string> responsiblePeers = SHA1FailedPiece.FindDiffs(sha1FailedPieces[piece], sfp, torrent, true);

                        // We will probably ban also good peers here
                        if (responsiblePeers != null)
                            foreach(string host in responsiblePeers)
                                BanPeer(host);
                    }

                    // Failure first time | Keep piece data for later review
                    else
                    {
                        if (Options.Verbosity > 0) Log($"[{peer.host.PadRight(15, ' ')}] [RECV][P]\tPiece: {piece} Block: {block} Offset: {offset} Size: {data.Length} Size2: {torrent.data.pieceProgress[piece].data.Length} SHA-1 failed | Adding for review");
                        SHA1FailedPiece sfp = new SHA1FailedPiece(piece, torrent.data.pieceProgress[piece].data, recvBlocksTracking, torrent.data.blocks);
                        sha1FailedPieces.TryAdd(piece, sfp);
                    }

                    Stats.BytesDropped      += torrent.data.pieceProgress[piece].data.Length;
                    Stats.BytesDownloaded   -= torrent.data.pieceProgress[piece].data.Length;
                    Stats.SHA1Failures++;

                    UnSetRequestsBit(piece);
                    torrent.data.pieceProgress.Remove(piece);

                    return;
                }

                // Finally SHA-1 previously failed now success (found responsible peer for previous failure)
                else if (sha1FailedPieces.ContainsKey(piece))
                {
                    if (Options.Verbosity > 0) Log($"[{peer.host.PadRight(15, ' ')}] [RECV][P]\tPiece: {piece} Block: {block} Offset: {offset} Size: {data.Length} Size2: {torrent.data.pieceProgress[piece].data.Length} SHA-1 Success | Doing Diff");
                    SHA1FailedPiece sfp = new SHA1FailedPiece(piece, torrent.data.pieceProgress[piece].data, recvBlocksTracking, torrent.data.blocks);
                    List<string> responsiblePeers = SHA1FailedPiece.FindDiffs(sha1FailedPieces[piece], sfp, torrent);

                    if (responsiblePeers != null)
                        foreach(string host in responsiblePeers)
                            BanPeer(host);

                    // Clean-up
                    sha1FailedPieces.TryRemove(piece, out SHA1FailedPiece tmp01);
                }

                // Save Piece in PartFiles [Thread-safe?]
                SavePiece(torrent.data.pieceProgress[piece].data, piece, torrent.data.pieceProgress[piece].data.Length);
                if (Options.Verbosity > 0) Log($"[{peer.host.PadRight(15, ' ')}] [RECV][P]\tPiece: {piece} Full"); 

                // [SetBit for Progress | Remove Block Progress] | Done => CreateFiles
                SetProgressBit(piece);
                SetRequestsBit(piece);
                torrent.data.pieceProgress. Remove(piece);

                if (torrent.data.progress.GetFirst0() == - 1) // just compare with pieces size
                {
                    FillStats();

                    FinishingArgs fArgs = new FinishingArgs(false);
                    OnFinishing?.Invoke(this, fArgs);

                    if (!fArgs.Cancel || torrent.data.progress.GetFirst0() == - 1)
                    {
                        Log("[FINISH]");

                        torrent.data.isDone = true;
                        status              = Status.STOPPED;
                    }
                    else
                    {
                        Stats.EndGameMode   = false;
                        Log("[FINISH] Canceled");
                    }
                        
                }
            }

            // Clean-Up
            for (int i=0; i<torrent.data.blocks; i++)
                recvBlocksTracking.TryRemove($"{piece}|{i}", out string tmp01);
        }

        // PieceBlock [Torrent  Rejected | Timeout | Failed]
        internal void ResetRequest(Peer peer, int piece, int offset, int size)
        {
            if (!isRunning) return;

            List<Tuple<int, int, int>> pieces = new List<Tuple<int, int, int>>();
            pieces.Add(new Tuple<int, int, int>(piece, offset, size));
            ResetRequests(peer, pieces);
        }
        internal void ResetRequests(Peer peer, List<Tuple<int, int, int>> pieces) // piece, offset, len
        {
            if (!isRunning) return;

            lock (lockerTorrent)
            {
                foreach (var pieceT in pieces)
                {
                    int piece = pieceT.Item1;
                    int block = pieceT.Item2 / torrent.data.blockSize;

                    bool containsKey = torrent.data.pieceProgress.ContainsKey(piece);

                    // !(Piece Done | Block Done)
                    if ( !(( !containsKey && torrent.data.progress.GetBit(piece)) 
                         ||(  containsKey && torrent.data.pieceProgress[piece].progress.GetBit(block))) )
                    {
                        if (Options.Verbosity > 0) Log($"[{peer.host.PadRight(15, ' ')}] [REQR][P]\tPiece: {piece} Block: {block} Offset: {pieceT.Item2} Size: {pieceT.Item3} Requests: {peer.PiecesRequested}");

                        if (containsKey) torrent.data.pieceProgress[piece].requests.UnSetBit(block);
                        UnSetRequestsBit(piece);
                    }
                }
            }
            
            // Trying to avoid Drop-bytes | On Choke / Unchoked Directly cases will re-request the same pieces (if we dont get rejects by the peer)
            //Thread.Sleep(25 * (pieces.Count + 1));
        }

        // Piece      [Torrent  Save]
        private void SavePiece(byte[] data, int piece, int size)
        {
            if (size > torrent.file.pieceLength) { Log("[SAVE] pieceSize > chunkSize"); return; }

            long firstByte  = (long)piece * torrent.file.pieceLength;
            long ends       = firstByte + size;
            long sizeLeft   = size;
            int  writePos   = 0;
            long curSize    = 0;

            for (int i=0; i<torrent.file.lengths.Count; i++)
            {
                curSize += torrent.file.lengths[i];

                if (firstByte < curSize) {
                    if (torrent.data.files[i] == null) { Log("[SAVE] Null file:\t\t" + (i + 1)); return; }

		            int writeSize 	= (int) Math.Min(sizeLeft, curSize - firstByte);
                    int chunkId     = (int) (((firstByte + torrent.file.pieceLength - 1) - (curSize - torrent.file.lengths[i]))/torrent.file.pieceLength);

		            if (firstByte == curSize - torrent.file.lengths[i])
                    {
                        if (Options.Verbosity > 2) Log("[SAVE] F file:\t\t" + (i + 1) + ", chunkId:\t\t" +     0 +     ", pos:\t\t" + writePos + ", len:\t\t" + writeSize);
                        torrent.data.files[i].WriteFirst(data, writePos, writeSize);
                    } else if (ends < curSize) {
                        if (Options.Verbosity > 2) Log("[SAVE] M file:\t\t" + (i + 1) + ", chunkId:\t\t" + chunkId +   ", pos:\t\t" + writePos + ", len:\t\t" + torrent.file.pieceLength + " | " + writeSize);
                        torrent.data.files[i].Write(chunkId,data);
                    } else if (ends >= curSize) {
                        if (Options.Verbosity > 2) Log("[SAVE] L file:\t\t" + (i + 1) + ", chunkId:\t\t" + chunkId +   ", pos:\t\t" + writePos + ", len:\t\t" + writeSize);
                        torrent.data.files[i].WriteLast(chunkId, data, writePos, writeSize);
                    }

                    if (ends - 1 < curSize) break;

		            firstByte = curSize;
                    sizeLeft -= writeSize;
                    writePos += writeSize;
                }
            }
        }
        #endregion

        #region Set/UnSet Bits on Main Progress/Requests [Keep Sync with Previous]
        private void SetProgressBit(int piece)
        {
            torrent.data.progress    .SetBit(piece);
            torrent.data.progressPrev.SetBit(piece); // Ensures that if we receive a piece for an excluded file, if we included later we will not re-request it
        }
        private void SetRequestsBit(int piece)
        {
            torrent.data.requests.SetBit(piece);
        }
        private void UnSetProgressBit(int piece)
        {
            torrent.data.progress    .UnSetBit(piece);
            torrent.data.progressPrev.UnSetBit(piece);
        }
        private void UnSetRequestsBit(int piece)
        {
            torrent.data.requests.UnSetBit(piece);

            // Ensures that we will re-request Focus Area pieces in case of rejects/fails
            if (focusArea == null && lastFocusArea != null && piece >= lastFocusArea.Item1 && piece <= lastFocusArea.Item2)
            {
                Log($"[FOCUS {lastFocusArea.Item1} - {lastFocusArea.Item2}] Reset");
                focusArea       = new Tuple<int, int>(lastFocusArea.Item1, lastFocusArea.Item2);
                lastFocusArea   = null;
            }
                
        }
        #endregion

        #region Ban | SHA-1 Fails
        private void BanPeer(string host)
        {
            Log($"[BAN] {host}");

            peersBanned.Add(host);

            lock (peersForDispatch)
                foreach (var thread in bstp.Threads)
                {
                    if (thread != null && thread.peer != null && thread.peer.host == host)
                    {
                        Log($"[BAN] {host} Found in BSTP");
                        thread.peer.Disconnect();  thread.peer = null;
                        peersStored.TryRemove(host, out int tmp01); 
                    }
                }
        }
        private class SHA1FailedPiece
        {
            public int      piece;
            public byte[]   data;
            public ConcurrentDictionary<string, string> recvBlocksTracking = new ConcurrentDictionary<string, string>();

            public SHA1FailedPiece(int piece, byte[] data, ConcurrentDictionary<string, string> tracking, int numberOfBlocks)
            {
                this.piece  = piece;
                this.data   = data;

                // Copy From Recv Tracking only Piece Specific
                for (int i=0; i<numberOfBlocks; i++)
                {
                    if (!tracking.ContainsKey($"{piece}|{i}"))
                        continue;
                    else
                        recvBlocksTracking[$"{piece}|{i}"] = tracking[$"{piece}|{i}"];
                }
                    
            }

            public static List<string> FindDiffs(SHA1FailedPiece sfp1, SHA1FailedPiece sfp2, Torrent torrent, bool bothSide = true)
            {
                try
                {
                    if (sfp1 == null || sfp2 == null || sfp1.piece != sfp2.piece || sfp1.data.Length != sfp2.data.Length)
                        return null;

                    List<string> responsiblePeers = new List<string>();

                    Dictionary<string, int> famousCounter = new Dictionary<string, int>();

                    // Creates the list with responsible peers (diff blocks sent)
                    int blocks = sfp1.piece == torrent.data.pieces - 1 ? torrent.data.blocksLastPiece : torrent.data.blocks;
                    int blockLastSize = sfp1.piece == torrent.data.pieces - 1 ? torrent.data.blockLastSize : torrent.data.blockLastSize2;

                    for(int i=0; i<blocks; i++)
                    {
                        if (!famousCounter.ContainsKey(sfp1.recvBlocksTracking[$"{sfp1.piece}|{i}"])) famousCounter.Add(sfp1.recvBlocksTracking[$"{sfp1.piece}|{i}"], 0);
                        if (!famousCounter.ContainsKey(sfp2.recvBlocksTracking[$"{sfp2.piece}|{i}"])) famousCounter.Add(sfp2.recvBlocksTracking[$"{sfp2.piece}|{i}"], 0);

                        famousCounter[sfp1.recvBlocksTracking[$"{sfp1.piece}|{i}"]]++;
                        famousCounter[sfp2.recvBlocksTracking[$"{sfp2.piece}|{i}"]]++;

                        byte[] block1, block2;
                        int offset = i * torrent.data.blockSize;

                        if (i == blocks - 1)
                        {
                            block1 = Utils.ArraySub(ref sfp1.data, offset, blockLastSize);
                            block2 = Utils.ArraySub(ref sfp2.data, offset, blockLastSize);
                        }
                        else
                        {
                            block1 = Utils.ArraySub(ref sfp1.data, offset, torrent.data.blockSize);
                            block2 = Utils.ArraySub(ref sfp2.data, offset, torrent.data.blockSize);
                        }

                        if (!Utils.ArrayComp(block1, block2))
                        {
                            if (!responsiblePeers.Contains(sfp1.recvBlocksTracking[$"{sfp1.piece}|{i}"]))
                                responsiblePeers.Add(sfp1.recvBlocksTracking[$"{sfp1.piece}|{i}"]);

                            if (bothSide && !responsiblePeers.Contains(sfp2.recvBlocksTracking[$"{sfp2.piece}|{i}"]))
                                responsiblePeers.Add(sfp2.recvBlocksTracking[$"{sfp2.piece}|{i}"]);
                        }
                    }

                    // Same invalid blocks probably from same peer (let's gamble with posibilities)
                    if (responsiblePeers.Count == 0)
                    {
                        string  mostFamous  = "";
                        int     curMin      = 0;
                        foreach (var famous in famousCounter)
                        {
                            if (famous.Value > curMin)
                            {
                                curMin      = famous.Value;
                                mostFamous  = famous.Key;
                            }
                        }
                        if (mostFamous != "") responsiblePeers.Add(mostFamous);
                    }
                
                    return responsiblePeers;
                }
                catch (Exception) { return null; }
            }
        }
        HashSet<string> peersBanned = new HashSet<string>();
        ConcurrentDictionary<int, SHA1FailedPiece> sha1FailedPieces = new ConcurrentDictionary<int, SHA1FailedPiece>();
        ConcurrentDictionary<string, string> recvBlocksTracking = new ConcurrentDictionary<string, string>();
        #endregion

        #region Misc
        private void CreatePieceProgress(int piece)
        {
            lock (lockerTorrent)
            {
                if (!torrent.data.pieceProgress.ContainsKey(piece))
                {
                    PieceProgress pp = new PieceProgress(ref torrent.data, piece);
                    torrent.data.pieceProgress.Add(piece, pp);
                } 
            }
        }
        private int GetBlockSize(int piece, int block)
        {
            if (piece == torrent.data.pieces - 1 && block == torrent.data.blocksLastPiece - 1)
                return torrent.data.blockLastSize;
            else if (block == torrent.data.blocks - 1)
                return torrent.data.blockLastSize2;
            else
                return torrent.data.blockSize;
        }
        internal bool NoPiecesPeer(Peer peer) { return torrent.metadata.isDone && !peer.stageYou.haveAll && (peer.stageYou.haveNone || peer.stageYou.bitfield == null || torrent.data.requests.GetFirst01(peer.stageYou.bitfield) == -1); }
        public void CancelRequestedPieces()
        {
            lock (peersForDispatch)
                foreach (var thread in bstp.Threads)
                {
                    if (thread != null && thread.peer != null && thread.peer.status == Peer.Status.DOWNLOADING)
                        thread.peer.CancelPieces();
                }
        }
        internal void StopWithError(string Message, string StackTrace = "")
        {
            status = Status.STOPPED;

            Log($"[CRITICAL ERROR] Msg: {Message}\r\n{StackTrace}"); StatusChanged?.Invoke(this, new StatusChangedArgs(2, Message + "\r\n"+ StackTrace));

            Dispose();

            throw new Exception(Message);
        }
        internal void Log(string msg) { if (Options.Verbosity > 0) log.Write($"[BitSwarm] {msg}"); }
        #endregion
    }
}