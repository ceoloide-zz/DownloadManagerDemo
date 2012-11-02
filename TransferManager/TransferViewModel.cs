using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace TransferManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TransferViewModel<T> : TransferManagerContext, INotifyPropertyChanged where T : class, ITransferable
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

        /// <summary>
        /// The reference data context.
        /// </summary>
        private TransferDataContext<T> _DataContext;

        private ObservableCollection<T> _AllTransfers;
        /// <summary>
        /// Gets or sets the ObservableCollection of all the transfers available.
        /// </summary>
        public ObservableCollection<T> AllTransfers
        {
            get { return _AllTransfers; }
            set
            {
                _AllTransfers = value;
                NotifyPropertyChanged("AllTransfers");
            }
        }

        private ObservableCollection<T> _ActiveTransfers;
        /// <summary>
        /// Gets or sets the ObservableCollection of all the currently active transfers.
        /// </summary>
        public ObservableCollection<T> ActiveTransfers
        {
            get { return _ActiveTransfers; }
            set
            {
                _ActiveTransfers = value;
                NotifyPropertyChanged("ActiveTransfers");
            }
        }

        private ObservableCollection<T> _FailedTransfers;
        /// <summary>
        /// Gets or sets the ObservableCollection of all the failed or canceled transfers.
        /// </summary>
        public ObservableCollection<T> FailedTransfers
        {
            get { return _FailedTransfers; }
            set
            {
                _FailedTransfers = value;
                NotifyPropertyChanged("FailedTransfers");
            }
        }

        private ObservableCollection<T> _CompletedTransfers;
        /// <summary>
        /// Gets or sets the ObservableCollection of all the completed transfers.
        /// </summary>
        public ObservableCollection<T> CompletedTransfers
        {
            get { return _CompletedTransfers; }
            set
            {
                _CompletedTransfers = value;
                NotifyPropertyChanged("CompletedTransfers");
            }
        }

        /// <summary>
        /// This constructor istantiates the data context.
        /// </summary>
        /// <param name="DBConnectionString">The DB connection string to pass to the data context.</param>
        public TransferViewModel(string DBConnectionString)
        {
            _DataContext = new TransferDataContext<T>(DBConnectionString);
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
        /// Loads the collections of this TransferViewModel object from the database.
        /// </summary>
        public void LoadCollectionsFromDatabase()
        {
            // We retrieve all the transfers that are existing
            var AllTransfersFound = from T Transfer in _DataContext.Transfers
                                    select Transfer;
            _AllTransfers = new ObservableCollection<T>(AllTransfersFound);

            foreach (T Transfer in AllTransfersFound)
            {
                Transfer.OnStatusChanged += StatusChangeHandler;
            }

            // We retrieve all the transfers that are active
            var AllActiveTransfersFound = from T Transfer in _DataContext.Transfers
                                          where Transfer.TransferStatus != ExtendedTransferStatus.Canceled &&
                                            Transfer.TransferStatus != ExtendedTransferStatus.Completed &&
                                            Transfer.TransferStatus != ExtendedTransferStatus.Failed &&
                                            Transfer.TransferStatus != ExtendedTransferStatus.FailedServer &&
                                            Transfer.TransferStatus != ExtendedTransferStatus.None
                                          select Transfer;
            _ActiveTransfers = new ObservableCollection<T>(AllActiveTransfersFound);

            // We retrieve all the transfers that are failed or canceled
            var AllFailedTransfersFound = from T Transfer in _DataContext.Transfers
                                          where Transfer.TransferStatus == ExtendedTransferStatus.Canceled ||
                                          Transfer.TransferStatus == ExtendedTransferStatus.Failed ||
                                          Transfer.TransferStatus == ExtendedTransferStatus.FailedServer
                                          select Transfer;
            _FailedTransfers = new ObservableCollection<T>(AllFailedTransfersFound);

            // We retrieve all the transfers that are completed
            var AllCompletedTransfersFound = from T Transfer in _DataContext.Transfers
                                             where Transfer.TransferStatus == ExtendedTransferStatus.Completed
                                             select Transfer;
            _CompletedTransfers = new ObservableCollection<T>(AllCompletedTransfersFound);
        }

        /// <summary>
        /// Adds a ITransferable item to the database and collections.
        /// </summary>
        /// <param name="Item">The ITransferable object to add.</param>
        public void Add(T Item)
        {
            // Register event handler for status change
            Item.OnStatusChanged += StatusChangeHandler;

            // Add a Page item to the data context
            _DataContext.Transfers.InsertOnSubmit(Item);

            // Save changes to the database.
            _DataContext.SubmitChanges();

            // Add the item to the related collections
            OperateByStatus(Item, Operations.Add);
        }

        /// <summary>
        /// Removes a ITransferable item to the database and collections.
        /// </summary>
        /// <param name="Item">The ITransferable object to remove.</param>
        public void Remove(T Item)
        {
            // Un-register event handler for status change
            Item.OnStatusChanged -= StatusChangeHandler;

            // Add a Page item to the data context
            _DataContext.Transfers.DeleteOnSubmit(Item);

            // Save changes to the database.
            _DataContext.SubmitChanges();

            // Remove the item from the related collections
            OperateByStatus(Item, Operations.Remove);
        }

        /// <summary>
        /// This method returns a list containing ITransferable objects
        /// that are still pending in the TransferManager.
        /// </summary>
        /// <returns>A list of ITransferable objects.</returns>
        public ICollection<ITransferable> RetrievePending()
        {
            var AllFoundTransfers = from ITransferable Item in _DataContext.Transfers
                                    where Item.TransferStatus == ExtendedTransferStatus.Queued
                                    select Item;

            return new List<ITransferable>(AllFoundTransfers);
        }

        /// <summary>
        /// This method provides a way to retrieve an ITransferable object based on
        /// a BackgroundTransfer Tag property. This method returns NULL if the Tag cannot
        /// be parsed correctly or if no ITransferable object is found. This method returns the first instance
        /// of ITransferable item that is found.
        /// </summary>
        /// <param name="Tag">A string that represents the Tag property of a BackgroundTransfer object</param>
        /// <returns>An ITransferable object.</returns>
        public ITransferable FindByTag(string Tag)
        {
            try
            {
                int TransferId = int.Parse(Tag);

                var AllFoundTransfers = from ITransferable Item in _DataContext.Transfers
                                        where Item.UID == TransferId
                                        select Item;

                return (AllFoundTransfers.Count<ITransferable>() > 0) ? AllFoundTransfers.First<ITransferable>() : null;
            }
            catch(Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Handler for the OnStatusChange event of a ITransferable object.
        /// </summary>
        /// <param name="Previous">The transfer status that the ITransferable object had before changing.</param>
        /// <param name="Current">The transfer status that the ITransferable object currently has.</param>
        public void StatusChangeHandler(ExtendedTransferStatus Previous, ExtendedTransferStatus Current, ITransferable Item)
        {
            if (Previous != Current)
            {
                // First remove the item from the related collections based on its previous status
                switch (Previous)
                {
                    case ExtendedTransferStatus.Queued:
                    case ExtendedTransferStatus.Transferring:
                    case ExtendedTransferStatus.Waiting:
                    case ExtendedTransferStatus.WaitingForWiFi:
                    case ExtendedTransferStatus.WaitingForExternalPower:
                    case ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    case ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork:
                    case ExtendedTransferStatus.Paused:
                        _ActiveTransfers.Remove((T)Item);
                        break;
                    case ExtendedTransferStatus.Completed:
                        _CompletedTransfers.Remove((T)Item);
                        break;
                    case ExtendedTransferStatus.Failed:
                    case ExtendedTransferStatus.FailedServer:
                    case ExtendedTransferStatus.Canceled:
                        _FailedTransfers.Remove((T)Item);
                        break;
                }

                // Then add the item to the related collection based on its current status
                switch (Current)
                {
                    case ExtendedTransferStatus.Queued:
                    case ExtendedTransferStatus.Transferring:
                    case ExtendedTransferStatus.Waiting:
                    case ExtendedTransferStatus.WaitingForWiFi:
                    case ExtendedTransferStatus.WaitingForExternalPower:
                    case ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    case ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork:
                    case ExtendedTransferStatus.Paused:
                        _ActiveTransfers.Add((T)Item);
                        break;
                    case ExtendedTransferStatus.Completed:
                        _CompletedTransfers.Add((T)Item);
                        break;
                    case ExtendedTransferStatus.Failed:
                    case ExtendedTransferStatus.FailedServer:
                    case ExtendedTransferStatus.Canceled:
                        _FailedTransfers.Add((T)Item);
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
        private void OperateByStatus(T Item, Operations Operation)
        {
            switch (Item.TransferStatus)
            {
                case ExtendedTransferStatus.Queued:
                case ExtendedTransferStatus.Transferring:
                case ExtendedTransferStatus.Waiting:
                case ExtendedTransferStatus.WaitingForWiFi:
                case ExtendedTransferStatus.WaitingForExternalPower:
                case ExtendedTransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                case ExtendedTransferStatus.WaitingForNonVoiceBlockingNetwork:
                case ExtendedTransferStatus.Paused:
                    if (Operation == Operations.Remove) _ActiveTransfers.Remove(Item);
                    else _ActiveTransfers.Add(Item);
                    break;
                case ExtendedTransferStatus.Completed:
                    if (Operation == Operations.Remove) _CompletedTransfers.Remove(Item);
                    else _CompletedTransfers.Add(Item);
                    break;
                case ExtendedTransferStatus.Failed:
                case ExtendedTransferStatus.FailedServer:
                case ExtendedTransferStatus.Canceled:
                    if (Operation == Operations.Remove) _FailedTransfers.Remove(Item);
                    else _FailedTransfers.Add(Item);
                    break;
            }

            if (Operation == Operations.Remove) _AllTransfers.Remove(Item);
            else _AllTransfers.Add(Item);
        }
    }
}
