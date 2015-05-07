using GCVerify.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GCVerify");

            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            SetStatus("Getting Redump data...");
            try
            {
                await Redump.Open(dataPath);
            }
            catch (WebException)
            {
                SetStatus("Could not retrieve Redump data.");
            }

            SetStatus("Getting GameTDB data...");
            try
            {
                await GameTDB.Open(dataPath);
            }
            catch (WebException)
            {
                SetStatus("Could not retrieve GameTDB data.");
            }

            SetStatus("Drop Gamecube game images or folders here to start");
            dropTarget.AllowDrop = true;
        }

        private void SetStatus(string text)
        {
            displayText.Text = text;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
