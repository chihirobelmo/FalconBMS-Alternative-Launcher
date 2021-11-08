using Leak.Client.Swarm;
using Leak.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS.Launcher
{
    public class Torrent
    {
        public static async Task Main()
        {
            string tracker = "udp://tracker.opentrackr.org:1337/announce";
            FileHash hash = FileHash.Parse("BA5DA8EFC007C2A57FF4F234625EDE94464C90A0");

            using (SwarmClient client = new SwarmClient())
            {
                Leak.Client.Notification notification = null;
                SwarmSession session = await client.ConnectAsync(hash, tracker);

                session.Download("d:\\leak");

                Console.WriteLine("StartPeerConnecting");
                do
                {
                    Console.WriteLine("PeerConnecting");
                    notification = await session.NextAsync();
                }
                while (notification.Type != Leak.Client.NotificationType.PeerConnected);
                Console.WriteLine("PeerConnected");

                Console.WriteLine("StartAsync");
                do
                {
                    Console.WriteLine("Asyncing");
                    notification = await session.NextAsync();
                }
                while (notification.Type != Leak.Client.NotificationType.DataCompleted);
                Console.WriteLine("AsyncComplete");
            }
        }
    }
}
