using System.Windows;
using YP1.Pages;

namespace YP1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OpenCatalog();
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            OpenCatalog();
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
            PageHost.Content = new CatalogPage();
        }
    }
}
