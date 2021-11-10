using FalconBMS.Launcher.Windows;
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
        public string extraction;
        public double size;
        public string[] tracker;
        public string hash;
        public string md5;
        public string magnet;
        public string link;
        public TorrentFile[] file;
        public Dir[] dir;

        static readonly HashAlgorithm hashProvider = new MD5CryptoServiceProvider();

        public virtual void Execute()
        {
            try
            {
                System.Diagnostics.Process.Start(extraction + destination + "\\" + name);
            }
            catch
            { }
        }

        public virtual bool MD5Check()
        {
            return MD5Check(extraction);
        }

        public virtual bool MD5Check(string extraction)
        {
            try
            {
                if (md5 == null)
                    return true;
                using (var fs = new FileStream(extraction + destination + "\\" + name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

        public void Delete()
        {
            if (File.Exists(extraction + destination + "\\" + name))
                File.Delete(extraction + destination + "\\" + name);
        }

        public void Download(DownloadWindow dw)
        {
            Torrent.status = true;
            Torrent.Download(dw, hash, name, extraction + destination);
        }


        public virtual bool Exist()
        {
            return Exist(extraction);
        }

        public virtual bool Exist(string extraction)
        {
            return File.Exists(extraction + destination + "\\" + name);
        }
        public bool DestinationExist()
        {
            return DestinationExist(extraction);
        }

        public bool DestinationExist(string extraction)
        {
            return Directory.Exists(extraction + destination);
        }

        public void DeleteBsf()
        {
            foreach (string pathFrom in System.IO.Directory.EnumerateFiles(extraction + destination, "*.bsf"))
                File.Delete(pathFrom);
            foreach (string pathFrom in System.IO.Directory.EnumerateFiles(extraction + destination, "*.torrent"))
                File.Delete(pathFrom);
        }
    }

    public class Installer : Zip
    {
        public TorrentFile exe;
    }

    public class InclementalUpdate : TorrentFile
    {
    }

    public class Zip : Dir
    {
        public void ExtractToDirectory()
        {
            ExtractToDirectory(extraction);
        }
        public void ExtractToDirectory(string extraction)
        {
            FileStream zipStream = File.OpenRead(extraction + destination + "\\" + name);
            using (ZipArchive archive = new ZipArchive(zipStream))
                ExtractToDirectory(archive, extraction);
        }
        public void ExtractToDirectory(ZipArchive archive, string extraction)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(extraction);
                return;
            }

            DirectoryInfo di = Directory.CreateDirectory(extraction);
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

    public class Dir : TorrentFile
    {

        public bool Exist(string extraction)
        {
            return Directory.Exists(extraction + destination + "\\" + name);
        }

        public bool MD5Check()
        {
            if (file == null)
                return true;
            for (int i = 0; i < file.Length; i++)
            {
                if (file[i] == null)
                    return true;
                if (!file[i].MD5Check())
                    return false;
            }
            return true;
        }
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
