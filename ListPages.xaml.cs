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
using DownloadManager.Library;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using TransferManager;
using System.ComponentModel;
using Microsoft.Phone.Shell;
using DownloadManager.Resources;

namespace DownloadManager
{
    public partial class ListPages : PhoneApplicationPage
    {
        
        ApplicationBarIconButton AppBarButton_Add;
        ApplicationBarIconButton AppBarButton_Select;
        ApplicationBarIconButton AppBarButton_StopAll;

        ApplicationBarIconButton AppBarButton_StartSelected;
        ApplicationBarIconButton AppBarButton_StopSelected;
        ApplicationBarIconButton AppBarButton_DeleteSelected;

        ApplicationBarMenuItem AppBarMenuItem_SelectAll;
        ApplicationBarMenuItem AppBarMenuItem_StartAll;
        ApplicationBarMenuItem AppBarMenuItem_DeleteAll;

        MultiselectList CurrentMultiselectList;

        public ListPages()
        {
            InitializeComponent();

            // Add tilt effect to elements of the MultiSelect list
            TiltEffect.TiltableItems.Add(typeof(MultiselectItem));

            #region Prepare app bar buttons

            AppBarButton_Add = new ApplicationBarIconButton();
            AppBarButton_Add.IconUri = new Uri("/Icons/appbar.add.rest.png", UriKind.RelativeOrAbsolute);
            AppBarButton_Add.Text = AppResources.Common_add;
            AppBarButton_Add.Click += AppBarButton_Add_Handler;

            AppBarButton_Select = new ApplicationBarIconButton();
            AppBarButton_Select.IconUri = new Uri("/Icons/appbar.manage.rest.png", UriKind.RelativeOrAbsolute);
            AppBarButton_Select.Text = AppResources.Common_select;
            AppBarButton_Select.Click += AppBarButton_Select_Handler;

            AppBarButton_StopAll = new ApplicationBarIconButton();
            AppBarButton_StopAll.IconUri = new Uri("/Icons/appbar.stop.rest.png", UriKind.RelativeOrAbsolute);
            AppBarButton_StopAll.Text = AppResources.Common_stop_all;
            AppBarButton_StopAll.Click += Menu_StopAll_Handler;

            AppBarButton_StartSelected = new ApplicationBarIconButton();
            AppBarButton_StartSelected.IconUri = new Uri("/Icons/appbar.download.rest.png", UriKind.RelativeOrAbsolute);
            AppBarButton_StartSelected.Text = AppResources.Common_start;
            AppBarButton_StartSelected.Click += AppBarButton_StartSelected_Handler;

            AppBarButton_StopSelected = new ApplicationBarIconButton();
            AppBarButton_StopSelected.IconUri = new Uri("/Icons/appbar.stop.rest.png", UriKind.RelativeOrAbsolute);
            AppBarButton_StopSelected.Text = AppResources.Common_stop;
            AppBarButton_StopSelected.Click += AppBarButton_StopSelected_Handler;

            AppBarButton_DeleteSelected = new ApplicationBarIconButton();
            AppBarButton_DeleteSelected.IconUri = new Uri("/Icons/appbar.delete.rest.png", UriKind.RelativeOrAbsolute);
            AppBarButton_DeleteSelected.Text = AppResources.Common_remove;
            AppBarButton_DeleteSelected.Click += AppBarButton_DeleteSelected_Handler;

            #endregion

            #region Prepare app bar menu items

            AppBarMenuItem_StartAll = new ApplicationBarMenuItem();
            AppBarMenuItem_StartAll.Text = AppResources.Common_start_all;
            AppBarMenuItem_StartAll.Click += new EventHandler(Menu_StartAll_Handler);

            AppBarMenuItem_DeleteAll = new ApplicationBarMenuItem();
            AppBarMenuItem_DeleteAll.Text = AppResources.Common_remove_all;
            AppBarMenuItem_DeleteAll.Click += new EventHandler(Menu_RemoveAll_Handler);

            AppBarMenuItem_SelectAll = new ApplicationBarMenuItem();
            AppBarMenuItem_SelectAll.Text = AppResources.Common_select_all;
            AppBarMenuItem_SelectAll.Click += new EventHandler(Menu_SelectAll_Handler);

            #endregion

            #region Prepare app bar

            ApplicationBar.Buttons.Add(AppBarButton_Select);
            ApplicationBar.Buttons.Add(AppBarButton_Add);
            ApplicationBar.Buttons.Add(AppBarButton_StopAll);

            ApplicationBar.MenuItems.Add(AppBarMenuItem_StartAll);
            ApplicationBar.MenuItems.Add(AppBarMenuItem_DeleteAll);

            #endregion

            #region Initialize multiselect lists
            DownloadsMultiselectList.ItemsSource = App.TransferViewModel.PendingTransfers;
            ArchiveMultiselectList.ItemsSource = App.TransferViewModel.CompletedTransfers;
            #endregion
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (CurrentMultiselectList.IsSelectionEnabled)
            {   // If the MultiSelect list is open, we close it on back
                CurrentMultiselectList.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }
        
        private void TransferTap_Handler(object sender, RoutedEventArgs e)
        {
            var Tag = ((FrameworkElement)sender).Tag;
            AbstractTransfer Page = (AbstractTransfer)App.TransferViewModel.FindByTag(Tag.ToString());

            object item = ((FrameworkElement)sender).DataContext;

            if (CurrentMultiselectList.IsSelectionEnabled)
            {
                MultiselectItem container = CurrentMultiselectList.ItemContainerGenerator.ContainerFromItem(item) as MultiselectItem;
                if (container != null)
                {
                    container.IsSelected = !container.IsSelected;
                }
            }
            else
            {
                #region Display message based on trnasfer status or open preview page
                switch (Page.TransferStatus)
                {
                    case ExtendedTransferStatus.None:
                        MessageBox.Show(AppResources.TapMessage_None, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.Queued:
                        MessageBox.Show(AppResources.TapMessage_Queued, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.Transferring:
                        MessageBox.Show(AppResources.TapMessage_Transferring, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.Waiting:
                        MessageBox.Show(AppResources.TapMessage_Waiting, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.WaitingForRetry:
                        MessageBox.Show(AppResources.TapMessage_WaitingForRetry, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.WaitingForWiFi:
                        MessageBox.Show(AppResources.TapMessage_WaitingForWiFi, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.WaitingForExternalPower:
                        MessageBox.Show(AppResources.TapMessage_WaitingForExternalPower, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                        MessageBox.Show(AppResources.TapMessage_WaitingForExternalPowerDueToBatterySaverMode, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork:
                        MessageBox.Show(AppResources.TapMessage_WaitingForNonVoiceBlockingNetwork, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.Paused:
                        MessageBox.Show(AppResources.TapMessage_Paused, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.Completed:
                        NavigationService.Navigate(new Uri("/ViewPage.xaml?pid=" + Page.UID, UriKind.Relative));
                        break;
                    case ExtendedTransferStatus.Failed:
                        MessageBox.Show(AppResources.TapMessage_Failed, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.FailedServer:
                        MessageBox.Show(AppResources.TapMessage_FailedServer, Page.Filename, MessageBoxButton.OK);
                        break;
                    case ExtendedTransferStatus.Canceled:
                        MessageBox.Show(AppResources.TapMessage_Canceled, Page.Filename, MessageBoxButton.OK);
                        break;
                }
                #endregion
            }            
        }

        #region App bar button event handlers

        private void AppBarButton_Add_Handler(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void AppBarButton_Select_Handler(object sender, EventArgs e)
        {
            if (!CurrentMultiselectList.IsSelectionEnabled)
            {
                CurrentMultiselectList.IsSelectionEnabled = true;
            }
        }

        #endregion

        #region MultiselectList event handlers

        #region Base event handlers

        private void MultiselectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MultiselectList target = (MultiselectList)sender;

            if (target.IsSelectionEnabled)
            {
                ApplicationBarIconButton i = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                ApplicationBarIconButton j = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                ApplicationBarIconButton k = (ApplicationBarIconButton)ApplicationBar.Buttons[2];

                if (target.SelectedItems.Count > 0)
                {
                    i.IsEnabled = j.IsEnabled = k.IsEnabled = true;
                }
                else
                {
                    i.IsEnabled = j.IsEnabled = k.IsEnabled = false;
                }
            }
        }

        private void MultiselectListList_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Remove all the application bar buttons
            ApplicationBar.Buttons.Clear();

            // Remove al the application bar menu items
            ApplicationBar.MenuItems.Clear();

            // If the selection has been enabled
            if ((bool)e.NewValue)
            {
                ApplicationBar.Buttons.Add(AppBarButton_StartSelected);
                ApplicationBarIconButton i = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                i.IsEnabled = false;

                ApplicationBar.Buttons.Add(AppBarButton_StopSelected);
                ApplicationBarIconButton j = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                j.IsEnabled = false;

                ApplicationBar.Buttons.Add(AppBarButton_DeleteSelected);
                ApplicationBarIconButton k = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                k.IsEnabled = false;
            }
            else
            {
                ApplicationBar.Buttons.Add(AppBarButton_Select);
                ApplicationBar.Buttons.Add(AppBarButton_Add);
                ApplicationBar.Buttons.Add(AppBarButton_StopAll);

                ApplicationBar.MenuItems.Add(AppBarMenuItem_StartAll);
                ApplicationBar.MenuItems.Add(AppBarMenuItem_DeleteAll);
            }

            // Lock the pivot
            pivot.IsLocked = !pivot.IsLocked;
        }

        #endregion

        #region Specific app bar button event handlers

        private void AppBarButton_StartSelected_Handler(object sender, EventArgs e)
        {
            AbstractTransfer[] SelectedItems = new AbstractTransfer[CurrentMultiselectList.SelectedItems.Count];
            CurrentMultiselectList.SelectedItems.CopyTo(SelectedItems, 0);

            foreach (AbstractTransfer SelectedItem in SelectedItems)
            {
                StartTransfer(SelectedItem);
            }

            CurrentMultiselectList.IsSelectionEnabled = false;
        }

        private void AppBarButton_StopSelected_Handler(object sender, EventArgs e)
        {
            AbstractTransfer[] SelectedItems = new AbstractTransfer[CurrentMultiselectList.SelectedItems.Count];
            CurrentMultiselectList.SelectedItems.CopyTo(SelectedItems, 0);

            foreach (AbstractTransfer SelectedItem in SelectedItems)
            {
                StopTransfer(SelectedItem);
            }

            CurrentMultiselectList.IsSelectionEnabled = false;
        }

        private void AppBarButton_DeleteSelected_Handler(object sender, EventArgs e)
        {
            if (MessageBox.Show(AppResources.WarningMessage_remove_selected, AppResources.WarningTitle_remove_selected, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                AbstractTransfer[] SelectedItems = new AbstractTransfer[CurrentMultiselectList.SelectedItems.Count];
                CurrentMultiselectList.SelectedItems.CopyTo(SelectedItems, 0);

                // User confirmed
                foreach (AbstractTransfer SelectedItem in SelectedItems)
                {
                    RemoveTransfer(SelectedItem);
                }

                CurrentMultiselectList.IsSelectionEnabled = false;
            }
        }

        #endregion

        #endregion

        #region Context menu event handlers

        private void ContextMenu_StartTransfer_Handler(object sender, RoutedEventArgs e)
        {
            var Tag = ((FrameworkElement)sender).Tag;
            AbstractTransfer Page = (AbstractTransfer)App.TransferViewModel.FindByTag(Tag.ToString());

            StartTransfer(Page);
        }

        private void ContextMenu_StopTransfer_Handler(object sender, RoutedEventArgs e)
        {
            var Tag = ((FrameworkElement)sender).Tag;
            AbstractTransfer Page = (AbstractTransfer)App.TransferViewModel.FindByTag(Tag.ToString());

            App.TransferManager.Cancel(Page);
        }

        private void ContextMenu_RemoveTransfer_Handler(object sender, RoutedEventArgs e)
        {
            var Tag = ((FrameworkElement)sender).Tag;
            AbstractTransfer Page = (AbstractTransfer)App.TransferViewModel.FindByTag(Tag.ToString());

            if (MessageBox.Show(AppResources.WarningMessage_remove_single, AppResources.WarningTitle_remove_single, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                // User confirmed
                App.TransferViewModel.Remove(Page);
                App.TransferManager.Cancel(Page);
            }
        }

        #endregion

        #region Menu event handlers

        private void Menu_SelectAll_Handler(object sender, EventArgs e)
        {
            ItemContainerGenerator ItemContainerGenerator = CurrentMultiselectList.ItemContainerGenerator;

            foreach (ITransferable Item in CurrentMultiselectList.ItemsSource)
            {
                DependencyObject VisualItem = ItemContainerGenerator.ContainerFromItem(Item);
                MultiselectItem MultiselectItem = VisualItem as MultiselectItem;
                if (MultiselectItem != null)
                {
                    MultiselectItem.IsSelected = true;
                }
            }
        }

        private void Menu_StartAll_Handler(object sender, EventArgs e)
        {
            foreach (AbstractTransfer Page in App.TransferViewModel.PendingTransfers)
            {
                StartTransfer(Page);
            }
        }

        private void Menu_StopAll_Handler(object sender, EventArgs e)
        {
            App.TransferManager.CancelAll();
        }


        private void Menu_RemoveAll_Handler(object sender, EventArgs e)
        {
            if (MessageBox.Show(AppResources.WarningMessage_remove_all, AppResources.WarningTitle_remove_all, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                foreach (AbstractTransfer Page in App.TransferViewModel.AllTransfers)
                {
                    RemoveTransfer(Page);
                }
            }
        }

        private void Menu_ShowQuota_Handler(object sender, EventArgs e)
        {
            // Obtain an isolated store for an application.
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    MessageBox.Show(String.Format("Available: {0} MB", (store.AvailableFreeSpace) / (1024 * 1024)));
                }
            }

            catch (IsolatedStorageException)
            {
                MessageBox.Show("Unable to access store.");

            }
        }

        #region DEBUG: Add items

        private void Add10Items_Handler(object sender, EventArgs e)
        {
            for (int i = 1; i <= 10; i++)
            {
                App.TransferViewModel.Add(new DownloadTransfer { TransferUrl = @"http://ceoloide.com/error/redir.php?n=" + i, Path = "/OnePiece", Filename = i + ".jpg" });
            }
        }

        private void Add100Items_Handler(object sender, EventArgs e)
        {
            for (int i = 1; i <= 100; i++)
            {
                App.TransferViewModel.Add(new DownloadTransfer { TransferUrl = @"http://ceoloide.com/error/redir.php?n=" + i, Path = "/OnePiece", Filename = i + ".jpg" });
            }
        }

        private void Add500Items_Handler(object sender, EventArgs e)
        {
            for (int i = 1; i <= 500; i++)
            {
                App.TransferViewModel.Add(new DownloadTransfer { TransferUrl = @"http://ceoloide.com/error/redir.php?n=" + i, Path = "/OnePiece", Filename = i + ".jpg" });
            }
        }

        private void Add1000Items_Handler(object sender, EventArgs e)
        {
            for (int i = 1; i <= 1000; i++)
            {
                App.TransferViewModel.Add(new DownloadTransfer { TransferUrl = @"http://ceoloide.com/error/redir.php?n=" + i, Path = "/OnePiece", Filename = i + ".jpg" });
            }
        }

        private void Add5000Items_Handler(object sender, EventArgs e)
        {
            for (int i = 1; i <= 5000; i++)
            {
                App.TransferViewModel.Add(new DownloadTransfer { TransferUrl = @"http://ceoloide.com/error/redir.php?n=" + i, Path = "/OnePiece", Filename = i + ".jpg" });
            }
        }

        private void Add10000Items_Handler(object sender, EventArgs e)
        {
            for (int i = 1; i <= 10000; i++)
            {
                App.TransferViewModel.Add(new DownloadTransfer { TransferUrl = @"http://ceoloide.com/error/redir.php?n=" + i, Path = "/OnePiece", Filename = i + ".jpg" });
            }
        }

        #endregion

        #endregion

        #region Helper methods

        /// <summary>
        /// Keep track of the currently displayed Multiselect List at each change
        /// of the Pivot control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
            if(CurrentMultiselectList != null)
                CurrentMultiselectList.IsSelectionEnabled = false;

            if (e.Item == DownloadsPivotItem)
            {
                CurrentMultiselectList = DownloadsMultiselectList;
            }
            else if (e.Item == ArchivePivotItem)
            {
                CurrentMultiselectList = ArchiveMultiselectList;
            }
        }

        /// <summary>
        /// This method starts a PersistedPersistedDownloadTransfer object if the transfer is either:
        ///  - Completed
        ///  - Canceled
        ///  - Failed on the server
        ///  - Failed on the device
        ///  - Not yet stated (no status)
        /// </summary>
        /// <param name="Transfer"></param>
        private void StartTransfer(AbstractTransfer Transfer)
        {
            if (Transfer.TransferStatus == ExtendedTransferStatus.Completed
                    || Transfer.TransferStatus == ExtendedTransferStatus.Canceled
                    || Transfer.TransferStatus == ExtendedTransferStatus.Failed
                    || Transfer.TransferStatus == ExtendedTransferStatus.FailedServer
                    || Transfer.TransferStatus == ExtendedTransferStatus.None)
            {
                App.TransferManager.Start(Transfer);
            }
        }

        /// <summary>
        /// This method stops the transfer of a PersistedPersistedDownloadTransfer object.
        /// </summary>
        /// <param name="Transfer"></param>
        private void StopTransfer(AbstractTransfer Transfer)
        {
            App.TransferManager.Cancel(Transfer);
        }

        /// <summary>
        /// This method removes a PersistedPersistedDownloadTransfer object from the ViewModel and cancels
        /// its transfer.
        /// 
        /// WARNING: You should ask for confirmation to the user BEFORE calling this method.
        /// </summary>
        /// <param name="Transfer"></param>
        private void RemoveTransfer(AbstractTransfer Transfer)
        {
            App.TransferViewModel.Remove(Transfer);
            App.TransferManager.Cancel(Transfer);
        }

        #endregion
    }
}