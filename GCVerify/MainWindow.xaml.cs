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
    public partial class MainWindow : Window
    {
        private StartPage startPage;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new GameListView();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            startPage = new StartPage();
            startPage.dropTarget.Drop += dropTarget_Drop;
            startPage.displayText.Drop += dropTarget_Drop;
            navigationFrame.Content = startPage;
        }

        async void dropTarget_Drop(object sender, DragEventArgs e)
        {
            var huh = e.Data.GetDataPresent(DataFormats.FileDrop);
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var page = new ScanPage(files);
            navigationFrame.Content = page;
            await page.CalculateHashesAsync();
        }
    }
}
