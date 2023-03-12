using FalconBMS.Launcher.Servers.ViewModel;
using IniParser;
using IniParser.Model;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using static System.Net.Mime.MediaTypeNames;

namespace FalconBMS.Launcher.Servers
{
    internal class PhoneBookParser: ViewModelBase
    {
        private readonly string BMS_PHONEBOOK_RELATIVE_PATH = "\\User\\Config\\PhoneBkn.ini";
        private readonly string BMS_PHONEBOOK_ENTRY_PREFIX = "Contact";

        // Default BMS data
        private readonly string NAME_KEY = "Description";
        private readonly string BMS_IP_KEY = "IPaddress";
        private readonly string IVC_IP_KEY = "Voicehostip";
        private readonly string IVC_PASSWORD_KEY = "VoicehostPwd";
        private readonly string DOWNLOAD_BANDWIDTH_KEY = "SetBwDownload";
        private readonly string UPLOAD_BANDWIDTH_KEY = "SetBwUpload";

        // Custom data
        private readonly string THEATER_NAME_KEY = "Theater";

        public ObservableCollection<ServerConnection> ServerConnections { get; set; }

        private ICommand removeCommand, addCommand;
        
        private AppRegInfo AppReg;
        private FileIniDataParser parser = new FileIniDataParser();

        public ICommand RemoveCommand
        {
            get
            {
                if (removeCommand == null)
                {
                    removeCommand = new DelegateCommand(RemoveEntry);
                }
                return removeCommand;
            }
        }

        public ICommand AddCommand
        {
            get
            {
                if (addCommand == null)
                {
                    addCommand = new DelegateCommand(AddEntry);
                }
                return addCommand;
            }
        }

        public async void AddEntry(object parameter)
        {
            ServerConnection serverConnection = (ServerConnection)parameter;
            if (serverConnection == null)
            {
                throw new Exception("can not add null entry");
            }

            List<ServerConnection> newServerConnections = await F4ServerInfoService.GetRemoteInfo(serverConnection.BmsIp);

            foreach (ServerConnection newServerConnection in newServerConnections)
            {
                ServerConnection oldServerConnection = ServerConnections.FirstOrDefault(s => s.BmsIp == newServerConnection.BmsIp);
                int index = ServerConnections.IndexOf(oldServerConnection);
                if (oldServerConnection == null)
                {
                    // new connection
                    newServerConnection.PropertyChanged+= ServerConnection_PropertyChanged;
                    ServerConnections.Add(newServerConnection);
                    SaveNewConnectionToFile(newServerConnection);

                }
                else
                {
                    // update old connection
                    oldServerConnection.UpdateWith(newServerConnection);
                    ForceUpdateConnectionInFile(oldServerConnection, index);
                }
            }
        }

        public void RemoveEntry(object parameter)
        {
            int index = ServerConnections.IndexOf(parameter as ServerConnection);
            if (index > -1 && index < ServerConnections.Count)
            {
                ServerConnections.RemoveAt(index);
                DeleteConnectionFromFile(index);
            }
        }
    

        public PhoneBookParser(AppRegInfo appReg)
        {
            this.AppReg = appReg;

            ServerConnections = new ObservableCollection<ServerConnection>(ReadConnectionsFromFile());
            this.PropertyChanged += ServerConnection_PropertyChanged;
        }

        private void ServerConnection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ServerConnection changedServerConnection = (ServerConnection)sender;
            int index = ServerConnections.IndexOf(changedServerConnection);
            if (index > -1)
            {
                ForceUpdateConnectionInFile(changedServerConnection, index);
            }
        }

        private List<ServerConnection> ReadConnectionsFromFile()
        {

            List<ServerConnection> serverConnections = new List<ServerConnection>();
            IniData data = parser.ReadFile(AppReg.GetInstallDir() + BMS_PHONEBOOK_RELATIVE_PATH);

            long rowIndex = 0;
            // assume that sections are in order
            foreach (SectionData sectionData in data.Sections)
            {
                if (!sectionData.SectionName.StartsWith(BMS_PHONEBOOK_ENTRY_PREFIX)) continue;

                double bandwidthDown = 1024;
                double bandwidthUp = 1024;

                Double.TryParse(sectionData.Keys[UPLOAD_BANDWIDTH_KEY], out bandwidthUp);
                Double.TryParse(sectionData.Keys[DOWNLOAD_BANDWIDTH_KEY], out bandwidthDown);

                if (bandwidthDown == -1)
                {
                    bandwidthDown = 1024;
                }

                if (bandwidthUp == -1)
                {
                    bandwidthUp = 1024;
                }

                ServerConnection newServerConnection = new ServerConnection(
                    name: sectionData.Keys[NAME_KEY] ?? "",
                    bmsIp: sectionData.Keys[BMS_IP_KEY] ?? "",
                    ivcIp: sectionData.Keys[IVC_IP_KEY] ?? "",
                    ivcPassword: sectionData.Keys[IVC_PASSWORD_KEY] ?? "",
                    bandwidthUp: bandwidthUp,
                    bandwidthDown: bandwidthDown,
                    theaterName: sectionData.Keys[THEATER_NAME_KEY] ?? ""
                    );

                newServerConnection.PropertyChanged += ServerConnection_PropertyChanged;
                serverConnections.Add(newServerConnection);

                rowIndex++;
            }

            return serverConnections;

        }

        public void SaveNewConnectionToFile(ServerConnection serverConnection)
        {
            if (serverConnection != null)
            {
                IniData data = parser.ReadFile(AppReg.GetInstallDir() + BMS_PHONEBOOK_RELATIVE_PATH);

                
                int lastSectionIndex = 0;

                // get last section index
                if (data.Sections.Count > 0)
                {
                    Regex regex = new Regex("(\\d+$)");
                    Match match = regex.Match(data.Sections.Last<SectionData>().SectionName);
                    if (match.Success)
                    {
                        lastSectionIndex = int.Parse(match.Groups[0].Value);
                    }
                    else
                    {
                        throw new Exception("Malformed phonebook file");
                    }
                }

                SectionData newSectionData = new SectionData(String.Format("{0} {1}", BMS_PHONEBOOK_ENTRY_PREFIX, lastSectionIndex + 1));
                newSectionData.Keys[NAME_KEY] = serverConnection.Name;
                newSectionData.Keys[BMS_IP_KEY] = serverConnection.BmsIp;
                newSectionData.Keys[IVC_IP_KEY] = serverConnection.IvcIp;
                newSectionData.Keys[IVC_PASSWORD_KEY] = serverConnection.IvcPassword;
                newSectionData.Keys[DOWNLOAD_BANDWIDTH_KEY] = serverConnection.BandwidthDown.ToString();
                newSectionData.Keys[UPLOAD_BANDWIDTH_KEY] = serverConnection.BandwidthUp.ToString();
                newSectionData.Keys[THEATER_NAME_KEY] = serverConnection.TheaterName;

                data.Sections.Add(newSectionData);
                parser.WriteFile(AppReg.GetInstallDir() + BMS_PHONEBOOK_RELATIVE_PATH, data);
            }
        }

        public void OverwriteAllConnectionsInFile(List<ServerConnection> connections)
        {
            IniData data = new IniData();
            
            int contactIndex = 1; // BMS quirk

            foreach (ServerConnection serverConnection in connections)
            {
                string sectionName = String.Format("{0} {1}", BMS_PHONEBOOK_ENTRY_PREFIX, contactIndex);
                data.Sections.AddSection(sectionName);
                SectionData newSectionData = new SectionData(String.Format("{0} {1}", BMS_PHONEBOOK_ENTRY_PREFIX, contactIndex));
                data[sectionName].AddKey(NAME_KEY, serverConnection.Name);
                data[sectionName].AddKey(BMS_IP_KEY, serverConnection.BmsIp);
                data[sectionName].AddKey(IVC_IP_KEY, serverConnection.IvcIp);
                data[sectionName].AddKey(IVC_PASSWORD_KEY, serverConnection.IvcPassword);
                data[sectionName].AddKey(DOWNLOAD_BANDWIDTH_KEY, serverConnection.BandwidthDown.ToString());
                data[sectionName].AddKey(UPLOAD_BANDWIDTH_KEY, serverConnection.BandwidthUp.ToString());
                data[sectionName].AddKey(THEATER_NAME_KEY, serverConnection.TheaterName);

                contactIndex++;
            }

            parser.WriteFile(AppReg.GetInstallDir() + BMS_PHONEBOOK_RELATIVE_PATH, data, new UTF8Encoding(false)); // do not use the regular UTF8 encoder since the BOM trips up BMS
        }

        public void ForceUpdateConnectionInFile(ServerConnection serverConnection, int index)
        {
            if (serverConnection != null)
            {
                
                IniData data = parser.ReadFile(AppReg.GetInstallDir() + BMS_PHONEBOOK_RELATIVE_PATH);

                if (index > data.Sections.Count - 1)
                {
                    string sectionName = String.Format("{0} {1}", BMS_PHONEBOOK_ENTRY_PREFIX, index  + 1);
                    data.Sections.AddSection(sectionName);
                    data[sectionName].AddKey(NAME_KEY, serverConnection.Name);
                    data[sectionName].AddKey(BMS_IP_KEY, serverConnection.BmsIp);
                    data[sectionName].AddKey(IVC_IP_KEY, serverConnection.IvcIp);
                    data[sectionName].AddKey(IVC_PASSWORD_KEY, serverConnection.IvcPassword);
                    data[sectionName].AddKey(DOWNLOAD_BANDWIDTH_KEY, serverConnection.BandwidthDown.ToString());
                    data[sectionName].AddKey(UPLOAD_BANDWIDTH_KEY, serverConnection.BandwidthUp.ToString());
                    data[sectionName].AddKey(THEATER_NAME_KEY, serverConnection.TheaterName);
                }
                else
                {
                    SectionData sectionData = data.Sections.ElementAt(index);

                    sectionData.Keys[NAME_KEY] = serverConnection.Name;
                    sectionData.Keys[BMS_IP_KEY] = serverConnection.BmsIp;
                    sectionData.Keys[IVC_IP_KEY] = serverConnection.IvcIp;
                    sectionData.Keys[IVC_PASSWORD_KEY] = serverConnection.IvcPassword;
                    sectionData.Keys[DOWNLOAD_BANDWIDTH_KEY] = serverConnection.BandwidthDown.ToString();
                    sectionData.Keys[UPLOAD_BANDWIDTH_KEY] = serverConnection.BandwidthUp.ToString();
                    sectionData.Keys[THEATER_NAME_KEY] = serverConnection.TheaterName;
                }

                parser.WriteFile(AppReg.GetInstallDir() + BMS_PHONEBOOK_RELATIVE_PATH, data, new UTF8Encoding(false));
            }
        }

        private void DeleteConnectionFromFile(int index)
        {
            IniData data = parser.ReadFile(AppReg.GetInstallDir() + BMS_PHONEBOOK_RELATIVE_PATH);

            String sectionName = String.Format("{0} {1}", BMS_PHONEBOOK_ENTRY_PREFIX, index + 1);
            if (!data.Sections.RemoveSection(sectionName))
            {
                throw new Exception(String.Format("section {0} not found", sectionName));
            }

            // fix all "holes"
            int sectionIndex = 1;
            foreach (SectionData section in data.Sections)
            {
                if (section.SectionName.StartsWith(BMS_PHONEBOOK_ENTRY_PREFIX))
                {
                    section.SectionName = String.Format("{0} {1}", BMS_PHONEBOOK_ENTRY_PREFIX, sectionIndex);
                    sectionIndex++;
                }
            }

            parser.WriteFile(AppReg.GetInstallDir() + BMS_PHONEBOOK_RELATIVE_PATH, data, new UTF8Encoding(false));
        }

        public async Task RefreshAllOnline()
        {
            IEnumerable<ServerConnection> queryableServers = ServerConnections.Where(c => !string.IsNullOrEmpty(c.BmsIp));
            Task<List<ServerConnection>>[] asyncTasks = (from serverconnection in queryableServers select F4ServerInfoService.GetRemoteInfo(serverconnection.BmsIp)).ToArray();

            List<ServerConnection>[] newConnections = await Task.WhenAll(asyncTasks);
            List<String> parsedIps= new List<String>();

            foreach (ServerConnection newConnection in newConnections.SelectMany(c => c))
            {
                if (parsedIps.Contains(newConnection.BmsIp)) { continue; }

                ServerConnection oldConnection = ServerConnections.Where(c => c.BmsIp== newConnection.BmsIp).FirstOrDefault();
                if (oldConnection == null)
                {
                    ServerConnections.Add(newConnection);
                }
                else
                {
                    oldConnection.UpdateWith(newConnection);
                }
                parsedIps.Add(newConnection.BmsIp);
            }
        }
    }
}


 
