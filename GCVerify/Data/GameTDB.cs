﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace GCVerify.Data
{
    static class GameTDB
    {
        static XDocument doc;
        static Dictionary<string, string> titles;
        static string _dataPath;

        static GameTDB()
        {
            StringReader r = new StringReader(Properties.Resources.titles);
            string line;
            titles = new Dictionary<string, string>();
            string[] spl;
            while ((line = r.ReadLine()) != null)
            {
                spl = line.Split('=');
                titles.Add(spl[0].Trim(), spl[1].Trim());
            }
        }

        public static void Update(string dataPath)
        {
            //var filePath = Path.Combine(dataPath, "wiitdb.zip");
            //var req = HttpWebRequest.Create(string.Format("http://www.gametdb.com/wiitdb.zip?LANG={0}&FALLBACK=true&GAMECUBE=true&WIIWARE=true", "EN"));
            //var resp = req.GetResponse();
            Stream strm = null;
            //if (resp != null)
            //{
            //    strm = resp.GetResponseStream();
            //    if (strm != null)
            //    {
            //        var f = File.Create(filePath);
            //        strm.CopyTo(f);
            //        f.Close();
            //        strm.Close();
            //    }
            //}

            var filePath = Path.Combine(dataPath, "titles.txt");
            var req = HttpWebRequest.Create(string.Format("http://www.gametdb.com/titles.txt?LANG={0}", "EN"));
            var resp = req.GetResponse();
            if (resp != null)
            {
                strm = resp.GetResponseStream();
                if (strm != null)
                {
                    var f = File.Create(filePath);
                    strm.CopyTo(f);
                    f.Close();
                    strm.Close();
                }
            }
        }

        public static string GetDisplayName(string id)
        {
            if (titles.ContainsKey(id))
                return titles[id];

            return null;
        }

        public static ImageSource GetCover(string titleId)
        {
            if (!Directory.Exists("covers"))
                Directory.CreateDirectory("covers");

            var filePath = Path.Combine(_dataPath, string.Format(@"covers\{0}.png", titleId));
            if (!File.Exists(filePath))
            {
                var uri = string.Format("http://art.gametdb.com/wii/cover/{0}/{1}.png", "US", titleId);
                var req = HttpWebRequest.Create(uri);
                var resp = req.GetResponse();
                var strm = resp.GetResponseStream();

                if (strm != null)
                {
                    var f = File.Create(filePath);
                    strm.CopyTo(f);
                    f.Close();
                    strm.Close();
                }
            }

            return new BitmapImage(new Uri(filePath));
        }
    }
}
