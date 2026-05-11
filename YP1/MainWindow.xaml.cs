using System.Windows;
using System.Windows.Controls;
using YP1.Data;
using YP1.Models;
using YP1.Pages;

namespace YP1
{
    public partial class MainWindow : Window
    {
        private readonly LibraryRepository _repository;
        private UserModel _currentUser;

        public MainWindow(UserModel currentUser, LibraryRepository repository)
        {
            InitializeComponent();
            _repository = repository;
            _currentUser = currentUser;
            ApplyUserState();
            OpenCatalog();
        }

        public UserModel CurrentUser
        {
            get { return _currentUser; }
        }

        public LibraryRepository Repository
        {
            get { return _repository; }
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowCatalogPage();
        }

        private void ListsButton_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "Списки книг";
            PageHost.Content = new ReadingListsPage();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "Администрирование";
            PageHost.Content = new AdminPage();
        }

        private void AuthorButton_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "Страница автора";
            PageHost.Content = new AuthorPage();
        }

        private void FrozenButton_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "Предупреждение";
            PageHost.Content = new FrozenWarningPage();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            PageTitleTextBlock.Text = "Профиль";
            PageHost.Content = new ProfilePage();
        }

        private void OpenCatalog()
        {
            PageTitleTextBlock.Text = "Каталог книг";
            PageHost.Content = new CatalogPage(this);
        }

        public void ShowCatalogPage()
        {
            OpenCatalog();
        }

        public void OpenBookPage(int bookId)
        {
            PageTitleTextBlock.Text = "Страница книги";
            PageHost.Content = new BookPage(this, bookId);
        }

        public void RefreshCurrentUser()
        {
            _currentUser = _repository.GetUserById(_currentUser.UserId);
            AppSession.CurrentUser = _currentUser;
            ApplyUserState();
        }

        private void ApplyUserState()
        {
            CurrentUserTextBlock.Text = _currentUser.FullName + " (" + TranslateRole(_currentUser.RoleName) + ")";
            AdminButton.Visibility = _currentUser.IsAdministrator ? Visibility.Visible : Visibility.Collapsed;
            AuthorButton.Visibility = _currentUser.IsAuthor ? Visibility.Visible : Visibility.Collapsed;
            FrozenButton.Visibility = _currentUser.IsFrozen ? Visibility.Visible : Visibility.Collapsed;
        }

        private static string TranslateRole(string roleName)
        {
            if (roleName == "administrator")
            {
                return "администратор";
            }

            if (roleName == "author")
            {
                return "автор";
            }

            return "читатель";
        }
    }
}
