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

namespace GCVerify.Data
{
    static class Redump 
    {
        const string redumpUri = "http://redump.org/datfile/gc/";
        static XDocument doc;

        static Redump()
        {
            using (var arc = new ZipArchive(new MemoryStream(Properties.Resources.redump), ZipArchiveMode.Read))
            {
                var entries = arc.Entries;
                if (entries.Count > 0)
                {
                    foreach (var entry in entries)
                    {
                        if (entry.Name.Contains(".dat"))
                        {
                            doc = XDocument.Load(entry.Open());
                            break;
                        }
                    }
                }
            }
        }

        public static void Update(string dataPath)
        {
            var req = HttpWebRequest.Create(redumpUri);
            var resp = req.GetResponse();
            var strm = resp.GetResponseStream();
            var path = Path.Combine(dataPath, "redump.zip");

            if (strm != null)
            {
                var f = File.Create(path);
                strm.CopyTo(f);
                f.Close();
            }
        }

        public static bool IsMD5Valid(string hash)
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
