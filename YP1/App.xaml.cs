using System.Windows;
using YP1.Data;
using YP1.Models;
using YP1.Windows;

namespace YP1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                DatabaseInitializer initializer = new DatabaseInitializer();
                initializer.Initialize();

                DatabaseSeeder seeder = new DatabaseSeeder();
                seeder.SeedDemoData();

                AppSession.DatabaseReady = true;
                AppSession.DatabaseError = string.Empty;
            }
            catch (System.Exception exception)
            {
                AppSession.DatabaseReady = false;
                AppSession.DatabaseError = exception.Message;
            }

            var authWindow = new AuthWindow();
            authWindow.Show();
        }
    }
}
