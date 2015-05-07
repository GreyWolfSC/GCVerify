using GCVerify.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GCVerify
{
    public class GameInfo 
    {
        public string GameCode { get; private set; }
        public string CompanyCode { get; private set; }
        public int DiscNumber { get; private set; }
        public int DiscVersion { get; private set; }
        public bool AudioStreaming { get; private set; }
        public uint DvdMagic { get; private set; }
        public string InternalName { get; private set; }

        public bool IsGCGame { get; set; }
        public Nullable<bool> Redump { get; set; }
        public Nullable<bool> GameTDB { get; set; }
        public string TitleId { get; set; }
        public string DisplayName { get; set; }
        public string Path { get; set; }
        public string MD5Hash { get; set; }

        public GameInfo() { }

        public static GameInfo Open(string path)
        {
            var ret = new GameInfo();
            var discStream = File.OpenRead(path);
            var buf = new byte[0x440];
            discStream.Read(buf, 0, 0x440);
            discStream.Close();
            ret.GameCode = Encoding.ASCII.GetString(buf, 0, 4);
            ret.CompanyCode = Encoding.ASCII.GetString(buf, 4, 2);
            ret.DiscNumber = buf[6];
            ret.DiscVersion = buf[7];
            ret.AudioStreaming = (buf[8] != 0);
            ret.IsGCGame = (BitConverter.ToUInt32(buf, 0x001c) == 0x3d9f33c2);
            var name = Encoding.ASCII.GetString(buf, 0x0020, 0x03ff);
            ret.InternalName = name.Substring(0, name.IndexOf('\0'));

            ret.Path = path;
            ret.TitleId = ret.GameCode + ret.CompanyCode;
            
            ret.DisplayName = ret.InternalName;
            var tname = Data.GameTDB.GetDisplayName(ret.TitleId);
            if (tname != null)
                ret.DisplayName = tname;

            return ret;
        }
    }

    class GameInfoComparer : IComparer<GameInfo>
    {
        static readonly string[] Articles = { "a ", "an ", "the " };

        public int Compare(GameInfo x, GameInfo y)
        {
            int xi = 0, yi = 0;
            foreach (var a in Articles)
            {
                if (x.DisplayName.ToLower().StartsWith(a))
                    xi = a.Length;
                if (y.DisplayName.ToLower().StartsWith(a))
                    yi = a.Length;
            }

            var cmp = x.DisplayName.Substring(xi).CompareTo(y.DisplayName.Substring(yi));

            return (cmp == 0) ? x.DiscNumber.CompareTo(y.DiscNumber) : cmp;
        }
    }

}
