using CommandLine;

using BitSwarmOptions = SuRGeoNix.BitSwarmLib.Options;

namespace SuRGeoNix.BitSwarmConsole
{
    public class Options
    {
        // NOTE: For bool variables should have the opposite from BitSwarm's

        private static readonly int NOT_SET = -9999;


        [Option("fc",           HelpText = "Folder for completed files (Default: .)")]
        public string   FolderComplete  { get; set; }
        [Option("fi",           HelpText = "Folder for .apf incompleted files (Default: %temp%/BitSwarm/.data)")]
        public string   FolderIncomplete{ get; set; }
        [Option("ft",           HelpText = "Folder for .torrent files (Default: %temp%/BitSwarm/.torrents)")]
        public string   FolderTorrents  { get; set; }
        [Option("fs",           HelpText = "Folder for .bsf session files (Default: %temp%/BitSwarm/.sessions)")]
        public string   FolderSessions  { get; set; }

        [Option("mc",           HelpText = "Max new connection threads")]
        public int      MinThreads      { get; set; } = NOT_SET;

        [Option("mt",           HelpText = "Max total threads")]
        public int      MaxThreads      { get; set; } = NOT_SET;

        [Option("boostmin",     HelpText = "Boost new connection threads")]
        public int      BoostThreads    { get; set; } = NOT_SET;

        [Option("boostsecs",    HelpText = "Boost time in seconds")]
        public int      BoostTime       { get; set; } = NOT_SET;
        [Option("sleep",        HelpText = "Sleep activation at this down rate KB/s (-1: Automatic | 0: Disabled)")]
        public int      SleepModeLimit  { get; set; } = NOT_SET;

        [Option("no-dht",       HelpText = "Disable DHT")]
        public bool     DisableDHT      { get; set; }

        [Option("no-pex",       HelpText = "Disable PEX")]
        public bool     DisablePEX      { get; set; }

        [Option("no-trackers",  HelpText = "Disable Trackers")]
        public bool     DisableTrackers { get; set; }

        [Option("trackers-num", HelpText = "# of peers will be requested from each tracker")]
        public int      PeersFromTracker{ get; set; } = NOT_SET;

        [Option("trackers-file",HelpText = "Trackers file to include (format 'scheme://host:port' per line)")]
        public string   TrackersPath    { get; set; }

        [Option("ct",           HelpText = "Connection timeout in ms")]
        public int      ConnectionTimeout{get; set; } = NOT_SET;

        [Option("ht",           HelpText = "Handshake timeout in ms")]
        public int      HandshakeTimeout{ get; set; } = NOT_SET;

        [Option("pt",           HelpText = "Piece timeout in ms")]
        public int      PieceTimeout    { get; set; } = NOT_SET;

        [Option("pr",           HelpText = "Piece retries")]
        public int      PieceRetries    { get; set; } = NOT_SET;

        [Option("req-blocks",   HelpText = "Parallel block requests per peer")]
        public int      BlockRequests   { get; set; } = NOT_SET;

        [Option("log",          HelpText = "Log verbosity [0-4]")]
        public int      LogVerbosity    { get; set; } = NOT_SET;

        [Option("log-dht",      HelpText = "Enable logging for DHT")]
        public bool     LogDHT          { get; set; }

        [Option("log-peers",    HelpText = "Enable logging for Peers")]
        public bool     LogPeer         { get; set; }

        [Option("log-trackers", HelpText = "Enable logging for Trackers")]
        public bool     LogTracker      { get; set; }

        [Option("log-stats",    HelpText = "Enable logging for Stats")]
        public bool     LogStats        { get; set; }

        [Value(0, MetaName = "Torrent|Magnet|Hash|Session", Required = true, HelpText = "")]
        public string   Input           { get; set; }

        public static bool ParseOptionsToBitSwarm(Options userOptions, ref BitSwarmOptions bitSwarmOptions)
        {
            if (userOptions.FolderComplete      != null)
                bitSwarmOptions.FolderComplete      = userOptions.FolderComplete;

            if (userOptions.FolderIncomplete    != null)
                bitSwarmOptions.FolderIncomplete    = userOptions.FolderIncomplete;

            if (userOptions.FolderTorrents      != null)
                bitSwarmOptions.FolderTorrents      = userOptions.FolderTorrents;

            if (userOptions.FolderSessions      != null)
                bitSwarmOptions.FolderSessions      = userOptions.FolderSessions;

            if (userOptions.TrackersPath        != null)
                bitSwarmOptions.TrackersPath        = userOptions.TrackersPath;

            if (userOptions.MinThreads          != NOT_SET)
                bitSwarmOptions.MinThreads          = userOptions.MinThreads;
            if (userOptions.MaxThreads          != NOT_SET)
                bitSwarmOptions.MaxThreads          = userOptions.MaxThreads;

            if (userOptions.BoostThreads        != NOT_SET)
                bitSwarmOptions.BoostThreads        = userOptions.BoostThreads;
            if (userOptions.BoostTime           != NOT_SET)
                bitSwarmOptions.BoostTime           = userOptions.BoostTime;
            if (userOptions.SleepModeLimit      != NOT_SET)
                bitSwarmOptions.SleepModeLimit      = userOptions.SleepModeLimit;

            if (userOptions.ConnectionTimeout   != NOT_SET)
                bitSwarmOptions.ConnectionTimeout   = userOptions.ConnectionTimeout;
            if (userOptions.HandshakeTimeout    != NOT_SET)
                bitSwarmOptions.HandshakeTimeout    = userOptions.HandshakeTimeout;
            if (userOptions.PieceTimeout        != NOT_SET)
                bitSwarmOptions.PieceTimeout        = userOptions.PieceTimeout;
            if (userOptions.PieceRetries        != NOT_SET)
                bitSwarmOptions.PieceRetries        = userOptions.PieceRetries;
            
            if (userOptions.SleepModeLimit      != NOT_SET)
                bitSwarmOptions.PeersFromTracker    = userOptions.PeersFromTracker;
            
            if (userOptions.DisableDHT)
                bitSwarmOptions.EnableDHT           = !userOptions.DisableDHT;
            if (userOptions.DisablePEX)
                bitSwarmOptions.EnablePEX           = !userOptions.DisablePEX;
            if (userOptions.DisableTrackers)
                bitSwarmOptions.EnableTrackers      = !userOptions.DisableTrackers;

            if (userOptions.BlockRequests       != NOT_SET)
                bitSwarmOptions.BlockRequests       = userOptions.BlockRequests;

            if (userOptions.LogVerbosity        != NOT_SET)
                bitSwarmOptions.Verbosity           = userOptions.LogVerbosity;

            if (bitSwarmOptions.Verbosity > 0)
            {
                if (userOptions.LogDHT)
                    bitSwarmOptions.LogDHT          = userOptions.LogDHT;
                if (userOptions.LogPeer)
                    bitSwarmOptions.LogPeer         = userOptions.LogPeer;
                if (userOptions.LogTracker)
                    bitSwarmOptions.LogTracker      = userOptions.LogTracker;
                if (userOptions.LogStats)
                    bitSwarmOptions.LogStats        = userOptions.LogStats;
            }

            return true;
        }
    }
}