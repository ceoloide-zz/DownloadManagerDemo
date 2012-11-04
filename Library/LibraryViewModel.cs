using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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
        }

        /// <summary>
        /// Adds a Page item to the database and collections.
        /// </summary>
        /// <param name="Item">The Page object to add.</param>
        public void Add(Page Item)
        {

            // Add a Page item to the data context
            _DataContext.Pages.InsertOnSubmit(Item);

            // Save changes to the database.
            _DataContext.SubmitChanges();

            // Add the item to the related collections
            _AllPages.Add(Item);
        }

        /// <summary>
        /// Removes a Page item to the database and collections.
        /// </summary>
        /// <param name="Item">The Page object to remove.</param>
        public void Remove(Page Item)
        {
            // Add a Page item to the data context
            _DataContext.Pages.DeleteOnSubmit(Item);

            // Save changes to the database.
            _DataContext.SubmitChanges();

            // Remove the item from the related collections
            _AllPages.Remove(Item);
        }
    }
}