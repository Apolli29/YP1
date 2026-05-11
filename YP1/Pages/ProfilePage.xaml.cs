using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using YP1.Models;
using YP1.Windows;

namespace YP1.Pages
{
    public partial class ProfilePage : UserControl
    {
        private readonly MainWindow _host;

        public ProfilePage()
        {
            InitializeComponent();
        }

        public ProfilePage(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            LoadProfile();
        }

        private void AuthorRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null)
            {
                return;
            }

            string message = RequestText("Заявка на роль автора", "Напиши коротко, почему хочешь получить роль автора.");

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _host.Repository.CreateAuthorApplication(_host.CurrentUser.UserId, message);
            MessageBox.Show("Заявка отправлена администратору.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadProfile();
        }

        private void AppealButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null || !_host.CurrentUser.IsFrozen)
            {
                return;
            }

            string message = RequestText("Оспаривание заморозки", "Опиши, почему заморозку стоит снять.");

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _host.Repository.CreateFreezeAppeal(_host.CurrentUser.UserId, "user", _host.CurrentUser.UserId, message);
            MessageBox.Show("Заявка на снятие заморозки отправлена.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadProfile();
        }

        private void LoadProfile()
        {
            if (_host == null)
            {
                return;
            }

            _host.RefreshCurrentUser();
            UserModel user = _host.CurrentUser;

            FullNameTextBlock.Text = user.FullName;
            LoginTextBlock.Text = "Логин: " + user.Login;
            EmailTextBlock.Text = "Почта: " + user.Email;
            RoleTextBlock.Text = "Роль: " + TranslateRole(user.RoleName);

            List<ReviewModel> reviews = _host.Repository.GetUserReviews(user.UserId);
            ReviewsItemsControl.ItemsSource = reviews;

            bool hasPendingAuthorRequest = _host.Repository.HasPendingAuthorApplication(user.UserId);
            AuthorRequestButton.Visibility = user.IsAdministrator ? Visibility.Collapsed : Visibility.Visible;
            AuthorRequestButton.IsEnabled = !user.IsAuthor && !hasPendingAuthorRequest;
            AuthorRequestButton.Content = user.IsAuthor ? "Уже автор" : hasPendingAuthorRequest ? "Заявка уже отправлена" : "Стать автором";

            if (user.IsFrozen)
            {
                FreezeInfoBorder.Visibility = Visibility.Visible;
                FreezeReasonTextBlock.Text = "Причина: " + user.FreezeReason;
                AppealButton.Visibility = Visibility.Visible;
                AppealButton.IsEnabled = !_host.Repository.HasPendingFreezeAppeal(user.UserId, "user", user.UserId);
                AppealButton.Content = AppealButton.IsEnabled ? "Оспорить заморозку" : "Заявка уже отправлена";
            }
            else
            {
                FreezeInfoBorder.Visibility = Visibility.Collapsed;
                AppealButton.Visibility = Visibility.Collapsed;
            }
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
