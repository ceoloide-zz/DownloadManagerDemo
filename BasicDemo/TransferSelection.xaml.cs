using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DownloadManager.BasicDemo
{
    public partial class TransferSelection : PhoneApplicationPage
    {
        public TransferSelection()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BasicDemo/AddDownload.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}