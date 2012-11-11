using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TransferManager;

namespace DownloadManager.Library
{
    public class LibraryViewModel
    {

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify Silverlight that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        protected void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion

        // LINQ to SQL data context for the local database.
        private LibraryDataContext _DataContext;

        private ObservableCollection<Page> _AllPages;
        /// <summary>
        /// Gets or sets the ObservableCollection of all the transfers available.
        /// </summary>
        public ObservableCollection<Page> AllPages
        {
            get { return _AllPages; }
            set
            {
                NotifyPropertyChanging("AllPages");
                _AllPages = value;
                NotifyPropertyChanged("AllPages");
            }
        }

        private ObservableCollection<Page> _DownloadedPages;
        /// <summary>
        /// Gets or sets the ObservableCollection of all the transfers available.
        /// </summary>
        public ObservableCollection<Page> DownloadedPages
        {
            get { return _DownloadedPages; }
            set
            {
                NotifyPropertyChanging("DownloadedPages");
                _DownloadedPages = value;
                NotifyPropertyChanged("DownloadedPages");
            }
        }

        private ObservableCollection<Page> _PendingPages;
        /// <summary>
        /// Gets or sets the ObservableCollection of all the transfers available.
        /// </summary>
        public ObservableCollection<Page> PendingPages
        {
            get { return _PendingPages; }
            set
            {
                NotifyPropertyChanging("PendingPages");
                _PendingPages = value;
                NotifyPropertyChanged("PendingPages");
            }
        }

        private ObservableCollection<Page> _FailedPages;
        /// <summary>
        /// Gets or sets the ObservableCollection of all the transfers available.
        /// </summary>
        public ObservableCollection<Page> FailedPages
        {
            get { return _FailedPages; }
            set
            {
                NotifyPropertyChanging("FailedPages");
                _FailedPages = value;
                NotifyPropertyChanged("FailedPages");
            }
        }

        /// <summary>
        /// This constructor istantiates the data context.
        /// </summary>
        /// <param name="DBConnectionString">The DB connection string to pass to the data context.</param>
        public LibraryViewModel(string DBConnectionString)
        {
            _DataContext = new LibraryDataContext(DBConnectionString);
        }

        /// <summary>
        /// This method calls the SubmitChanges method of the data context to store the changes
        /// in the database.
        /// </summary>
        public void SaveChangesToDB()
        {
            _DataContext.SubmitChanges();
        }

        /// <summary>
        /// Loads the collections of this PageViewModel object from the database.
        /// </summary>
        public void LoadCollectionsFromDatabase()
        {
            // We retrieve all the pages that are existing
            var AllPagesFound = from Page PageFound in _DataContext.Pages
                                select PageFound;
            _AllPages = new ObservableCollection<Page>(AllPagesFound);

            foreach (Library.Page Page in AllPagesFound)
            {
                Page.Transfer.OnStatusChanged += StatusChangeHandler;
            }
            
            // We retrieve all the pages that are completed
            _DownloadedPages = new ObservableCollection<Page>();
            foreach(AbstractTransfer Transfer in App.TransferViewModel.CompletedTransfers)
            {
                Library.Page CompletedPage = FindByUID(Transfer.ExternalReference);
                if (CompletedPage != null)
                {
                    _DownloadedPages.Add(CompletedPage);
                }
            }

            // We retrieve all the pages that are pending
            _PendingPages = new ObservableCollection<Page>();
            foreach (AbstractTransfer Transfer in App.TransferViewModel.PendingTransfers)
            {
                Library.Page PendingPage = FindByUID(Transfer.ExternalReference);
                if (PendingPage != null)
                {
                    _PendingPages.Add(PendingPage);
                }
            }

            // We retrieve all the pages that are failed
            _FailedPages = new ObservableCollection<Page>();
            foreach (AbstractTransfer Transfer in App.TransferViewModel.FailedTransfers)
            {
                Library.Page FailedPage = FindByUID(Transfer.ExternalReference);
                if (FailedPage != null)
                {
                    _FailedPages.Add(FailedPage);
                }
            }
        }

        /// <summary>
        /// Adds a Page item to the database and collections.
        /// </summary>
        /// <param name="Item">The Page object to add.</param>
        public void Add(Page Item)
        {
            // Register event handler for status change
            Item.Transfer.OnStatusChanged += StatusChangeHandler;

            // Add a Page item to the data context
            _DataContext.Pages.InsertOnSubmit(Item);

            // Save changes to the database.
            _DataContext.SubmitChanges();

            // Add the item to the related collections
            OperateByStatus(Item, Operations.Add);
        }

        /// <summary>
        /// Removes a Page item to the database and collections.
        /// </summary>
        /// <param name="Item">The Page object to remove.</param>
        public void Remove(Page Item)
        {
            // Un-register event handler for status change
            Item.Transfer.OnStatusChanged -= StatusChangeHandler;

            // Add a Page item to the data context
            _DataContext.Pages.DeleteOnSubmit(Item);

            // Save changes to the database.
            _DataContext.SubmitChanges();

            // Remove the item from the related collections
            OperateByStatus(Item, Operations.Remove);
        }

        /// <summary>
        /// This method provides a way to retrieve a Page object based on
        /// its UID. This method returns NULL if the UID cannot
        /// be parsed correctly or if no Page object is found. This method returns the first instance
        /// of Page item that is found.
        /// </summary>
        /// <param name="Tag">A string that represents the UID property of a Page object</param>
        /// <returns>A Page object.</returns>
        public Page FindByUID(string UID)
        {
            try
            {
                int PageUID = int.Parse(UID);

                var AllFoundTransfers = from Page Item in _DataContext.Pages
                                        where Item.UID == PageUID
                                        select Item;

                return (AllFoundTransfers.Count<Page>() > 0) ? AllFoundTransfers.First<Page>() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Handler for the OnStatusChange event of a ITransferable object associated with a Page.
        /// </summary>
        /// <param name="Previous">The transfer status that the ITransferable object had before changing.</param>
        /// <param name="Current">The transfer status that the ITransferable object currently has.</param>
        public void StatusChangeHandler(ExtendedTransferStatus Previous, ExtendedTransferStatus Current, ITransferable Item)
        {
            Library.Page Page = FindByUID(Item.ExternalReference);

            if (Page != null)
            {
                // Then add the item to the related collection based on its current status
                switch (Current)
                {
                    case ExtendedTransferStatus.Queued:
                    case ExtendedTransferStatus.Transferring:
                    case ExtendedTransferStatus.Waiting:
                    case ExtendedTransferStatus.WaitingForRetry:
                    case ExtendedTransferStatus.WaitingForWiFi:
                    case ExtendedTransferStatus.WaitingForExternalPower:
                    case ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    case ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork:
                    case ExtendedTransferStatus.Paused:
                    case ExtendedTransferStatus.Canceled:
                        if (Previous == ExtendedTransferStatus.Completed)
                        {
                            _DownloadedPages.Remove(Page);
                            _PendingPages.Add(Page);
                        }
                        else if (Previous == ExtendedTransferStatus.Failed || Previous == ExtendedTransferStatus.FailedServer)
                        {
                            _FailedPages.Remove(Page);
                            _PendingPages.Add(Page);
                        }
                        break;
                    case ExtendedTransferStatus.Completed:
                        if (Previous == ExtendedTransferStatus.Failed || Previous == ExtendedTransferStatus.FailedServer)
                        {
                            _FailedPages.Remove(Page);
                            _DownloadedPages.Add(Page);
                        }
                        else if (Previous != ExtendedTransferStatus.Completed)
                        {
                            _PendingPages.Remove(Page);
                            _DownloadedPages.Add(Page);
                        }

                        break;
                    case ExtendedTransferStatus.Failed:
                    case ExtendedTransferStatus.FailedServer:
                        if (Previous == ExtendedTransferStatus.Completed)
                        {
                            _DownloadedPages.Remove(Page);
                            _FailedPages.Add(Page);
                        }
                        else if (Previous != ExtendedTransferStatus.Failed && Previous != ExtendedTransferStatus.FailedServer)
                        {
                            _PendingPages.Remove(Page);
                            _FailedPages.Add(Page);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// The possible operations to perform.
        /// </summary>
        private enum Operations { Add, Remove }

        /// <summary>
        /// Removes an item from the correct ObservableCollection, based on its status.
        /// </summary>
        /// <param name="Item">The item to remove.</param>
        private void OperateByStatus(Library.Page Item, Operations Operation)
        {
            switch (Item.Transfer.TransferStatus)
            {
                case ExtendedTransferStatus.None:
                case ExtendedTransferStatus.Queued:
                case ExtendedTransferStatus.Transferring:
                case ExtendedTransferStatus.Waiting:
                case ExtendedTransferStatus.WaitingForRetry:
                case ExtendedTransferStatus.WaitingForWiFi:
                case ExtendedTransferStatus.WaitingForExternalPower:
                case ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                case ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork:
                case ExtendedTransferStatus.Paused:
                case ExtendedTransferStatus.Canceled:
                    if (Operation == Operations.Remove) _PendingPages.Remove(Item);
                    else _PendingPages.Add(Item);
                    break;
                case ExtendedTransferStatus.Completed:
                    if (Operation == Operations.Remove) _DownloadedPages.Remove(Item);
                    else _DownloadedPages.Add(Item);
                    break;
                case ExtendedTransferStatus.Failed:
                case ExtendedTransferStatus.FailedServer:
                    if (Operation == Operations.Remove) _FailedPages.Remove(Item);
                    else _FailedPages.Add(Item);
                    break;
            }

            if (Operation == Operations.Remove) _AllPages.Remove(Item);
            else _AllPages.Add(Item);
        }
    }
}