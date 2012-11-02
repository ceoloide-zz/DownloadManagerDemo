using System;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using System.Data.Linq.Mapping;
using System.Collections.Generic;
using System.Data.Linq;
using System.ComponentModel;
using System.Windows;

namespace TransferManager
{
    /// <summary>
    /// 
    /// </summary>
    [Table]
    public class PersistedDownloadTransfer : INotifyPropertyChanged, INotifyPropertyChanging, ITransferable
    {
        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        protected void NotifyPropertyChanged(string propertyName)
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

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public PersistedDownloadTransfer() : base()
        {
            _Method = "GET";
            //_Headers = new KeyValuePair<string, string>();
            _TransferStatus = ExtendedTransferStatus.None;
            _FailedTransferAttempts = 0;
            _IsIndeterminateTransfer = false;
            _TotalBytesToReceive = -1;
            _BytesReceived = 0;
            _RequestId = string.Empty;
        }

        private int _UID;
        /// <summary>
        /// Gets or sets the unique ID of the transfer.
        /// </summary>
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int UID
        {
            get { return _UID; }
            set
            {
                if (_UID != value)
                {
                    NotifyPropertyChanging("UID");
                    _UID = value;
                    NotifyPropertyChanged("UID");
                }
            }
        }

        private string _Path;
        /// <summary>
        /// The path to the downloaded resource (e.g. /local/download/).
        /// </summary>
        [Column]
        public string Path 
        { 
            get { return _Path; }
            set
            {
                NotifyPropertyChanging("Path");

                _Path = value;

                // The path should start and end with a "/" 
                if (_Path.EndsWith("/"))
                    _Path = _Path.Substring(0, _Path.Length - 1);
                if (!_Path.StartsWith("/"))
                    _Path = "/" + _Path;

                NotifyPropertyChanged("Path");
            }
        }

        private string _Filename;
        /// <summary>
        /// Gets the filename of the downloaded file.
        /// </summary>
        [Column]
        public string Filename
        {
            get { return _Filename; }
            set
            {
                NotifyPropertyChanging("Filename");
                _Filename = value;
                NotifyPropertyChanged("Filename");
            }
        }

        /// <summary>
        /// Gets the full path to the downloaded file (e.g. /local/download/file.txt)
        /// </summary>
        public string FilenameWithPath { get { return _Path + "/" + _Filename; } }

        /// <summary>
        /// Checks whether the destination folder of the download exists. If it doesn't
        /// exists, this method creates it.
        /// </summary>
        public void OnBeforeAdd()
        {
            using (IsolatedStorageFile IsoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!IsoStore.DirectoryExists("shared/transfers" + Path))
                    IsoStore.CreateDirectory("shared/transfers" + Path);
            }
        }

        private string _Method;
        /// <summary>
        /// Gets the transfer method (GET).
        /// </summary>
        [Column]
        public string Method
        {
            get { return _Method; }
            set
            {
                NotifyPropertyChanging("Method");
                _Method = value;
                NotifyPropertyChanged("Method");
            }
        }

        //private KeyValuePair<string, string> _Headers;
        ///// <summary>
        ///// A KeyValuePair collection that holds the Headers to set. The following headers are reserved for use by the system and cannot be used by calling applications.
        ///// Adding one of the following headers to the Headers collection will cause a NotSupportedException to be thrown when the Add(BackgroundTransferRequest) method is used to queue the transfer request:
        ///// - If-Modified-Since
        ///// - If-None-Match
        ///// - If-Range
        ///// - Range
        ///// - Unless-Modified-Since
        ///// </summary>
        //public KeyValuePair<string, string> Headers 
        //{ 
        //    get { return _Headers; }
        //    set { _Headers = value; }
        //}

        /// <summary>
        /// Gets the URI of temporary location of the transfer.
        /// </summary>
        public Uri TransferLocationUri { get { return new Uri("shared/transfers" + FilenameWithPath, UriKind.RelativeOrAbsolute); } }

        private string _TransferUrl;
        /// <summary>
        /// Gets or sets the url of the ITransferable object.
        /// </summary>
        [Column]
        public string TransferUrl
        {
            get { return _TransferUrl; }
            set
            {
                NotifyPropertyChanging("TransferUrl");
                _TransferUrl = value;
                NotifyPropertyChanged("TransferUrl");
            }
        }

        /// <summary>
        /// Gets the URI of the ITransferable object.
        /// </summary>
        public Uri TransferUri
        {
            get { return new Uri(_TransferUrl, UriKind.RelativeOrAbsolute); }
        }

        private string _RequestId;
        /// <summary>
        /// Public property that holds the RequestId property of the BackgroundTransfer
        /// instance asssociated with this object.
        /// </summary>
        [Column]
        public string RequestId
        {
            get { return _RequestId; }
            set
            {
                NotifyPropertyChanging("RequestId");
                _RequestId = value;
                NotifyPropertyChanged("RequestId");
            }
        }

        private ExtendedTransferStatus _TransferStatus;
        /// <summary>
        /// Public property that holds the TransferStatus of the ITransferable object.
        /// The implementation should define the behaviour for each transfer status.
        /// </summary>
        [Column]
        public ExtendedTransferStatus TransferStatus
        {
            get
            {
                return _TransferStatus;
            }
            set
            {
                ExtendedTransferStatus PreviousTransferStatus = _TransferStatus;

                NotifyPropertyChanging("TransferStatus");
                _TransferStatus = value;
                NotifyPropertyChanged("TransferStatus");

                if (OnStatusChanged != null)
                {
                    StatusChangeHandler EventHandler = OnStatusChanged;
                    EventHandler(PreviousTransferStatus, _TransferStatus, this);
                }
            }
        }

        /// <summary>
        /// Event fired when the TransferStatus ob the ITransferable object changes.
        /// </summary>
        public event StatusChangeHandler OnStatusChanged;

        /// <summary>
        /// This method is called when a transfer has been completed successfully.
        /// </summary>
        public void OnComplete()
        {
            // We need to move the file to its intended location.
            // To avoid blocking the UI, we let a BackgroundWorker do the job and asynchronously
            // inform us of its completion.

            BackgroundWorker Worker = new BackgroundWorker { WorkerReportsProgress = false, WorkerSupportsCancellation = false };
            Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);

            Worker.RunWorkerAsync();
        }

        /// <summary>
        /// The DoWork method of the BackgroundWorker used to process the Completed state. This method
        /// moves the downloaded file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // TODO: What happens if the user closes the application while the BackgroundWorker is still pending?
            // Should it be handled?
            try
            {
                using (IsolatedStorageFile IsoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // First we ensure that the destination folder is existing
                    if (!IsoStore.DirectoryExists(Path))
                        IsoStore.CreateDirectory(Path);

                    // Then we check whether the downloaded file is existing
                    if (IsoStore.FileExists(TransferLocationUri.OriginalString))
                    {
                        // Then we check whether the destination file is existing and in case we delete it
                        if (IsoStore.FileExists(FilenameWithPath))
                            IsoStore.DeleteFile(FilenameWithPath);

                        // Finally we move the downloaded file to its new location
                        IsoStore.MoveFile(TransferLocationUri.OriginalString, FilenameWithPath);

                        // The job has been done, the result is Completed.
                        e.Result = ExtendedTransferStatus.Completed;
                    }
                    else
                    {
                        // An error occurred during completion, the result is Failed.
                        e.Result = ExtendedTransferStatus.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                // An error occurred during completion, the result is Failed.
                e.Result = ExtendedTransferStatus.Failed;
            }
        }

        /// <summary>
        /// Processes the completion of the BackgroundWorker.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The arguments passed to this method.</param>
        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if((ExtendedTransferStatus)e.Result == ExtendedTransferStatus.Completed) 
            {
                // We now reset the number of failed downloads
                FailedTransferAttempts = 0;

                // We set the BytesReceived to the TotalBytesToReceive
                BytesReceived = TotalBytesToReceive;
                TransferProgress = 1;
            }
            TransferStatus = (ExtendedTransferStatus)e.Result;
        }

        private int _FailedTransferAttempts;
        /// <summary>
        /// Public property that holds the number of failed transfer attempts of this object.
        /// </summary>
        [Column]
        public int FailedTransferAttempts
        {
            get
            {
                return _FailedTransferAttempts;
            }
            set
            {
                NotifyPropertyChanging("FailedTransferAttempts");
                _FailedTransferAttempts = value;
                NotifyPropertyChanged("FailedTransferAttempts");
            }
        }

        private bool _IsIndeterminateTransfer;
        /// <summary>
        /// Gets or sets whether the current transfer is indeterminate or not. A download
        /// transfer is indeterminate if TotalBytesToReceive is -1.
        /// </summary>
        [Column]
        public bool IsIndeterminateTransfer
        {
            get { return _IsIndeterminateTransfer; }
            set
            {
                NotifyPropertyChanging("IsIndeterminateTransfer");
                _IsIndeterminateTransfer = value;
                NotifyPropertyChanged("IsIndeterminateTransfer");
            }
        }

        private long _TotalBytesToReceive;
        /// <summary>
        /// Gets or sets the total bytes to receive for the current download transfer.
        /// </summary>
        [Column]
        public long TotalBytesToReceive
        {
            get { return _TotalBytesToReceive; }
            set
            {
                NotifyPropertyChanging("TotalBytesToReceive");
                _TotalBytesToReceive = value;
                NotifyPropertyChanged("TotalBytesToReceive");
            }
        }

        private long _BytesReceived;
        /// <summary>
        /// Gets or sets the bytes that have been currently received.
        /// </summary>
        [Column]
        public long BytesReceived
        {
            get { return _BytesReceived; }
            set
            {
                NotifyPropertyChanging("BytesReceived");
                _BytesReceived = value;
                NotifyPropertyChanged("BytesReceived");
            }
        }

        private double _TransferProgress;
        /// <summary>
        /// Gets or sets the transfer progress.
        /// </summary>
        [Column]
        public double TransferProgress
        {
            get { return _TransferProgress; }
            set
            {
                NotifyPropertyChanging("TransferProgress");
                _TransferProgress = value;
                NotifyPropertyChanged("TransferProgress");
            }
        }

        /// <summary>
        /// Provides an handler for changes of transfer progress. Mainly used to update UI.
        /// Event argument contains BytesReceived / BytesSent that represent current transfer status
        /// and TotalBytesToReceive / TotalBytesToSend that represent the amount to transfer.
        /// 
        /// NOTE: TotalBytesToReceive can be -1 if the amount to receive is unknown.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event args.</param>
        void ITransferable.TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            // If TotalBytesToReceive == -1 the transfer is indeterminate
            IsIndeterminateTransfer = (e.Request.TotalBytesToReceive == -1 && TransferStatus == ExtendedTransferStatus.Transferring);

            TotalBytesToReceive = e.Request.TotalBytesToReceive;
            BytesReceived = e.Request.BytesReceived;

            if (!IsIndeterminateTransfer)
                TransferProgress = (double)BytesReceived / (double)TotalBytesToReceive;
        }
    }
}
