using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using YP1.Models;
using YP1.Windows;

namespace YP1.Pages
{
    public partial class AdminPage : UserControl
    {
        private readonly MainWindow _host;

        public AdminPage()
        {
            InitializeComponent();
        }

        public AdminPage(MainWindow host)
        {
            InitializeComponent();
            _host = host;
            LoadData();
        }

        private void ApproveReportButton_Click(object sender, RoutedEventArgs e)
        {
            ResolveReport(sender, true);
        }

        private void RejectReportButton_Click(object sender, RoutedEventArgs e)
        {
            ResolveReport(sender, false);
        }

        private void ApproveAppealButton_Click(object sender, RoutedEventArgs e)
        {
            ResolveAppeal(sender, true);
        }

        private void RejectAppealButton_Click(object sender, RoutedEventArgs e)
        {
            ResolveAppeal(sender, false);
        }

        private void ApproveAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            ResolveAuthorApplication(sender, true);
        }

        private void RejectAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            ResolveAuthorApplication(sender, false);
        }

        private void SaveRoleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null)
            {
                return;
            }

            FrameworkElement source = sender as FrameworkElement;
            UserModel user = source == null ? null : source.DataContext as UserModel;

            if (user == null)
            {
                return;
            }

            _host.Repository.UpdateUserRole(user.UserId, user.RoleName);
            MessageBox.Show("Роль обновлена.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadData();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_host == null)
            {
                return;
            }

            FrameworkElement source = sender as FrameworkElement;
            UserModel user = source == null ? null : source.DataContext as UserModel;

            if (user == null)
            {
                return;
            }

            TextPromptWindow dialog = new TextPromptWindow("Смена пароля", "Введи новый пароль для пользователя " + user.Login + ".", string.Empty);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            _host.Repository.ChangePassword(user.UserId, dialog.ResultText);
            MessageBox.Show("Пароль изменён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResolveReport(object sender, bool approve)
        {
            if (_host == null)
            {
                return;
            }

            FrameworkElement source = sender as FrameworkElement;
            ReportModel report = source == null ? null : source.DataContext as ReportModel;

            if (report == null)
            {
                return;
            }

            _host.Repository.ResolveReport(report.ReportId, approve);
            LoadData();
        }

        private void ResolveAppeal(object sender, bool approve)
        {
            if (_host == null)
            {
                return;
            }

            FrameworkElement source = sender as FrameworkElement;
            FreezeAppealModel appeal = source == null ? null : source.DataContext as FreezeAppealModel;

            if (appeal == null)
            {
                return;
            }

            _host.Repository.ResolveFreezeAppeal(appeal.AppealId, approve);
            LoadData();
        }

        private void ResolveAuthorApplication(object sender, bool approve)
        {
            if (_host == null)
            {
                return;
            }

            FrameworkElement source = sender as FrameworkElement;
            AuthorApplicationModel application = source == null ? null : source.DataContext as AuthorApplicationModel;

            if (application == null)
            {
                return;
            }

            _host.Repository.ResolveAuthorApplication(application.ApplicationId, approve);
            LoadData();
        }

        private void LoadData()
        {
            if (_host == null)
            {
                return;
            }

            List<ReportModel> reports = _host.Repository.GetReports();
            List<FreezeAppealModel> appeals = _host.Repository.GetFreezeAppeals();
            List<AuthorApplicationModel> applications = _host.Repository.GetAuthorApplications();
            List<FrozenItemModel> frozenItems = _host.Repository.GetFrozenItems();
            List<UserModel> users = _host.Repository.GetUsers();

            ReportsDataGrid.ItemsSource = reports;
            AppealsDataGrid.ItemsSource = appeals;
            AuthorApplicationsDataGrid.ItemsSource = applications;
            FrozenItemsDataGrid.ItemsSource = frozenItems;
            UsersDataGrid.ItemsSource = users;
        }
    }
}
