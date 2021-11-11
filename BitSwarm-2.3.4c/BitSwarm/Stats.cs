namespace SuRGeoNix.BitSwarmLib
{
    /// <summary>
    /// BitSwarm's Statistics
    /// </summary>
    public class Stats
    {
        /// <summary>
        /// Current download rate
        /// </summary>
        public int      DownRate            { get; internal set; }

        /// <summary>
        /// Average download rate
        /// </summary>
        public int      AvgRate             { get; internal set; }

        /// <summary>
        /// Max download rate
        /// </summary>
        public int      MaxRate             { get; internal set; }

        /// <summary>
        /// Average ETA
        /// </summary>
        public int      AvgETA              { get; internal set; }

        /// <summary>
        /// Current ETA
        /// </summary>
        public int      ETA                 { get; internal set; }

        /// <summary>
        /// Complete progress
        /// </summary>
        public int      Progress            { get; internal set; }

        /// <summary>
        /// Total required pieces to complete
        /// </summary>
        public int      PiecesIncluded      { get; internal set; }

        /// <summary>
        /// Total required bytes to complete
        /// </summary>
        public long     BytesIncluded       { get; internal set; }

        /// <summary>
        /// Downloaded bytes that have been saved on disk
        /// </summary>
        public long     BytesCurDownloaded  { get; internal set; }

        /// <summary>
        /// Downloaded bytes that have been saved on disk and memory (working pieces)
        /// </summary>
        public long     BytesDownloaded     { get; internal set; }

        /// <summary>
        /// Downloaded bytes from previous session (total part files bytes)
        /// </summary>
        public long     BytesDownloadedPrevSession  { get; internal set; }


        //public long     BytesUploaded       { get; internal set; }

        /// <summary>
        /// Dropped bytes (from SHA1 validation fails and already received)
        /// </summary>
        public long     BytesDropped        { get; internal set; }

        /// <summary>
        /// Peers total from DHT, PEX and Trackers (Stored)
        /// </summary>
        public int      PeersTotal          { get; internal set; }

        /// <summary>
        /// Peers that we are going to connect (Dispatch stack)
        /// </summary>
        public int      PeersInQueue        { get; internal set; }

        /// <summary>
        /// Peers that we are connecting (Shortrun threads) 
        /// </summary>
        public int      PeersConnecting     { get; internal set; }

        /// <summary>
        /// Peers that we are already connected (Longrun threads = Choked + Unchoked + Downloading)
        /// </summary>
        public int      PeersConnected      { get; internal set; }

        /// <summary>
        /// Peers that are chocked
        /// </summary>
        public int      PeersChoked         { get; internal set; }

        /// <summary>
        /// Peers that are unchoked
        /// </summary>
        public int      PeersUnChoked       { get; internal set; }

        /// <summary>
        /// Peers that we are downloading
        /// </summary>
        public int      PeersDownloading    { get; internal set; }

        /// <summary>
        /// Session start timestamp
        /// </summary>
        public long     StartTime           { get; internal set; }

        /// <summary>
        /// Session current timestamp
        /// </summary>
        public long     CurrentTime         { get; internal set; }

        /// <summary>
        /// Session end timestamp
        /// </summary>
        public long     EndTime             { get; internal set; }

        /// <summary>
        /// When sleep mode is active
        /// </summary>
        public bool     SleepMode           { get; internal set; }

        /// <summary>
        /// When boost mode is active
        /// </summary>
        public bool     BoostMode           { get; internal set; }

        /// <summary>
        /// When end game mode is active
        /// </summary>
        public bool     EndGameMode         { get; internal set; }

        /// <summary>
        /// Number of pieces that already have been received
        /// </summary>
        public int      AlreadyReceived     { get; internal set; }

        /// <summary>
        /// Number of peers found from DHT (at least)
        /// </summary>
        public int      DHTPeers            { get; internal set; }

        /// <summary>
        /// Number of peers found from PEX (at least)
        /// </summary>
        public int      PEXPeers            { get; internal set; }

        /// <summary>
        /// Number of peers found from Trackers (at least)
        /// </summary>
        public int      TRKPeers            { get; internal set; }
        
        /// <summary>
        /// Times that SHA1 validation failed
        /// </summary>
        public int      SHA1Failures        { get; internal set; }

        /// <summary>
        /// Number of block rejects that we have received
        /// </summary>
        public int      Rejects;

        /// <summary>
        /// Number of timeouts during Handshake
        /// </summary>
        public int      HandshakeTimeouts;

        /// <summary>
        /// Number of timeouts during Piece retrieval
        /// </summary>
        public int      PieceTimeouts;
    }
}
