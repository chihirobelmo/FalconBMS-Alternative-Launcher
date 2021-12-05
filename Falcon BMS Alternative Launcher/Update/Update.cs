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
        public BMS2 bms = new BMS2();
    }

    public class BMS2
    {
        public bool release;
        public Version version;
        public Tutorial tutorial;
        public Registory registry;
        public Installer installer;
        public InclementalUpdate2[] inclementalUpdate;
        public DownloadFiles[] files;

        public BMS2()
        {
        }
    }

    public class Download
    {
        public BMS bms = new BMS();

        public Download()
        {
        }
    }

    public class BMS
    {
        public bool release = true;
        public int major = 4;
        public int minor = 35;
        public int build = 3;
        public string setupExeName = "Setup.exe";
        public Setup setup;
        public IncrementalUpdate[] incrementalUpdates = new IncrementalUpdate[0];

        public BMS()
        {
        }

        public void addUpdate(IncrementalUpdate iu)
        {
            Array.Resize(ref incrementalUpdates, incrementalUpdates.Length + 1);
            incrementalUpdates[incrementalUpdates.Length - 1] = iu;
        }
    }
    public class Version
    {
        public int major;
        public int minor;
        public int build;
    }

    public class Setup : TorrentFile
    {
        public Setup() : base()
        {
        }

        public Setup(string args) : base(args)
        {
        }
    }

    public class IncrementalUpdate : TorrentFile
    {
        public IncrementalUpdate() : base()
        {
        }

        public IncrementalUpdate(string args) : base(args)
        {
        }
    }

    public class TorrentFile
    {
        public string hash = "!!!INPUT_TORRENT_HASH_HERE!!!";
        public string name;
        private string path;
        public File[] files;

        public TorrentFile()
        {
        }

        public TorrentFile(string args)
        {
            path = args;

            DirectoryInfo di = new DirectoryInfo(args);
            FileInfo[] fileInfos = di.GetFiles("*.*", SearchOption.AllDirectories);

            files = new File[fileInfos.Count()];

            for (int i = 0; i < fileInfos.Count(); i++)
            {
                files[i] = new File(fileInfos[i]);
            }
        }

        public void Download(string destination)
        {
            Torrent.status = true;
            Torrent.Download(hash, destination);
        }

        public void Download(DownloadWindow dw, string destination)
        {
            Torrent.status = true;
            Torrent.Download(dw, hash, name, destination);
        }

        public bool DestinationExist(string destination)
        {
            return Directory.Exists(destination);
        }

        public void DeleteBsf(string destination)
        {
            foreach (string pathFrom in Directory.EnumerateFiles(destination, "*.bsf"))
                System.IO.File.Delete(pathFrom);
            foreach (string pathFrom in Directory.EnumerateFiles(destination, "*.torrent"))
                System.IO.File.Delete(pathFrom);
        }
        public void calcSHA1()
        {
            for (int i = 0; i < files.Count(); i++)
                files[i].calcSHA1();
        }

        public void removePath()
        {
            for (int i = 0; i < files.Count(); i++)
            {
                files[i].removePath(path);
            }
        }
    }

    public class File
    {
        public string name;
        public string destination;
        public string hash;
        public string sha1;

        static readonly HashAlgorithm hashProvider = new SHA1CryptoServiceProvider();

        public File()
        {
        }

        public File(FileInfo fileInfo)
        {
            name = fileInfo.Name;
            destination = fileInfo.DirectoryName;
        }

        public virtual void Execute()
        {
            try
            {
                System.Diagnostics.Process.Start(destination + "\\" + name);
            }
            catch
            { }
        }
        public void calcSHA1()
        {
            using (var fs = new FileStream(destination + "\\" + name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bs = hashProvider.ComputeHash(fs);
                sha1 = BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }

        public virtual bool SHA1Check()
        {
            return SHA1Check(destination);
        }

        public virtual bool SHA1Check(string extraction)
        {
            try
            {
                if (sha1 == null)
                    return true;
                using (var fs = new FileStream(extraction + destination + "\\" + name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bs = hashProvider.ComputeHash(fs);
                    return BitConverter.ToString(bs).ToLower().Replace("-", "") == sha1;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Delete()
        {
            if (System.IO.File.Exists(destination + "\\" + name))
                System.IO.File.Delete(destination + "\\" + name);
        }

        // Remove
        public void Download(DownloadWindow dw)
        {
            Torrent.status = true;
            Torrent.Download(dw, hash, name, destination);
        }

        public virtual bool Exist()
        {
            return Exist(destination + name);
        }

        public virtual bool Exist(string extraction)
        {
            return System.IO.File.Exists(extraction + destination + "\\" + name);
        }

        // Remove
        public bool DestinationExist()
        {
            return DestinationExist(destination);
        }

        // Remove
        public bool DestinationExist(string extraction)
        {
            return Directory.Exists(extraction + destination);
        }

        // Remove
        public void DeleteBsf()
        {
            foreach (string pathFrom in System.IO.Directory.EnumerateFiles(destination, "*.bsf"))
                System.IO.File.Delete(pathFrom);
            foreach (string pathFrom in System.IO.Directory.EnumerateFiles(destination, "*.torrent"))
                System.IO.File.Delete(pathFrom);
        }

        public void removePath(string args)
        {
            destination = destination.Replace(args, "");
        }
    }

    /// <summary>
    /// ////////////////////////////////////////////////////
    /// </summary>

    public class DownloadFiles
    {
        public FileInfo fileInfo;
        public string name;
        public bool overwrite;
        public string destination;
        public string extraction;
        public double size;
        public string[] tracker;
        public string hash;
        public string sha1;
        public string magnet;
        public string link;
        public DownloadFiles[] file;
        public Dir[] dir;

        static readonly HashAlgorithm hashProvider = new SHA1CryptoServiceProvider();

        public DownloadFiles()
        {
        }

        public DownloadFiles(FileInfo fileInfo)
        {
            name = fileInfo.Name;
            destination = fileInfo.DirectoryName;
        }

        public virtual void Execute()
        {
            try
            {
                System.Diagnostics.Process.Start(extraction + destination + "\\" + name);
            }
            catch
            { }
        }
        public void calcSHA1()
        {
            using (var fs = new FileStream(destination + "\\" + name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bs = hashProvider.ComputeHash(fs);
                sha1 = BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }

        public virtual bool SHA1Check()
        {
            return SHA1Check(extraction);
        }

        public virtual bool SHA1Check(string extraction)
        {
            try
            {
                if (sha1 == null)
                    return true;
                using (var fs = new FileStream(extraction + destination + "\\" + name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bs = hashProvider.ComputeHash(fs);
                    return BitConverter.ToString(bs).ToLower().Replace("-", "") == sha1;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Delete()
        {
            if (System.IO.File.Exists(extraction + destination + "\\" + name))
                System.IO.File.Delete(extraction + destination + "\\" + name);
        }

        // Remove
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
            return System.IO.File.Exists(extraction + destination + "\\" + name);
        }

        // Remove
        public bool DestinationExist()
        {
            return DestinationExist(extraction);
        }

        // Remove
        public bool DestinationExist(string extraction)
        {
            return Directory.Exists(extraction + destination);
        }

        // Remove
        public void DeleteBsf()
        {
            foreach (string pathFrom in System.IO.Directory.EnumerateFiles(extraction + destination, "*.bsf"))
                System.IO.File.Delete(pathFrom);
            foreach (string pathFrom in System.IO.Directory.EnumerateFiles(extraction + destination, "*.torrent"))
                System.IO.File.Delete(pathFrom);
        }
    }

    public class Installer : Zip
    {
        public DownloadFiles exe;
    }

    public class InclementalUpdate2 : DownloadFiles
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
            FileStream zipStream = System.IO.File.OpenRead(extraction + destination + "\\" + name);
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

    public class Dir : DownloadFiles
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
                if (!file[i].SHA1Check())
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
