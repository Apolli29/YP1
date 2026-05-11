using System.Windows;
using YP1.Windows;

namespace YP1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var authWindow = new AuthWindow();
            authWindow.Show();
        }
    }
}
