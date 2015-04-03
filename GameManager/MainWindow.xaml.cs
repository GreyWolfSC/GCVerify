﻿using System;
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
                worker.RunWorkerAsync(iter.Current);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var game = e.Argument as GameInfo;
            game.GenerateHash();
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
    }
}
