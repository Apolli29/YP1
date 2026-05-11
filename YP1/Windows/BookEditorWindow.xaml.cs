using System.Collections.Generic;
using System.Linq;
using System.Windows;
using YP1.Models;

namespace YP1.Windows
{
    public partial class BookEditorWindow : Window
    {
        private readonly BookModel _book;
        private readonly List<GenreModel> _genres;

        public BookEditorWindow(BookModel book, List<GenreModel> genres)
        {
            InitializeComponent();
            _book = book ?? new BookModel();
            _genres = genres.Select(x => new GenreModel
            {
                GenreId = x.GenreId,
                Name = x.Name,
                IsSelected = _book.GenreIds.Contains(x.GenreId)
            }).ToList();

            HeaderTextBlock.Text = _book.BookId == 0 ? "Новая книга" : "Редактирование книги";
            TitleTextBox.Text = _book.Title;
            DescriptionTextBox.Text = _book.Description;
            BookTextTextBox.Text = _book.BookText;
            CoverColorTextBox.Text = string.IsNullOrWhiteSpace(_book.CoverColor) ? "#5E6C84" : _book.CoverColor;
            GenresItemsControl.ItemsSource = _genres;
        }

        public BookModel EditedBook
        {
            get { return _book; }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) || string.IsNullOrWhiteSpace(DescriptionTextBox.Text) || string.IsNullOrWhiteSpace(BookTextTextBox.Text))
            {
                MessageBox.Show("Название, описание и текст книги обязательны.", "Не все поля заполнены", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _book.Title = TitleTextBox.Text.Trim();
            _book.Description = DescriptionTextBox.Text.Trim();
            _book.BookText = BookTextTextBox.Text.Trim();
            _book.CoverColor = string.IsNullOrWhiteSpace(CoverColorTextBox.Text) ? "#5E6C84" : CoverColorTextBox.Text.Trim();
            _book.GenreIds = _genres.Where(x => x.IsSelected).Select(x => x.GenreId).ToList();

            if (_book.GenreIds.Count == 0)
            {
                MessageBox.Show("Выбери хотя бы один жанр.", "Нет жанра", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
