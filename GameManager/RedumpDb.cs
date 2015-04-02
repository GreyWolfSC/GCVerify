using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Net;

namespace GCVerify
{
    class RedumpDb
    {
        class Header
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime Version { get; set; }
            public DateTime Date { get; set; }
            public string Author { get; set; }
            public string Homepage { get; set; }
            public string Url { get; set; }
        }

        XDocument doc;

        public RedumpDb()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "GCManager");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "redump.xml");
            if (!File.Exists(path))
                GetDatabase(path);

            if (File.Exists(path))
                doc = XDocument.Load(path);
        }

        private void GetDatabase(string path)
        {
            var req = HttpWebRequest.Create("http://redump.org/datfile/gc/");
            var resp = req.GetResponse();
            var strm = resp.GetResponseStream();
            if (strm != null)
            {
                var arc = new ZipArchive(strm, ZipArchiveMode.Read);
                var entries = arc.Entries;
                if (entries.Count > 0)
                {
                    foreach (var entry in entries)
                    {
                        if (entry.Name.Contains(".dat"))
                        {
                            var estrm = entry.Open();
                            var f = File.Create(path);
                            estrm.CopyTo(f);
                            f.Close();
                            break;
                        }
                    }
                }
            }
        }

        public bool IsValid(string hash)
        {
            var games =
                from g in doc.Root.Elements("game")
                select g;

            var rom =
                from r in games.Elements("rom")
                where (string)r.Attribute("md5") == hash
                select r;

            return (rom.Count() == 1);
        }
    }
}
