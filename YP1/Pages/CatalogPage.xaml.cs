using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using YP1.Models;

namespace YP1.Pages
{
    public partial class CatalogPage : UserControl
    {
        private readonly MainWindow _host;

        public CatalogPage()
        {
            InitializeComponent();
        }

        public CatalogPage(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            LoadGenres();
            LoadBooks();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
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

        private void AddToListButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            BookModel book = source == null ? null : source.DataContext as BookModel;

            if (book == null || _host == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(book.ListName))
            {
                book.ListName = "planned";
            }

            _host.Repository.AddBookToList(_host.CurrentUser.UserId, book.BookId, book.ListName);
            MessageBox.Show("Книга сохранена в выбранный список.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
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
            List<BookModel> books = _host.Repository.GetCatalogBooks(SearchTextBox.Text, sortMode, genreId);

            foreach (BookModel book in books)
            {
                if (string.IsNullOrWhiteSpace(book.ListName))
                {
                    book.ListName = "planned";
                }
            }

            BooksItemsControl.ItemsSource = books;
            SummaryTextBlock.Text = books.Count == 0
                ? "По выбранным параметрам книги не найдены."
                : "Найдено книг: " + books.Count + ".";
        }
    }
}
