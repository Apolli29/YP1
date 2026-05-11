using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YP1.Models;
using YP1.Windows;

namespace YP1.Pages
{
    public partial class BookPage : UserControl
    {
        private readonly MainWindow _host;
        private readonly int _bookId;
        private BookModel _book;

        public BookPage()
        {
            InitializeComponent();
        }

        public BookPage(MainWindow host, int bookId)
        {
            InitializeComponent();
            _host = host;
            _bookId = bookId;
            LoadBook();
            LoadReviews();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host != null)
            {
                _host.ShowCatalogPage();
            }
        }

        private void AddToListButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null || _book == null)
            {
                return;
            }

            ComboBoxItem selectedItem = ListComboBox.SelectedItem as ComboBoxItem;
            string listName = selectedItem == null ? "planned" : selectedItem.Tag.ToString();
            _host.Repository.AddBookToList(_host.CurrentUser.UserId, _book.BookId, listName);
            MessageBox.Show("Книга добавлена в список.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReportBookButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitReport("book", _book == null ? 0 : _book.BookId, "Жалоба на книгу");
        }

        private void ReportAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitReport("author", _book == null ? 0 : _book.AuthorId, "Жалоба на автора");
        }

        private void FreezeBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null || _book == null)
            {
                return;
            }

            string reason = RequestText("Заморозка книги", "Укажи причину заморозки книги.");

            if (string.IsNullOrWhiteSpace(reason))
            {
                return;
            }

            _host.Repository.FreezeBook(_book.BookId, reason);
            LoadBook();
            MessageBox.Show("Книга заморожена.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null || _book == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ReviewTextBox.Text))
            {
                MessageBox.Show("Напиши текст отзыва.", "Пустой отзыв", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ComboBoxItem selectedItem = ReviewRatingComboBox.SelectedItem as ComboBoxItem;
            int rating = selectedItem == null ? 5 : int.Parse(selectedItem.Tag.ToString());
            _host.Repository.SaveReview(_host.CurrentUser.UserId, _book.BookId, rating, ReviewTextBox.Text.Trim());
            ReviewTextBox.Text = string.Empty;
            LoadBook();
            LoadReviews();
            MessageBox.Show("Отзыв сохранён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReportReviewButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            ReviewModel review = source == null ? null : source.DataContext as ReviewModel;

            if (review == null)
            {
                return;
            }

            SubmitReport("review", review.ReviewId, "Жалоба на отзыв");
        }

        private void FreezeReviewButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            ReviewModel review = source == null ? null : source.DataContext as ReviewModel;

            if (review == null || _host == null)
            {
                return;
            }

            string reason = RequestText("Заморозка отзыва", "Укажи причину заморозки отзыва.");

            if (string.IsNullOrWhiteSpace(reason))
            {
                return;
            }

            _host.Repository.FreezeReview(review.ReviewId, reason);
            LoadReviews();
            MessageBox.Show("Отзыв заморожен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadBook()
        {
            if (_host == null)
            {
                return;
            }

            _book = _host.Repository.GetBookById(_bookId);

            if (_book == null)
            {
                return;
            }

            BookTitleTextBlock.Text = _book.Title;
            CoverTitleTextBlock.Text = _book.Title;
            BookAuthorTextBlock.Text = "Автор: " + _book.AuthorName;
            BookGenresTextBlock.Text = "Жанры: " + _book.GenresText;
            BookRatingTextBlock.Text = "Рейтинг: " + _book.AverageRating.ToString("0.0");
            BookDescriptionTextBlock.Text = _book.Description;
            BookTextTextBox.Text = _book.BookText;
            FreezeBookButton.Visibility = _host.CurrentUser.IsAdministrator ? Visibility.Visible : Visibility.Collapsed;
            CoverBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_book.CoverColor));

            if (_book.IsFrozen)
            {
                FrozenInfoBorder.Visibility = Visibility.Visible;
                FrozenInfoTextBlock.Text = "Книга заморожена. Причина: " + _book.FreezeReason;
            }
            else
            {
                FrozenInfoBorder.Visibility = Visibility.Collapsed;
                FrozenInfoTextBlock.Text = string.Empty;
            }
        }

        private void LoadReviews()
        {
            if (_host == null)
            {
                return;
            }

            List<ReviewModel> reviews = _host.Repository.GetReviewsByBook(_bookId);

            foreach (ReviewModel review in reviews)
            {
                review.CanAdminFreeze = _host.CurrentUser.IsAdministrator;
            }

            ReviewsItemsControl.ItemsSource = reviews;
        }

        private void SubmitReport(string targetType, int targetId, string title)
        {
            if (_host == null || _book == null)
            {
                return;
            }

            string reason = RequestText(title, "Коротко опиши причину жалобы.");

            if (string.IsNullOrWhiteSpace(reason))
            {
                return;
            }

            _host.Repository.SaveReport(_host.CurrentUser.UserId, targetType, targetId, reason);
            MessageBox.Show("Жалоба отправлена.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string RequestText(string title, string description)
        {
            TextPromptWindow dialog = new TextPromptWindow(title, description, string.Empty);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                return dialog.ResultText;
            }

            return string.Empty;
        }
    }
}
