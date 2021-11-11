/* DHT Protocol 
 * http://www.bittorrent.org/beps/bep_0005.html
 * 
 * TODO
 * 
 * DHT IPv6 Extension http://bittorrent.org/beps/bep_0032.html | We can use the ipv4 dht to get ipv6 nodes (by adding 'want' parameter to the request with both n4 and n6 nodes) but we cant use it to get 18-octet IPv6 values
 * 
 * 1. Min ThreadPool for Nodes to avoid re-creating threads all the time
 * 2. Possible review the new rEP / ipEP to ensure not receiving wrong packets (maybe rEP for all threads and also ipEP on thread 0)
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

using BencodeNET.Objects;
using BencodeNET.Parsing;

namespace SuRGeoNix.BitSwarmLib.BEP
{
    internal class DHT
    {
        public struct Options
        {
            public int ConnectionTimeout        { get; set; }
            public int BoostConnectionTimeout   { get; set; }
            public int MaxBucketNodes           { get; set; }
            public int MinBucketDistance        { get; set; }
            public int MinBucketDistance2       { get; set; }
            public int NodesPerLevel            { get; set; }

            public int      Verbosity           { get; set; }
            public Logger   LogFile             { get; set; }

            internal BitSwarm Beggar            { get; set; }

        }
        public enum Status
        {
            RUNNING,
            PAUSED,
            STOPPING,
            STOPPED
        }

        public Status                           status      { get; private set; }
        public long                             StartedAt   { get; private set; }
        public long                             StoppedAt   { get; private set; }

        private Dictionary<string, Node>        bucketNodesPointer;
        private Dictionary<string, Node>        bucketNodes;        // Weird
        private Dictionary<string, Node>        bucketNodes2;       // Normal
        private HashSet<string>                 rememberBadNodes;   // Bad / Timed-out
        private Thread                          beggar;
        
        static readonly BencodeParser           bParser     = new BencodeParser();
        readonly Random                         rnd         = new Random();
        readonly object                         lockerNodes = new object();

        private Options                         options;
        private string                          infoHash;
        private byte[]                          infoHashBytes;
        private string[]                        infoHashBits;

        private byte[]                          getPeersBytes;
        private IPEndPoint                      ipEP;
        private UdpClient                       udpClient;

        private int     havePeers, requested, responded, inBucket;
        private bool    isWeirdStrategy;
        private int     minBucketDistance;
        private int     weirdPeers;
        private int     normalPeers;

        class Node
        {
            public Node(string host, int port, short distance = 160)
            {
                this.host       = host;
                this.port       = port;
                this.distance   = distance;
                this.status     = Status.NEW;
            }
            public enum Status
            {
                NEW,
                REQUESTING,
                REQUESTED,
                FAILED
            }

            public string   host;
            public int      port;
            public short    distance;
            //public bool     hasPeers;

            public Status   status;
        }
        public DHT(string infoHash, Options? opt = null)
        {
            options         = (opt == null) ? GetDefaultOptions() : (Options) opt;
            bucketNodes     = new Dictionary<string, Node>();
            bucketNodes2    = new Dictionary<string, Node>();
            rememberBadNodes= new HashSet<string>();
            infoHashBytes   = Utils.StringHexToArray(infoHash);
            infoHashBits    = new string[20];
            this.infoHash   = infoHash;

            for (int i=0; i<20; i++)
                infoHashBits[i] = Convert.ToString(infoHashBytes[i], 2).PadLeft(8, '0');

            udpClient       = new UdpClient(0, AddressFamily.InterNetwork); // Ensure that we use IPv4 DHT
            ipEP            = (IPEndPoint) udpClient.Client.LocalEndPoint;

            udpClient.Client.SendTimeout    = options.ConnectionTimeout;
            udpClient.Client.ReceiveTimeout = options.ConnectionTimeout;
            udpClient.Client.Ttl            = 255;

            status          = Status.STOPPED;
            isWeirdStrategy = true;

            FlipStrategy();
            AddBootstrapNodes();
            FlipStrategy();
            AddBootstrapNodes();

            PrepareRequest();
        }
        public static Options GetDefaultOptions()
        {
            Options options = new Options();
            options.ConnectionTimeout       = 350;
            options.MaxBucketNodes          = 300;
            options.MinBucketDistance       = 145;
            options.MinBucketDistance2      = 100;
            options.NodesPerLevel           = 8;
            
            return options;
        }
        
        private void AddNewNode(string host, int port, short distance = 160) { bucketNodesPointer.Add(host, new Node(host, port, distance)); }
        private void AddBootstrapNodes()
        {
            bucketNodesPointer.Clear();
            bucketNodesPointer.Add("router.utorrent.com",   new Node("router.utorrent.com",     6881));
            bucketNodesPointer.Add("dht.libtorrent.org",    new Node("dht.libtorrent.org",     25401));
            bucketNodesPointer.Add("router.bittorrent.com", new Node("router.bittorrent.com",   6881));
            bucketNodesPointer.Add("dht.transmissionbt.com",new Node("dht.transmissionbt.com",  6881));

            lock (rememberBadNodes)
            {
                rememberBadNodes.Remove("router.utorrent.com");
                rememberBadNodes.Remove("dht.libtorrent.org");
                rememberBadNodes.Remove("router.bittorrent.com");
                rememberBadNodes.Remove("dht.transmissionbt.com");
            }

            Thread.Sleep(20);
        }

        private void FlipStrategy()
        {
            isWeirdStrategy     = !isWeirdStrategy;
            minBucketDistance   = isWeirdStrategy ? options.MinBucketDistance : options.MinBucketDistance2;
            bucketNodesPointer  = isWeirdStrategy ? bucketNodes : bucketNodes2;

            if (options.Verbosity > 0) Log($"[STRATEGY] Flip to {isWeirdStrategy}");
            Thread.Sleep(20);
        }
        private string GetMinDistanceNode()
        {
            int curMin = 161;
            
            string host = null;
            foreach (KeyValuePair<string, Node> node in bucketNodesPointer)
                if (node.Value.status == Node.Status.NEW && node.Value.distance < curMin) { curMin = node.Value.distance; host = node.Value.host; }

            return host;
        }
        // The Right Way | Closest Nodes | Stable
        private short CalculateDistance2(byte[] nodeId)
        {
            short distance = 0;

            for (int i=0; i<20; i++)
            {
                if (nodeId[i] != infoHashBytes[i])
                {
                    string ab = Convert.ToString(nodeId[i], 2).PadLeft(8, '0');

                    for (int l=0; l<8; l++)
                    {
                        if (ab[l] != infoHashBits[i][l])
                            distance += 1;
                    }
                }
            }

            return (short) distance;
        }
        // The Weird Way | More Like Our Hash Nodes | Faster
        private short CalculateDistance(byte[] nodeId)
        {
            short distance = 0;

            for (int i=0; i<20; i++)
            {
                if (nodeId[i] != infoHashBytes[i])
                {
                    string ab = Convert.ToString(nodeId[i], 2).PadLeft(8, '0');

                    for (int l=0; l<8; l++)
                    {
                        if (ab[l] != infoHashBits[i][l])
                            { distance = (short) ((i * 8) + l + 1); break; }
                    }

                    break;
                }
            }

            if (distance == 0) return 0;

            return (short) (160 - distance);
        }

        private void PrepareRequest()
        {
            /* BEndode get_peers Request
             * 
             * "t" -> <transId>, 
             * "y" -> "q", 
             * "q" -> "get_peers", 
             * "a" -> { "id" -> <nodeId> , "info_hash" -> <infoHashBytes> }
             * 
             */
            byte[] transId  = new byte[ 2]; rnd.NextBytes(transId);
            byte[] nodeId   = new byte[20]; rnd.NextBytes(nodeId);

            BDictionary bRequest=   new BDictionary();
            bRequest.Add("t",       new BString(transId));
            bRequest.Add("y", "q");
            bRequest.Add("q", "get_peers");

            BDictionary bDicA   =   new BDictionary();
            bDicA.Add("id",         new BString(nodeId));
            bDicA.Add("info_hash",  new BString(infoHashBytes));
            bRequest.Add("a", bDicA);

            getPeersBytes = bRequest.EncodeAsBytes();
        }
        private BDictionary GetResponse(Node node)
        {
            try
            {
                byte[] recvBuff = null;

                if (Regex.IsMatch(node.host, @"^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$"))
                {
                    IPEndPoint rEP = new IPEndPoint(IPAddress.Parse(node.host), node.port);
                    udpClient.Send(getPeersBytes, getPeersBytes.Length, rEP);
                    recvBuff = udpClient.Receive(ref rEP);
                }
                else
                {
                    udpClient.Send(getPeersBytes, getPeersBytes.Length, node.host, node.port);
                    recvBuff = udpClient.Receive(ref ipEP);
                }

                if (recvBuff == null || recvBuff.Length == 0) return null;

                return bParser.Parse<BDictionary>(recvBuff);

            } catch (Exception) { return null; }
        }
        private void GetPeers(Node node, int selfRecursionLevel = 0)
        {
            try
            {
                /* BEndode get_peers Response
                 * 
                 * "t" -> <transId>, 
                 * "y" -> "r", 
                 * "r" -> { "id" -> <nodeId> , "token" -> <token> , "nodes" -> "nodeId + host + port...", "values" -> [host + port, ...] }
                 * 
                 */

                if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [REQ ]");

                requested++;

                BDictionary bResponse = GetResponse(node);

                if (bResponse == null || !bResponse.ContainsKey("y") || ((BString) bResponse["y"]).ToString() != "r")
                {
                    node.status = Node.Status.FAILED;
                    if (!options.Beggar.Stats.BoostMode) { lock(rememberBadNodes) rememberBadNodes.Add(node.host); }
                    if (bResponse != null) bResponse.Clear();

                    if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [RESP] Failed");

                    return;
                }

                if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [RESP]");

                bResponse = (BDictionary) bResponse["r"];

                // r -> Nodes 
                if (bResponse.ContainsKey("nodes"))
                {
                    byte[] curNodes = ((BString) bResponse["nodes"]).Value.ToArray();

                    for (int i=0; i<curNodes.Length; i+=26)
                    {
                        byte[]  curNodeId   = Utils.ArraySub(ref curNodes, (uint) i, 20, false);
                        short   curDistance = isWeirdStrategy ? CalculateDistance(curNodeId) : CalculateDistance2(curNodeId);
                        string  curIP       = (new IPAddress(Utils.ArraySub(ref curNodes, (uint) i + 20, 4))).ToString();
                        UInt16  curPort     = (UInt16) BitConverter.ToInt16(Utils.ArraySub(ref curNodes, (uint) i + 24, 2, true), 0);
                    
                        if (curPort < 100) continue; // Drop fake

                        if (i > (5 * 26) && curDistance > minBucketDistance) break; // Avoid collecting too many nodes out of distance from a single node

                        if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [NODE] [{curDistance}] {curIP}:{curPort}");

                        lock (lockerNodes)
                        {
                            if (!bucketNodesPointer.ContainsKey(curIP) && !rememberBadNodes.Contains(curIP))
                                AddNewNode(curIP, curPort, curDistance);
                        }
                    }
                }

                // r -> Peers
                if (bResponse.ContainsKey("values"))
                {
                    int newPeers = 0;

                    BList values = (BList) bResponse["values"];

                    if (values.Count == 0)
                    { 
                        node.status = Node.Status.REQUESTED;
                        responded++;
                        bResponse.Clear();

                        return;
                    }

                    Dictionary<string, int> curPeers = new Dictionary<string, int>();

                    if (isWeirdStrategy)
                        weirdPeers += values.Count;
                    else
                        normalPeers += values.Count;

                    //node.hasPeers = true;
                    havePeers++;

                    foreach (IBObject cur in values)
                    {
                        byte[] value    = ((BString) cur).Value.ToArray();
                        string curIP    = (new IPAddress(Utils.ArraySub(ref value, 0, 4))).ToString();
                        UInt16 curPort  = (UInt16) BitConverter.ToInt16(Utils.ArraySub(ref value, 4, 2, true), 0);

                        if (curPort < 500) continue; // Drop fake / Avoid DDOS

                        //if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [PEER] {curIP}:{curPort}");

                        curPeers[curIP] = curPort;
                    }

                    if (curPeers.Count > 0) options.Beggar.FillPeers(curPeers, BitSwarm.PeersStorage.DHT);

                    //if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [NEW PEERS] {newPeers}");

                    // Re-requesting same Node with Peers > 99 (max returned peers are 100?)
                    // Possible fake/random peers (escape recursion? 30 loops?) | NOTE: we loosing sync with scheduler because of that
                    if (status == Status.RUNNING && values.Count > 99)
                    {
                        if (selfRecursionLevel > 30)
                        {
                            if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [RE-REQUEST LIMIT EXCEEDED]");
                        }
                        else
                        {
                            selfRecursionLevel++;
                            if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [RE-REQUEST {selfRecursionLevel}] {newPeers}");
                            Thread.Sleep(10);
                            GetPeers(node, selfRecursionLevel); 
                        }   
                    }
                }

                node.status = Node.Status.REQUESTED;
                responded++;
                bResponse.Clear();
            } 
            catch (Exception e)
            {
                node.status = Node.Status.FAILED;
                if (options.Verbosity > 0) Log($"[{node.distance}] [{node.host}] [ERROR] {e.Message}\r\n{e.StackTrace}");
            }
        }

        private void Beggar()
        {
            if (options.Verbosity > 0) Log($"[BEGGAR] STARTED {infoHash}");

            long lastTicks      = DateTime.UtcNow.Ticks;
            int  curThreads     = 0;
            int  curSeconds     = 0;
            bool clearBucket    = false;
            List<string> curNodeKeys;

            while (status == Status.RUNNING)
            {
                // Prepare <NodesPerLevel> Nodes for Requesting
                curNodeKeys = new List<string>();
                for (int i=0; i<options.NodesPerLevel; i++)
                {
                    string newNodeHost = GetMinDistanceNode(); // Node.Status.New && Min(distance)
                    if (newNodeHost == null) break;
                    bucketNodesPointer[newNodeHost].status = Node.Status.REQUESTING;
                    curNodeKeys.Add(newNodeHost);
                }

                curThreads = curNodeKeys.Count;

                // End of Recursion | Reset This BucketNodes with Bootstraps and Flip Strategy
                if (curThreads == 0) 
                { 
                    if (status != Status.RUNNING) break;

                    if (options.Verbosity > 0) Log($"[BEGGAR] Recursion Ended (curThreads=0)... Flippping");
                    AddBootstrapNodes();
                    FlipStrategy();

                    continue;
                }

                // Start <NodesPerLevel> Nodes & Wait For Them
                foreach (string curNodeKey in curNodeKeys)
                {
                    Node node = bucketNodesPointer[curNodeKey];

                    System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                    {
                        GetPeers(node);
                        Interlocked.Decrement(ref curThreads);
                    }), null);
                }

                while (curThreads > 0) Thread.Sleep(20);

                // Clean Bucket from FAILED | REQUESTED | Out Of Distance
                inBucket = 0;
                curNodeKeys = new List<string>();
                
                if (bucketNodesPointer.Count > options.MaxBucketNodes)
                    clearBucket = true;

                foreach (KeyValuePair<string, Node> nodeKV in bucketNodesPointer)
                {
                    Node node = nodeKV.Value;
                    if (node.status == Node.Status.FAILED) 
                        curNodeKeys.Add(node.host);
                    //else if (node.status == Node.Status.REQUESTED && node.distance > minBucketDistance)
                    else if (node.status == Node.Status.REQUESTED) // Currently we dont re-use In Bucket Nodes
                        curNodeKeys.Add(node.host);
                    else if (clearBucket && node.distance > minBucketDistance)
                        curNodeKeys.Add(node.host);

                    if (node.distance <= minBucketDistance) inBucket++;
                }

                foreach (string curNodeKey in curNodeKeys)
                    bucketNodesPointer.Remove(curNodeKey);

                clearBucket = false;

                if (status != Status.RUNNING) break;

                // TODO: Review Scheduler seconds/levels messed up | with addition for GetPeers Recursion this is not accurate at all (TBR)
                if (DateTime.UtcNow.Ticks - lastTicks > 9000000)
                {
                    lastTicks = DateTime.UtcNow.Ticks;
                    curSeconds++;

                    // Stats
                    if (options.Verbosity > 0) Log($"[STATS] [REQs: {requested}]\t[RESPs: {responded}]\t[BUCKETSIZE: {bucketNodesPointer.Count}]\t[INBUCKET: {inBucket}]\t[PEERNODES: {havePeers}]\t[WEIRD]: {weirdPeers} | [NORMAL] {normalPeers}");

                    // Flip Strategy
                    if (curSeconds % 6 == 0) FlipStrategy();

                    // Clear Bad Nodes
                    if (curSeconds % 50 == 0) rememberBadNodes.Clear();

                    // TODO: Re-request existing Peer Nodes for new Peers
                }

                Thread.Sleep(20);
            }

            if (options.Verbosity > 0) Log($"[BEGGAR] STOPPED {infoHash}");
        }

        // Public
        public void Start()
        {
            if (status == Status.RUNNING || (beggar != null && beggar.IsAlive)) return;

            status = Status.RUNNING;
            StartedAt = DateTime.UtcNow.Ticks;

            beggar = new Thread(() =>
            {
                Beggar();
                status = Status.STOPPED;
                StoppedAt = DateTime.UtcNow.Ticks;
            });
            beggar.IsBackground = true;
            beggar.Start();
        }
        public void Stop()
        {
            if (status != Status.RUNNING) return;

            status = Status.STOPPING;
        }

        // Misc
        internal void Log(string msg) { if (options.Verbosity > 0) options.LogFile.Write($"[DHT] {msg}"); }
    }
}