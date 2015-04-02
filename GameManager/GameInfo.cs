using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GCVerify
{
    public class GameInfo 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string MD5Hash { get; set; }
        public bool IsGCGame { get; private set; }
        public bool RedumpValid { get; set; }

        internal DateTime Modified { get; set; }

        public GameInfo() { }

        public GameInfo(string path)
        {
            this.Path = path;
            byte[] buf = new byte[1024];
            var f = System.IO.File.OpenRead(path);
            f.Read(buf, 0, 1024);
            this.Id = Encoding.ASCII.GetString(buf, 0, 6);
            this.Name = Encoding.ASCII.GetString(buf, 32, 256).Trim('\0');
            this.IsGCGame = BitConverter.ToUInt32(buf, 0x1C) == 0x3d9f33c2;
            f.Close();
        }
    }
}
