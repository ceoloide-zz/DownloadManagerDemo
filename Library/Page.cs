using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Text;
using TransferManager;
using System.ComponentModel;
using System.Windows;

namespace DownloadManager.Library
{
    [Table]
    public class Page : INotifyPropertyChanged, INotifyPropertyChanging
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

        public Page()
        {
            _Name = "No name";
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

        private string _Name;
        [Column]
        public string Name { get { return _Name; } set { _Name = value; } }

        private int _TransferUID;
        [Column]
        public int TransferUID
        {
            get
            {
                return _TransferUID;
            }
            set
            {
                NotifyPropertyChanging("Transfer");
                NotifyPropertyChanging("TransferUID");

                _TransferUID = value;
                _Transfer = null;

                NotifyPropertyChanged("Transfer");
                NotifyPropertyChanged("TransferUID");
            }
        }

        private DownloadTransfer _Transfer;
        public DownloadTransfer Transfer
        {
            get
            {
                if (_Transfer == null)
                {
                    _Transfer = (DownloadTransfer)App.TransferViewModel.FindByUID(_TransferUID);
                }
                return _Transfer;
            }
            set
            {
                NotifyPropertyChanging("Transfer");
                NotifyPropertyChanging("TransferUID");
                _Transfer = value;

                if (value != null)
                {
                    _TransferUID = value.UID;
                }

                NotifyPropertyChanged("Transfer");
                NotifyPropertyChanged("TransferUID");
            }
        }
    }
}