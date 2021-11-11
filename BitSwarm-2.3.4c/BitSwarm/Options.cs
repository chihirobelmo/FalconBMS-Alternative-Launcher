using System;
using System.IO;
using System.Xml.Serialization;

namespace SuRGeoNix.BitSwarmLib
{
    /// <summary>
    /// BitSwarm's Options ([CP] stands for copy, that changes will not affect bitswarm's running session)
    /// </summary>
    public class Options : ICloneable
    {
        /// <summary>
        /// [CP] Folder where the completed files will be saved
        /// </summary>
        public string   FolderComplete      { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// [CP] Folder where the incomplete (part) files will be saved
        /// </summary>
        public string   FolderIncomplete    { get; set; } = Path.Combine(Path.GetTempPath(), "BitSwarm", ".data");

        /// <summary>
        /// [CP] Folder where the .torrent files will be saved
        /// </summary>
        public string   FolderTorrents      { get; set; } = Path.Combine(Path.GetTempPath(), "BitSwarm", ".torrents");

        /// <summary>
        /// [CP] Folder where the .bsf BitSwarm's Session Files will be saved
        /// </summary>
        public string   FolderSessions      { get; set; } = Path.Combine(Path.GetTempPath(), "BitSwarm", ".sessions");

        /// <summary>
        /// [CP] Trackers file to include (Format: udp://host:port , one tracker per line)
        /// </summary>
        public string   TrackersPath        { get; set; } = "";

        /// <summary>
        /// [CP] Max number of threads for total connections (Shortrun + Longrun)
        /// </summary>
        public int      MaxThreads          { get; set; } =  150;   // Max Total  Connection Threads  | Short-Run + Long-Run

        /// <summary>
        /// Max number of threads for new connections / dispatching (Shortrun)
        /// </summary>
        public int      MinThreads          { get; set; } =   15;   // Max New    Connection Threads  | Short-Run

        /// <summary>
        /// [CP] Max number of threads for new connections during initial boosting (Shortrun)
        /// </summary>
        public int      BoostThreads        { get; set; } =   60;   // Max New    Connection Threads  | Boot Boost

        /// <summary>
        /// Boost seconds duration
        /// </summary>
        public int      BoostTime           { get; set; } =   30;   // Boot Boost Time (Seconds)

        /// <summary>
        /// Sleep mode activation on specific download rate (-1: Automatic based on max rate, 0: Disabled, >0: KB/s rate)
        /// </summary>
        public int      SleepModeLimit      { get; set; } =    0;   // Activates Sleep Mode (Low Resources) at the specify DownRate | DHT Stop, Re-Fills Stop (DHT/Trackers) & MinThreads Drop to MinThreads / 2
                                                                    // -1: Auto | 0: Disabled | Auto will figure out SleepModeLimit from MaxRate

        //public int      DownloadLimit       { get; set; } = -1;
        //public int      UploadLimit         { get; set; }

        /// <summary>
        /// Peer's TCP Connection Timeout (ms)
        /// </summary>
        public int      ConnectionTimeout   { get; set; } =  600;

        /// <summary>
        /// Peer's Handshake retrieval Timeout (ms)
        /// </summary>
        public int      HandshakeTimeout    { get; set; } =  800;

        /// <summary>
        /// Peer's Metadata Piece Timeout (ms) - Re-requests timed out piece
        /// </summary>
        public int      MetadataTimeout     { get; set; } = 1600;

        /// <summary>
        /// [CP] Metadata parallel requests from peers (2 from each, must be multiple of 2)
        /// </summary>
        public int      MetadataParallelReq { get; set; } = 14;

        /// <summary>
        /// Peer's Piece Timeout (ms) - Re-requests timed out piece
        /// </summary>
        public int      PieceTimeout        { get; set; } = 5000;   // Large timeouts without resets will cause more working pieces (more memory/more lost bytes on force stop)

        /// <summary>
        /// Peer's Piece Timeout Retries - Resets happens on first timeout.
        /// Used mainly for streaming with smaller timeouts and more retries for fast piece retrieval and without dropping the slow download rated peers
        /// </summary>
        public int      PieceRetries        { get; set; } =    0;   // Retries should be used only for specific time periods, otherwise will let the same peers to timeout and not drop them with the result of too many drop bytes / already received

        /// <summary>
        /// Peer's Piece Timeout (ms) during reading/buffering - Re-requests timed out piece
        /// </summary>
        public int      PieceBufferTimeout  { get; set; } = 1000;

        /// <summary>
        /// Peer's Piece Timeout Retries during reading/buffering - Resets happens on first timeout.
        /// Used mainly for streaming with smaller timeouts and more retries for fast piece retrieval and without dropping the slow download rated peers
        /// </summary>
        public int      PieceBufferRetries  { get; set; } =    4;

        /// <summary>
        /// Peers will use PieceBufferTimeout/PieceBufferRetries if focus area data are not available
        /// </summary>
        public bool     EnableBuffering     { get; set; } = false;

        /// <summary>
        /// Enable/Disable PEX
        /// </summary>
        public bool     EnablePEX           { get; set; } = true;

        /// <summary>
        /// Enable/Disable DHT
        /// </summary>
        public bool     EnableDHT           { get; set; } = true;

        /// <summary>
        /// Enable/Disable Trackers
        /// </summary>
        public bool     EnableTrackers      { get; set; } = true;

        /// <summary>
        /// Number of peers to request from each tracker (-1: Max)
        /// </summary>
        public int      PeersFromTracker    { get; set; } = -1;

        /// <summary>
        /// Number of blocks to request from each peer at once (Large ensures better speed / Small guarantees less drop bytes)
        /// </summary>
        public int      BlockRequests       { get; set; } =  9;     // Blocks that we request at once for each peer (large for better speed / small on streaming to avoid delayed resets on timeouts)
                                                                    // TODO: Should be based on peer's downRate (slow downRates less blocks to avoid dropped bytes) | Related also with peer's Receive() timeouts
        
        /// <summary>
        /// Log verbosity [0-4]
        /// </summary>
        public int      Verbosity           { get; set; } =  0;     // [0 - 4]

        /// <summary>
        /// Enable/Disable logging for trackers
        /// </summary>
        public bool     LogTracker          { get; set; } = false;  // Verbosity 1

        /// <summary>
        /// Enable/Disable logging for peers
        /// </summary>
        public bool     LogPeer             { get; set; } = false;  // Verbosity 1 - 4

        /// <summary>
        /// Enable/Disable logging for DHT
        /// </summary>
        public bool     LogDHT              { get; set; } = false;  // Verbosity 1

        /// <summary>
        /// Enable/Disable logging for statistics
        /// </summary>
        public bool     LogStats            { get; set; } = false;  // Verbosity 1

        

        /// <summary>
        /// Default XML configuration file name
        /// </summary>
        public static string ConfigFile     { get; private set; } = "bitswarm.config.xml";

        /// <summary>
        /// Creates an XML configuration file
        /// </summary>
        /// <param name="opt">BitSwarm's options that will be used for the creation</param>
        /// <param name="path">The folder to export the configuration (Default: current)</param>
        public static void CreateConfig(Options opt, string path = null)
        {
            if (path == null) path = Directory.GetCurrentDirectory();

            using (FileStream fs = new FileStream(Path.Combine(path, ConfigFile), FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Options));
                xmlSerializer.Serialize(fs, opt);
            }
        }

        /// <summary>
        /// Loads BitSwarm's options from an XML configuration file
        /// </summary>
        /// <param name="customFilePath">The folder of the configuration file (Default: Searches in current and a) for unix in %HOME%/.bitswarm b) for windows in %APPDATA%/BitSwarm)</param>
        /// <returns>BitSwarm's options</returns>
        public static Options LoadConfig(string customFilePath = null)
        {
            string foundPath;

            if (customFilePath != null)
            {
                if (!File.Exists(customFilePath)) return null;
                foundPath = customFilePath;
            } 
            else
            {
                foundPath = SearchConfig();
                if (foundPath == null) return null;
            }

            using (FileStream fs = new FileStream(foundPath, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Options));
                return (Options) xmlSerializer.Deserialize(fs);
            }
        }
        private static string SearchConfig()
        {
            string  foundPath;

            foundPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFile);
            if (File.Exists(foundPath)) return foundPath;

            if (Utils.IsWindows)
            {
                foundPath = Path.Combine(Environment.ExpandEnvironmentVariables("%APPDATA%") , "BitSwarm", ConfigFile);
                if (File.Exists(foundPath)) return foundPath;
            }
            else
            {
                foundPath = Path.Combine(Environment.ExpandEnvironmentVariables("%HOME%") , ".bitswarm", ConfigFile);
                if (File.Exists(foundPath)) return foundPath;

                foundPath = $"/etc/bitswarm/{ConfigFile}";
                if (File.Exists(foundPath)) return foundPath;
            }

            return null;
        }

        /// <summary>
        /// Options clone to seperate live changes from the ones that are not allowed / require restart
        /// </summary>
        /// <returns></returns>
        public object Clone() { return MemberwiseClone(); }
    }
}
