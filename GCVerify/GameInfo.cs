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
    public class GameInfo : DependencyObject
    {
        public string GameCode { get; private set; }
        public string CompanyCode { get; private set; }
        public int DiscNumber { get; private set; }
        public int DiscVersion { get; private set; }
        public bool AudioStreaming { get; private set; }
        public uint DvdMagic { get; private set; }
        public string InternalName { get; private set; }

        public bool IsGCGame { get; private set; }

        public string TitleId
        {
            get { return (string)GetValue(TitleIdProperty); }
            set { SetValue(TitleIdProperty, value); }
        }

        public static readonly DependencyProperty TitleIdProperty =
            DependencyProperty.Register("TitleId", typeof(string), typeof(GameInfo), new PropertyMetadata(""));
        
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(GameInfo), new PropertyMetadata(""));

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(GameInfo), new PropertyMetadata(""));
         
        public string MD5Hash
        {
            get { return (string)GetValue(MD5HashProperty); }
            set { SetValue(MD5HashProperty, value); }
        }

        public static readonly DependencyProperty MD5HashProperty =
            DependencyProperty.Register("MD5Hash", typeof(string), typeof(GameInfo), new PropertyMetadata("Unknown"));

        public string Redump
        {
            get { return (string)GetValue(RedumpProperty); }
            set { SetValue(RedumpProperty, value); }
        }

        public static readonly DependencyProperty RedumpProperty =
            DependencyProperty.Register("Redump", typeof(string), typeof(GameInfo), new PropertyMetadata("Unknown"));
        
        public GameInfo() { }

        public static GameInfo Open(string path)
        {
            var ret = new GameInfo();
            ret.Path = path;

            var discStream = File.OpenRead(path);
            var buf = new byte[0x440];
            discStream.Read(buf, 0, 0x440);
            discStream.Close();
            ret.GameCode = Encoding.ASCII.GetString(buf, 0, 4);
            ret.CompanyCode = Encoding.ASCII.GetString(buf, 4, 2);
            ret.DiscNumber = buf[6] + 1;
            ret.DiscVersion = buf[7];
            ret.AudioStreaming = (buf[8] != 0);
            ret.IsGCGame = (BitConverter.ToUInt32(buf, 0x001c) == 0x3d9f33c2);
            var name = Encoding.ASCII.GetString(buf, 0x0020, 0x03ff);
            ret.InternalName = name.Substring(0, name.IndexOf('\0'));

            ret.TitleId = ret.GameCode + ret.CompanyCode;
            ret.DisplayName = ret.InternalName;
            var tdbName = Data.GameTDB.GetDisplayName(ret.TitleId);
            if (tdbName != null)
                ret.DisplayName = tdbName;

            return ret;
        }
    }
}
