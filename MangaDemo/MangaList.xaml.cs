using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using TransferManager;

namespace DownloadManager.MangaDemo
{
    public class Chapter
    {
        public string ID { get; set; }
        public string SeriesID { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
    }

    public class Series
    {
        public string ID { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
    }

    public partial class MangaList : PhoneApplicationPage
    {
        private ObservableCollection<Series> _Series;
        private ObservableCollection<Chapter> _Chapters;

        public MangaList()
        {
            InitializeComponent();
            _Series = new ObservableCollection<Series>();
            _Chapters = new ObservableCollection<Chapter>();
            SeriesList.ItemsSource = _Series;
            ListSeries();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (IsChapter)
            {
                IsChapter = false;
                SeriesList.ItemsSource = _Series;
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        private void ListSeries()
        {
            string url = @"http://ceoloide.com/manga/list.php";

            //Retrieve web page
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(SeriesListDownloaded);
            webClient.DownloadStringAsync(new Uri(url));
        }

        private void SeriesListDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {
            lock (this)
            {
                if (e.Error != null) { MessageBox.Show(e.Error.Message); return; }

                XDocument loadedData = XDocument.Parse(e.Result);
                var mangas = from series in loadedData.Descendants("series")
                             select series;

                foreach (XElement series in mangas.ToArray<XElement>())
                {
                    string ID = (string)series.Element("id");
                    string Title = (string)series.Element("title");
                    Series Item = new Series { ID = ID, Title = Title, URL = HttpUtility.UrlEncode(Title) };
                    _Series.Add(Item);
                }
            }
        }

        private void ListChapters(string Source)
        {
            string url = @"http://ceoloide.com/manga/series.php?id=" + Source;

            //Retrieve web page
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ChapterListDownloaded);
            webClient.DownloadStringAsync(new Uri(url));
        }

        private void ChapterListDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {
            lock (this)
            {
                if (e.Error != null) { MessageBox.Show(e.Error.Message); return; }

                TextReader textreader = new StringReader(e.Result);
                XmlReader reader = XmlReader.Create(textreader, new XmlReaderSettings { CheckCharacters = false });

                XDocument loadedData = XDocument.Load(reader);
                var mangas = from series in loadedData.Descendants("chapter")
                             select series;

                foreach (XElement series in mangas.ToArray<XElement>())
                {
                    string ID = (string)series.Element("chapter_id");
                    string SeriesID = (string)series.Element("series_id");
                    string Title = (string)series.Element("title");
                    string URL = (string)series.Element("source_path");
                    Chapter Item = new Chapter { ID = ID, SeriesID = SeriesID, Title = Title, URL = URL };
                    _Chapters.Add(Item);
                }
                IsChapter = true;
                _Chapters.Reverse<Chapter>();

                SeriesList.ItemsSource = _Chapters.Reverse<Chapter>();
            }
        }

        private void ListPages(string SeriesID, string ChapterID)
        {
            string url = @"http://ceoloide.com/manga/pages.php?cid=" + ChapterID + @"&sid=" + SeriesID;

            //Retrieve web page
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(PageListDownloaded);
            webClient.DownloadStringAsync(new Uri(url));
        }

        private void PageListDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {
            lock (this)
            {
                if (e.Error != null) { MessageBox.Show(e.Error.Message); return; }

                TextReader textreader = new StringReader(e.Result);
                XmlReader reader = XmlReader.Create(textreader, new XmlReaderSettings { CheckCharacters = false });

                XDocument loadedData = XDocument.Load(reader);
                var mangas = from series in loadedData.Descendants("page")
                             select series;

                foreach (XElement series in mangas.ToArray<XElement>())
                {
                    string ChapterID = (string)series.Element("chapter_id");
                    string SeriesID = (string)series.Element("series_id");
                    string PageNum = (string)series.Element("page_number");
                    string SeriesTitle = _Series[int.Parse(SeriesID) - 1].Title;
                    string Title = SeriesTitle + " - Page " + PageNum;
                    string URL = (string)series.Element("image_source");

                    // Add pages
                    Library.Page NewPage = new Library.Page{ Name = Title };

                    DownloadTransfer Transfer = new DownloadTransfer { TransferUrl = URL, Path = "/" + HttpUtility.UrlEncode(SeriesTitle)+"/"+ChapterID, Filename = PageNum + ".jpg" };
                    App.TransferViewModel.Add(Transfer);

                    NewPage.Transfer = Transfer;
                    App.LibraryViewModel.Add(NewPage);

                    Transfer.ExternalReference = NewPage.UID.ToString();
                }

                NavigationService.Navigate(new Uri("/MangaDemo/MangaDownloadList.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private bool IsChapter = false;
        private void SeriesList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (SeriesList.SelectedItem != null)
            {
                if (IsChapter == false)
                {
                    Series Selection = (Series)SeriesList.SelectedItem;
                    ListChapters(Selection.ID);
                }
                else
                {
                    Chapter Selection = (Chapter)SeriesList.SelectedItem;
                    ListPages(Selection.SeriesID, Selection.ID);
                }
            }
        }
    }
}