using System.Windows;
using YP1.Data;
using YP1.Models;

namespace YP1.Windows
{
    public partial class AuthWindow : Window
    {
        private readonly LibraryRepository _repository;

        public AuthWindow()
        {
            InitializeComponent();
            _repository = new LibraryRepository();
            UpdateDatabaseState();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSession.DatabaseReady)
            {
                MessageBox.Show("Сначала нужно настроить подключение к SQL Server в App.config.", "Нет подключения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UserModel user = _repository.Authenticate(LoginTextBox.Text, PasswordBox.Password);

            if (user == null)
            {
                MessageBox.Show("Неправильный логин или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AppSession.CurrentUser = user;
            OpenMainWindow(user);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSession.DatabaseReady)
            {
                MessageBox.Show("Сначала нужно настроить подключение к SQL Server в App.config.", "Нет подключения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string errorMessage;

            if (!_repository.RegisterUser(RegisterNameTextBox.Text, RegisterLoginTextBox.Text, RegisterEmailTextBox.Text, RegisterPasswordBox.Password, out errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UserModel user = _repository.Authenticate(RegisterLoginTextBox.Text, RegisterPasswordBox.Password);
            AppSession.CurrentUser = user;
            OpenMainWindow(user);
        }

        private void OpenMainWindow(UserModel user)
        {
            MainWindow mainWindow = new MainWindow(user, _repository);
            mainWindow.Show();
            Close();
        }

        private void UpdateDatabaseState()
        {
            if (AppSession.DatabaseReady)
            {
                DatabaseWarningBorder.Visibility = Visibility.Collapsed;
                LoginButton.IsEnabled = true;
                RegisterButton.IsEnabled = true;
                return;
            }

            DatabaseWarningBorder.Visibility = Visibility.Visible;
            DatabaseWarningTextBlock.Text = "База данных пока недоступна. Проверь строку подключения к SQL Server в App.config. Техническая ошибка: "
                + AppSession.DatabaseError;
            LoginButton.IsEnabled = false;
            RegisterButton.IsEnabled = false;
        }
    }
}
