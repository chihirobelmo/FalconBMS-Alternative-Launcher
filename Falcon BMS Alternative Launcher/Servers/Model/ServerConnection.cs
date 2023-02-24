using FalconBMS.Launcher.Servers.ViewModel;
using IniParser.Model;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Windows.Documents;

namespace FalconBMS.Launcher.Servers
{
    internal class ServerConnection : ViewModelBase
    {
        public enum ServerStatus
        {
            Running, Offline, Unknown
        }

        private static readonly string BMS_SCHEME = "bms";

        private static readonly string BMS_PARAM_NAME = "name";
        private static readonly string BMS_PARAM_IVC_IP = "ivc";
        private static readonly string BMS_PARAM_IVC_PASSWORD = "ivc_pass";
        private static readonly string BMS_PARAM_BANDWIDTH_UP = "bw_up";
        private static readonly string BMS_PARAM_BANDWIDTH_DOWN = "bw_down";
        private static readonly string BMS_PARAM_THEATER_NAME = "theater";

        private ServerStatus status = ServerStatus.Unknown;
        private string name;
        private string bmsIp;
        private string iniFileName;
        private string iniFileUrl;
        private string ivcIp;
        private int pilotsOnline;
        private string ivcPassword;
        private double bandwidthUp = 2000;
        private double bandwidthDown = 2000;
        private string theaterName;
        private string serverInfoUrl;
        private long internalId;

        public string Name { get => name; set {
                name = value; 
                OnPropertyChanged("Name"); } 
        }
        public string BmsIp { get => bmsIp; set { bmsIp = value; OnPropertyChanged("BmsIp"); } }
        public string IvcIp { get => ivcIp; set { ivcIp = value; OnPropertyChanged("IvcIp"); } }
        public string IvcPassword { get => ivcPassword; set { ivcPassword = value; OnPropertyChanged("IvcPassword"); } }
        public double BandwidthUp { get => bandwidthUp; set { bandwidthUp = value; OnPropertyChanged("BandwidthUp"); } }
        
        public double BandwidthDown { get => bandwidthDown; set { bandwidthDown = value; OnPropertyChanged("BandwidthDown"); } }

        public double BandwidthUpMbps { get => bandwidthUp / 1000; set { bandwidthUp = value * 1000; OnPropertyChanged("BandwidthUpMbps"); } }

        public double BandwidthDownMbps { get => bandwidthDown / 1000; set {
                bandwidthDown = value * 1000;
                OnPropertyChanged("BandwidthDownMbps");
            } }

        public string TheaterName { get => theaterName; set {
                theaterName = value;
                OnPropertyChanged("TheaterName");
            } }

        public string Status { get => status.ToString(); set {
                status = (ServerStatus)Enum.Parse(typeof(ServerStatus), value);
                OnPropertyChanged("Status");
            } }

        public int PilotsOnline { get => pilotsOnline; set {
                pilotsOnline = value;
                OnPropertyChanged("PilotsOnline");
            } }
        public string ServerInfoUrl { get => serverInfoUrl; set {
                serverInfoUrl = value;
                OnPropertyChanged("ServerInfoUrl");
            } }
        public string IniFileName { get => iniFileName; set {
                iniFileName = value;
                OnPropertyChanged("IniFileName");
            } }
        public string IniFileUrl { get => iniFileUrl; set {
                iniFileUrl = value;
                OnPropertyChanged("IniFileUrl");
            } }
        public long InternalId { get => internalId; set {
                internalId = value;
                OnPropertyChanged("InternalId");
            } }

        public ServerConnection() { }

        public Boolean IsValid()
        {
            return !String.IsNullOrEmpty(BmsIp);
        }
        public ServerConnection(string name, string bmsIp, string ivcIp = "", string ivcPassword = "", double bandwidthUp = 0, double bandwidthDown = 0, string theaterName = "", ServerStatus serverStatus = ServerStatus.Unknown)
        {
            this.Name = name;
            this.BmsIp = bmsIp;
            this.IvcIp = ivcIp;
            this.IvcPassword = ivcPassword;
            this.BandwidthUp = bandwidthUp;
            this.BandwidthDown = bandwidthDown;
            this.TheaterName = theaterName;
            this.status = serverStatus;
        }

        public static ServerConnection From(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri is invalid or null");
            if (uri.Scheme == null || uri.Scheme != BMS_SCHEME) throw new ArgumentNullException("uri is not in the expected scheme: " + BMS_SCHEME);

            NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);

            ServerConnection serverConnection = new ServerConnection(
                name: queryParams.Get(BMS_PARAM_NAME) ?? "",
                bmsIp: uri.Host ?? "",
                ivcIp: queryParams.Get(BMS_PARAM_IVC_IP) ?? "",
                ivcPassword: queryParams.Get(BMS_PARAM_IVC_PASSWORD) ?? "",
                bandwidthUp: Double.Parse(queryParams.Get(BMS_PARAM_BANDWIDTH_UP) ?? "0"),
                bandwidthDown: Double.Parse(queryParams.Get(BMS_PARAM_BANDWIDTH_DOWN) ?? "0"),
                theaterName: queryParams.Get(BMS_PARAM_THEATER_NAME) ?? ""
                );

            if (!serverConnection.IsValid()) throw new ArgumentNullException("invalid bms uri");
            return serverConnection;
        }

        public void UpdateWith(ServerConnection newServerConnection)
        {
            // updated with remote server info
            if (newServerConnection.Status != ServerStatus.Unknown.ToString())
            {
                // always replace server name and basic data
                Name = newServerConnection.Name;
                IvcIp = newServerConnection.IvcIp;
                IniFileName = newServerConnection.IniFileName;
                IniFileUrl = newServerConnection.IniFileUrl;

                // replace the following data only if the update provides us with new data
                if (!String.IsNullOrEmpty(newServerConnection.IvcPassword)) IvcPassword = newServerConnection.IvcPassword;
                if (bandwidthDown <= 0) BandwidthDown = newServerConnection.BandwidthDown;
                if (bandwidthUp <= 0) BandwidthUp = newServerConnection.BandwidthUp;
                if (!String.IsNullOrEmpty(newServerConnection.TheaterName)) TheaterName = newServerConnection.TheaterName;

                PilotsOnline = newServerConnection.PilotsOnline;
                ServerInfoUrl = newServerConnection.ServerInfoUrl;
                Status = newServerConnection.Status;
            }
            else
            {
                // keep user defined stuff but clear out data which can only come from the server
                PilotsOnline = newServerConnection.PilotsOnline;
                ServerInfoUrl = newServerConnection.ServerInfoUrl;
                IniFileName = newServerConnection.IniFileName;
                Status = newServerConnection.Status;
            }
        }
    }
}
