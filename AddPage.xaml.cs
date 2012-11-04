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
using TransferManager;

namespace DownloadManager
{
    public partial class AddPage : PhoneApplicationPage
    {
        public AddPage()
        {
            InitializeComponent();
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            try
            {
                AbstractTransfer Transfer = new DownloadTransfer { TransferUrl = Source.Text, Path = Path.Text, Filename = Filename.Text };

                App.TransferViewModel.Add(Transfer);

                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
            catch (ArgumentException exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK);
            }            
        }

        private void Source_TextChanged(object sender, TextChangedEventArgs e)
        {
            int pos = Source.Text.LastIndexOf('/'); //Position of the last slash
            if (pos != Source.Text.Length && Source.Text[pos - 1] != '/' && Source.Text.LastIndexOf('.') > pos)
            { //If a slash was found
              //and it was not preceded by another slash (as in "http://")
              //and a dot was found after it (as in "1.jpg")
                Filename.Text = Source.Text.Substring(pos + 1);
            }
            else
            { //No valid filename was found, set the filename to empty
                Filename.Text = string.Empty;
            }
        }
    }
}