using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS.Launcher.Update
{
    public class Update
    {
        public BMS bms = new BMS();
    }

    public class BMS
    {
        public bool release;
        public Version version;
        public Tutorial tutorial;
        public Registory registry;
        public Installer installer;
        public InclementalUpdate[] inclementalUpdate;
    }

    public class Version
    {
        public int major;
        public int minor;
        public int build;
    }

    public class TorrentFile
    {
        public string name;
        public bool overwrite;
        public string destination;
        public double size;
        public string[] tracker;
        public string hash;
        public string md5;
        public string magnet;

        static readonly HashAlgorithm hashProvider = new MD5CryptoServiceProvider();

        public bool MD5Check()
        {
            return MD5Check(destination);
        }

        public bool MD5Check(string destination)
        {
            try
            {
                using (var fs = new FileStream(destination + "\\" + name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bs = hashProvider.ComputeHash(fs);
                    return BitConverter.ToString(bs).ToLower().Replace("-", "") == md5;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class Installer
    {
        public Zip zip;
        public InclementalUpdate[] inclementalUpdate;
    }

    public class InclementalUpdate : TorrentFile
    {
        public TorrentFile[] file;
        public Dir dir;
    }

    public class Zip : TorrentFile
    {
        public TorrentFile[] file;
        public Dir dir;
        public void ExtractToDirectory()
        {
            ExtractToDirectory(destination);
        }
        public void ExtractToDirectory(string destination)
        {
            FileStream zipStream = File.OpenRead(destination + "\\" + name);
            using (ZipArchive archive = new ZipArchive(zipStream))
                ExtractToDirectory(archive, destination);
        }
        public void ExtractToDirectory(ZipArchive archive, string destination)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destination);
                return;
            }

            DirectoryInfo di = Directory.CreateDirectory(destination);
            string destinationDirectoryFullPath = di.FullName;

            foreach (ZipArchiveEntry file in archive.Entries)
            {

                string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                }

                if (file.Name == "")
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }
    }

    public class Dir
    {
        public string name;
        public TorrentFile[] file;
        public Dir[] dir;
    }

    public class Registory
    {
        public string name;
        public string path;
    }

    public class Tutorial
    {
        public string downloadUrl;
        public string installUrl;
        public string updateUrl;
    }
}
