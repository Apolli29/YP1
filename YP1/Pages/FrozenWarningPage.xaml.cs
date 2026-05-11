using System.Windows;
using System.Windows.Controls;
using YP1.Windows;

namespace YP1.Pages
{
    public partial class FrozenWarningPage : UserControl
    {
        private readonly MainWindow _host;

        public FrozenWarningPage()
        {
            InitializeComponent();
        }

        public FrozenWarningPage(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            LoadState();
        }

        private void AppealFreezeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null || !_host.CurrentUser.IsFrozen)
            {
                return;
            }

            TextPromptWindow dialog = new TextPromptWindow("Оспаривание заморозки", "Опиши, почему заморозку аккаунта нужно пересмотреть.", string.Empty);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            _host.Repository.CreateFreezeAppeal(_host.CurrentUser.UserId, "user", _host.CurrentUser.UserId, dialog.ResultText);
            MessageBox.Show("Обращение отправлено администратору.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadState();
        }

        private void LoadState()
        {
            if (_host == null)
            {
                return;
            }

            _host.RefreshCurrentUser();

            if (_host.CurrentUser.IsFrozen)
            {
                WarningTextBlock.Text = "Ваш аккаунт сейчас заморожен. Причина: " + _host.CurrentUser.FreezeReason;
                bool hasPendingAppeal = _host.Repository.HasPendingFreezeAppeal(_host.CurrentUser.UserId, "user", _host.CurrentUser.UserId);
                HintTextBlock.Text = hasPendingAppeal
                    ? "Заявка на снятие заморозки уже отправлена и ждёт решения администратора."
                    : "Если считаешь решение ошибочным, можно отправить объяснение и просьбу о пересмотре.";
                AppealFreezeButton.Visibility = Visibility.Visible;
                AppealFreezeButton.IsEnabled = !hasPendingAppeal;
                AppealFreezeButton.Content = hasPendingAppeal ? "Заявка уже отправлена" : "Оспорить заморозку";
            }
            else
            {
                WarningTextBlock.Text = "Аккаунт не заморожен, ограничений сейчас нет.";
                HintTextBlock.Text = "Эта страница остаётся для истории предупреждений, но активных блокировок нет.";
                AppealFreezeButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
