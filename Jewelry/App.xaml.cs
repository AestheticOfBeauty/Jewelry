using Jewelry.Services;
using System.Windows;

namespace Jewelry
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = Dependency.Resolve<MainWindow>();
            MainWindow.Show();
            base.OnStartup(e);
        }
    }
}
