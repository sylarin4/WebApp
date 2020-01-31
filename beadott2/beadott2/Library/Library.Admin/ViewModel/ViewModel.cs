using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Library.Data;
using Library.Admin.View;

namespace Library.Admin
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Private fields

        private CommunicateModel _model;
        private AdminWindow _adminWindow;

        private int _selectedBookIndex;
        private int _selectedLendingIndex;

        private BookDTO _selectedBook;
        private LendingDTO _selectedLending;

        private ObservableCollection<BookDTO> _books;
        private ObservableCollection<LendingDTO> _lendings;

        #endregion

        #region Binded properties

        public String LoginUserName { get; set; }
        public String LoginUserPassword { get; set; }

        public string LoginErrorMessage { get; set; }
        public String LendingErrorMessage { get; set; }
        public String BookErrorMessage { get; set; }

        public int CurrentVolID { get; set; }

        public Int32 SelectedBookIndex
        {
            get { return _selectedBookIndex; }
            set
            {
                if (_selectedBookIndex != value)
                {
                    _selectedBookIndex = value;
                    OnPropertyChanged("SelectedBookIndex");
                }
                if (_selectedBookIndex >= 0 && _selectedBookIndex < _books.Count)
                    SelectedBook = new BookDTO
                    {
                        ID = _books[_selectedBookIndex].ID,
                        ISBN = _books[_selectedBookIndex].ISBN,
                        Author = _books[_selectedBookIndex].Author,
                        Title = _books[_selectedBookIndex].Title,
                        VolIDs = _books[_selectedBookIndex].VolIDs,
                        ReleaseYear = _books[_selectedBookIndex].ReleaseYear
                    };
            }
        }

        public Int32 SelectedLendingIndex
        {
            get { return _selectedLendingIndex; }
            set
            {
                if (_selectedLendingIndex != value)
                {
                    _selectedLendingIndex = value;
                    OnPropertyChanged("SelectedLendingIndex");
                    LendingErrorMessage = "";
                    OnPropertyChanged("LendingErrorMessage");
                }
                if(_selectedLendingIndex >= 0 && _selectedLendingIndex < _lendings.Count)
                {
                    SelectedLending = new LendingDTO
                    {
                        ID = _lendings[_selectedLendingIndex].ID,
                        VolID = _lendings[_selectedLendingIndex].VolID,
                        GuestID = _lendings[_selectedLendingIndex].GuestID,
                        StartDay = _lendings[_selectedLendingIndex].StartDay,
                        EndDay = _lendings[_selectedLendingIndex].EndDay,
                        IsActive = _lendings[_selectedLendingIndex].IsActive
                    };
                    OnPropertyChanged("SelectedLending");
                }
            }
        }

        public BookDTO SelectedBook
        {
            get { return _selectedBook; }
            private set
            {
                if (_selectedBook != value)
                {
                    _selectedBook = value;
                    OnPropertyChanged("SelectedBook");
                    BookErrorMessage = "";
                    OnPropertyChanged("BookErrorMessage");
                }
            }
        }

        public LendingDTO SelectedLending
        {
            get { return _selectedLending; }
            private set
            {
                if (_selectedLending != value)
                {
                    _selectedLending = value;
                    OnPropertyChanged("SelectedLending");
                }
            }
        }

        public ObservableCollection<BookDTO> Books
        {
            get { return _books; }
            private set
            {
                if (_books != value)
                {
                    _books = value;
                    OnPropertyChanged("Books");
                }
            }
        }

        public ObservableCollection<LendingDTO> Lendings
        {
            get { return _lendings; }
            private set
            {
                if(_lendings != value)
                {
                    _lendings = value;
                    OnPropertyChanged("Lendings");
                }
            }
        }


        #endregion

        #region Commands

        public DelegateCommand LoginCommand { get; private set; }
        public DelegateCommand LogoutCommand { get; private set; }
        public DelegateCommand ListBooksCommand { get; private set; }
        public DelegateCommand ListLendingsCommand { get; private set; }
        public DelegateCommand DeleteVolCommand { get; private set; }
        public DelegateCommand AddBookCommand { get; private set; }
        public DelegateCommand AddVolCommand { get; private set; }
        public DelegateCommand ActivateLendingCommand { get; private set; }
        public DelegateCommand InactivateLendingCommand { get; private set; }

        #endregion

        #region Constructor

        public ViewModel()
        {
            _model = new CommunicateModel();
            LoginCommand = new DelegateCommand(param => OnLoginCommand(param));
            LogoutCommand = new DelegateCommand(param => OnLogoutCommand());
            ListBooksCommand = new DelegateCommand(param => OnListBooksCommand());
            ListLendingsCommand = new DelegateCommand(param => OnListLendingsCommand());
            DeleteVolCommand = new DelegateCommand(param => OnDeleteVolCommand());
            AddBookCommand = new DelegateCommand(param => OnAddBookCommand());
            AddVolCommand = new DelegateCommand(param => OnAddVolCommand());
            ActivateLendingCommand = new DelegateCommand(param => OnActivateLendingCommand());
            InactivateLendingCommand = new DelegateCommand(param => OnInactivateLendingCommand());
        }

        #endregion

        #region Private methods

        private async void OnLoginCommand(object loginWindow)
        {
            try
            {
                // If logged in successfully, open the new window and close the login window.
                await _model.Login(LoginUserName, LoginUserPassword);
                LoginErrorMessage = "";
                OnPropertyChanged("ErrorMessage");
                OpenAdminWindow();
                CloseWindow((Window)loginWindow);
            }
            catch
            {
                LoginErrorMessage = "Invalid user name or password.";
                OnPropertyChanged("ErrorMessage");
            }
        }

        private async void OnLogoutCommand()
        {
            await _model.Logout();
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            _adminWindow.Close();
        }

        private void OpenAdminWindow()
        {
            _adminWindow = new AdminWindow(this);
            _adminWindow.Show();
        }

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        private async void OnListBooksCommand()
        {
            var books = await _model.ListBooks();
            Books = new ObservableCollection<BookDTO>(books);
        }

        private async void OnListLendingsCommand()
        {
            var lendings = await _model.ListLendings();
            Lendings = new ObservableCollection<LendingDTO>(lendings);
        }

        private async void OnDeleteVolCommand()
        {
            if(CurrentVolID != 0)
            {
                int volId = CurrentVolID;
                var isSucceeded = await _model.DeleteVol(volId);
                if (isSucceeded)
                {
                    BookErrorMessage = "Volume deleted Successfully.";
                    OnPropertyChanged("BookErrorMessage");
                    OnListBooksCommand();
                }
                else
                {
                    BookErrorMessage = "Deleting volume was unsuccessful: maybe the volume is lended.";
                    OnPropertyChanged("BookErrorMessage");
                }
            }
            else
            {
                BookErrorMessage = "Volume must be selected.";
                OnPropertyChanged("BookErrorMessage");
            }
            
        }

        private async void OnAddBookCommand()
        {
            if(SelectedBook == null)
            {
                BookErrorMessage = "Book must be given.";
                OnPropertyChanged("BookErrorMessage");
            }
            else
            {
                BookDTO book = SelectedBook;
                bool isSucceeded = await _model.AddBook(book);
                if (isSucceeded)
                {
                    BookErrorMessage = "Book added successfully";
                    OnPropertyChanged("BookErrorMessage");
                    OnListBooksCommand();
                }
                else
                {
                    BookErrorMessage = "Adding new book was unsuccessful.";
                    OnPropertyChanged("BookErrorMessage");
                }
            }
        }

        public async void OnAddVolCommand()
        {
            if (SelectedBook == null)
            {
                BookErrorMessage = "Book must be given.";
                OnPropertyChanged("BookErrorMessage");
            }
            else
            {
                bool isSucceeded = await _model.AddVol(SelectedBook.ID);
                if (isSucceeded)
                {
                    BookErrorMessage = "Volume added successfully.";
                    OnPropertyChanged("BookErrorMessage");
                    OnListBooksCommand();
                }
                else
                {
                    BookErrorMessage = "Failed to add volume to the selected book.";
                    OnPropertyChanged("BookErrorMessage");
                }
            }
        }

        public async void OnActivateLendingCommand()
        {
            if (SelectedLending == null)
            {
                LendingErrorMessage = "Lending must be selected.";
                OnPropertyChanged("LendingErrorMessage");
            }
            else
            {
                bool isSucceeded = await _model.ActivateLending(SelectedLending.ID);
                if (isSucceeded)
                {
                    LendingErrorMessage = "Lending activated successfully.";
                    OnPropertyChanged("LendingErrorMessage");
                    OnListLendingsCommand();
                }
                else
                {
                    LendingErrorMessage = "Failed to activate lending.";
                    OnPropertyChanged("LendingErrorMessage");
                }
            }
        }

        public async void OnInactivateLendingCommand()
        {
            if(SelectedLending == null)
            {
                LendingErrorMessage = "Lending must be selected.";
                OnPropertyChanged("LendingErrorMessage");
            }
            else
            {
                bool isSucceeded = await _model.InactivateLending(SelectedLending.ID);
                if (isSucceeded)
                {
                    LendingErrorMessage = "Lending inactivated successfully.";
                    OnPropertyChanged("LendingErrorMessage");
                    OnListLendingsCommand();
                }
                else
                {
                    LendingErrorMessage = "Failed to inactivate lending.";
                    OnPropertyChanged("LendingErrorMessage");
                }
            }
        }

        #endregion

        #region Property change

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

    }
}
