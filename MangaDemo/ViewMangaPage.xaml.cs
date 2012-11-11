using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using TransferManager;

namespace DownloadManager
{
    public partial class ViewMangaPage : PhoneApplicationPage
    {
        public ViewMangaPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            IDictionary<string, string> queryString = this.NavigationContext.QueryString;
            if (queryString.ContainsKey("pid"))
            {
                Library.Page Page = (Library.Page)App.LibraryViewModel.FindByUID(queryString["pid"]);
                BitmapImage bi = new BitmapImage();

                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (myIsolatedStorage.FileExists(Page.Transfer.FilenameWithPath))
                    {
                        using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(Page.Transfer.FilenameWithPath, FileMode.Open, FileAccess.Read))
                        {
                            bi.SetSource(fileStream);
                            fileStream.Close();
                        }
                        ImageContainer.Source = bi;
                    }
                }
            }

            base.OnNavigatedTo(e);
        }
    }
}