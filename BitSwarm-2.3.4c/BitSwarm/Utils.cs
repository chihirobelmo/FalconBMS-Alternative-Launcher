using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security;

using BencodeNET.Parsing;
using BencodeNET.Objects;

namespace SuRGeoNix
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial class Utils
    {
        public static bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // Dir / File Next Available
        public static string GetValidFileName(string name)      { return string.Join("_", name.Split(Path.GetInvalidFileNameChars())); }
        public static string GetValidPathName(string name)      { return string.Join("_", name.Split(Path.GetInvalidPathChars())).Replace("..", "_"); }
        public static string GetValidFilePathName(string name)  { return (GetValidPathName(GetValidFileName(name))); }
        public static string FindNextAvailablePartFile(string fileName)
        {
            // Windows MAX_PATH = 260
            if (fileName.Length > 250) fileName = fileName.Substring(0, 250 - Path.GetExtension(fileName).Length) + Path.GetExtension(fileName);

            if (!File.Exists(fileName) && !File.Exists(fileName + ".part")) return fileName;

            string tmp = Path.Combine(Path.GetDirectoryName(fileName),Regex.Replace(Path.GetFileNameWithoutExtension(fileName), @"(.*) (\([0-9]+)\)$", "$1"));
            string newName;

            for (int i=1; i<1000; i++)
            {
                newName = tmp  + " (" + i + ")" + Path.GetExtension(fileName);
                if (!File.Exists(newName) && !File.Exists(newName + ".part")) return newName;
            }

            return null;
        }
        public static string FindNextAvailableDir(string dir)
        {
            if (!Directory.Exists(dir)) return dir;

            string tmp = Regex.Replace(dir, @"(.*)\\+$", "$1");
            tmp = Path.Combine(Path.GetDirectoryName(tmp),Regex.Replace(Path.GetFileName(tmp), @"(.*) (\([0-9]+)\)$", "$1"));
            string newName;

            for (int i=1; i<101; i++)
            {
                newName = tmp  + " (" + i + ")";
                if (!Directory.Exists(newName)) return newName;
            }

            return null;
        }
        public static string FindNextAvailableFile(string fileName)
        {
            if (!File.Exists(fileName)) return fileName;

            string tmp = Path.Combine(Path.GetDirectoryName(fileName),Regex.Replace(Path.GetFileNameWithoutExtension(fileName), @"(.*) (\([0-9]+)\)$", "$1"));
            string newName;

            for (int i=1; i<101; i++)
            {
                newName = tmp  + " (" + i + ")" + Path.GetExtension(fileName);
                if (!File.Exists(newName)) return newName;
            }

            return null;
        }

        // BEncode
        public static object GetFromBDic(BDictionary dic, string[] path)
        {

            return GetFromBDicRec(dic, path, 0);
        }
        private static object GetFromBDicRec(BDictionary dic, string[] path, int level)
        {
            IBObject ibo = dic[path[level]];
            if (ibo == null) return null;

            if (level == path.Length - 1)
            {
                if (ibo.GetType() == typeof(BString))
                    return ibo.ToString();
                else if (ibo.GetType() == typeof(BNumber))
                    return (int)((BNumber)ibo).Value;
                else if (ibo.GetType() == typeof(BDictionary))
                    return ((BDictionary)ibo).Value;
            }
            else
                if (ibo.GetType() == typeof(BDictionary))
                return GetFromBDicRec((BDictionary)ibo, path, level + 1);

            return null;
        }
        public static void printBEnc(string benc)
        {
            BencodeParser parser = new BencodeParser();
            BDictionary bdic = parser.ParseString<BDictionary>(benc.Trim());
            printDicRec(bdic, 0);
        }
        public static void printDicRec(BDictionary dic, int level = 0)
        {
            foreach (KeyValuePair<BString, IBObject> a in dic) {
                if (a.Value.GetType() == typeof(BDictionary))
                {
                    Console.WriteLine(String.Concat(Enumerable.Repeat("\t", level)) + a.Key);
                    level++;
                    printDicRec((BDictionary) a.Value, level);
                    level--;
                } else if (a.Value.GetType() == typeof(BList)) {

                } else
                {
                    Console.WriteLine(String.Concat(Enumerable.Repeat("\t", level)) + a.Key + "-> " + a.Value);
                }
            }
        }

        // To Big Endian
        public static int ToBigEndian(byte[] input)
        {
            if (BitConverter.IsLittleEndian) Array.Reverse(input);
            return BitConverter.ToInt32(input, 0);
        }
        public static byte[] ToBigEndian(byte input)
        {
            return new byte[1] {input};
        }
        public static byte[] ToBigEndian(Int16 input)
        {
            byte[] output = BitConverter.GetBytes(input);
            if (!BitConverter.IsLittleEndian) return output;

            Array.Reverse(output);
            return output;
        }
        public static byte[] ToBigEndian(Int32 input)
        {
            byte[] output = BitConverter.GetBytes(input);
            if (!BitConverter.IsLittleEndian) return output;

            Array.Reverse(output);
            return output;
        }
        public static byte[] ToBigEndian(Int64 input)
        {
            byte[] output = BitConverter.GetBytes(input);
            if (!BitConverter.IsLittleEndian) return output;

            Array.Reverse(output);
            return output;
        }

        // Arrays
        public static T[] ArraySub<T>(ref T[] data, long index, long length, bool reverse = false)
        {
            T[] result = new T[length];
            Buffer.BlockCopy(data, (int)index, result, 0, (int)length);
            if (reverse) Array.Reverse(result);
            return result;
        }
        public static unsafe bool ArrayComp(byte[] a1, byte[] a2)
        {
          if(a1==a2) return true;
          if(a1==null || a2==null || a1.Length!=a2.Length)
            return false;
          fixed (byte* p1=a1, p2=a2) {
            byte* x1=p1, x2=p2;
            int l = a1.Length;
            for (int i=0; i < l/8; i++, x1+=8, x2+=8)
              if (*((long*)x1) != *((long*)x2)) return false;
            if ((l & 4)!=0) { if (*((int*)x1)!=*((int*)x2)) return false; x1+=4; x2+=4; }
            if ((l & 2)!=0) { if (*((short*)x1)!=*((short*)x2)) return false; x1+=2; x2+=2; }
            if ((l & 1)!=0) if (*((byte*)x1) != *((byte*)x2)) return false;
            return true;
          }
        }
        public static byte[] ArrayMerge(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays) {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        public static string ArrayToStringHex(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-","");
        }
        public static byte[] StringHexToArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }
        public static string StringHexToUrlEncode(string hex)
        {
            string hex2 = "";

            for (int i=0; i<hex.Length; i++)
            {
                if (i % 2 == 0) hex2 += "%";
                hex2 += hex[i];
            }

            return hex2;
        }

        // Misc
        public static List<Tuple<long, int>> MergeByteRanges(List<Tuple<long, int>> byteRanges, int allowSpaceBytes = 0)
        {
            if (byteRanges == null || byteRanges.Count < 1) return new List<Tuple<long, int>>();

            List<Tuple<long, int>> byteRangesCopy = new List<Tuple<long, int>>(byteRanges);
            
            byteRangesCopy.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            List<Tuple<long, int>> cByteRanges = new List<Tuple<long, int>>();

            Tuple<long, int> prevRange = byteRangesCopy[0];

            for (int i=1; i<byteRangesCopy.Count; i++)
            {
                Tuple<long, int> curRange = byteRangesCopy[i];

                // Same Start => End = Max(Len1/2)
                if (curRange.Item1 == prevRange.Item1) { prevRange = new Tuple<long, int>(prevRange.Item1, Math.Max(prevRange.Item2, curRange.Item2)); continue; }

                // Out of Allowed Bytes => Add new Range / Set prev to cur
                if (curRange.Item1 > prevRange.Item1 + prevRange.Item2 + allowSpaceBytes) { cByteRanges.Add(prevRange); prevRange = new Tuple<long, int>(curRange.Item1, curRange.Item2); continue; }

                // It's inside prev ... skip
                if (curRange.Item1 + curRange.Item2 <= prevRange.Item1 + prevRange.Item2) continue;

                // From prev Start to Max(End1/2) - Start
                prevRange = new Tuple<long, int>(prevRange.Item1, (int) (Math.Max(curRange.Item1 + curRange.Item2, prevRange.Item1 + prevRange.Item2) - prevRange.Item1));
            }

            cByteRanges.Add(prevRange);

            return cByteRanges;
        }
        public static string BytesToReadableString(long bytes)
        {
            string bd = "";

            if (        bytes < 1024)
                bd =    bytes + " B";
            else if (   bytes > 1024    && bytes < 1024 * 1024)
                bd =    String.Format("{0:n1}",bytes / 1024.0)                  + " KB";
            else if (   bytes > 1024 * 1024 && bytes < 1024 * 1024 * 1024)
                bd =    String.Format("{0:n1}",bytes / (1024 * 1024.0))         + " MB";
            else if (   bytes > 1024 * 1024 * 1024 )
                bd =    String.Format("{0:n1}",bytes / (1024 * 1024 * 1024.0))  + " GB";

            return bd;
        }
        public static byte[] FromBase32String(string encoded) { // https://gist.github.com/BravoTango86
            Dictionary<char, int> CHAR_MAP = new Dictionary<char, int>();
            char[] DIGITS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
            int MASK = DIGITS.Length - 1;
            int SHIFT = numberOfTrailingZeros(DIGITS.Length);

            for (int i = 0; i < DIGITS.Length; i++) CHAR_MAP[DIGITS[i]] = i;
            
            encoded = encoded.Trim().Replace("-", "");
            encoded = Regex.Replace(encoded, "[=]*$", "");
            encoded = encoded.ToUpper();
            if (encoded.Length == 0) return new byte[0];

            int encodedLength = encoded.Length;
            int outLength = encodedLength * SHIFT / 8;
            byte[] result = new byte[outLength];
            int buffer = 0;
            int next = 0;
            int bitsLeft = 0;
            foreach (char c in encoded.ToCharArray()) {
                if (!CHAR_MAP.ContainsKey(c)) {
                    throw new Exception("Illegal character: " + c);
                }
                buffer <<= SHIFT;
                buffer |= CHAR_MAP[c] & MASK;
                bitsLeft += SHIFT;
                if (bitsLeft >= 8) {
                    result[next++] = (byte)(buffer >> (bitsLeft - 8));
                    bitsLeft -= 8;
                }
            }
        
            return result;
        }
        private static int numberOfTrailingZeros(int i) {
            // HD, Figure 5-14
            int y;
            if (i == 0) return 32;
            int n = 31;
            y = i << 16; if (y != 0) { n = n - 16; i = y; }
            y = i << 8; if (y != 0) { n = n - 8; i = y; }
            y = i << 4; if (y != 0) { n = n - 4; i = y; }
            y = i << 2; if (y != 0) { n = n - 2; i = y; }
            return n - (int)((uint)(i << 1) >> 31);
        }

        public static void EnsureThreadDoneNoAbort(Thread t, long maxMS = 500, int minMS = 10)
        {
            if (t != null && !t.IsAlive) return;

            long escapeInfinity = maxMS / minMS;

            while (t != null && t.IsAlive && escapeInfinity > 0)
            {
                Thread.Sleep(minMS);
                escapeInfinity--;
            }

            if (t != null && t.IsAlive)
                Debug.WriteLine("Thread X didn't finish properly!");
        }

        public static List<string> MovieExts = new List<string>() { "mp4", "m4v", "m4e", "mkv", "mpg", "mpeg" , "mpv", "mp4p", "mpe" , "m1v", "m2ts", "m2p", "m2v", "movhd", "moov", "movie", "movx", "mjp", "mjpeg", "mjpg", "amv" , "asf", "m4v", "3gp", "ogm", "ogg", "vob", "ts", "rm", "3gp", "3gp2", "3gpp", "3g2", "f4v", "f4a", "f4p", "f4b", "mts", "m2ts", "gifv", "avi", "mov", "flv", "wmv", "qt", "avchd", "swf", "cam", "nsv", "ram", "rm", "x264", "xvid", "wmx", "wvx", "wx", "video", "viv", "vivo", "vid", "dat", "bik", "bix", "dmf", "divx" };
        public static List<string> GetMoviesSorted(List<string> movies)
        {
            List<string> moviesSorted = new List<string>();

            for (int i=0; i<movies.Count; i++)
            {
                string ext = Path.GetExtension(movies[i]);
                if (ext == null || ext.Trim() == "") continue;

                if (MovieExts.Contains(ext.Substring(1,ext.Length-1))) moviesSorted.Add(movies[i]);
            }

            moviesSorted.Sort(CompareStrings);

            return moviesSorted;
        }
        public static int CompareStrings(string aStr, string bStr)
        {
            char[] a = aStr.ToLower().ToCharArray();
            char[] b = bStr.ToLower().ToCharArray();

            for (int i=0; i<Math.Min(a.Length, b.Length); i++)
            {
                if (a[i] > b[i]) 
                    return 1;
                else if (a[i] < b[i]) 
                    return -1;
            }

            return 0;
        }
        
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        public static extern uint TimeEndPeriod(uint uMilliseconds);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);
    }
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}