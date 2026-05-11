using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using YP1.Models;

namespace YP1.Pages
{
    public partial class ReadingListsPage : UserControl
    {
        private readonly MainWindow _host;
        private string _currentListName;

        public ReadingListsPage()
        {
            InitializeComponent();
        }

        public ReadingListsPage(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            _currentListName = "planned";
            LoadGenres();
            LoadBooks();
        }

        private void PlannedListButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchList("planned");
        }

        private void ReadingListButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchList("reading");
        }

        private void CompletedListButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchList("completed");
        }

        private void AbandonedListButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchList("abandoned");
        }

        private void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SortComboBox.SelectedIndex = 0;

            if (GenreComboBox.Items.Count > 0)
            {
                GenreComboBox.SelectedIndex = 0;
            }

            LoadBooks();
        }

        private void OpenBookButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            BookModel book = source == null ? null : source.DataContext as BookModel;

            if (book == null || _host == null)
            {
                return;
            }

            _host.OpenBookPage(book.BookId);
        }

        private void MoveBookButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            BookModel book = source == null ? null : source.DataContext as BookModel;

            if (book == null || _host == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(book.ListName))
            {
                MessageBox.Show("Выбери список для переноса.", "Нет списка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _host.Repository.AddBookToList(_host.CurrentUser.UserId, book.BookId, book.ListName);
            LoadBooks();
        }

        private void LoadGenres()
        {
            if (_host == null)
            {
                return;
            }

            List<GenreModel> genres = _host.Repository.GetGenres();
            genres.Insert(0, new GenreModel { GenreId = 0, Name = "Все жанры" });
            GenreComboBox.ItemsSource = genres;
            GenreComboBox.SelectedIndex = 0;
        }

        private void LoadBooks()
        {
            if (_host == null)
            {
                return;
            }

            int? genreId = null;
            GenreModel selectedGenre = GenreComboBox.SelectedItem as GenreModel;

            if (selectedGenre != null && selectedGenre.GenreId > 0)
            {
                genreId = selectedGenre.GenreId;
            }

            ComboBoxItem selectedSortItem = SortComboBox.SelectedItem as ComboBoxItem;
            string sortMode = selectedSortItem == null ? "name" : selectedSortItem.Tag.ToString();
            List<BookModel> books = _host.Repository.GetBooksFromUserList(_host.CurrentUser.UserId, _currentListName, SearchTextBox.Text, sortMode, genreId);
            BooksItemsControl.ItemsSource = books;
            SummaryTextBlock.Text = books.Count == 0
                ? "В этом списке пока ничего нет."
                : "Книг в списке: " + books.Count + ".";
            UpdateButtons();
        }

        private void SwitchList(string listName)
        {
            _currentListName = listName;
            LoadBooks();
        }

        private void UpdateButtons()
        {
            ApplyButtonStyle(PlannedListButton, _currentListName == "planned");
            ApplyButtonStyle(ReadingListButton, _currentListName == "reading");
            ApplyButtonStyle(CompletedListButton, _currentListName == "completed");
            ApplyButtonStyle(AbandonedListButton, _currentListName == "abandoned");
        }

        private static void ApplyButtonStyle(Button button, bool selected)
        {
            button.Style = (Style)button.FindResource(selected ? "AuthActionButtonStyle" : "SecondaryButtonStyle");
        }
    }
}
