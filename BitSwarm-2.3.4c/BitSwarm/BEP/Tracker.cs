using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

using BencodeNET.Parsing;
using BencodeNET.Objects;

namespace SuRGeoNix.BitSwarmLib.BEP
{
    /* TODO
     * 
     * 1. UDP: Review connID expiration (use interval from announce response)
     * 2. TCP: Not tested yet
     */

    internal class Tracker
    {
        public Uri      uri                         { get; private set; }
        public string   host                        { get; private set; }
        public int      port                        { get; private set; }
        public Type     type                        { get; private set; }
        public bool     failed                      { get; private set; }
        
        public Options  options;

        public struct Options
        {
            public string   InfoHash;
            public byte[]   PeerId;

            public int      ConnectTimeout;
            public int      ReceiveTimeout;

            public int      Verbosity;
            public Logger   LogFile;
        }
        public enum Type
        {
            UDP     = 1,
            HTTP    = 2,
            HTTPS   = 3
        }

        public UInt32   seeders     { get; private set; }
        public UInt32   leechers    { get; private set; }
        public UInt32   completed   { get; private set; }
        public UInt32   interval    { get; private set; }
        
        private const Int64 CONNECTION_ID = 0x41727101980;

        private UdpClient  udpClient;
        public  IPEndPoint rEP;

        private TcpClient       tcpClient;
        private NetworkStream   netStream;
        private SslStream       sslStream;
        
        private byte[]          recvBuff;

        private byte[]          action;
        private byte[]          connID;
        private byte[]          tranID;
        private byte[]          key;
        private byte[]          data;

        private string          typeHostPort;

        internal static BitSwarm Beggar;
        
        public static class Action
        {
            public const byte CONNECT   = 0x00;
            public const byte ANNOUNCE  = 0x01;
            public const byte SCRAPE    = 0x02;
            public const byte ERROR     = 0x03;
        }

        public Tracker(Uri url, Options options)
        {
            this.options    = options;
            uri             = url;
            host            = url.DnsSafeHost;
            port            = url.Port;
            key             = Utils.ToBigEndian((Int32) new Random().Next(1, Int32.MaxValue));

            switch (url.Scheme.ToLower())
            {
                case "http":
                    type = Type.HTTP;
                    break;
                case "https":
                    type = Type.HTTPS;
                    break;
                case "udp":
                    type = Type.UDP;
                    break;
                default:
                    break;
            }

            typeHostPort = (type.ToString().ToLower() + "://" + host + ":" + port).PadRight(50, ' ');
        }

        private void InitializeUDP()
        {
            IPAddress[] ips = null;

            try { ips = Dns.GetHostAddresses(uri.DnsSafeHost); } catch (Exception) { }
                    
            if (ips == null || ips.Length == 0) { failed = true; return; }

            foreach (var ip in ips)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    rEP = new IPEndPoint(ip, port);
                    
            // Need to implement also IPv6 retrieval to support IPv6
            //if (rEP == null)
            //    foreach (var ip in ips)
            //        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            //        { rEP = new IPEndPoint(ip, port); break; }

            if (rEP == null) { Log("DNS failed"); failed = true; return; }

            udpClient   = new UdpClient(0, AddressFamily.InterNetwork); // Currently only IPv4
            udpClient.Client.ReceiveTimeout = options.ReceiveTimeout;
        }

        // Main Implementation [Connect | Receive | Announce | Scrape]
        private bool ConnectTCP()
        {
            bool connected;
            tcpClient = new TcpClient();

            try
            {
                connected = tcpClient.ConnectAsync(host, port).Wait(options.ConnectTimeout);

                if (!connected) return false;

                if (type == Type.HTTP)
                    netStream               = tcpClient.GetStream();
                else
                {
                    sslStream               = new SslStream(tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
                    sslStream.AuthenticateAsClient(host);
                }

                //tcpClient.SendBufferSize    = 1024;
                //tcpClient.ReceiveBufferSize = 1500;
            }
            catch (Exception e)
            {
                Log("Failed " + e.Message);
                return false;
            }

            tcpClient.ReceiveTimeout = options.ReceiveTimeout;

            return true;
        }
        private bool ReceiveTCP()
        {
            try
            {
                string  headers = "";
                int     newLines= 0;
                recvBuff       = new byte[1];

                do
                {
                    if (type == Type.HTTP)
                        netStream.Read(recvBuff, 0, 1);
                    else
                        sslStream.Read(recvBuff, 0, 1);

                    if (recvBuff[0] == '\r' || recvBuff[0] == '\n')
                    {
                        if (recvBuff[0] == '\n')
                            newLines++;
                    }
                    else
                    {
                        newLines = 0;
                    }

                    headers += Convert.ToChar(recvBuff[0]);

                } while (newLines != 2);

                int len = int.Parse(Regex.Match(headers, @"Content-Length: ([0-9]+)").Groups[1].Value);

                recvBuff = new byte[len];
                if (type == Type.HTTP)
                    netStream.Read(recvBuff, 0, len);
                else
                    sslStream.Read(recvBuff, 0, len);

            } catch (Exception) { return false; }

            return true;
        }
        public bool Announce(   Int32 num_want = -1, Int64 downloaded = 0, Int64 left = 0, Int64 uploaded = 0)
        {
            if (type == Type.UDP)
                { AnnounceUDP(num_want, downloaded, left, uploaded); return true; }
            else
                return AnnounceTCP(num_want, downloaded, left, uploaded);
        }
        public bool AnnounceTCP(Int32 num_want = -1, Int64 downloaded = 0, Int64 left = 0, Int64 uploaded = 0)
        {
            if (!ConnectTCP()) return false;
            
            try
            {
                byte[] sendBuff = System.Text.Encoding.UTF8.GetBytes($"GET {uri.AbsolutePath}?info_hash={Utils.StringHexToUrlEncode(options.InfoHash)}&peer_id={Utils.StringHexToUrlEncode(BitConverter.ToString(options.PeerId).Replace("-",""))}&port=11111&left={left}&downloaded={downloaded}&uploaded={uploaded}&event=started&compact=1&numwant={num_want} HTTP/1.1\r\nHost: {host}:{port}\r\nConection: close\r\n\r\n");

                if (type == Type.HTTP)
                    netStream.Write(sendBuff, 0, sendBuff.Length);
                else
                    sslStream.Write(sendBuff, 0, sendBuff.Length);

                if (!ReceiveTCP()) return false;

                BencodeParser parser= new BencodeParser();
                BDictionary extDic  = parser.Parse<BDictionary>(recvBuff);

                byte[] hashBytes    = ((BString) extDic["peers"]).Value.ToArray();
                Dictionary<string, int> peers = new Dictionary<string, int>();

                for (int i=0; i<hashBytes.Length; i+=6)
                {
                    IPAddress curIP = new IPAddress(Utils.ArraySub(ref hashBytes,(uint) i, 4, false));
                    UInt16 curPort  = (UInt16) BitConverter.ToInt16(Utils.ArraySub(ref hashBytes,(uint) i + 4, 2, true), 0);
                    if (curPort > 0) peers[curIP.ToString()] = curPort;
                }

                if (options.Verbosity > 0) Log($"Success ({peers.Count} Peers)");

                if (peers.Count > 0) Beggar.FillPeers(peers, BitSwarm.PeersStorage.TRACKER);
            }
            catch (Exception e)
            {
                Log($"Failed {e.Message}\r\n{e.StackTrace}");
                return false;
            }

            return true;
        }
        public void AnnounceUDP(Int32 num_want = -1, Int64 downloaded = 0, Int64 left = 0, Int64 uploaded = 0)
        {
            try
            {
                // http://www.bittorrent.org/beps/bep_0015.html

                if (failed) return;
                if (udpClient == null) InitializeUDP();
                if (failed) return;

                // Allow Re-Requesting a Full Response Tracker (200 per response on Tracker & 100 on DHT?)
                if (num_want != -1 && curRecursions * 200 > num_want) return;
                if (num_want == -1 && curRecursions > 3) return;

                if (rEP == null) return;

                //udpClient.Close(); // To cancel previous BeginReceives?

                if (connID == null)
                {
                    // Connect Request
                    action = Utils.ToBigEndian((Int32) Action.CONNECT);
                    tranID = Utils.ToBigEndian((Int32) new Random().Next(1,Int32.MaxValue));
                    data   = Utils.ArrayMerge(Utils.ToBigEndian(CONNECTION_ID), action, tranID);
                
                    udpClient.Send(data, data.Length, rEP);
                    Log($"Connecting");
                }
                else
                {
                    // Announce Request
                    Int32 event_ = 0, externalIp = 0;
                    Int16 externalPort = 17253;

                    action  = Utils.ToBigEndian((Int32) Action.ANNOUNCE);
                    data    = Utils.ArrayMerge(connID, action, tranID, Utils.StringHexToArray(options.InfoHash), options.PeerId, Utils.ToBigEndian(downloaded), Utils.ToBigEndian(left), Utils.ToBigEndian(uploaded), Utils.ToBigEndian(event_), Utils.ToBigEndian(externalIp), key, Utils.ToBigEndian(num_want), Utils.ToBigEndian(externalPort));
                    udpClient.Send(data, data.Length, rEP);
                    Log($"Announcing (Recursion: {curRecursions})");
                }

                udpClient.BeginReceive(ReceiveUDP, null);

            } catch (Exception e) { Log(e.Message + " - " + e.StackTrace); }
        }

        private int curRecursions = 0;

        private void ReceiveUDP(IAsyncResult ar)
        {
            try
            {
                recvBuff = udpClient.EndReceive(ar, ref rEP);
                Log($"Receive Data ({recvBuff?.Length})");

                if (recvBuff == null || recvBuff.Length < 4) { Log($"Failed"); return;}

                byte[] action = Utils.ArraySub(ref recvBuff, 0, 4);

                if (Utils.ArrayComp(Utils.ToBigEndian((Int32) Action.CONNECT), action))
                {
                    if (recvBuff.Length < 12) { Log($"Failed"); return;}

                    connID = Utils.ArraySub(ref recvBuff, 8, 8); // Valid for 60 seconds
                    Log($"Connected (-> Announcing)");
                    AnnounceUDP();
                }
                else if (Utils.ArrayComp(Utils.ToBigEndian((Int32) Action.ERROR), action))
                {
                    //TODO: Validate error message (Connection ID failed/bad etc) and/or use internval returned from Announce Response to Re-connect
                    Log($"Re-Connecting (-> Connecting)");
                    connID = null;
                    AnnounceUDP();
                }
                else if (Utils.ArrayComp(Utils.ToBigEndian((Int32) Action.ANNOUNCE), action))
                {
                    if (recvBuff.Length < 20) { Log($"Failed"); return;}

                    // Announce Response | Currently not in use
                    //interval    = (UInt32) BitConverter.ToInt32(Utils.ArraySub(ref recvBuff,  8, 4, true), 0);
                    //seeders     = (UInt32) BitConverter.ToInt32(Utils.ArraySub(ref recvBuff, 12, 4, true), 0);
                    //leechers    = (UInt32) BitConverter.ToInt32(Utils.ArraySub(ref recvBuff, 16, 4, true), 0);

                    Dictionary<string, int> peers = new Dictionary<string, int>();

                    for (int i=0; i<(recvBuff.Length - 20) / 6; i++)
                    {
                        IPAddress curIP = new IPAddress(Utils.ArraySub(ref recvBuff,(uint) (20 + (i*6)), 4, false));
                        UInt16 curPort  = (UInt16) BitConverter.ToInt16(Utils.ArraySub(ref recvBuff,(uint) (24 + (i*6)), 2, true), 0);

                        if (curPort < 500) continue; // Drop fake / Avoid DDOS

                        peers[curIP.ToString()] = curPort;
                    }

                    if (options.Verbosity > 0) Log($"Success ({peers.Count} Peers)");

                    if (peers.Count > 0) Beggar.FillPeers(peers, BitSwarm.PeersStorage.TRACKER);

                    // Check with bytes cause Peers maybe < 200 | We drop some invalid ports or we dont read them properly?
                    if (recvBuff.Length == 1220) { curRecursions++; AnnounceUDP(); }
                }

            } catch (Exception e) { Log(e.Message + " - " + e.StackTrace); }
        }

        public bool ScrapeUDP(string infoHash)
        {
            // Currently not used

            //if (ConnectUDP()) return false;

            // Scrape Request
            action = Utils.ToBigEndian((Int32) Action.SCRAPE);
            data   = Utils.ArrayMerge(connID, action, tranID, Utils.StringHexToArray(infoHash));

            udpClient.Send(data, data.Length, rEP);
            recvBuff = udpClient.Receive(ref rEP);

            if (recvBuff == null || recvBuff.Length == 0) return false;

            // Scrape Response
            leechers    = (UInt32) BitConverter.ToInt32(Utils.ArraySub(ref recvBuff,  8, 4, true), 0);
            completed   = (UInt32) BitConverter.ToInt32(Utils.ArraySub(ref recvBuff, 12, 4, true), 0);
            seeders     = (UInt32) BitConverter.ToInt32(Utils.ArraySub(ref recvBuff, 16, 4, true), 0);

            return true;
        }

        // Misc
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; }
        internal void Log(string msg) { if (options.Verbosity > 0) options.LogFile.Write($"[Tracker ] [{typeHostPort}] {msg}"); }
    }
}