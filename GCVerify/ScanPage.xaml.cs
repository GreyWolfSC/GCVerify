using System;
using System.Collections.Generic;
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
        List<GameInfo> games = new List<GameInfo>();
        MD5 md5 = MD5.Create();

        public ScanPage()
        {
            InitializeComponent();
        }

        public ScanPage(string[] files)
            : this()
        {
            foreach (var f in files)
            {
                if (System.IO.File.Exists(f))
                    GetTitle(f);
                else if (System.IO.Directory.Exists(f))
                    GetTitles(f);
            }

            games.Sort(new GameInfoComparer());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            progressBar.Value = 0;
            gameList.ItemsSource = games;
        }

        private void GetTitle(string path)
        {
            if (System.IO.Path.GetExtension(path) != ".iso" && System.IO.Path.GetExtension(path) != ".gcm")
                return;

            var info = GameInfo.Open(path);

            if (info.IsGCGame)
                games.Add(info);
        }

        private void GetTitles(string path)
        {
            var gameDirs = Directory.GetDirectories(path);
            var discs = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(ff => (System.IO.Path.GetExtension(ff).ToLower() == ".iso" || System.IO.Path.GetExtension(ff).ToLower() == ".gcm"));
            GameInfo info;

            foreach (var game in discs)
            {
                info = GameInfo.Open(game);

                if (info.IsGCGame)
                    games.Add(info);
            }
        }

        public async Task CalculateHashesAsync()
        {
            foreach (var game in games)
            {
                SetStatus(string.Format("Calculating hash for {0} [{1}]", game.DisplayName, game.DiscNumber));
                gameList.Items.Refresh();

                await Task.Run(() =>
                {
                    using (var fl = File.OpenRead(game.Path))
                    {
                        using (var strm = new ProgressFileStream(fl, HashCalcProgressChanged))
                        {
                            var hash = md5.ComputeHash(strm);
                            var hex = BitConverter.ToString(hash);
                            hex = Regex.Replace(hex, "-", "", RegexOptions.Compiled).ToLower();
                            game.MD5Hash = hex;
                            game.Redump = GCVerify.Data.Redump.IsMD5Valid(hex);
                            game.GameTDB = GCVerify.Data.GameTDB.IsMD5Valid(hex);
                        }
                    }
                });

                gameList.Items.Refresh();
                progressBar.Value = 0;
                SetStatus("Done");
            }
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
}
