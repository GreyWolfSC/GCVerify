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

namespace GCVerify
{
    /// <summary>
    /// Interaction logic for ScanPage.xaml
    /// </summary>
    public partial class ScanPage : UserControl
    {
        MD5 md5 = MD5.Create();
        GameListView view = new GameListView();

        public ScanPage()
        {
            InitializeComponent();
            this.gameList.DataContext = view;
        }

        public ScanPage(string[] files)
            : this()
        {
            foreach (var f in files)
            {
                if (System.IO.File.Exists(f))
                    view.AddTitle(f);
                else if (System.IO.Directory.Exists(f))
                    view.AddTitles(f);
            }
        }

        public async Task CalculateHashesAsync()
        {
            foreach (var game in view.Items)
            {
                SetStatus(string.Format("Calculating hash for {0} [{1}]", game.DisplayName, game.DiscNumber));

                await Task.Run(() =>
                {
                    FileStream fl = null;
                    Dispatcher.Invoke(() => fl = File.OpenRead(game.Path));
                    using (var strm = new ProgressFileStream(fl, HashCalcProgressChanged))
                    {
                        var hash = md5.ComputeHash(strm);
                        var hex = BitConverter.ToString(hash);
                        hex = Regex.Replace(hex, "-", "", RegexOptions.Compiled).ToLower();
                        Dispatcher.Invoke(() => game.MD5Hash = hex);
                        Dispatcher.Invoke(() => game.Redump = GCVerify.Data.Redump.IsMD5Valid(hex) ? "Found" : "Not found");
                    }
                });

                progressBar.Value = 0;
            }

            SetStatus("Done");
        }

        private void SetStatus(string text)
        {
            Dispatcher.Invoke(() => { statusText.Text = text; });
        }

        private void HashCalcProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage % 5 == 0)
                Dispatcher.Invoke(() => { progressBar.Value = e.ProgressPercentage; });
        }

        #region Custom FileStream

        class ProgressFileStream : FileStream
        {
            private ProgressChangedEventHandler _handler;

            public ProgressFileStream(FileStream b, ProgressChangedEventHandler handler = null)
                : base(b.SafeFileHandle, FileAccess.Read)
            {
                _handler = handler;
            }

            public override int Read(byte[] array, int offset, int count)
            {
                int ret = base.Read(array, offset, count);
                int pct = (int)(((double)this.Position / (double)this.Length) * 100);

                if (_handler != null)
                    _handler(this, new ProgressChangedEventArgs(pct, this));

                return ret;
            }
        }

        #endregion
    }

    class GameListView
    {
        private ObservableCollection<GameInfo> _items;

        public ObservableCollection<GameInfo> Items { get { return _items; } }

        public GameListView()
        {
            _items = new ObservableCollection<GameInfo>();
        }

        public void AddTitle(string path)
        {
            if (System.IO.Path.GetExtension(path) != ".iso" && System.IO.Path.GetExtension(path) != ".gcm")
                return;

            var info = GameInfo.Open(path);

            if (info.IsGCGame)
                this.Items.Add(info);
        }

        public void AddTitles(string path)
        {
            var gameDirs = Directory.GetDirectories(path);
            var discs = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(ff => (System.IO.Path.GetExtension(ff).ToLower() == ".iso" || System.IO.Path.GetExtension(ff).ToLower() == ".gcm"));
            GameInfo info;

            foreach (var game in discs)
            {
                info = GameInfo.Open(game);

                if (info.IsGCGame)
                    this.Items.Add(info);
            }
        }
    }
}
