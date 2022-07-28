using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FTPDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(720, 240));
            Instance = this;
        }
        public static MainPage Instance;

        public void SetDownloadInfo(string downloadURI, string downloadPath)
        {
            this.downloadURI.Text = downloadURI;
            this.downloadPath.Text = downloadPath;
        }

        public void SetDownloadStatus(string status)
        {
            this.downloadStatus.Text = status;
        }

        public void UpdateProgress(double value)
        {
            this.downloadProgress.Value = value;
        }
    }
}
