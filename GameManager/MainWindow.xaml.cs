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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace GCVerify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RedumpDb redump;
        static List<GameInfo> games;
        BackgroundWorker worker;
        IEnumerator<GameInfo> iter;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            redump = new RedumpDb();
            games = new List<GameInfo>();
            gameList.ItemsSource = games;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            gameList.Items.Refresh();

            if (iter.MoveNext())
            {
                worker.RunWorkerAsync(iter.Current);
                return;
            }

            if (games.Count > 1)
            {
                var ser = new XmlSerializer(typeof(List<GameInfo>));
                var path = new System.IO.DirectoryInfo(games[0].Path).Parent.Parent.FullName;
                var file = File.CreateText(System.IO.Path.Combine(path, "gcvcache.xml"));
                ser.Serialize(file, games);
                file.Close();
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var game = e.Argument as GameInfo;
            GenerateHash(game);
        }

        private void GetTitle(string path)
        {
            if (System.IO.Path.GetExtension(path) != ".iso")
                return;

            var info = new GameInfo(path);

            if (info.IsGCGame)
                games.Add(info);
        }

        private void GetTitles(string path)
        {
            var gameDirs = Directory.GetDirectories(path);
            var discs = Directory.GetFiles(path, "*.iso", SearchOption.AllDirectories);
            GameInfo info;

            foreach (var game in discs)
            {
                info = new GameInfo(game);

                if (info.IsGCGame)
                    games.Add(info);
            }
        }

        private void gameList_Drop(object sender, DragEventArgs e)
        {
            if ((worker != null) && worker.IsBusy)
                return;

            games.Clear();
            gameList.Items.Refresh();
            var huh = e.Data.GetDataPresent(DataFormats.FileDrop);
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var f in files)
            {
                if (System.IO.File.Exists(f))
                {
                    GetTitle(f);
                }
                else if (System.IO.Directory.Exists(f))
                {
                    GetTitles(f);
                }
            }

            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            iter = games.GetEnumerator();
            if (iter.MoveNext())
                worker.RunWorkerAsync(iter.Current);
        }

        public void GenerateHash(GameInfo info)
        {
            info.MD5Hash = "calculating";
            var hash = CalcHash(info);
            info.MD5Hash = hash;
            info.IsValid = RedumpDb.IsValid(hash);
        }

        MD5 md5 = MD5.Create();

        private string CalcHash(GameInfo info)
        {
            var b = new FooStream(File.OpenRead(info.Path));
            b.ProgressChanged += b_ProgressChanged;
            var hash = md5.ComputeHash(b);
            var hex = BitConverter.ToString(hash);
            return Regex.Replace(hex, "-", "", RegexOptions.Compiled).ToLower();
        }

        void b_ProgressChanged(object sender, ProgressEventArgs e)
        {
            if (e.Progress % 5 != 0)
                return;

            iter.Current.MD5Hash = string.Format("calculating {0}%", (int)e.Progress);
            Dispatcher.Invoke(() =>
            {
                gameList.Items.Refresh();
            });
        }

        class ProgressEventArgs : EventArgs
        {
            public int Progress { get; set; }
        }

        class FooStream : FileStream
        {
            public event EventHandler<ProgressEventArgs> ProgressChanged;

            public FooStream(FileStream b) : base(b.SafeFileHandle, FileAccess.Read) { }

            public override int Read(byte[] array, int offset, int count)
            {
                int opct = (int)(((double)this.Position / (double)this.Length) * 100);
                int ret = base.Read(array, offset, count);

                if (ProgressChanged != null)
                {
                    int pct = (int)(((double)this.Position / (double)this.Length) * 100);
                    if (pct != opct)
                        this.ProgressChanged(this, new ProgressEventArgs() { Progress = pct });
                }

                return ret;
            }
        }
    }
}
