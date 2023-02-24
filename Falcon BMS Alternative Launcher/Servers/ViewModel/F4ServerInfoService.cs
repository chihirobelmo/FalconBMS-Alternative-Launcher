using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FalconBMS.Launcher.Servers
{
    internal class F4ServerInfoService
    {
        private static int F4_SERVER_INFO_PORT = 2936;
        private static HttpClient client = new HttpClient();
        public static async Task<List<ServerConnection>> GetRemoteInfo(String remoteIp)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {

                    client.Timeout = TimeSpan.FromSeconds(1);
                    client.BaseAddress = new Uri("http://" + remoteIp + ":" + F4_SERVER_INFO_PORT);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                }
                catch (System.UriFormatException e)
                {
                    return new List<ServerConnection>();
                }

                try
                {
                    HttpResponseMessage response = await client.GetAsync("serverinfo");

                    if (response.IsSuccessStatusCode)
                    {
                        String responseJson = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<ServerConnection>>(responseJson);

                    }
                    else
                    {
                        throw new Exception();
                    }

                }
                catch (Exception e)
                {
                    // create a ServerConnection with the basic info we can deduce
                    return new List<ServerConnection> { new ServerConnection(name: remoteIp, bmsIp: remoteIp) };
                }
                
                client.Dispose();
            }
        }

        public static async Task DownloadIniFile(ServerConnection serverConnection, string outputFile)
        {
            using (var stream = await client.GetStreamAsync(serverConnection.IniFileUrl))
            {
                using (var fs = new FileStream(outputFile, FileMode.OpenOrCreate))
                {
                    stream.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
            }
        }
    }
}
