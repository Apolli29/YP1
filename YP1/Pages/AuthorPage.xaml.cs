using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using YP1.Models;
using YP1.Windows;

namespace YP1.Pages
{
    public partial class AuthorPage : UserControl
    {
        private readonly MainWindow _host;

        public AuthorPage()
        {
            InitializeComponent();
        }

        public AuthorPage(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            LoadBooks();
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null)
            {
                return;
            }

            BookModel book = new BookModel();
            book.AuthorId = _host.CurrentUser.UserId;
            OpenEditor(book);
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

        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            BookModel book = source == null ? null : source.DataContext as BookModel;

            if (book == null || _host == null)
            {
                return;
            }

            BookModel fullBook = _host.Repository.GetBookById(book.BookId);
            OpenEditor(fullBook);
        }

        private void AppealFrozenBookButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            BookModel book = source == null ? null : source.DataContext as BookModel;

            if (book == null || _host == null)
            {
                return;
            }

            if (_host.Repository.HasPendingFreezeAppeal(_host.CurrentUser.UserId, "book", book.BookId))
            {
                MessageBox.Show("По этой книге уже есть заявка на снятие заморозки.", "Заявка уже есть", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TextPromptWindow dialog = new TextPromptWindow("Оспаривание заморозки", "Опиши, какие исправления были внесены и почему книгу стоит вернуть.", string.Empty);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            _host.Repository.CreateFreezeAppeal(_host.CurrentUser.UserId, "book", book.BookId, dialog.ResultText);
            MessageBox.Show("Заявка отправлена администратору.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenEditor(BookModel book)
        {
            if (_host == null)
            {
                return;
            }

            List<GenreModel> genres = _host.Repository.GetGenres();
            BookEditorWindow window = new BookEditorWindow(book, genres);
            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() != true)
            {
                return;
            }

            BookModel editedBook = window.EditedBook;
            editedBook.AuthorId = _host.CurrentUser.UserId;
            _host.Repository.SaveBook(editedBook);
            LoadBooks();
        }

        private void LoadBooks()
        {
            if (_host == null)
            {
                return;
            }

            List<BookModel> authorBooks = _host.Repository.GetAuthorBooks(_host.CurrentUser.UserId, false);
            List<BookModel> frozenBooks = _host.Repository.GetAuthorBooks(_host.CurrentUser.UserId, true);

            AuthorBooksItemsControl.ItemsSource = authorBooks;
            FrozenBooksItemsControl.ItemsSource = frozenBooks;
        }
    }
}
